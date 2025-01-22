using Microsoft.Extensions.Configuration;

namespace SupernovaSchool.Tests.Extensions;

public static class WebApplicationFactoryExtensions
{
    public static void AddDefaultConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<BaseCommandTest>();
    }
}