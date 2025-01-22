using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using SupernovaSchool.Tests.Fixtures;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Tests.Commands;

[Collection("CommandsCollection"), Order(2)]
public class RegisterStudentCommandTest : BaseCommandTest, IClassFixture<WebAppFactoryBuilder>
{
    private readonly WebAppFactoryBuilder _applicationFactory;

    public RegisterStudentCommandTest(WebAppFactoryBuilder applicationFactory)
    {
        _applicationFactory = applicationFactory;
    }

    [Fact]
    public async Task RegisterStudentCommandTest_ShouldRegisterStudent()
    {
        var webApp = _applicationFactory.Build();
        await InitializeAsync(webApp);

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

        Assert.Empty(expectedMessagesInOrder);
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message is not DefaultStepMessage.ProcessingRequest && message !=
            RegisterStudentStepMessage.CommandStartMessage.Replace("\r\n", "\n");
    }
}