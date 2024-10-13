using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram;

public class BackgroundTelegramService : UpdateHandler, IHostedService
{
    public BackgroundTelegramService(ITelegramBotClient telegramBotClient, IWorkflowHost workflowHost,
        CommandRegistry commandRegistry, IUserSessionStorage userSessionStorage,
        IConversationHistory conversationHistory, ICommandUploader commandUploader) : base(telegramBotClient,
        workflowHost, commandRegistry, userSessionStorage, conversationHistory, commandUploader)
    {
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _telegramBotClient.StartReceiving(async (client, update, arg3) => { await HandleUpdateAsync(update, arg3); },
            (client, exception, arg3) => Task.CompletedTask, cancellationToken: cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}