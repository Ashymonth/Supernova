using System.Globalization;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.MyAppointments.Steps;

public class SendAppointmentToDelete : IUserStep, IStepBody
{
    private readonly ITelegramBotClient _telegramBotClient;

    public SendAppointmentToDelete(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointments { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        foreach (var studentAppointmentInfoChunk in StudentAppointments.Chunk(3))
        {
            var tasks = studentAppointmentInfoChunk.Select(async studentAppointmentInfo =>
            {
                var button = InlineKeyboardButton.WithCallbackData("Удалить",
                    studentAppointmentInfo.DueDate.ToString(CultureInfo.GetCultureInfo("ru-RU")));

                await _telegramBotClient.SendTextMessageAsync(UserId,
                    $"{studentAppointmentInfo.TeacherName} - {studentAppointmentInfo.DueDate}",
                    replyMarkup: new InlineKeyboardMarkup(button),
                    cancellationToken: context.CancellationToken);
            });

            await Task.WhenAll(tasks);
        }

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}