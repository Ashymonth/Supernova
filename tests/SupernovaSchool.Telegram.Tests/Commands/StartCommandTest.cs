using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;

namespace SupernovaSchool.Telegram.Tests.Commands;

public class StartCommandTest : BaseCommandTest, IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _fixture;

    public StartCommandTest(WebAppFactory fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task StartCommandTest_ShouldSendStartMessageAndUploadAllCommands()
    {
        var webClient = _fixture.CreateClient();

        var expectedMessagesInOrder = new Queue<string>([
            CommandText.StartCommandMessage // we need this because in telegram message that contains \r\n is just \n
        ]);

        var tgClient = await WTelegramClientFactory.CreateClient(Config);

        using var locker = new AutoResetEvent(false);

        // ReSharper disable once AccessToDisposedClosure
        tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);
        
        await SendUpdate(webClient, Telegram.Commands.StartCommand);

        locker.WaitOne();

        Assert.True(expectedMessagesInOrder.Count == 0);
    }
}