using System.Globalization;

namespace SupernovaSchool.Extensions;

public static class DateTimeExtensions
{
    private static readonly string[] Formats = ["dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy"];

    private const int MaxAppointmentDaysToSelect = 7;

    public static IEnumerable<DateTime> GetTeacherWorkingDays(this DateTime date)
    {
        return Enumerable.Range(0, MaxAppointmentDaysToSelect).Select(i => date.AddDays(i))
            .Where(time => time.DayOfWeek != DayOfWeek.Sunday && time.DayOfWeek != DayOfWeek.Saturday);
    }

    public static string ToApplicationDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("dd.MM.yyyy");
    }

    public static DateTime ParseApplicationDateFormat(this string dateTime)
    {
        return DateTime.ParseExact(dateTime, Formats, CultureInfo.InvariantCulture);
    }
    
    public static DateOnly ParseApplicationDateOnlyFormat(this string dateTime)
    {
        return DateOnly.ParseExact(dateTime, Formats, CultureInfo.InvariantCulture);
    }

    public static bool TryParseApplicationDateFormat(this string dateTime, out DateTime result)
    {
        return DateTime.TryParseExact(dateTime, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }
}