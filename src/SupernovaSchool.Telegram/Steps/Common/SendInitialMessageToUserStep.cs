using Telegram.Bot;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendInitialMessageToUserStep : IStepBody, IUserStep
{
    private const string InitialMessageTemplate = "{0}\n Для завершения команды введите 'Выйти'";

    private readonly ITelegramBotClient _client;

    public SendInitialMessageToUserStep(ITelegramBotClient client)
    {
        _client = client;
    }

    public string Message { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var message = string.Format(InitialMessageTemplate, Message);

        await _client.SendTextMessageAsync(UserId, message, cancellationToken: context.CancellationToken);

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}