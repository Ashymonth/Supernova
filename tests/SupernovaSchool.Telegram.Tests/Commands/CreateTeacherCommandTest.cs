using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using WTelegram;

namespace SupernovaSchool.Telegram.Tests.Commands;

public class CreateTeacherCommandTest : BaseCommandTest, IClassFixture<WebAppFactoryWhenUserIsNotAdmin>
{
    private readonly WebAppFactoryWhenUserIsNotAdmin _factory;
    private Client _tgClient = null!;

    public CreateTeacherCommandTest(WebAppFactoryWhenUserIsNotAdmin factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTeacherTest_WhenUserIsNotAnAdmin_ReturnErrorMessage()
    {
        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.NotEnoughRightToCreateATeacher,
        ]);

        var webClient = _factory.CreateClient();

        _tgClient = await WTelegramClientFactory.CreateClient(Config);

        using var locker = new ManualResetEventSlim();
        // ReSharper disable once AccessToDisposedClosure
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);

        await SendUpdate(webClient, Telegram.Commands.CreateTeacherCommand);

        locker.Wait();
    }
}