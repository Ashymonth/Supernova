using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using TL;
using WTelegram;

namespace SupernovaSchool.Telegram.Tests.Commands;

public class RegisterStudentCommandTest : BaseCommandTest, IClassFixture<WebAppFactory>, IDisposable, IAsyncDisposable
{
    private readonly WebAppFactory _factory;
    private Client _tgClient = null!;

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
            DefaultStepMessage.CreateInitialMessage(RegisterStudentStepMessage.CommandStartMessage),
            RegisterStudentStepMessage.InputName,
            RegisterStudentStepMessage.InputClass,
            DefaultStepMessage.ProcessingRequest,
            RegisterStudentStepMessage.CreateSuccessMessage(expectedName, expectedClass),
        ]);

        var webClient = _factory.CreateClient();

        _tgClient = await WTelegramClientFactory.CreateClient(Config);

        using var locker = new AutoResetEvent(false);
        // ReSharper disable once AccessToDisposedClosure
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);

        await SendUpdate(webClient, Telegram.Commands.RegisterAsStudentCommand);

        locker.WaitOne();

        await SendUpdate(webClient, expectedName);
        locker.WaitOne();

        await SendUpdate(webClient, expectedClass);
        locker.WaitOne();

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message is not DefaultStepMessage.ProcessingRequest && message !=
            DefaultStepMessage.CreateInitialMessage(RegisterStudentStepMessage.CommandStartMessage)
                .Replace("\r\n", "\n");
    }

    public void Dispose()
    {
        _factory.Dispose();
        _tgClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _tgClient.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}