using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
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
        
        return base.CreateHost(builder);
    }
 
}