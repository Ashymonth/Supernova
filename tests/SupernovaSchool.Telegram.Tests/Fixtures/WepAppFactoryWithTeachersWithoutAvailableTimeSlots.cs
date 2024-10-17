using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Data;
using SupernovaSchool.Telegram.Tests.Extensions;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WepAppFactoryWithTeachersWithoutAvailableTimeSlots : WebAppFactory
{
    private readonly Mock<IAppointmentService> _mock;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly Mock<IStudentService> _studentServiceMock;

    public WepAppFactoryWithTeachersWithoutAvailableTimeSlots(Mock<IAppointmentService> mock,
        Mock<IDateTimeProvider> dateTimeProvider, Mock<IStudentService> studentServiceMock)
    {
        _mock = mock;
        _dateTimeProvider = dateTimeProvider;
        _studentServiceMock = studentServiceMock;
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.ReplaceRequiredService(_mock.Object);
            services.ReplaceRequiredService(_dateTimeProvider.Object);
            services.ReplaceRequiredService(_studentServiceMock.Object);
        });
        
        return base.CreateHost(builder);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.ReplaceRequiredService(_dateTimeProvider.Object);
    }

    protected virtual void SeedData(SupernovaSchoolDbContext dbContext)
    {
        
    }
}