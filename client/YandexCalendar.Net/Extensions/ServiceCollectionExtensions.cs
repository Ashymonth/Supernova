using Microsoft.Extensions.DependencyInjection;

namespace YandexCalendar.Net.Extensions;

public static class ServiceCollectionExtensions
{
    private const string BaseUrl = "https://caldav.yandex.ru";

    public static void YandexCalendarClient(this IServiceCollection services)
    {
        services.AddHttpClient<IEventsResource, EventsResource>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(BaseUrl));

        services.AddHttpClient<ICalendarResource, CalendarResource>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(BaseUrl));

        services.AddScoped<IYandexCalendarClient, YandexCalendarClient>();
    }
}