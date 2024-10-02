using SupernovaSchool.Models;

namespace SupernovaSchool.Abstractions;

public interface ITeacherService
{
    Task<TimeRange[]> FindAvailableTimeSlots(Guid teacherId, DateTime meetingDay,
        CancellationToken ct = default);
    
    Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(CancellationToken ct = default);
    
    IEnumerable<DateTime> GetAvailableMeetingDates();
}