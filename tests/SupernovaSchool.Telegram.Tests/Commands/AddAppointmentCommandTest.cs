using Ical.Net.CalendarComponents;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using Xunit.Extensions.Ordering;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Telegram.Tests.Commands;

[Collection("CommandsCollection"), Order(4)]
public class AddAppointmentCommandTest : BaseCommandTest
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IAppointmentService> _appointmentServiceMock = new();
    private readonly Mock<IEventService> _eventServiceMock = new();


    [Fact, Order(1)]
    public async Task
        CreateAppointmentAsync_WhenTeacherHasAppointmentSlotAndStudentDoesntHaveAppointment_ShouldCreateAppointment()
    {
        const string selectedTeacherIndex = "0";
        var selectedDate = DateTime.Parse("2024.10.14");
        TimeRange[] availableAppointmentSlots =
        [
            new(new TimeOnly(9, 00), new TimeOnly(9, 30)),
            new(new TimeOnly(9, 30), new TimeOnly(10, 00))
        ];

        var selectedAppointmentSlot = availableAppointmentSlots[0];
        var appointmentStartDate = selectedDate.Add(selectedAppointmentSlot.Start.ToTimeSpan());
        var appointmentEndDate = selectedDate.Add(selectedAppointmentSlot.End.ToTimeSpan());

        var teachers = new List<Teacher>();
        var student = new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" };

        var webApp = new WebAppFactoryBuilder()
            .WithReplacedService(_eventServiceMock.Object)
            .WithReplacedService(_dateTimeProviderMock.Object)
            .WithStudent(new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" })
            .WithTeachers(provider =>
            {
                var passwordProtector = provider.GetRequiredService<IPasswordProtector>();
                teachers.AddRange([
                    Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
                    Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
                ]);

                teachers = teachers.OrderBy(teacher => teacher.Name).ToList();

                return teachers;
            });

        await InitializeAsync(webApp);

        var selectedTeacher = teachers[0];

        SetupStudentAppointment(selectedDate, []);

        _eventServiceMock.Setup(service => service.GetEventsAsync(
                It.Is<Teacher>(teacher => teacher.Id == selectedTeacher.Id), selectedDate,
                selectedDate.AddHours(23).AddMinutes(59), It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(() => Times.Exactly(2));

        _eventServiceMock.Setup(service =>
                service.CreateEventAsync(It.Is<Teacher>(teacher => teacher.Id == selectedTeacher.Id),
                    It.Is<Appointment>(appointment =>
                        appointment.StartDate == appointmentStartDate &&
                        appointment.EndDate == appointmentEndDate &&
                        appointment.Summary == student.CreateAppointmentSummary()),
                    Config.SenderId.ToString(), It.IsAny<CancellationToken>()))
            .Verifiable(Times.Once);

        _dateTimeProviderMock.Setup(provider => provider.Now).Returns(selectedDate);

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.InitialMessage,
            CreateAppointmentStepMessage.CreateChooseTeacherMessage(teachers),
            CreateAppointmentStepMessage.SelectAppointmentDay,
            DefaultStepMessage.ProcessingRequest,
            CreateAppointmentStepMessage.SelectTimeSlot,
            DefaultStepMessage.ProcessingRequest,
            CreateAppointmentStepMessage.CreateSuccessMessage(teachers[int.Parse(selectedTeacherIndex)].Name,
                selectedDate, CreateAppointmentStepMessage.CreateTimeSlotMessage(availableAppointmentSlots[0]))
        ]);


        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);
        await SendUpdate(selectedTeacherIndex);
        await SendUpdate(selectedDate.ToShortDateString());
        await SendUpdate(CreateAppointmentStepMessage.CreateTimeSlotMessage(availableAppointmentSlots[0]));

        Assert.True(expectedMessagesInOrder.Count == 0);
        _eventServiceMock.VerifyAll();
    }

    [Fact, Order(2)]
    public async Task CreateAppointmentAsync_WhenStudentNotRegistered_ShouldReturnErrorMessage()
    {
        var userIsNotRegisteredFactory = new WebAppFactory();

        await InitializeAsync(userIsNotRegisteredFactory);

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.UserNotRegistered,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    [Fact, Order(3)]
    public async Task CreateAppointmentAsync_WhenForDayNoTimeSlots_ShouldReturnErrorMessage()
    {
        var selectedDate = DateTime.Parse("2024.10.14");

        var teachers = new List<Teacher>();

        var webApp = new WebAppFactoryBuilder()
            .WithReplacedService(_appointmentServiceMock.Object)
            .WithReplacedService(_dateTimeProviderMock.Object)
            .WithStudent(new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" })
            .WithTeachers(provider =>
            {
                var passwordProtector = provider.GetRequiredService<IPasswordProtector>();
                teachers.AddRange([
                    Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
                    Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
                ]);

                return teachers;
            });

        await InitializeAsync(webApp);

        _appointmentServiceMock.Setup(service =>
                service.FindTeacherAvailableAppointmentSlotsAsync(teachers[0].Id, selectedDate,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        SetupStudentAppointment(selectedDate, []);

        await AssertThatErrorMessageWillBeSend(teachers, CreateAppointmentStepMessage.NoAvailableTimeSlots,
            selectedDate);
    }

    [Fact, Order(4)]
    public async Task CreateAppointmentAsync_WhenStudentHasAppointmentOnThisDate_ShouldReturnErrorMessage()
    {
        var selectedDate = DateTime.Parse("2024.10.14");

        var teachers = new List<Teacher>();

        var webApp = new WebAppFactoryBuilder()
            .WithReplacedService(_appointmentServiceMock.Object)
            .WithReplacedService(_dateTimeProviderMock.Object)
            .WithStudent(new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" })
            .WithTeachers(provider =>
            {
                var passwordProtector = provider.GetRequiredService<IPasswordProtector>();
                teachers.AddRange([
                    Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
                    Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
                ]);

                return teachers;
            });

        SetupStudentAppointment(selectedDate,
            [new StudentAppointmentInfo { DueDate = selectedDate, EventId = "test", TeacherName = "test" }]);

        await InitializeAsync(webApp);

        await AssertThatErrorMessageWillBeSend(teachers,
            CreateAppointmentStepMessage.AlreadyHaveAppointmentOnSelectedDay, selectedDate);
    }

    private async Task AssertThatErrorMessageWillBeSend(List<Teacher> teachers, string errorMessage,
        DateTime selectedDate)
    {
        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.InitialMessage,
            CreateAppointmentStepMessage.CreateChooseTeacherMessage(teachers),
            CreateAppointmentStepMessage.SelectAppointmentDay,
            DefaultStepMessage.ProcessingRequest,
            errorMessage
        ]);

        const string selectedTeacherIndex = "0";
        _dateTimeProviderMock.Setup(provider => provider.Now).Returns(selectedDate);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);
        await SendUpdate(selectedTeacherIndex);
        await SendUpdate(selectedDate.ToShortDateString());

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    protected override bool IsFinalUpdateInStep(string message)
    {
        return message != CreateAppointmentStepMessage.InitialMessage.Replace("\r\n", "\n") &&
               message != DefaultStepMessage.ProcessingRequest;
    }

    private void SetupStudentAppointment(DateTime selectedDate, List<StudentAppointmentInfo> appointmentInfos)
    {
        _appointmentServiceMock.Setup(service => service.GetStudentAppointmentsAsync(selectedDate,
                selectedDate.AddHours(23).AddMinutes(59), Config.SenderId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentInfos);
    }
}