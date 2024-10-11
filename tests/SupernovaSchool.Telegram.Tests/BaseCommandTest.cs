using Microsoft.Extensions.Configuration;
using SupernovaSchool.Telegram.Tests.Options;

namespace SupernovaSchool.Telegram.Tests;

public class BaseCommandTest
{
    public BaseCommandTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("telegram-client-configuration.json")
            .Build();

        var config = new WTelegramConfig();
        configuration.GetSection("WTelegram").Bind(config);

        Config = config;
    }

    public WTelegramConfig Config { get; }
}