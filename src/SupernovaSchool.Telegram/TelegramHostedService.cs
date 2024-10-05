using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.MyAppointments;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;

public class TelegramHostedService : IHostedService
{
    private static readonly ConcurrentDictionary<string, string> UserIdToWorkflowIdMap = new();
    private readonly IServiceProvider _serviceProvider;

    private readonly ITelegramBotClient _telegramBotClient;

    public TelegramHostedService(ITelegramBotClient telegramBotClient, IServiceProvider serviceProvider)
    {
        _telegramBotClient = telegramBotClient;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _telegramBotClient.SetMyCommandsAsync([
            new BotCommand { Command = Commands.CreateAppointmentCommand, Description = "Записаться к психологу" },
            new BotCommand { Command = Commands.DeleteAppointmentCommand, Description = "Отменить запись к психологу" },
            new BotCommand { Command = Commands.RegisterAsStudentCommand, Description = "Зарегистрироваться" }
        ], cancellationToken: cancellationToken);

        await _telegramBotClient.ReceiveAsync(async (client, update, arg3) =>
            {
                var message = update.Message?.Text ?? update.CallbackQuery?.Data;
                var messageId = update.Message?.MessageId ?? update.CallbackQuery?.Message?.MessageId!;
                var userId = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From.Id.ToString();

                var workflowHost = _serviceProvider.GetRequiredService<IWorkflowHost>();
                using var scope = _serviceProvider.CreateScope();

                await _telegramBotClient.SendChatActionAsync(long.Parse(userId!), ChatAction.Typing,
                    cancellationToken: cancellationToken);
                switch (message)
                {
                    case "Выйти":
                        if (UserIdToWorkflowIdMap.TryRemove(userId!, out var workflowId))
                            await workflowHost.TerminateWorkflow(workflowId);

                        await _telegramBotClient.SendTextMessageAsync(long.Parse(userId!), "Вы завершили команду",
                            cancellationToken: cancellationToken);
                        break;
                    case Commands.CreateTeacherCommand:
                        var createTeacherWorkflowId = await workflowHost.StartWorkflow(
                            nameof(CreateTeacherWorkflow),
                            new CreateTeacherWorkflowData { UserId = userId! });

                        UserIdToWorkflowIdMap.TryAdd(userId!, createTeacherWorkflowId);
                        break;
                    case Commands.DeleteAppointmentCommand:
                        var deleteAppointmentWorkflowId = await workflowHost.StartWorkflow(
                            nameof(DeleteMyAppointmentsWorkflow),
                            new DeleteMyAppointmentsWorkflowData { UserId = userId! });

                        UserIdToWorkflowIdMap.TryAdd(userId!, deleteAppointmentWorkflowId);
                        break;
                    case Commands.RegisterAsStudentCommand:
                        var registerWorkflowId = await workflowHost.StartWorkflow(nameof(RegisterStudentWorkflow),
                            new RegisterStudentWorkflowData { UserId = userId! });

                        UserIdToWorkflowIdMap.TryAdd(userId!, registerWorkflowId);
                        break;
                    case Commands.CreateAppointmentCommand:
                        var appointmentWorkflowId = await workflowHost.StartWorkflow(nameof(CreateAppointmentWorkflow),
                            new CreateAppointmentWorkflowData { UserId = userId! });
                        UserIdToWorkflowIdMap.TryAdd(userId!, appointmentWorkflowId);
                        break;
                    default:
                        await workflowHost.PublishUserMessageAsync(update.Type, userId!,
                            new UserMessage(message!, messageId!.Value));
                        break;
                }
            }, (client, exception, arg3) => Task.CompletedTask, new ReceiverOptions { ThrowPendingUpdates = true },
            cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}