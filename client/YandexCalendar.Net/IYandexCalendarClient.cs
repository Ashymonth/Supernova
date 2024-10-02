namespace YandexCalendar.Net;

public interface IYandexCalendarClient
{
    public ICalendarResource CalendarResource { get; }

    public IEventsResource EventsResource { get; set; }
}