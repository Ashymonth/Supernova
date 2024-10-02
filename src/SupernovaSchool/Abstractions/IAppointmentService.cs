using YandexCalendar.Net.Models;

namespace SupernovaSchool.Abstractions;

public interface IAppointmentService
{
    Task CreateAppointment(Guid teacherId, string studentId, DateTime startDate, DateTime endDate,
        CancellationToken ct = default);

    Task<IEnumerable<TimeSlot>> GetAppointmentsAsync(Guid teacherId, DateTime from, DateTime to,
        CancellationToken ct = default);

}