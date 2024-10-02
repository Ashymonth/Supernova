using SupernovaSchool.Abstractions;

namespace SupernovaSchool.Models;

public class TeacherCalendar
{
    private static readonly TimeOnly WorkStartTime = new(8, 30);
    private static readonly TimeOnly WorkEndTime = new(18, 30);
    private static readonly TimeSpan SlotInterval = TimeSpan.FromMinutes(30);

    private readonly IDateTimeProvider _timeProvider;

    public TeacherCalendar(IDateTimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public IReadOnlyCollection<TimeRange> FindAvailableTimeSlots(DateTime meetingDay, List<TimeRange> reservedSlots)
    {
        // Sort reserved slots by their start time for easier comparison.
        reservedSlots.Sort((x, y) => x.Start.CompareTo(y.Start));

        var result = new List<TimeRange>();
        var today = _timeProvider.Now;

        var slotStart = WorkStartTime;

        // Iterate through each possible time slot within working hours.
        while (slotStart < WorkEndTime)
        {
            if (IsSlotAfterCurrentTime(meetingDay, slotStart, today) &&
                !IsSlotReserved(slotStart, reservedSlots))
            {
                result.Add(new TimeRange(slotStart, slotStart.Add(SlotInterval)));
            }

            slotStart = slotStart.Add(SlotInterval);
        }

        return result;
    }

    // Checks if the slot is after the current time.
    private static bool IsSlotAfterCurrentTime(DateTime meetingDay, TimeOnly slotStart, DateTime today)
    {
        if (meetingDay.Date < today.Date)
        {
            return false;
        }

        return !(meetingDay.Date == today.Date && slotStart.ToTimeSpan() <= today.TimeOfDay);
    }

    // Determines if a given slot is reserved.
    private static bool IsSlotReserved(TimeOnly slotStart, List<TimeRange> reservedSlots)
    {
        // Iterate through the sorted reserved slots and check if the slot overlaps with any reserved slot.
        foreach (var reservedSlot in reservedSlots)
        {
            if (slotStart < reservedSlot.Start)
            {
                break;
            }

            if (slotStart >= reservedSlot.Start && slotStart < reservedSlot.End)
            {
                return true;
            }
        }

        return false;
    }
}