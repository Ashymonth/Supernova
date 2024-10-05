using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Application.Services;

public class AppointmentService : IAppointmentService
{
    private static readonly TimeZoneInfo MoscowTimeZone =
        Environment.OSVersion.Platform == PlatformID.Win32Windows
            ? TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    private static readonly TimeOnly WorkStartTime = new(8, 30);
    private static readonly TimeOnly WorkEndTime = new(18, 30);
    private static readonly TimeSpan SlotInterval = TimeSpan.FromMinutes(30);
    private readonly IEventService _eventService;
    private readonly ILogger<AppointmentService> _logger;

    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly IDateTimeProvider _timeProvider;

    public AppointmentService(IStudentService studentService, IEventService eventService,
        ITeacherService teacherService, IDateTimeProvider timeProvider, ILogger<AppointmentService> logger)
    {
        _studentService = studentService;
        _eventService = eventService;
        _teacherService = teacherService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task CreateAppointment(Guid teacherId,
        string studentId,
        DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        var teacher = await GetTeacherOrThrowAsync(teacherId, ct);

        var student = await _studentService.GetStudentAsync(studentId) ?? throw new ArgumentNullException();

        var appointment = new Appointment(student.CreateAppointmentSummary(), startDate, endDate);

        await _eventService.CreateEventAsync(teacher, appointment, studentId, ct);
    }

    public async Task<IReadOnlyCollection<StudentAppointmentInfo>> GetStudentAppointmentsAsync(DateTime from,
        DateTime to,
        string userId,
        CancellationToken ct = default)
    {
        var teachers = await _teacherService.GetTeachersAsync(ct);

        var result = new ConcurrentBag<StudentAppointmentInfo>();
        
        var resultTasks = teachers.Select(teacher =>
        {
            return _eventService.GetEventsAsync(teacher, from, to, ct)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogWarning(task.Exception, "Unable to get events for teacher: {Teacher} {EventDay}",
                            teacher.Name, to);
                        return;
                    }

                    foreach (var @event in task.Result.Where(@event => @event.Description == userId))
                    {
                        var appointmentInfo = new StudentAppointmentInfo
                        {
                            EventId = @event.Uid,
                            TeacherName = teacher.Name,
                            DueDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(@event.DtStart.AsUtc,
                                MoscowTimeZone.Id)
                        };

                        result.Add(appointmentInfo);
                    }
                }, ct);
        });

        await Task.WhenAll(resultTasks);
        
        return result;
    }

    public async Task DeleteStudentAppointmentAsync(DateTime appointmentDay, string userId,
        CancellationToken ct = default)
    {
        var teachers = await _teacherService.GetTeachersAsync(ct);

        var result = teachers.Select(teacher =>
        {
            return _eventService.GetEventsForDayAsync(teacher, DateOnly.FromDateTime(appointmentDay), ct)
                .ContinueWith(async task =>
                {
                    var userEvent = task.Result.FirstOrDefault(@event => @event.Description == userId);
                    if (userEvent is null) return;

                    await _eventService.DeleteEventAsync(teacher, userEvent.Uid, ct);
                }, ct);
        });

        await Task.WhenAll(result);
    }

    public async Task<TimeRange[]> FindTeacherAvailableAppointmentSlotsAsync(Guid teacherId, DateTime meetingDay,
        CancellationToken ct = default)
    {
        var reservedTimeSlots = await GetAppointmentsDatesAsync(teacherId, meetingDay.Date,
            meetingDay.Date.AddHours(23).AddMinutes(59), ct);

        return FindAvailableTimeSlots(meetingDay, reservedTimeSlots.ToList()).ToArray();
    }

    private async Task<IEnumerable<TimeSlot>> GetAppointmentsDatesAsync(Guid teacherId, DateTime from, DateTime to,
        CancellationToken ct = default)
    {
        var teacher = await GetTeacherOrThrowAsync(teacherId, ct);

        var events = await _eventService.GetEventsAsync(teacher, from, to, ct);

        return events.Select(@event =>
        {
            var start = TimeZoneInfo.ConvertTime(@event.DtStart.AsUtc, MoscowTimeZone);
            var end = TimeZoneInfo.ConvertTime(@event.DtEnd.AsUtc, MoscowTimeZone);
            return new TimeSlot(TimeOnly.FromDateTime(start), TimeOnly.FromDateTime(end));
        });
    }

    private async Task<Teacher> GetTeacherOrThrowAsync(Guid teacherId, CancellationToken ct)
    {
        var teacher = await _teacherService.GetTeacherAsync(teacherId, ct);

        if (teacher is null) throw new InvalidOperationException();

        return teacher;
    }

    private List<TimeRange> FindAvailableTimeSlots(DateTime meetingDay, List<TimeSlot> reservedSlots)
    {
        var sw = Stopwatch.GetTimestamp();
        // Sort reserved slots by their start time for easier comparison.
        reservedSlots.Sort((x, y) => x.Start.CompareTo(y.Start));

        var result = new List<TimeRange>();
        var today = _timeProvider.Now;

        var slotStart = WorkStartTime;

        // Iterate through each possible time slot within working hours.
        while (slotStart < WorkEndTime)
        {
            if (IsSlotAfterCurrentTime(meetingDay, slotStart, today) &&
                !IsSlotReserved(slotStart, reservedSlots))
                result.Add(new TimeRange(slotStart, slotStart.Add(SlotInterval)));

            slotStart = slotStart.Add(SlotInterval);
        }

        Console.WriteLine(Stopwatch.GetElapsedTime(sw));
        return result;
    }

    // Checks if the slot is after the current time.
    private static bool IsSlotAfterCurrentTime(DateTime meetingDay, TimeOnly slotStart, DateTime today)
    {
        if (meetingDay.Date < today.Date) return false;

        return !(meetingDay.Date == today.Date && slotStart.ToTimeSpan() <= today.TimeOfDay);
    }

    // Determines if a given slot is reserved.
    private static bool IsSlotReserved(TimeOnly slotStart, List<TimeSlot> reservedSlots)
    {
        // Iterate through the sorted reserved slots and check if the slot overlaps with any reserved slot.
        return reservedSlots.TakeWhile(reservedSlot => slotStart >= reservedSlot.Start).Any(reservedSlot =>
            slotStart >= reservedSlot.Start && slotStart < reservedSlot.End);
    }
}