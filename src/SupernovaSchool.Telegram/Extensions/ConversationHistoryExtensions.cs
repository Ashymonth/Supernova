using Telegram.Bot;

namespace SupernovaSchool.Telegram.Extensions;

internal static class ConversationHistoryExtensions
{
    public static async Task DeleteMessagesAsync(this IConversationHistory conversationHistory,
        string userId,
        ITelegramBotClient botClient,
        CancellationToken ct = default)
    {
        conversationHistory.CleanMessages(userId, out var messageIds);

        await Parallel.ForEachAsync(messageIds,
            new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = ct },
            async (messageId, token) =>
            {
                try
                {
                    await botClient.DeleteMessageAsync(userId, messageId, cancellationToken: token);
                }
                catch (Exception e)
                {
                    //
                }
            });
    }
}