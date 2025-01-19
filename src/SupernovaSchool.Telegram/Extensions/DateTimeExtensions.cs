namespace SupernovaSchool.Telegram.Extensions;

internal static class DateTimeExtensions
{
    public static string ToApplicationDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("dd-MM-yyyy");
    }
}