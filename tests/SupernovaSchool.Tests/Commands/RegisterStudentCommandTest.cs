using Moq;
using SupernovaSchool.Telegram;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
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
        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == Config.SenderId),
                    RegisterStudentStepMessage.CommandStartMessage, It.IsAny<ReplyKeyboardRemove>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Text = RegisterStudentStepMessage.CommandStartMessage })
            .Verifiable(Times.Once);
        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == Config.SenderId),
                    RegisterStudentStepMessage.InputName, It.IsAny<ReplyKeyboardRemove>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Text = RegisterStudentStepMessage.InputName })
            .Verifiable(Times.Once);

        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == Config.SenderId),
                    RegisterStudentStepMessage.InputClass, It.IsAny<ReplyKeyboardMarkup>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Text = RegisterStudentStepMessage.InputClass })
            .Verifiable(Times.Once);

        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == Config.SenderId),
                    DefaultStepMessage.ProcessingRequest,
                    It.IsAny<ReplyKeyboardRemove>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message
                { Text = DefaultStepMessage.ProcessingRequest, })
            .Verifiable(Times.Once);
        
        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == Config.SenderId),
                    RegisterStudentStepMessage.CreateSuccessMessage(expectedName, expectedClass),
                    It.IsAny<ReplyKeyboardRemove>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message
                { Text = RegisterStudentStepMessage.CreateSuccessMessage(expectedName, expectedClass), })
            .Verifiable(Times.Once);

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