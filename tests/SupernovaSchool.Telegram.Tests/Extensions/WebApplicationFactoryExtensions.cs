using Microsoft.Extensions.Configuration;

namespace SupernovaSchool.Telegram.Tests.Extensions;

public static class WebApplicationFactoryExtensions
{
    public static IConfigurationBuilder AddDefaultConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        return configurationBuilder
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<BaseCommandTest>();
    }
}