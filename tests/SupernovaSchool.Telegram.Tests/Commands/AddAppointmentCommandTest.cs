using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Telegram.Tests.Commands;

[Collection("CommandsCollection"), Order(4)]
public class AddAppointmentCommandTest : BaseCommandTest
{
    private readonly Mock<IStudentService> _mock = new();
    private readonly WebApplicationFactory<Program> _factory;

    public AddAppointmentCommandTest()
    {
        _factory = new WebApplicationFactory<Program>()
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
                        descriptor.ServiceType == typeof(IStudentService));

                    collection.Remove(service);

                    collection.AddSingleton<IStudentService>(_ => _mock.Object);
                });
            });
    }
    
    [Fact]
    public async Task CreateAppointmentAsync_WhenStudentNotRegistered_ShouldReturnErrorMessage()
    {
        _mock.Setup(studentService =>
                studentService.GetStudentAsync(It.Is<string>(userId => userId == Config.SenderId.ToString()),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);
        
        await InitializeAsync(_factory);
        
        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.UserNotRegistered,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);

        Assert.True(expectedMessagesInOrder.Count == 0);
    }
}