using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;

public class UpdateHandler
{
  
    private const string StartCommandName = "/start";

    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IWorkflowHost _workflowHost;
    private readonly CommandRegistry _commandRegistry;
    private readonly IUserSessionStorage _userSessionStorage;
    private readonly IConversationHistory _conversationHistory;
    private readonly ICommandUploader _commandUploader;

    public UpdateHandler(ITelegramBotClient telegramBotClient, IWorkflowHost workflowHost,
        CommandRegistry commandRegistry, IUserSessionStorage userSessionStorage,
        IConversationHistory conversationHistory, ICommandUploader commandUploader)
    {
        _telegramBotClient = telegramBotClient;
        _workflowHost = workflowHost;
        _commandRegistry = commandRegistry;
        _userSessionStorage = userSessionStorage;
        _conversationHistory = conversationHistory;
        _commandUploader = commandUploader;
    }

    public async Task<Message?> HandleUpdateAsync(Update update, CancellationToken token = default)
    {
        var message = update.Message?.Text ?? update.CallbackQuery?.Data!;
        var messageId = update.Message?.MessageId ?? update.CallbackQuery?.Message?.MessageId!;
        var userId = update.Message?.From?.Id.ToString() ?? update.CallbackQuery?.From.Id.ToString()!;

        await _telegramBotClient.SendChatActionAsync(long.Parse(userId), ChatAction.Typing,
            cancellationToken: token);

        if (string.Equals(Commands.StartCommand, message, StringComparison.InvariantCultureIgnoreCase))
        {
            await _commandUploader.UploadUserCommandsAsync(userId, token);
            return await _telegramBotClient.SendTextMessageAsync(userId, CommandText.StartCommandMessage,
                cancellationToken: token);
        }

        _conversationHistory.AddMessage(userId, messageId.Value);

        if (string.Equals("выйти", message, StringComparison.InvariantCultureIgnoreCase))
        {
            await _userSessionStorage.TerminateWorkflow(userId, token);
            return await _telegramBotClient.SendTextMessageAsync(userId, CommandText.CommandCanceled,
                cancellationToken: token);
        }

        if (_commandRegistry.TryGetWorkflowByCommandName(message, out var workflowDataFactory))
        {
            if (!await _userSessionStorage.StartWorkflow(userId, message, workflowDataFactory!(userId)))
            {
                return await _telegramBotClient.SendTextMessageAsync(userId,
                    CommandText.CannotInvokeCommandUntilActiveCommandExist,
                    cancellationToken: token);
            }

            return null;
        }

        await _workflowHost.PublishUserMessageAsync(update.Type, userId,
            new UserMessage(message, messageId.Value));

        return null;
    }
}