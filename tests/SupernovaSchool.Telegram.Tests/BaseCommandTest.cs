using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SupernovaSchool.Telegram.Tests.Options;
using Telegram.Bot.Types;

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

    protected async Task<Message?> SendUpdate(HttpClient client, string message)
    {
        using var response = await client.PostAsJsonAsync("/updates", new Update
        {
            Message = new Message { Text = message, From = new User { Id = Config.SenderId } }
        });

        var content = await response.Content.ReadAsStringAsync();

        return content.Length != 0 ? JsonSerializer.Deserialize<Message>(content) : null;
    }
}