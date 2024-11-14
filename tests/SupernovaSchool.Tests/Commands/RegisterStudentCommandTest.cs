using SupernovaSchool.Tests.Helpers;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using SupernovaSchool.Tests.Fixtures;
using TL;
using WTelegram;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Tests.Commands;

[Collection("CommandsCollection"), Order(2)]
public class RegisterStudentCommandTest : BaseCommandTest, IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    public RegisterStudentCommandTest(WebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterStudentCommandTest_ShouldRegisterStudent()
    {
        await InitializeAsync(_factory);

        const string expectedName = "Test name";
        const string expectedClass = "7";

        var expectedMessagesInOrder = new Queue<string>([
            RegisterStudentStepMessage.CommandStartMessage,
            RegisterStudentStepMessage.InputName,
            RegisterStudentStepMessage.InputClass,
            DefaultStepMessage.ProcessingRequest,
            RegisterStudentStepMessage.CreateSuccessMessage(expectedName, expectedClass),
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.RegisterAsStudentCommand);

        await SendUpdate(expectedName);

        await SendUpdate(expectedClass);

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message is not DefaultStepMessage.ProcessingRequest && message !=
            RegisterStudentStepMessage.CommandStartMessage.Replace("\r\n", "\n");
    }
}