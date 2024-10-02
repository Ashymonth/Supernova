using SupernovaSchool.Models;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Abstractions;

public interface IAppointmentService
{
    Task CreateAppointment(Guid teacherId, string studentId, DateTime startDate, DateTime endDate,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<StudentAppointmentInfo>> GetStudentAppointmentsAsync(DateOnly day, string userId,
        CancellationToken ct = default);
    
    Task<bool>DeleteStudentAppointmentAsync(DateTime appointmentDay, string userId, CancellationToken ct = default);
    
    Task<bool> IsStudentHasAppointmentForDateAsync(DateOnly date, string userId,
        CancellationToken ct = default);
    
    Task<IEnumerable<TimeSlot>> GetAppointmentsDatesAsync(Guid teacherId, DateTime from, DateTime to,
        CancellationToken ct = default);

}