using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupernovaSchool.Data;
using SupernovaSchool.Models;
using SupernovaSchool.Tests.Extensions;
using SupernovaSchool.Tests.Helpers;

namespace SupernovaSchool.Tests.Fixtures;

public class WebAppFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration();
            ConfigureAppConfiguration(configurationBuilder);
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.First(
                d => d.ServiceType ==
                     typeof(DbContextOptions<SupernovaSchoolDbContext>));

            services.Remove(dbContextDescriptor);

            services.AddSingleton(SqliteConnectionHelper.CreateConnection());

            services.AddDbContext<SupernovaSchoolDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
            
            ConfigureServices(services);
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupernovaSchoolDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        
        SeedData(scope.ServiceProvider, db);

        db.SaveChanges();

        return host;
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected virtual void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    protected virtual void SeedData(IServiceProvider provider, SupernovaSchoolDbContext dbContext)
    {
    }
}