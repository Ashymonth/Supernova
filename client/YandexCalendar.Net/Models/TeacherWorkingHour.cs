namespace YandexCalendar.Net.Models;

public record TeacherWorkingHour
{
    public static readonly HashSet<TimeOnly> Hour = GenerateTimeSlots();
 
    private static HashSet<TimeOnly> GenerateTimeSlots()
    {
        var timeSlots = new HashSet<TimeOnly>();
        var startTime = new TimeOnly(8, 30);  // Start time: 08:30
        var endTime = new TimeOnly(18, 0);    // End time: 18:00

        // Add time slots in 30-minute intervals.
        for (var currentTime = startTime; currentTime <= endTime; currentTime = currentTime.AddMinutes(30))
        {
            timeSlots.Add(currentTime);
        }

        return timeSlots;
    }
}