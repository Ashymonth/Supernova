using System.Net;
using YandexCalendar.Net.Models;

namespace YandexCalendar.Net;

public interface IAuthorizationClient
{
    Task<bool> AuthorizeAsync(UserCredentials credentials, CancellationToken ct = default);
}

public class AuthorizationClient
{
    private readonly HttpClient _httpClient;

    public AuthorizationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> AuthorizeAsync(UserCredentials credentials, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync("", ct);

        return response.StatusCode != HttpStatusCode.Unauthorized;
    }
}