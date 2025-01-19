using System.Globalization;

namespace SupernovaSchool.Telegram.Extensions;

internal static class DateTimeExtensions
{
    private static readonly string[] Formats = ["dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy"];

    public static string ToApplicationDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("dd.MM.yyyy");
    }

    public static DateTime ParseApplicationDateFormat(this string dateTime)
    {
        return DateTime.ParseExact(dateTime, Formats, CultureInfo.InvariantCulture);
    }
}