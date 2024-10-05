using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types;

namespace SupernovaSchool.Telegram;

public interface IConversationHistory
{
    void AddMessage(string userId, int messageId);

    HashSet<int>? GetMessages(string userId);

    void CleanMessages(string userId, out HashSet<int> messageIds);
}

public class ConversationHistory : IConversationHistory
{
    private const string CacheKeyTemplate = "ConversationHistory_{0}";

    private readonly IMemoryCache _memoryCache;

    public ConversationHistory(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void AddMessage(string userId, int messageId)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);

        if (_memoryCache.TryGetValue(cacheKey, out HashSet<int>? cachedMessage))
        {
            cachedMessage!.Add(messageId);
            return;
        }

        _memoryCache.Set(cacheKey, new HashSet<int> { messageId });
    }

    public HashSet<int>? GetMessages(string userId)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);

        return _memoryCache.Get<HashSet<int>>(cacheKey);
    }

    public void CleanMessages(string userId, out HashSet<int> messageIds)
    {
        var cacheKey = string.Format(CacheKeyTemplate, userId);

        messageIds = _memoryCache.Get<HashSet<int>>(cacheKey) ?? [];
        _memoryCache.Remove(userId);
    }
}