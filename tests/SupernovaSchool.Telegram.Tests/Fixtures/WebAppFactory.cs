using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupernovaSchool.Data;
using SupernovaSchool.Telegram.Tests.Extensions;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WebAppFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration();
        });

        builder.ConfigureServices(services =>
        {
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