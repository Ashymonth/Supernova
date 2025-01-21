using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupernovaSchool.Data;
using SupernovaSchool.Tests.Extensions;
using SupernovaSchool.Tests.Helpers;
using Testcontainers.PostgreSql;

namespace SupernovaSchool.Tests.Fixtures;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("supernova_school.test")
        .WithUsername("testUser")
        .WithPassword("testPassword")
        .WithExposedPort(5432)
        .Build();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration();
            ConfigureAppConfiguration(configurationBuilder);
        });

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SupernovaSchoolDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }
 
            // Add SQLite connection
            services.AddSingleton(SqliteConnectionHelper.CreateConnection());

            // Configure DbContext to use SQLite
            services.AddDbContext<SupernovaSchoolDbContext>((container, options) =>
            {
                options.UseNpgsql(_container.GetConnectionString());
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

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}