using SupernovaSchool.Telegram.Tests.Options;
using TL;
using WTelegram;

namespace SupernovaSchool.Telegram.Tests.Helpers;

public class WTelegramClientFactory
{
    public static async Task<(Client client, User bot)> CreateClient(WTelegramConfig telegramConfig)
    {
        var client = new Client(telegramConfig.ApiId, telegramConfig.ApiHash);

        await client.Login(telegramConfig.PhoneNumber);

        var chat = await client.Messages_GetPeerDialogs(new InputDialogPeer
            { peer = new InputPeerUser(telegramConfig.BotChatId, telegramConfig.BotChatId) });

        return (client, chat.users[0]);
    }
}