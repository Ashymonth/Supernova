using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendMessageToUser : IStepBody, IUserStep
{
    private readonly ITelegramBotClient _telegramBotClient;

    public SendMessageToUser(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public string Message { get; set; } = default!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var message =
            await _telegramBotClient.SendTextMessageAsync(UserId, Message, replyMarkup: new ReplyKeyboardRemove());

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = default!;
}