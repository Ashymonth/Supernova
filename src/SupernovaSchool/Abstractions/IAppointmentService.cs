using YandexCalendar.Net.Models;

namespace SupernovaSchool.Abstractions;

public interface IAppointmentService
{
    Task CreateAppointment(Guid teacherId, string studentId, DateTime startDate, DateTime endDate,
        CancellationToken ct = default);

    Task<bool> IsUserHasAppointmentForDateAsync(DateOnly date, string userId,
        CancellationToken ct = default);
    
    Task<IEnumerable<TimeSlot>> GetAppointmentsDatesAsync(Guid teacherId, DateTime from, DateTime to,
        CancellationToken ct = default);

}