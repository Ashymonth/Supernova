using Ical.Net.CalendarComponents;
using SupernovaSchool.Models;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Abstractions;

public interface IEventService
{
    Task CreateEventAsync(Teacher teacher, Appointment appointment,
        string userId,
        CancellationToken ct = default);

    Task<IEnumerable<CalendarEvent>> GetEventsForDayAsync(Teacher teacher, DateOnly eventDate,
        CancellationToken ct = default);

    Task<IEnumerable<CalendarEvent>> GetEventsAsync(Teacher teacher, DateTime from, DateTime to,
        CancellationToken ct = default);

    Task DeleteEventAsync(Teacher teacher, string eventId, CancellationToken ct = default);
}