using SupernovaSchool.Models;

namespace SupernovaSchool.Abstractions;

public interface IAppointmentService
{
    Task<IReadOnlyCollection<StudentAppointmentInfo>> GetStudentAppointmentsAsync(DateTime from, DateTime to,
        string userId,
        CancellationToken ct = default);

    Task<TimeRange[]> FindTeacherAvailableAppointmentSlotsAsync(Guid teacherId, DateTime meetingDay,
        CancellationToken ct = default);

    Task CreateAppointment(Guid teacherId, string studentId, DateTime startDate, DateTime endDate,
        CancellationToken ct = default);

    Task DeleteStudentAppointmentAsync(DateTime appointmentDay, string userId, CancellationToken ct = default);
}