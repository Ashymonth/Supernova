using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;
using SupernovaSchool.Specifications;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Application.Services;

public class AppointmentService : IAppointmentService
{
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

        await _calendarClient.EventsResource.AddEventAsync(userInfo, appointment, defaultCalendarUrl, ct);
    }

    public async Task<IEnumerable<TimeSlot>> GetAppointmentsAsync(Guid teacherId, DateTime from, DateTime to,
        CancellationToken ct = default)
    {
        var defaultCalendarUrl = await GetDefaultCalendarUrl(teacherId, ct);
        var teacher = await GetTeacherOrThrowAsync(teacherId, ct);
        var userInfo = GetCredentials(teacher);

        return await _calendarClient.EventsResource.GetEventsAsync(userInfo, from, to, defaultCalendarUrl, ct);
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
}