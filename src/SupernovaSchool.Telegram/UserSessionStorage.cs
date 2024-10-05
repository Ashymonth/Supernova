using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;
 

public interface IUserSessionStorage
{
    Task<bool> StartWorkflow(string userId, string workflowName, object workflowData);

    Task TerminateWorkflow(string userId);
}

public class UserSessionStorage : IUserSessionStorage
{
    private const string CacheKeyTemplate = "UserSessionStorage_{0}";

    private readonly IMemoryCache _memoryCache;
    private readonly IWorkflowHost _workflowHost;
    private readonly IConversationHistory _conversationHistory;
    private readonly ITelegramBotClient _telegramBotClient;

    public UserSessionStorage(IMemoryCache memoryCache, IWorkflowHost workflowHost,
        IConversationHistory conversationHistory, ITelegramBotClient telegramBotClient)
    {
        _memoryCache = memoryCache;
        _workflowHost = workflowHost;
        _conversationHistory = conversationHistory;
        _telegramBotClient = telegramBotClient;
    }

    public async Task<bool> StartWorkflow(string userId, string workflowName, object workflowData)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);

        if (_memoryCache.TryGetValue(cacheKey, out _))
        {
            return false;
        }

        var workflow = await _workflowHost.StartWorkflow(workflowName, workflowData);

        _memoryCache.Set(cacheKey, workflow);

        return true;
    }

    public async Task TerminateWorkflow(string userId)
    {
        var sessionInfo = _memoryCache.Get<string>(userId);
        if (sessionInfo is null)
        {
            return;
        }

        await _workflowHost.TerminateWorkflow(sessionInfo);

        _conversationHistory.CleanMessages(userId, out var messageIds);

        await Parallel.ForEachAsync(messageIds, new ParallelOptions { MaxDegreeOfParallelism = 3 },
            async (messageId, token) =>
            {
                await _telegramBotClient.DeleteMessageAsync(userId, messageId, cancellationToken: token);
            });
    }
}