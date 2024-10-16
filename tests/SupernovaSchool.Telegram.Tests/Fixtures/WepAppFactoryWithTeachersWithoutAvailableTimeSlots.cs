using System.Data.Common;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Data;
using SupernovaSchool.Telegram.Tests.Extensions;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WepAppFactoryWithTeachersWithoutAvailableTimeSlots : WebApplicationFactory<Program>
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
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration();
        });
        
        builder.ConfigureServices(services =>
        {
            services.ReplaceRequiredService(_mock.Object);
            services.ReplaceRequiredService(_dateTimeProvider.Object);
            services.ReplaceRequiredService(_studentServiceMock.Object);
            
            var dbContextDescriptor = services.First(
                d => d.ServiceType ==
                     typeof(DbContextOptions<SupernovaSchoolDbContext>));

            services.Remove(dbContextDescriptor);
            
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<SupernovaSchoolDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });
        
        
        return base.CreateHost(builder);
    }
}