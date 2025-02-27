using System.Globalization;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments.Steps;

public class SendAppointmentToDelete : IUserStep, IStepBody
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IConversationHistory _conversationHistory;
    
    public SendAppointmentToDelete(ITelegramBotClient telegramBotClient, IConversationHistory conversationHistory)
    {
        _telegramBotClient = telegramBotClient;
        _conversationHistory = conversationHistory;
    }
    
    public string UserId { get; set; } = null!;
    
    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointments { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        foreach (var studentAppointmentInfoChunk in StudentAppointments.Chunk(3))
        {
            var tasks = studentAppointmentInfoChunk.Select(async studentAppointmentInfo =>
            {
                var button = InlineKeyboardButton.WithCallbackData("Удалить",
                    studentAppointmentInfo.DueDate.ToString(CultureInfo.GetCultureInfo("ru-RU")));

                var message = await _telegramBotClient.SendMessage(UserId,
                    $"{studentAppointmentInfo.TeacherName} - {studentAppointmentInfo.DueDate}",
                    replyMarkup: new InlineKeyboardMarkup(button),
                    cancellationToken: context.CancellationToken);

                _conversationHistory.AddMessage(UserId, message.MessageId);
            });

            await Task.WhenAll(tasks);
        }

        return ExecutionResult.Next();
    }
}