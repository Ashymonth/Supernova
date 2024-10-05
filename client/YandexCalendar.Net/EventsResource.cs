using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;
using Ical.Net;
using Ical.Net.CalendarComponents;
using YandexCalendar.Net.Contracts;
using YandexCalendar.Net.Extensions;
using YandexCalendar.Net.Models;

namespace YandexCalendar.Net;

public interface IEventsResource
{
    Task AddEventAsync(UserCredentials credentials, Appointment appointment, string calendarUrl,
        string description,
        CancellationToken ct = default);

    Task<IEnumerable<CalendarEvent>> GetEventsAsync(UserCredentials credentials, DateTime from, DateTime to,
        string calendarUrl,
        CancellationToken ct = default);

    Task DeleteEventAsync(UserCredentials credentials, string calendarUrl, string eventId,
        CancellationToken ct = default);
}

public class EventsResource : IEventsResource
{
    private const string CreateEventUrlTemplate = "{0}{1}.ics";

    private const string CreateEventTemplate = """
                                               BEGIN:VCALENDAR
                                               VERSION:2.0
                                               PRODID:-//Yandex//NONSGML iCal4j 1.0//EN
                                               BEGIN:VEVENT
                                               UID:{0}
                                               DTSTAMP;TZID=Europe/Moscow:{1}
                                               DTSTART;TZID=Europe/Moscow:{2}
                                               DTEND;TZID=Europe/Moscow:{3}
                                               SUMMARY:{4}
                                               DESCRIPTION:{5}
                                               LOCATION:Online
                                               END:VEVENT
                                               END:VCALENDAR
                                               """;

    private const string GetEventsUrlTemplate = """
                                                <?xml version="1.0" encoding="utf-8" ?>
                                                <C:calendar-query xmlns:D="DAV:" xmlns:C="urn:ietf:params:xml:ns:caldav">
                                                    <D:prop>
                                                        <D:getetag/>
                                                        <C:calendar-data/>
                                                    </D:prop>
                                                    <C:filter>
                                                        <C:comp-filter name="VCALENDAR">
                                                            <C:comp-filter name="VEVENT">
                                                                <C:time-range start="{0}" end="{1}"/>
                                                            </C:comp-filter>
                                                        </C:comp-filter>
                                                    </C:filter>
                                                </C:calendar-query>
                                                """;

    private readonly HttpClient _httpClient;

    public EventsResource(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task AddEventAsync(UserCredentials credentials, Appointment appointment, string calendarUrl,
        string description,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentException.ThrowIfNullOrWhiteSpace(calendarUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var uniqueIdForRequest = Guid.NewGuid().ToString();

        var eventInfo = CreateEventRequest(uniqueIdForRequest, appointment.StartDate, appointment.EndDate,
            appointment.Summary, description);

        var formattedUrl = string.Format(CreateEventUrlTemplate, calendarUrl, uniqueIdForRequest);

        using var createEventRequest = new HttpRequestMessage(HttpMethod.Put, formattedUrl);
        createEventRequest.Content = new StringContent(eventInfo, Encoding.UTF8, "text/calendar");
        createEventRequest.SetCredentials(credentials);

        // Send the request to add the event
        using var addEventResponse = await _httpClient.SendAsync(createEventRequest, ct);
        addEventResponse.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsAsync(UserCredentials credentials, DateTime from,
        DateTime to,
        string calendarUrl,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentException.ThrowIfNullOrWhiteSpace(calendarUrl);

        var formattedRequest = string.Format(GetEventsUrlTemplate, CreateFormatedDate(from.ToUniversalTime()),
            CreateFormatedDate(to.ToUniversalTime()));

        using var reportRequest = new HttpRequestMessage(new HttpMethod("REPORT"), calendarUrl);
        reportRequest.Content = new StringContent(formattedRequest, Encoding.UTF8, "application/xml");
        reportRequest.SetCredentials(credentials);

        // Set the depth header to "1" to retrieve all events in the specified time range
        reportRequest.Headers.Add("Depth", "1");

        using var reportResponse = await _httpClient.SendAsync(reportRequest, ct);
        reportResponse.EnsureSuccessStatusCode();
        
        var xmlResponse = await reportResponse.Content.ReadAsStringAsync(ct);
        
        var serializer = new XmlSerializer(typeof(CalendarResponse));

        using var reader = new StringReader(xmlResponse);
        var multiStatus = (CalendarResponse)serializer.Deserialize(reader)!;

        return multiStatus.CalendarItems
            .Select(item => Calendar.Load(item.PropertyStatus.CalendarProperties.CalendarEventData))
            .SelectMany(calendar => calendar.Events)
            .Distinct();
    }

    public async Task DeleteEventAsync(UserCredentials credentials, string calendarUrl, string eventId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentException.ThrowIfNullOrEmpty(calendarUrl);
        ArgumentException.ThrowIfNullOrEmpty(eventId);

        var formattedUrl = string.Format(CreateEventUrlTemplate, calendarUrl, eventId);
        using var request = new HttpRequestMessage(HttpMethod.Delete, formattedUrl);
        request.SetCredentials(credentials);

        using var response = await _httpClient.SendAsync(request,ct);
        response.EnsureSuccessStatusCode();
    }

    private static string CreateEventRequest(string uniqueIdForRequest, DateTime startDate, DateTime endDate,
        string summary, string description)
    {
        var now = CreateFormatedDate(DateTime.UtcNow);
        var start = CreateFormatedDate(startDate.ToUniversalTime());
        var end = CreateFormatedDate(endDate.ToUniversalTime());

        return string.Format(CreateEventTemplate, uniqueIdForRequest, now, start, end, summary, description);
    }

    private static string CreateFormatedDate(DateTime date)
    {
        return $"{date:yyyyMMddTHHmmssZ}";
    }
}