namespace SupernovaSchool.Tests.Options;

public class WTelegramConfig
{
    /// <summary>
    /// User id how can send a message to test bot.
    /// </summary>
    public long SenderId { get; set; }
    
    /// <summary>
    /// Bot chat id which you will test
    /// </summary>
    public long BotChatId { get; set; }

    /// <summary>
    /// Phone number to log in. 
    /// </summary>
    public string PhoneNumber { get; set; } = null!;

    public int ApiId { get; set; }

    public string ApiHash { get; set; } = null!;
}