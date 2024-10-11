using System.Net.Http.Json;
using SupernovaSchool.Telegram.Tests.Fixtures;
using Telegram.Bot.Types;

namespace SupernovaSchool.Telegram.Tests;

public class StartCommandTest : BaseCommandTest, IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _fixture;

    public StartCommandTest(WebAppFactory fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test()
    {
        var client = _fixture.CreateClient();

        using var startCommandResponse = await client.PostAsJsonAsync("/updates", new Update
        {
            Message = new Message
            {
                Text = "/start", From = new User
                {
                    Id = Config.SenderId,
                    Username = "@Ashymonth"
                }
            }
        });

        var message = await startCommandResponse.Content.ReadFromJsonAsync<Message>();

        Assert.NotNull(message);
        Assert.Equal(CommandText.StartCommandMessage.Replace("\r\n", "\n"), message.Text);
    }
}