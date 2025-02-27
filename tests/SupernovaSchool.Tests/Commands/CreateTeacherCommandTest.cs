using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Tests.Extensions;
using SupernovaSchool.Tests.Fixtures;
using Xunit.Extensions.Ordering;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Tests.Commands;

[Collection("CommandsCollection"), Order(3)]
public class CreateTeacherCommandTest : BaseCommandTest, IClassFixture<WebAppFactoryBuilder>
{
    private const string ExpectedYandexName = "Test name";
    private const string ExpectedYandexLogin = "Test login";
    private const string ExpectedYandexPassword = "Test password";

    private readonly Mock<IAuthorizationResource> _authorizationResourceMock = new();

    private readonly WebAppFactoryBuilder _appFactoryBuilder;

    public CreateTeacherCommandTest(WebAppFactoryBuilder appFactoryBuilder)
    {
        _appFactoryBuilder = appFactoryBuilder;
    }

    [Fact, Order(1)]
    public async Task CreateTeacherTest_WhenUserIsNotAnAdmin_ReturnErrorMessage()
    {
        var tgMock = new Mock<ITelegramBotClientWrapper>();
        tgMock.SetupSendMessage(Config.SenderId, CreateTeacherStepMessage.NotEnoughRightToCreateATeacher);

        var adminsMock = new Mock<IAdminsProvider>();
        adminsMock.Setup(provider => provider.IsAdmin(Config.SenderId.ToString()))
            .Returns(false)
            .Verifiable(Times.Once);

        var webApp = _appFactoryBuilder
            .WithReplacedService(tgMock.Object)
            .WithReplacedService(adminsMock.Object)
            .Build();

        await InitializeAsync(webApp);

        await SendUpdate(Telegram.Commands.CreateTeacherCommand);

        await Task.Delay(500);

        tgMock.VerifyAll();
        adminsMock.VerifyAll();
    }

    [Fact, Order(2)]
    public async Task CreateTeacherTest_WhenUserIsAnAdmin_ShouldCreateATeacher()
    {
        var tgMock = new Mock<ITelegramBotClientWrapper>();
        tgMock.SetupSendMessage(Config.SenderId, CreateTeacherStepMessage.InputName);
        tgMock.SetupSendMessage(Config.SenderId, CreateTeacherStepMessage.InputLoginFromYandexCalendar);
        tgMock.SetupSendMessage(Config.SenderId, CreateTeacherStepMessage.InputPasswordFromYandexCalendar);
        tgMock.SetupSendMessage(Config.SenderId, DefaultStepMessage.ProcessingRequest);
        tgMock.SetupSendMessage(Config.SenderId,
            CreateTeacherStepMessage.CreateSuccessMessage(ExpectedYandexName, ExpectedYandexLogin));

        var adminsMock = new Mock<IAdminsProvider>();
        adminsMock.Setup(provider => provider.IsAdmin(Config.SenderId.ToString()))
            .Returns(true)
            .Verifiable(Times.Once);

        _authorizationResourceMock.Setup(resource => resource.AuthorizeAsync(It.Is<UserCredentials>(credentials =>
            credentials.UserName == ExpectedYandexLogin &&
            credentials.Password == ExpectedYandexPassword), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var webApp = _appFactoryBuilder
            .WithReplacedService(tgMock.Object)
            .WithReplacedService(adminsMock.Object)
            .WithReplacedService(_authorizationResourceMock.Object)
            .Build();

        await InitializeAsync(webApp);

        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.InputName,
            CreateTeacherStepMessage.InputLoginFromYandexCalendar,
            CreateTeacherStepMessage.InputPasswordFromYandexCalendar,
            DefaultStepMessage.ProcessingRequest,
            CreateTeacherStepMessage.CreateSuccessMessage(ExpectedYandexName, ExpectedYandexLogin)
        ]);

        await SendUpdate(Telegram.Commands.CreateTeacherCommand);

        await Task.Delay(500);

        await SendUpdate(ExpectedYandexName);

        await Task.Delay(500);

        await SendUpdate(ExpectedYandexLogin);

        await Task.Delay(500);

        await SendUpdate(ExpectedYandexPassword);

        await Task.Delay(500);

        tgMock.VerifyAll();
        adminsMock.VerifyAll();
        _authorizationResourceMock.VerifyAll();
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message != DefaultStepMessage.ProcessingRequest;
    }
}