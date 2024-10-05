using Telegram.Bot;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendInitialMessageToUserStep : IStepBody, IUserStep
{
    private const string InitialMessageTemplate = "{0}\n Для завершения команды введите 'Выйти'";

    private readonly ITelegramBotClient _client;
    private readonly IConversationHistory _conversationHistory;

    public SendInitialMessageToUserStep(ITelegramBotClient client, IConversationHistory conversationHistory)
    {
        _client = client;
        _conversationHistory = conversationHistory;
    }

    public string UserId { get; set; } = null!;
    
    public string Message { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var message = string.Format(InitialMessageTemplate, Message);

        var sentMessage = await _client.SendTextMessageAsync(UserId, message, cancellationToken: context.CancellationToken);

        _conversationHistory.AddMessage(UserId, sentMessage.MessageId);
        
        return ExecutionResult.Next();
    }

}