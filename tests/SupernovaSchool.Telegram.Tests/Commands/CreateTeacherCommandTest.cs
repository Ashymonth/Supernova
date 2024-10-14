using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Tests.Helpers;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using WTelegram;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Telegram.Tests.Commands;

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

    [Fact]
    public async Task CreateTeacherTest_WhenUserIsNotAnAdmin_ReturnErrorMessage()
    {
        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.NotEnoughRightToCreateATeacher,
        ]);

        var webClient = _factory.CreateClient();

        _tgClient = await WTelegramClientFactory.CreateClient(Config);

        using var locker = new AutoResetEvent(false);
        // ReSharper disable once AccessToDisposedClosure
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);

        await SendUpdate(webClient, Telegram.Commands.CreateTeacherCommand);

        locker.WaitOne();
    }

    [Fact]
    public async Task CreateTeacherTest_WhenUserIsAnAdmin_ShouldCreateATeacher()
    {
        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.InputName,
            CreateTeacherStepMessage.InputLoginFromYandexCalendar,
            CreateTeacherStepMessage.InputPasswordFromYandexCalendar,
            DefaultStepMessage.ProcessingRequest,
            CreateTeacherStepMessage.CreateSuccessMessage(ExpectedYandexName, ExpectedYandexLogin).Replace("\r\n", "\n")
        ]);

        var webClient = _whenAdminFactory.CreateClient();

        _tgClient = await WTelegramClientFactory.CreateClient(Config);

        var locker = new AutoResetEvent(false);
        // ReSharper disable once AccessToDisposedClosure
        _tgClient.OnUpdates += update => TgClientOnOnUpdates(update, expectedMessagesInOrder, locker);

        await SendUpdate(webClient, Telegram.Commands.CreateTeacherCommand);

        locker.WaitOne();

        await SendUpdate(webClient, ExpectedYandexName);

        locker.WaitOne();

        await SendUpdate(webClient, ExpectedYandexLogin);

        locker.WaitOne();

        await SendUpdate(webClient, ExpectedYandexPassword);

        locker.WaitOne();
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message != DefaultStepMessage.ProcessingRequest;
    }
}