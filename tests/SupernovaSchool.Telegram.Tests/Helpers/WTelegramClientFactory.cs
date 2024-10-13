using SupernovaSchool.Telegram.Tests.Options;
using TL;
using WTelegram;

namespace SupernovaSchool.Telegram.Tests.Helpers;

public class WTelegramClientFactory
{
    // public static async Task<(Client client, IPeerInfo userChat)> CreateClient(WTelegramConfig telegramConfig)
    // {
    //     var client = new Client(telegramConfig.ApiId, telegramConfig.ApiHash);
    //
    //     await client.Login(telegramConfig.PhoneNumber);
    //
    //     var dialogs = await client.Messages_GetAllDialogs();
    //     var peer = dialogs.dialogs.First(x => x.Peer.ID == telegramConfig.BotChatId);
    //     var userChat = dialogs.UserOrChat(peer!.Peer);
    //
    //     var messages = await client.Messages_GetHistory(userChat.ToInputPeer());
    //     var messageIds = messages.Messages.Select(@base => @base.ID).ToArray();
    //
    //     await client.DeleteMessages(userChat.ToInputPeer(), messageIds);
    //
    //     return (client, userChat);
    // }
    
    public static async Task<Client> CreateClient(WTelegramConfig telegramConfig)
    {
        var client = new Client(telegramConfig.ApiId, telegramConfig.ApiHash);

        await client.Login(telegramConfig.PhoneNumber);

        var dialogs = await client.Messages_GetAllDialogs();
        var peer = dialogs.dialogs.First(x => x.Peer.ID == telegramConfig.BotChatId);
        var userChat = dialogs.UserOrChat(peer!.Peer);

        var messages = await client.Messages_GetHistory(userChat.ToInputPeer());
        var messageIds = messages.Messages.Select(@base => @base.ID).ToArray();

        await client.DeleteMessages(userChat.ToInputPeer(), messageIds);

        return client;
    }
}