using SupernovaSchool.Extensions;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment;

public class CreateAppointmentWorkflowData : MessagePaginator, IUserStep
{
    public bool IsStudentRegistered { get; set; }

    public List<Teacher> Teachers { get; set; } = null!;

    public string? SelectedTeacherIndex { get; set; }

    public Teacher SelectedTeacher { get; set; } = null!;
    
    public DateTime[] AvailableMeetingDays { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }
    
    public TimeRange[] AvailableTimeSlots { get; set; } = [];

    public bool UserHasAppointment { get; set; }
    
    public string UserId { get; set; } = null!;

    public bool IsTest()
    {
        return int.TryParse(SelectedTeacherIndex, out var result) && result >= 0 && result <= Teachers.Count;
    }

    public bool IsMeetingDateValid()
    {
        return PaginationMessage.TryParseApplicationDateFormat(out var date) &&
               AvailableMeetingDays.Any(time => time.Date == date.Date);
    }

    public TimeRange? GetTimeSlot()
    {
        var span = PaginationMessage.AsSpan();
        var dashIndex = span.IndexOf('-');
        if (dashIndex == -1) return null;

        if (!TimeOnly.TryParse(span[..dashIndex], out var from)) return null;

        if (!TimeOnly.TryParse(span[(dashIndex + 1)..], out var to)) return null;

        return new TimeRange(from, to);
    }
}