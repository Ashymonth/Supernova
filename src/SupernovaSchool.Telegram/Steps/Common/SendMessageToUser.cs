using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendMessageToUser : IStepBody, IUserStep
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IConversationHistory _conversationHistory;

    public SendMessageToUser(ITelegramBotClient telegramBotClient, IConversationHistory conversationHistory)
    {
        _telegramBotClient = telegramBotClient;
        _conversationHistory = conversationHistory;
    }

    public string UserId { get; set; } = default!;

    public string Message { get; set; } = default!;

    public bool ShouldBeDeleted { get; set; } = true;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var message =
            await _telegramBotClient.SendMessage(UserId, Message, replyMarkup: new ReplyKeyboardRemove());

        if (ShouldBeDeleted)
        {
            _conversationHistory.AddMessage(UserId, message.MessageId);
        }

        return ExecutionResult.Next();
    }
}