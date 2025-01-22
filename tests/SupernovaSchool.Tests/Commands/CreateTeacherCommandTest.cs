using Microsoft.Extensions.Configuration;
using Moq;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
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
        var webApp = _appFactoryBuilder.WithAdditionalConfiguration(builder =>
            builder.AddJsonFile("appsettings-without-admins.json")).Build();

        await InitializeAsync(webApp);

        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.NotEnoughRightToCreateATeacher,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateTeacherCommand);

        Assert.Empty(expectedMessagesInOrder);
    }

    [Fact, Order(2)]
    public async Task CreateTeacherTest_WhenUserIsAnAdmin_ShouldCreateATeacher()
    {
        var webApp = _appFactoryBuilder.WithReplacedService(_authorizationResourceMock.Object).Build();

        _authorizationResourceMock.Setup(resource => resource.AuthorizeAsync(It.Is<UserCredentials>(credentials =>
            credentials.UserName == ExpectedYandexLogin &&
            credentials.Password == ExpectedYandexPassword), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await InitializeAsync(webApp);

        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.InputName,
            CreateTeacherStepMessage.InputLoginFromYandexCalendar,
            CreateTeacherStepMessage.InputPasswordFromYandexCalendar,
            DefaultStepMessage.ProcessingRequest,
            CreateTeacherStepMessage.CreateSuccessMessage(ExpectedYandexName, ExpectedYandexLogin).Replace("\r\n", "\n")
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateTeacherCommand);

        await SendUpdate(ExpectedYandexName);
        await SendUpdate(ExpectedYandexLogin);

        await SendUpdate(ExpectedYandexPassword);

        _authorizationResourceMock.VerifyAll();

        Assert.Empty(expectedMessagesInOrder);
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message != DefaultStepMessage.ProcessingRequest;
    }
}