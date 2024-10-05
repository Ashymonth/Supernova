using System.Net.Http.Headers;
using YandexCalendar.Net.Models;

namespace YandexCalendar.Net;

internal class AuthenticationDelegateHandler : DelegatingHandler
{
    public const string CredentialsKey = "credentials";
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var key = new HttpRequestOptionsKey<UserCredentials>("credentials");
        if (!request.Options.TryGetValue(key, out var value))
        {
            throw new InvalidOperationException("The request does not contain a credentials.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", value.ToBasic64Credentials());

        return await base.SendAsync(request, cancellationToken);
    }
}