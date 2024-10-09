using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;

public class UpdateHandler
{
    private const string ExistCommandName = "Выйти";
    private const string StartCommandName = "/start";

    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IWorkflowHost _workflowHost;
    private readonly CommandRegistry _commandRegistry;
    private readonly IUserSessionStorage _userSessionStorage;
    private readonly IConversationHistory _conversationHistory;
    private readonly ICommandUploader _commandUploader;

    public UpdateHandler(ITelegramBotClient telegramBotClient, IWorkflowHost workflowHost,
        CommandRegistry commandRegistry, IUserSessionStorage userSessionStorage,
        IConversationHistory conversationHistory, ICommandUploader commandUploader)
    {
        _telegramBotClient = telegramBotClient;
        _workflowHost = workflowHost;
        _commandRegistry = commandRegistry;
        _userSessionStorage = userSessionStorage;
        _conversationHistory = conversationHistory;
        _commandUploader = commandUploader;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken token = default)
    {
        var message = update.Message?.Text ?? update.CallbackQuery?.Data!;
        var messageId = update.Message?.MessageId ?? update.CallbackQuery?.Message?.MessageId!;
        var userId = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From.Id.ToString()!;

        await _telegramBotClient.SendChatActionAsync(long.Parse(userId), ChatAction.Typing,
            cancellationToken: token);
        
        if (string.Equals(StartCommandName, message, StringComparison.InvariantCultureIgnoreCase))
        {
            await _telegramBotClient.SendTextMessageAsync(userId, $"""
                                                                   Привет! Я бот, с помощью которого можно удобно записаться к психологу.
                                                                   Первым делом тебе нужно зарегистрироваться, чтобы психолог мог видеть, кто к нему записался.
                                                                   Сделать это можно с помощью команды: {Commands.RegisterAsStudentCommand}.
                                                                   После регистрации ты сможешь записаться с помощью команды: {Commands.CreateAppointmentCommand}.
                                                                   Важно: На 1 день доступна только 1 запись. 
                                                                   Если ты передумал или захотел перенести запись, то с помощью команды: {Commands.DeleteAppointmentCommand} ты сможешь отменить свою запись.
                                                                   Любую команду можно преравть, если ты напишешь 'Выйти'
                                                                   """,
                cancellationToken: token);
            await _commandUploader.UploadUserCommandsAsync(userId, token);
            return;
        }
        
        _conversationHistory.AddMessage(userId, messageId.Value);

        if (string.Equals(ExistCommandName, message, StringComparison.InvariantCultureIgnoreCase))
        {
            await _userSessionStorage.TerminateWorkflow(userId, token);
            await _telegramBotClient.SendTextMessageAsync(userId, "Команда отменена",
                cancellationToken: token);
            return;
        }

        if (_commandRegistry.TryGetWorkflowByCommandName(message, out var workflowDataFactory))
        {
            if (!await _userSessionStorage.StartWorkflow(userId, message, workflowDataFactory!(userId)))
            {
                await _telegramBotClient.SendTextMessageAsync(userId,
                    "Нельзя вызвать новую команду, пока вы не завершили старую",
                    cancellationToken: token);
            }

            return;
        }

        await _workflowHost.PublishUserMessageAsync(update.Type, userId,
            new UserMessage(message, messageId.Value));
    }
}