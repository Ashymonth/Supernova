using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;
using TL;

namespace SupernovaSchool.Telegram.Tests;

public class RegisterStudentCommandTest : BaseCommandTest, IClassFixture<WebAppFactory>, IDisposable, IAsyncDisposable
{
    private readonly WebAppFactory _factory;
    private WTelegram.Client _tgClient = null!;

    public RegisterStudentCommandTest(WebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterStudentCommandTest_ShouldRegisterStudent()
    {
        const string expectedName = "Test name";
        const string expectedClass = "7";

        var expectedMessagesInOrder = new Queue<string>([
            "Для того, чтобы записаться к психологу, вам нужно указать свои данные.\n Для завершения команды введите 'Выйти'",
            "Введите имя",
            "Укажите ваш поток",
            "Обработка запроса...",
            $"Вы успешно зарегистрировались. Теперь вы можете записаться к психологу.\nВаши данные:{expectedName}-{expectedClass}"
        ]);

        var client = _factory.CreateClient();

        _tgClient = await WTelegramClientFactory.CreateClient(Config);
        
        using var locker = new ManualResetEventSlim();
        // ReSharper disable once AccessToDisposedClosure
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);

        await SendUpdate(client, Commands.RegisterAsStudentCommand);

        locker.Wait();

        await SendUpdate(client, expectedName);
        locker.Reset();
        locker.Wait();

        await SendUpdate(client, expectedClass);
        locker.Reset();
        locker.Wait();
    }

    private Task TgClientOnOnUpdates(UpdatesBase updateEvent, Queue<string> expectedMessagesInOrder, ManualResetEventSlim locker)
    {
        if (!updateEvent.Users.TryGetValue(Config.BotChatId, out _))
        {
            return Task.CompletedTask;
        }

        var update = updateEvent.UpdateList[0] as UpdateNewMessage;
        var message = update!.message as Message;

        var expectedMessage = expectedMessagesInOrder.Dequeue();

        Assert.Equal(expectedMessage, message!.message);
        if (message.message is "Обработка запроса..."
            or "Для того, чтобы записаться к психологу, вам нужно указать свои данные.\n Для завершения команды введите 'Выйти'")
        {
            return Task.CompletedTask;
        }

        // ReSharper disable once AccessToDisposedClosure
        locker.Set();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _factory.Dispose();
        _tgClient.Dispose();
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, null!, null!);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _tgClient.DisposeAsync();
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, null!, null!);
        GC.SuppressFinalize(this);
    }
}