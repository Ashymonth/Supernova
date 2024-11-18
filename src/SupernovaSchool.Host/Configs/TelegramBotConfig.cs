using Microsoft.Extensions.Options;

namespace SupernovaSchool.Host.Configs;

public class TelegramBotConfig
{
    public string Token { get; set; } = null!;

    public string WebHookUrl { get; set; } = null!;

    public HashSet<string> AdminUserIdsFromTelegram { get; set; } = null!;
}

public class TelegramBotConfigSetup : IConfigureOptions<TelegramBotConfig>
{
    private readonly IConfiguration _configuration;

    public TelegramBotConfigSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(TelegramBotConfig options)
    {
        _configuration.GetRequiredSection(nameof(TelegramBotConfig)).Bind(options);
    }
}