using SupernovaSchool.Models;

namespace SupernovaSchool.Tests;

public class TeacherCalendarTests
{
    [Test]
    public async Task Test()
    {
        var reservedSlots = new List<TimeRange>([
            new TimeRange(new TimeOnly(12, 0), new TimeOnly(12, 30)),
            new TimeRange(new TimeOnly(13, 0), new TimeOnly(13, 30))
        ]);

        var calendar = new TeacherCalendar(new DefaultDateTimeProvider());

        var result = calendar.FindAvailableTimeSlots(DateTime.Now, reservedSlots).ToArray();
    }
}