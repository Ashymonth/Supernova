using System.Net;
using YandexCalendar.Net.Extensions;
using YandexCalendar.Net.Models;

namespace YandexCalendar.Net;

public interface IAuthorizationResource
{
    Task<bool> AuthorizeAsync(UserCredentials credentials, CancellationToken ct = default);
}

public class AuthorizationResource : IAuthorizationResource
{
    private readonly HttpClient _httpClient;

    public AuthorizationResource(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> AuthorizeAsync(UserCredentials credentials, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.SetCredentials(credentials);

        using var response = await _httpClient.SendAsync(request, ct);
        
        return response.StatusCode != HttpStatusCode.Unauthorized;
    }
}