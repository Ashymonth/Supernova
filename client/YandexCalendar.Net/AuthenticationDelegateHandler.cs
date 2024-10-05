using System.Net.Http.Headers;
using YandexCalendar.Net.Models;

namespace YandexCalendar.Net;

public class AuthenticationDelegateHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Options.TryGetValue(new HttpRequestOptionsKey<UserCredentials>("credentials"), out var value))
        {
            throw new InvalidOperationException("The request does not contain a credentials.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", value.ToBasic64Credentials());

        return await base.SendAsync(request, cancellationToken);
    }
}