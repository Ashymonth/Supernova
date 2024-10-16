using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram.Tests.Extensions;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WebAppFactoryWhenUserIsNotRegistered : WebApplicationFactory<Program>
{
    private readonly Mock<IStudentService> _studentServiceMock;
     
    public WebAppFactoryWhenUserIsNotRegistered(Mock<IStudentService> studentServiceMock)
    {
        _studentServiceMock = studentServiceMock;
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration();
        });

        builder.ConfigureServices(collection =>
        {
            var descriptor = collection.First(descriptor => descriptor.ServiceType == typeof(IStudentService));

            collection.Remove(descriptor);

            collection.AddSingleton(_studentServiceMock.Object);
        });
        
        return base.CreateHost(builder);
    }
}