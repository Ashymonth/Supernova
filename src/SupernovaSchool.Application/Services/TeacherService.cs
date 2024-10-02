using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;

namespace SupernovaSchool.Application.Services;

public class TeacherService : ITeacherService
{
    private const int MaxAppointmentDaysToSelect = 7;
    private readonly IRepository<Teacher> _teacherRepository;
    private readonly IAppointmentService _appointmentService;
    private readonly IDateTimeProvider _timeProvider;

    public TeacherService(IRepository<Teacher> teacherRepository, IAppointmentService appointmentService,
        IDateTimeProvider timeProvider)
    {
        _teacherRepository = teacherRepository;
        _appointmentService = appointmentService;
        _timeProvider = timeProvider;
    }

    public async Task<TimeRange[]> FindAvailableTimeSlots(Guid teacherId, DateTime meetingDay,
        CancellationToken ct = default)
    {
        var reservedTimeSlots = await _appointmentService.GetAppointmentsAsync(teacherId, meetingDay.Date,
            meetingDay.Date.AddHours(23).AddMinutes(59), ct);

        var calendar = new TeacherCalendar(_timeProvider);
        return calendar.FindAvailableTimeSlots(meetingDay,
                reservedTimeSlots.Select(slot => new TimeRange(slot.Start, slot.End)).ToList())
            .ToArray();
    }

    public IEnumerable<DateTime> GetAvailableMeetingDates()
    {
        return Enumerable.Range(0, MaxAppointmentDaysToSelect).Select(i => _timeProvider.Now.AddDays(i))
            .Where(time => time.DayOfWeek != DayOfWeek.Sunday && time.DayOfWeek != DayOfWeek.Saturday);
    }

    public async Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(CancellationToken ct = default)
    {
        return await _teacherRepository.ListAsync(ct);
    }
}