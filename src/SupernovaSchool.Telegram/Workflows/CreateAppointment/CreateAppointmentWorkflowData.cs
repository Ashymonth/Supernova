using SupernovaSchool.Telegram.Steps;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment;

public class CreateAppointmentWorkflowData : MessagePaginator, IUserStep
{
    public string UserId { get; set; } = null!;

    public bool IsStudentRegistered { get; set; }
    
    public DateTime[] AvailableMeetingDays { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }

    public Guid TeacherId { get; set; }

    public TimeRange[] AvailableTimeSlots { get; set; } = [];

    public bool UserHasAppointment { get; set; }

    public bool IsMeetingDateValid()
    {
        return DateTime.TryParse(PaginationMessage, out var date) && AvailableMeetingDays.Any(time => time.Date == date.Date);
    }
    
    public TimeRange? GetTimeSlot()
    {
        var span = PaginationMessage.AsSpan();
        var dashIndex = span.IndexOf('-');
        if (dashIndex == -1)
        {
            return null;
        }

        if (!TimeOnly.TryParse(span[..4], out var from))
        {
            return null;
        }

        if (!TimeOnly.TryParse(span[(dashIndex + 1)..], out var to))
        {
            return null;
        }

        return new TimeRange(from, to);
    }
}