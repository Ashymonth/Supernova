using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendInlineDataMessageToUser : IUserStep, IStepBody
{
    private readonly ITelegramBotClient _telegramBotClient;

    public SendInlineDataMessageToUser(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public string Message { get; set; } = null!;

    public InlineKeyboardButton[] Options { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var replyMarkup = new InlineKeyboardMarkup(Options);

        var result =
            await _telegramBotClient.SendTextMessageAsync(int.Parse(UserId), Message, replyMarkup: replyMarkup);

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}