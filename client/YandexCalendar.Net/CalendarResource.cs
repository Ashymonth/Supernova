using System.Text;
using System.Xml.Serialization;
using YandexCalendar.Net.Contracts;
using YandexCalendar.Net.Extensions;
using YandexCalendar.Net.Models;

namespace YandexCalendar.Net;

public interface ICalendarResource
{
    Task<string> GetDefaultCalendarUrl(UserCredentials credentials, CancellationToken ct = default);
}

public class CalendarResource : ICalendarResource
{
    private const string GetCalendarsUrlTemplate = "/calendars/{0}%40yandex.ru";

    private const string GetCalendarsRequestContent = """
                                                      <d:propfind xmlns:d='DAV:' xmlns:cal='urn:ietf:params:xml:ns:caldav'>
                                                                             <d:prop>
                                                                                 <d:displayname />
                                                                                 <cal:calendar-description />
                                                                                 <cal:supported-calendar-component-set />
                                                                             </d:prop>
                                                                         </d:propfind>
                                                      """;

    private readonly HttpClient _httpClient;

    public CalendarResource(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetDefaultCalendarUrl(UserCredentials credentials, CancellationToken ct = default)
    {
        var formattedCalendarUrl = string.Format(GetCalendarsUrlTemplate, credentials.UserName);

        using var calendarListRequest = new HttpRequestMessage(new HttpMethod("PROPFIND"), formattedCalendarUrl);
        calendarListRequest.Content = new StringContent(GetCalendarsRequestContent, Encoding.UTF8, "application/xml");
        calendarListRequest.SetCredentials(credentials);

        calendarListRequest.Headers.Add("Depth", "1"); // to get all calendars

        using var calendarResponse = await _httpClient.SendAsync(calendarListRequest, ct);

        var xmlResponse = await calendarResponse.Content.ReadAsStringAsync(ct);

        var serializer = new XmlSerializer(typeof(CalendarContainer));

        using var reader = new StringReader(xmlResponse);
        var multiStatus = (CalendarContainer)serializer.Deserialize(reader)!;

        var result = multiStatus.Responses.FirstOrDefault(response => response.Href.Contains("events") &&
                                                                      response.Propstats.Any(propstat =>
                                                                          propstat.Status == "HTTP/1.1 200 OK"))
                     ?? throw new InvalidOperationException("No events found");

        return result.Href;
    }
}