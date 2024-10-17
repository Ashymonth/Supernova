using Microsoft.Extensions.Configuration;

namespace SupernovaSchool.Telegram.Tests.Fixtures;

public class WebAppFactoryWhenUserIsNotAdmin : WebAppFactory
{
    protected override void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        base.ConfigureAppConfiguration(configurationBuilder);
        configurationBuilder.AddJsonFile("appsettings-without-admins.json");
    }
}