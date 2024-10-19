using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Telegram.Tests.Commands;

[Collection("CommandsCollection"), Order(1)]
public class StartCommandTest : BaseCommandTest, IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _applicationFactory;

    public StartCommandTest(WebAppFactory applicationFactory)
    {
        _applicationFactory = applicationFactory;
    }

    [Fact]
    public async Task StartCommandTest_ShouldSendStartMessageAndUploadAllCommands()
    {
        await InitializeAsync(_applicationFactory);
        
        var expectedMessagesInOrder = new Queue<string>([
            CommandText.StartCommandMessage // we need this because in telegram message that contains \r\n is just \n
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);
        
        await SendUpdate(Telegram.Commands.StartCommand);
 
        Assert.True(expectedMessagesInOrder.Count == 0);
    }
}