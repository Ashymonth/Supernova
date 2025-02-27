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
        var bot = _serviceProvider.GetRequiredService<ITelegramBotClientWrapper>();
        
        await bot.ReceiveAsync(async (_, update, ct) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();

                await handler.HandleUpdateAsync(update, ct);
            }, (_, _, _) => Task.CompletedTask, cancellationToken: stoppingToken,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                DropPendingUpdates = true
            });
    }
}