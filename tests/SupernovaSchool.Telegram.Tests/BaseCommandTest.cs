using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SupernovaSchool.Telegram.Tests.Helpers;
using SupernovaSchool.Telegram.Tests.Options;
using TL;
using WTelegram;
using Message = TL.Message;
using TgUpdate = Telegram.Bot.Types.Update;
using TgMessage = Telegram.Bot.Types.Message;
using TgUser = Telegram.Bot.Types.User;


namespace SupernovaSchool.Telegram.Tests;

public class BaseCommandTest : IDisposable
{
    private Exception? _capturedException;  // To capture exceptions in the event handler
    
    protected BaseCommandTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<BaseCommandTest>()
            .Build();

        var config = new WTelegramConfig();
        configuration.GetSection("WTelegram").Bind(config);

        Config = config;
    }

    private AutoResetEvent Locker { get; } = new(false);

    private Client WTelegramClient { get; set; } = null!;

    private HttpClient AppClient { get; set; } = null!;
    
    protected WTelegramConfig Config { get; }

    protected async Task InitializeAsync(WebApplicationFactory<Program> applicationFactory)
    {
        WTelegramClient = await WTelegramClientFactory.CreateClient(Config);
        AppClient = applicationFactory.CreateClient();
    }

    protected void SubscribeOnUpdates(Queue<string> expectedMessagesInOrder)
    {
        WTelegramClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, Locker);
    }

    protected async Task SendUpdate(string message)
    {
        Assert.Null(_capturedException);
        
        using var response = await AppClient.PostAsJsonAsync("/updates", new TgUpdate
        {
            Message = new TgMessage { Text = message, From = new TgUser { FirstName = "Test",Id = Config.SenderId } }
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web){Converters = {  }});

        Locker.WaitOne();
    }

    protected virtual bool IsFinalUpdateInStep(string message)
    {
        return true;
    }
    
    private Task TgClientOnOnUpdates(UpdatesBase updateEvent, Queue<string> expectedMessagesInOrder, AutoResetEvent locker)
    {
        try
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

            // Handle line-ending differences in Telegram messages
            Assert.Equal(expectedMessage.Replace("\r\n", "\n"), messageText);

            if (!IsFinalUpdateInStep(messageText))
            {
                return Task.CompletedTask;
            }

            locker.Set();  // Signal the event is processed
        }
        catch (Exception ex)
        {
            _capturedException = ex;  // Capture exception
            locker.Set();  // Ensure the test does not the app froze
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Locker.Dispose();
        WTelegramClient.Dispose();
        AppClient.Dispose();
        GC.SuppressFinalize(this);
    }
}