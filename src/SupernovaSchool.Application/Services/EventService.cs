using Ical.Net.CalendarComponents;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Application.Services;

public class EventService : IEventService
{
    private readonly ICalendarService _calendarService;
    private readonly IEventsResource _eventsResource;

    public EventService(ICalendarService calendarService, IEventsResource eventsResource)
    {
        _calendarService = calendarService;
        _eventsResource = eventsResource;
    }

    public async Task CreateEventAsync(Teacher teacher, Appointment appointment,
        string userId,
        CancellationToken ct = default)
    {
        var calendarUrl = await _calendarService.GetDefaultCalendarUrlAsync(teacher, ct);

        await _eventsResource.AddEventAsync(GetCredentials(teacher), appointment, calendarUrl, userId, ct);
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsForDayAsync(Teacher teacher, DateOnly eventDate,
        CancellationToken ct = default)
    {
        var calendarUrl = await _calendarService.GetDefaultCalendarUrlAsync(teacher, ct);

        return await _eventsResource.GetEventsAsync(GetCredentials(teacher), eventDate.ToDateTime(new TimeOnly()),
            eventDate.ToDateTime(new TimeOnly(23, 59)), calendarUrl, ct);
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsAsync(Teacher teacher, DateTime from, DateTime to,
        CancellationToken ct = default)
    {
        var calendarUrl = await _calendarService.GetDefaultCalendarUrlAsync(teacher, ct);

        return await _eventsResource.GetEventsAsync(GetCredentials(teacher), from, to, calendarUrl, ct);
    }

    public async Task DeleteEventAsync(Teacher teacher, string eventId, CancellationToken ct = default)
    {
        var calendarUrl = await _calendarService.GetDefaultCalendarUrlAsync(teacher, ct);

        await _eventsResource.DeleteEventAsync(GetCredentials(teacher), calendarUrl, eventId, ct);
    }

    private static UserCredentials GetCredentials(Teacher teacher)
    {
        var userInfo = new UserCredentials(teacher.Email, teacher.YandexCalendarPassword);

        return userInfo;
    }
}