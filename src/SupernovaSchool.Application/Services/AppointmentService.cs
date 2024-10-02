using Ical.Net.CalendarComponents;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;
using SupernovaSchool.Specifications;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Application.Services;

public class AppointmentService : IAppointmentService
{
    private static readonly TimeZoneInfo MoscowTimeZone =
        Environment.OSVersion.Platform == PlatformID.Win32Windows
            ? TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    private readonly IYandexCalendarClient _calendarClient;
    private readonly IRepository<Teacher> _teacherRepository;
    private readonly IRepository<Student> _studentRepository;

    public AppointmentService(IYandexCalendarClient calendarClient, IRepository<Teacher> teacherRepository,
        IRepository<Student> studentRepository)
    {
        _calendarClient = calendarClient;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
    }

    public async Task CreateAppointment(Guid teacherId,
        string studentId,
        DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        var teacher = await GetTeacherOrThrowAsync(teacherId, ct);

        var student = await _studentRepository.FirstOrDefaultAsync(new StudentByIdSpecification(studentId), ct);
        if (student is null)
        {
            throw new ArgumentNullException();
        }

        var appointment = new Appointment(student.CreateAppointmentSummary(), startDate, endDate);

        var userInfo = new UserCredentials(teacher.Email, teacher.YandexCalendarPassword);

        var defaultCalendarUrl = await GetDefaultCalendarUrl(teacherId, ct);

        await _calendarClient.EventsResource.AddEventAsync(userInfo, appointment, defaultCalendarUrl, studentId, ct);
    }

    public async Task<IReadOnlyCollection<StudentAppointmentInfo>> GetStudentAppointmentsAsync(DateOnly day,
        string userId,
        CancellationToken ct = default)
    {
        var teachers = await _teacherRepository.ListAsync(ct);

        var result = new List<StudentAppointmentInfo>();
        foreach (var teacher in teachers)
        {
            var events = await GetEventsAsync(day, teacher, ct);

            result.AddRange(events.Where(@event => @event.Description == userId).Select(@event =>
                new StudentAppointmentInfo
                {
                    EventId = @event.Uid,
                    TeacherName = teacher.Name,
                    DueDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(@event.DtStart.AsUtc, MoscowTimeZone.Id)
                }));
        }

        return result;
    }

    public async Task<bool> DeleteStudentAppointmentAsync(DateTime appointmentDay, string userId,
        CancellationToken ct = default)
    {
        var teachers = await _teacherRepository.ListAsync(ct);
        foreach (var teacher in teachers)
        {
            var defaultCalendarUrl = await GetDefaultCalendarUrl(teacher.Id, ct);
            var userInfo = GetCredentials(teacher);

            var events = await _calendarClient.EventsResource.GetEventsAsync(userInfo, appointmentDay.Date,
                    appointmentDay.Date.Add(new TimeSpan(23, 59, 0)),
                defaultCalendarUrl,
                ct);

            var userEvent = events.FirstOrDefault(@event => @event.Description == userId);
            if (userEvent is null)
            {
                continue;
            }

            await _calendarClient.EventsResource.DeleteEventAsync(GetCredentials(teacher), defaultCalendarUrl,
                userEvent.Uid, ct);
        }

        return false;
    }

    public async Task<bool> IsStudentHasAppointmentForDateAsync(DateOnly date, string userId,
        CancellationToken ct = default)
    {
        var teachers = await _teacherRepository.ListAsync(ct);

        foreach (var teacher in teachers)
        {
            var events = await GetEventsAsync(date, teacher, ct);

            // we have an agreement that in the description will be only a user id
            if (events.Any(@event => @event.Description == userId))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<IEnumerable<TimeSlot>> GetAppointmentsDatesAsync(Guid teacherId, DateTime from, DateTime to,
        CancellationToken ct = default)
    {
        var defaultCalendarUrl = await GetDefaultCalendarUrl(teacherId, ct);
        var teacher = await GetTeacherOrThrowAsync(teacherId, ct);
        var userInfo = GetCredentials(teacher);

        var events = await _calendarClient.EventsResource.GetEventsAsync(userInfo, from, to, defaultCalendarUrl, ct);

        return events.Select(@event =>
        {
            var start = TimeZoneInfo.ConvertTime(@event.DtStart.AsUtc, MoscowTimeZone);
            var end = TimeZoneInfo.ConvertTime(@event.DtEnd.AsUtc, MoscowTimeZone);
            return new TimeSlot(TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end));
        });
    }

    private async Task<string> GetDefaultCalendarUrl(Guid teacherId, CancellationToken ct)
    {
        var teacher = await GetTeacherOrThrowAsync(teacherId, ct);

        var userInfo = new UserCredentials(teacher.Email, teacher.YandexCalendarPassword);
        var defaultCalendarUrl = await _calendarClient.CalendarResource.GetDefaultCalendarUrl(userInfo, ct);

        if (defaultCalendarUrl is null)
        {
            throw new ArgumentNullException();
        }

        return defaultCalendarUrl;
    }

    private static UserCredentials GetCredentials(Teacher teacher)
    {
        var userInfo = new UserCredentials(teacher.Email, teacher.YandexCalendarPassword);

        return userInfo;
    }

    private async Task<Teacher> GetTeacherOrThrowAsync(Guid teacherId, CancellationToken ct)
    {
        var teacher = await _teacherRepository.FirstOrDefaultAsync(new TeacherByIdSpecification(teacherId), ct);

        if (teacher is null)
        {
            throw new InvalidOperationException();
        }

        return teacher;
    }

    private async Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateOnly date, Teacher teacher, CancellationToken ct)
    {
        var defaultCalendarUrl = await GetDefaultCalendarUrl(teacher.Id, ct);
        var userInfo = GetCredentials(teacher);

        var events = await _calendarClient.EventsResource.GetEventsAsync(userInfo, date.ToDateTime(new TimeOnly()),
            date.ToDateTime(new TimeOnly(23, 59)), defaultCalendarUrl, ct);
        return events;
    }
}