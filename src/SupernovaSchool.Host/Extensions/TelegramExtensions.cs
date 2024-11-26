using Microsoft.Extensions.Options;
using SupernovaSchool.Host.Configs;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SupernovaSchool.Host.Extensions;

internal static class TelegramExtensions
{
    public static async Task MapTelegramWebHookAsync(this WebApplication webApplication)
    {
        var botUrl = webApplication.Services.GetRequiredService<IOptions<TelegramBotConfig>>().Value.WebHookUrl;
        var bot = webApplication.Services.GetRequiredService<ITelegramBotClient>();
        await bot.SetWebhook(string.Empty);
        await bot.SetWebhook(botUrl + "/updates",
            allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery], dropPendingUpdates: true);
    }
}