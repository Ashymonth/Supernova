using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WebAppFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<BaseCommandTest>();
        });
        
        return base.CreateHost(builder);
    }
 
}