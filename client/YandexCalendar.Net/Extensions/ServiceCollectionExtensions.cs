using Microsoft.Extensions.DependencyInjection;

namespace YandexCalendar.Net.Extensions;

public static class ServiceCollectionExtensions
{
    private const string BaseUrl = "https://caldav.yandex.ru";

    public static void YandexCalendarClient(this IServiceCollection services)
    {
        services.AddHttpClient<IEventsResource, EventsResource>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(BaseUrl))
            .AddStandardHedgingHandler();

        services.AddHttpClient<ICalendarResource, CalendarResource>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(BaseUrl))
            .AddStandardResilienceHandler();
        
        services.AddHttpClient<IAuthorizationResource, AuthorizationResource>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(BaseUrl))
            .AddHttpMessageHandler<AuthenticationDelegateHandler>()
            .AddStandardResilienceHandler();

        services.AddTransient<AuthenticationDelegateHandler>();
        services.AddScoped<IYandexCalendarClient, YandexCalendarClient>();
    }
}