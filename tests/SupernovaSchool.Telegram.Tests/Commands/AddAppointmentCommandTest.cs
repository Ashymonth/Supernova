using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Telegram.Tests.Commands;

[Collection("CommandsCollection"), Order(4)]
public class AddAppointmentCommandTest : BaseCommandTest
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IAppointmentService> _appointmentServiceMock = new();

    [Fact, Order(1)]
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

    [Fact, Order(2)]
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

        SetupAppointment(selectedDate, []);

        await AssertThatErrorMessageWillBeSend(teachers, CreateAppointmentStepMessage.NoAvailableTimeSlots, selectedDate);
    }

    [Fact, Order(3)]
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

        SetupAppointment(selectedDate,
            [new StudentAppointmentInfo { DueDate = selectedDate, EventId = "test", TeacherName = "test" }]);

        await InitializeAsync(webApp);

        await AssertThatErrorMessageWillBeSend(teachers, CreateAppointmentStepMessage.AlreadyHaveAppointmentOnSelectedDay, selectedDate);
    }

    private async Task AssertThatErrorMessageWillBeSend(List<Teacher> teachers, string errorMessage, DateTime selectedDate)
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

    private void SetupAppointment(DateTime selectedDate, List<StudentAppointmentInfo> appointmentInfos)
    {
        _appointmentServiceMock.Setup(service => service.GetStudentAppointmentsAsync(selectedDate,
                selectedDate.AddHours(23).AddMinutes(59), Config.SenderId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentInfos);
    }
}