using Moq;
using SupernovaSchool.Telegram;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using SupernovaSchool.Tests.Extensions;
using SupernovaSchool.Tests.Fixtures;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
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
        const string expectedName = "Test name";
        const string expectedClass = "7";

        var mock = new Mock<ITelegramBotClientWrapper>();
        mock.SetupSendMessage<ReplyKeyboardRemove>(Config.SenderId, RegisterStudentStepMessage.CommandStartMessage);
        mock.SetupSendMessage<ReplyKeyboardRemove>(Config.SenderId, RegisterStudentStepMessage.InputName);
        mock.SetupSendMessage<ReplyKeyboardMarkup>(Config.SenderId, RegisterStudentStepMessage.InputClass);
        mock.SetupSendMessage<ReplyKeyboardRemove>(Config.SenderId, DefaultStepMessage.ProcessingRequest);
        mock.SetupSendMessage<ReplyKeyboardRemove>(Config.SenderId,
            RegisterStudentStepMessage.CreateSuccessMessage(expectedName, expectedClass));
  
        var webApp = _applicationFactory
            .WithReplacedService(mock.Object)
            .Build();

        await InitializeAsync(webApp);
 
        await SendUpdate(Telegram.Commands.RegisterAsStudentCommand);

        await Task.Delay(500);
        await SendUpdate(expectedName);

        await Task.Delay(500);
        await SendUpdate(expectedClass);

        await Task.Delay(500);
        
        mock.VerifyAll();
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message is not DefaultStepMessage.ProcessingRequest && message !=
            RegisterStudentStepMessage.CommandStartMessage.Replace("\r\n", "\n");
    }
}