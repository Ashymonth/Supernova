using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;

public class UpdateHandler
{
    private const string ExistCommandName = "Выйти";

    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IWorkflowHost _workflowHost;
    private readonly CommandRegistry _commandRegistry;
    private readonly IUserSessionStorage _userSessionStorage;
    private readonly IConversationHistory _conversationHistory;

    public UpdateHandler(ITelegramBotClient telegramBotClient, IWorkflowHost workflowHost,
        CommandRegistry commandRegistry, IUserSessionStorage userSessionStorage,
        IConversationHistory conversationHistory)
    {
        _telegramBotClient = telegramBotClient;
        _workflowHost = workflowHost;
        _commandRegistry = commandRegistry;
        _userSessionStorage = userSessionStorage;
        _conversationHistory = conversationHistory;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken token = default)
    {
        var message = update.Message?.Text ?? update.CallbackQuery?.Data!;
        var messageId = update.Message?.MessageId ?? update.CallbackQuery?.Message?.MessageId!;
        var userId = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From.Id.ToString()!;

        await _telegramBotClient.SendChatActionAsync(long.Parse(userId), ChatAction.Typing,
            cancellationToken: token);

        _conversationHistory.AddMessage(userId, messageId.Value);

        if (string.Equals(ExistCommandName, message, StringComparison.InvariantCultureIgnoreCase))
        {
            await _userSessionStorage.TerminateWorkflow(userId, token);
            await _telegramBotClient.SendTextMessageAsync(userId, "Команда отменена",
                cancellationToken: token);
            return;
        }

        if (_commandRegistry.TryGetWorkflowByCommandName(message, out var workflowDataFactory))
        {
            if (!await _userSessionStorage.StartWorkflow(userId, message, workflowDataFactory!(userId)))
            {
                await _telegramBotClient.SendTextMessageAsync(userId,
                    "Нельзя вызвать новую команду, пока вы не завершили старую",
                    cancellationToken: token);
            }

            return;
        }

        await _workflowHost.PublishUserMessageAsync(update.Type, userId,
            new UserMessage(message, messageId.Value));
    }
}