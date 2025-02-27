using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupernovaSchool.Telegram;

public interface ITelegramBotClientWrapper
{
    Task<Message> SendMessage(
        ChatId chatId,
        string message,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    Task DeleteMessage(
        ChatId chatId,
        int messageId,
        CancellationToken cancellationToken = default
    );

    Task SetMyCommands(
        IEnumerable<BotCommand> commands,
        BotCommandScope? scope = null,
        string? languageCode = null,
        CancellationToken cancellationToken = default
    );

    Task SendChatAction(
        ChatId chatId,
        ChatAction action,
        int? messageThreadId = null,
        string? businessConnectionId = null,
        CancellationToken cancellationToken = default
    );

    Task ReceiveAsync(
        Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
        Func<ITelegramBotClient, Exception, CancellationToken, Task> errorHandler,
        ReceiverOptions? receiverOptions = null, CancellationToken cancellationToken = default);
}

public class TelegramBotClientWrapper : TelegramBotClient, ITelegramBotClientWrapper
{
    /// <summary>
    /// Only for tests
    /// </summary>
    public TelegramBotClientWrapper() : this("12345:srgsgr")
    {
    }

    public TelegramBotClientWrapper(TelegramBotClientOptions options, HttpClient? httpClient = null,
        CancellationToken cancellationToken = new()) : base(options, httpClient, cancellationToken)
    {
    }

    public TelegramBotClientWrapper(string token, HttpClient? httpClient = null,
        CancellationToken cancellationToken = new()) : base(token, httpClient, cancellationToken)
    {
    }

    public async Task<Message> SendMessage(ChatId chatId, string message, ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        return await TelegramBotClientExtensions.SendMessage(this, chatId, message, replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteMessage(ChatId chatId, int messageId, CancellationToken cancellationToken = default)
    {
        await TelegramBotClientExtensions.DeleteMessage(this, chatId, messageId, cancellationToken);
    }

    public async Task SetMyCommands(IEnumerable<BotCommand> commands, BotCommandScope? scope = null,
        string? languageCode = null,
        CancellationToken cancellationToken = default)
    {
        await TelegramBotClientExtensions.SetMyCommands(this, commands, scope, languageCode, cancellationToken);
    }

    public async Task SendChatAction(ChatId chatId, ChatAction action, int? messageThreadId = null,
        string? businessConnectionId = null, CancellationToken cancellationToken = default)
    {
        await TelegramBotClientExtensions.SendChatAction(this, chatId, action, messageThreadId, businessConnectionId,
            cancellationToken);
    }

    public async Task ReceiveAsync(Func<ITelegramBotClient, Update, CancellationToken, Task> updateHandler,
        Func<ITelegramBotClient, Exception, CancellationToken, Task> errorHandler,
        ReceiverOptions? receiverOptions = null,
        CancellationToken cancellationToken = default)
    {
        await TelegramBotClientExtensions.ReceiveAsync(this, updateHandler, errorHandler, receiverOptions,
            cancellationToken);
    }
}