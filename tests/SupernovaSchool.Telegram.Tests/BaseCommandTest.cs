using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SupernovaSchool.Telegram.Tests.Options;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using Telegram.Bot;
using TL;
using TgUpdate = Telegram.Bot.Types.Update;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;
 

namespace SupernovaSchool.Telegram.Tests;

public class BaseCommandTest
{
    public BaseCommandTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<BaseCommandTest>()
            .Build();

        var config = new WTelegramConfig();
        configuration.GetSection("WTelegram").Bind(config);

        Config = config;
    }

    public WTelegramConfig Config { get; }

    protected async Task<Message?> SendUpdate(HttpClient client, string message)
    {
        using var response = await client.PostAsJsonAsync("/updates", new TgUpdate
        {
            Message = new TgMessage { Text = message, From = new TgUser { Id = Config.SenderId } }
        });

        var content = await response.Content.ReadAsStringAsync();

        return content.Length != 0 ? JsonSerializer.Deserialize<Message>(content) : null;
    }
    
    protected Task TgClientOnOnUpdates(UpdatesBase updateEvent, Queue<string> expectedMessagesInOrder,
        ManualResetEventSlim locker)
    {
        if (updateEvent.UpdateList.FirstOrDefault() is UpdateUserStatus)
        {
            return Task.CompletedTask;
        }
        
        if (updateEvent.UpdateList.FirstOrDefault() is not UpdateNewMessage update ||
           update.message.Peer.ID != Config.BotChatId)
        {
            return Task.CompletedTask;
        }
        
        var expectedMessage = expectedMessagesInOrder.Dequeue();

        var messageText = (update.message as Message)!.message;
        Assert.Equal(expectedMessage, messageText);
        if (!IsFinalUpdateInStep(messageText))
        {
            return Task.CompletedTask;
        }

        // ReSharper disable once AccessToDisposedClosure
        locker.Reset();
        locker.Set();
        return Task.CompletedTask;
    }

    protected virtual bool IsFinalUpdateInStep(string message)
    {
        return true;
    }
}