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

public class WebAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _container;

    private readonly Action<IServiceCollection>[]? _configureServicesAction;
    private readonly Action<IConfigurationBuilder>[]? _configureAppAction;
    private readonly Action<IServiceProvider, SupernovaSchoolDbContext>[]? _seedAction;
 
    public WebAppFactory(Action<IServiceCollection>[]? configureServicesAction,
        Action<IConfigurationBuilder>[]? configureAppAction,
        Action<IServiceProvider, SupernovaSchoolDbContext>[]? seedAction,
        PostgreSqlContainer container)
    {
        _configureServicesAction = configureServicesAction;
        _configureAppAction = configureAppAction;
        _seedAction = seedAction;
        _container = container;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration();
            if (_configureAppAction is null) return;

            foreach (var action in _configureAppAction)
            {
                action(configurationBuilder);
            }
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
            services.AddDbContext<SupernovaSchoolDbContext>((_, options) =>
            {
                options.UseNpgsql(_container.GetConnectionString());
            });
            if (_configureServicesAction is null) return;

            foreach (var action in _configureServicesAction)
            {
                action(services);
            }
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupernovaSchoolDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        if (_seedAction is null) return host;

        foreach (var action in _seedAction)
        {
            action(scope.ServiceProvider, db);
        }

        db.SaveChanges();

        return host;
    }
}