namespace SupernovaSchool.Extensions;

public static class DateTimeExtensions
{
    private const int MaxAppointmentDaysToSelect = 7;

    public static IEnumerable<DateTime> GetTeacherWorkingDays(this DateTime date)
    {
        return Enumerable.Range(0, MaxAppointmentDaysToSelect).Select(i => date.AddDays(i))
            .Where(time => time.DayOfWeek != DayOfWeek.Sunday && time.DayOfWeek != DayOfWeek.Saturday);
    }
}