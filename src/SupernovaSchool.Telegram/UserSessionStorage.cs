using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Metrics;
using Telegram.Bot;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;

public interface IUserSessionStorage
{
    Task<bool> StartWorkflow(string userId, string workflowName, object workflowData);

    Task TerminateWorkflow(string userId, bool isCancelledByUser, CancellationToken ct = default);
}

public class UserSessionStorage : IUserSessionStorage
{
    private const string CacheKeyTemplate = "UserSessionStorage_{0}";

    private readonly IMemoryCache _memoryCache;
    private readonly IWorkflowHost _workflowHost;
    private readonly IConversationHistory _conversationHistory;
    private readonly ITelegramBotClientWrapper _telegramBotClient;
    private readonly WorkflowCancelledMeter _workflowCancelledMeter;
    private readonly WorkflowDurationGaugeMetric _durationGaugeMetric;

    public UserSessionStorage(IMemoryCache memoryCache, IWorkflowHost workflowHost,
        IConversationHistory conversationHistory, ITelegramBotClientWrapper telegramBotClient,
        WorkflowCancelledMeter workflowCancelledMeter, WorkflowDurationGaugeMetric durationGaugeMetric)
    {
        _memoryCache = memoryCache;
        _workflowHost = workflowHost;
        _conversationHistory = conversationHistory;
        _telegramBotClient = telegramBotClient;
        _workflowCancelledMeter = workflowCancelledMeter;
        _durationGaugeMetric = durationGaugeMetric;
    }

    public async Task<bool> StartWorkflow(string userId, string workflowName, object workflowData)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);

        if (_memoryCache.TryGetValue(cacheKey, out _))
        {
            return false;
        }

        var workflowId = await _workflowHost.StartWorkflow(workflowName, workflowData);

        _memoryCache.Set(cacheKey,
            new ConversationHistoryInfo { WorkflowName = workflowName, WorkflowId = workflowId });

        return true;
    }

    public async Task TerminateWorkflow(string userId, bool isCancelledByUser, CancellationToken ct = default)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);
        var historyInfo = _memoryCache.Get<ConversationHistoryInfo>(cacheKey);
        if (historyInfo is null)
        {
            return;
        }

        var workWorkflowInstance =
            await _workflowHost.PersistenceStore.GetWorkflowInstance(historyInfo.WorkflowId, ct);
        if (workWorkflowInstance is not null)
        {
            await _workflowHost.TerminateWorkflow(workWorkflowInstance.Id);

            if (isCancelledByUser)
            {
                _workflowCancelledMeter.WorkflowCancelled(workWorkflowInstance.WorkflowDefinitionId);
            }
        }

        await _conversationHistory.DeleteMessagesAsync(userId, _telegramBotClient, ct);

        _memoryCache.Remove(cacheKey);

        _durationGaugeMetric.RecordDuration(historyInfo.WorkflowName,
            Stopwatch.GetElapsedTime(historyInfo.StartedAt));
    }

    private class ConversationHistoryInfo
    {
        public long StartedAt { get; } = Stopwatch.GetTimestamp();

        public string WorkflowName { get; init; } = null!;

        public string WorkflowId { get; init; } = null!;
    }
}