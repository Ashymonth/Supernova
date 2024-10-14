using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using WTelegram;
using Xunit.Extensions.Ordering;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Telegram.Tests.Commands;

[Collection("CommandsCollection"), Order(3)]
public class CreateTeacherCommandTest : BaseCommandTest, IClassFixture<WebAppFactoryWhenUserIsNotAdmin>
{
    private const string ExpectedYandexName = "Test name";
    private const string ExpectedYandexLogin = "Test login";
    private const string ExpectedYandexPassword = "Test password";

    private readonly WebAppFactoryWhenUserIsNotAdmin _factory;
    private readonly WebApplicationFactory<Program> _whenAdminFactory;
    private readonly Mock<IAuthorizationResource> _mock = new();
    private Client _tgClient = null!;

    public CreateTeacherCommandTest(WebAppFactoryWhenUserIsNotAdmin factory)
    {
        _mock.Setup(resource => resource.AuthorizeAsync(It.Is<UserCredentials>(credentials =>
            credentials.UserName == ExpectedYandexLogin &&
            credentials.Password == ExpectedYandexPassword), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _whenAdminFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                {
                    configurationBuilder.AddJsonFile("appsettings.json")
                        .AddUserSecrets<BaseCommandTest>();
                });

                builder.ConfigureServices(collection =>
                {
                    var service = collection.First(descriptor =>
                        descriptor.ServiceType == typeof(IAuthorizationResource));

                    collection.Remove(service);

                    collection.AddSingleton<IAuthorizationResource>(_ => _mock.Object);
                });
            });
        _factory = factory;
    }

    [Fact, Order(1)]
    public async Task CreateTeacherTest_WhenUserIsNotAnAdmin_ReturnErrorMessage()
    {
        await InitializeAsync(_factory);
        
        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.NotEnoughRightToCreateATeacher,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);
        
        await SendUpdate(Telegram.Commands.CreateTeacherCommand);

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    [Fact, Order(2)]
    public async Task CreateTeacherTest_WhenUserIsAnAdmin_ShouldCreateATeacher()
    {
        await InitializeAsync(_whenAdminFactory);
        
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

        _mock.VerifyAll();

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message != DefaultStepMessage.ProcessingRequest;
    }
}