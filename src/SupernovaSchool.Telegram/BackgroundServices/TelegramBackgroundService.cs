using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace SupernovaSchool.Telegram.BackgroundServices;

public class TelegramBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ITelegramBotClient bot = null;

        try
        {
            bot = _serviceProvider.GetRequiredService<ITelegramBotClient>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        await bot.DeleteWebhook(true, stoppingToken);

        await bot.ReceiveAsync(async (client, update, ct) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();

                await handler.HandleUpdateAsync(update, ct);
            }, (client, exception, ct) => Task.CompletedTask, cancellationToken: stoppingToken,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            });
    }
}