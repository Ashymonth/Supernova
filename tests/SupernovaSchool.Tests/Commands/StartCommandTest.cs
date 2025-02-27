using Moq;
using SupernovaSchool.Telegram;
using SupernovaSchool.Tests.Fixtures;
using Telegram.Bot.Types;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Tests.Commands;

[Collection("CommandsCollection"), Order(1)]
public class StartCommandTest : BaseCommandTest, IClassFixture<WebAppFactoryBuilder>
{
    private readonly WebAppFactoryBuilder _applicationFactory;

    public StartCommandTest(WebAppFactoryBuilder applicationFactory)
    {
        _applicationFactory = applicationFactory;
    }

    [Fact]
    public async Task StartCommandTest_ShouldSendStartMessageAndUploadAllCommands()
    {
        var mock = new Mock<ITelegramBotClientWrapper>();
        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == Config.SenderId),
                    CommandText.StartCommandMessage, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Text = CommandText.StartCommandMessage })
            .Verifiable(Times.Once);

        var webApp = _applicationFactory.WithReplacedService(mock.Object).Build();
        await InitializeAsync(webApp);
        
        await SendUpdate(Telegram.Commands.StartCommand);

        mock.Verify();
    }
}