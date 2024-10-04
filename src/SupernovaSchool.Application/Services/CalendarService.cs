using Microsoft.Extensions.Caching.Memory;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using UtilityBills.Aggregates;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Application.Services;

public class CalendarService : ICalendarService
{
    private const string CacheKeyTemplate = "calendar-service_{0}";

    private readonly IMemoryCache _memoryCache;
    private readonly IYandexCalendarClient _yandexCalendarClient;
    private readonly IPasswordProtector _passwordProtector;

    public CalendarService(IMemoryCache memoryCache, IYandexCalendarClient yandexCalendarClient,
        IPasswordProtector passwordProtector)
    {
        _memoryCache = memoryCache;
        _yandexCalendarClient = yandexCalendarClient;
        _passwordProtector = passwordProtector;
    }

    public async ValueTask<string> GetDefaultCalendarUrlAsync(Teacher teacher, CancellationToken ct = default)
    {
        return (await _memoryCache.GetOrCreateAsync(string.Format(CacheKeyTemplate, teacher.Id),
            async entry =>
            {
                var url = await _yandexCalendarClient.CalendarResource.GetDefaultCalendarUrl(GetCredentials(teacher),
                    ct);

                entry.SetValue(url);
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));

                return url;
            }))!;
    }

    private UserCredentials GetCredentials(Teacher teacher)
    {
        var userInfo = new UserCredentials(teacher.Login,
            teacher.YandexCalendarPassword.GetUnprotected(_passwordProtector));

        return userInfo;
    }
}