using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendInlineDataMessageToUser : IUserStep, IStepBody
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IConversationHistory _conversationHistory;

    public SendInlineDataMessageToUser(ITelegramBotClient telegramBotClient, IConversationHistory conversationHistory)
    {
        _telegramBotClient = telegramBotClient;
        _conversationHistory = conversationHistory;
    }
    
    public string UserId { get; set; } = null!;
    
    public string Message { get; set; } = null!;

    public InlineKeyboardButton[] Options { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var replyMarkup = new InlineKeyboardMarkup(Options);

        var result =
            await _telegramBotClient.SendTextMessageAsync(int.Parse(UserId), Message, replyMarkup: replyMarkup);

        _conversationHistory.AddMessage(UserId, result.MessageId);

        return ExecutionResult.Next();
    }

}