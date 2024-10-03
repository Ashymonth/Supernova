namespace YandexCalendar.Net;

public class YandexCalendarClient : IYandexCalendarClient
{
    public YandexCalendarClient(ICalendarResource calendarResource, IEventsResource eventsResource)
    {
        CalendarResource = calendarResource;
        EventsResource = eventsResource;
    }

    public ICalendarResource CalendarResource { get; }

    public IEventsResource EventsResource { get; set; }
}