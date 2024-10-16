using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SupernovaSchool.Telegram.Tests.Extensions;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WebAppFactoryWhenUserIsNotAdmin : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddDefaultConfiguration()
                .AddJsonFile("appsettings-without-admins.json");
        });

        return base.CreateHost(builder);
    }
}