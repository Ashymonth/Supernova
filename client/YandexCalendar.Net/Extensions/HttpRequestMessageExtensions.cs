using YandexCalendar.Net.Models;

namespace YandexCalendar.Net.Extensions;

public static class HttpRequestMessageExtensions
{
    public static void SetCredentials(this HttpRequestMessage message, UserCredentials credentials)
    {
        var key = new HttpRequestOptionsKey<UserCredentials>(AuthenticationDelegateHandler.CredentialsKey);
        message.Options.Set(key, credentials);
    }
}