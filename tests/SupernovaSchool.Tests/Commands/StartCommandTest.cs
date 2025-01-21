using SupernovaSchool.Telegram;
using SupernovaSchool.Tests.Helpers;
using SupernovaSchool.Tests.Fixtures;
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
        await using var webApp = _applicationFactory.Build();
        await InitializeAsync(webApp);
        
        var expectedMessagesInOrder = new Queue<string>([
            CommandText.StartCommandMessage // we need this because in telegram message that contains \r\n is just \n
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);
        
        await SendUpdate(Telegram.Commands.StartCommand);
 
        Assert.True(expectedMessagesInOrder.Count == 0);
    }
}