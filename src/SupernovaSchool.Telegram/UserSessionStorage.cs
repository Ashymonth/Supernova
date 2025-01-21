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
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly WorkflowCancelledMeter _workflowCancelledMeter;
    private readonly WorkflowDurationHistogramMetric _durationHistogramMetric;

    public UserSessionStorage(IMemoryCache memoryCache, IWorkflowHost workflowHost,
        IConversationHistory conversationHistory, ITelegramBotClient telegramBotClient,
        WorkflowCancelledMeter workflowCancelledMeter, WorkflowDurationHistogramMetric durationHistogramMetric)
    {
        _memoryCache = memoryCache;
        _workflowHost = workflowHost;
        _conversationHistory = conversationHistory;
        _telegramBotClient = telegramBotClient;
        _workflowCancelledMeter = workflowCancelledMeter;
        _durationHistogramMetric = durationHistogramMetric;
    }

    public async Task<bool> StartWorkflow(string userId, string workflowName, object workflowData)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);

        if (_memoryCache.TryGetValue(cacheKey, out _))
        {
            return false;
        }

        var workflow = await _workflowHost.StartWorkflow(workflowName, workflowData);

        _memoryCache.Set(cacheKey, new ConversationHistoryInfo { Workflow = workflow });

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

        var workWorkflowInstance = await _workflowHost.PersistenceStore.GetWorkflowInstance(historyInfo.Workflow, ct);
        if (workWorkflowInstance is not null)
        {
            await _workflowHost.TerminateWorkflow(historyInfo.Workflow);

            if (isCancelledByUser)
            {
                _workflowCancelledMeter.WorkflowCancelled(workWorkflowInstance.WorkflowDefinitionId);
            }
        }

        await _conversationHistory.DeleteMessagesAsync(userId, _telegramBotClient, ct);

        _memoryCache.Remove(cacheKey);

        _durationHistogramMetric.RecordDuration(historyInfo.Workflow, Stopwatch.GetElapsedTime(historyInfo.StartedAt));
    }

    private class ConversationHistoryInfo
    {
        public long StartedAt { get; } = Stopwatch.GetTimestamp();

        public string Workflow { get; set; } = null!;
    }
}