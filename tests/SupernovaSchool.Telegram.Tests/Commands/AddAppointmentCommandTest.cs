using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Data;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Tests.Fixtures;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using Xunit.Extensions.Ordering;

namespace SupernovaSchool.Telegram.Tests.Commands;

[Collection("CommandsCollection"), Order(4)]
public class AddAppointmentCommandTest : BaseCommandTest
{
    [Fact, Order(1)]
    public async Task CreateAppointmentAsync_WhenStudentNotRegistered_ShouldReturnErrorMessage()
    {
        var mock = new Mock<IStudentService>();

        var userIsNotRegisteredFactory = new WebAppFactoryWhenUserIsNotRegistered(mock);

        await InitializeAsync(userIsNotRegisteredFactory);

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.UserNotRegistered,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);

        mock.VerifyAll();

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    [Fact, Order(2)]
    public async Task CreateAppointmentAsync_WhenForDayNoTimeSlots_ShouldReturnErrorMessage()
    {
        const string selectedTeacherIndex = "0";
        var dateWithoutAvailableTimeSlots = DateTime.Parse("2024.10.14");

        var appointmentServiceMock = new Mock<IAppointmentService>();
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var studentServiceMock = new Mock<IStudentService>();

        var factory =
            new WepAppFactoryWithTeachersWithoutAvailableTimeSlots(appointmentServiceMock, dateTimeProviderMock,
                studentServiceMock);

        await InitializeAsync(factory);

        var passwordProtector = factory.Services.GetRequiredService<IPasswordProtector>();
        List<Teacher> teachers =
        [
            Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
            Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
        ];

        await SeedTeachers(factory, teachers);

        dateTimeProviderMock.Setup(provider => provider.Now).Returns(dateWithoutAvailableTimeSlots);

        studentServiceMock.Setup(studentService =>
                studentService.GetStudentAsync(It.Is<string>(userId => userId == Config.SenderId.ToString()),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" });

        appointmentServiceMock.Setup(service =>
                service.FindTeacherAvailableAppointmentSlotsAsync(teachers[int.Parse(selectedTeacherIndex)].Id,
                    dateWithoutAvailableTimeSlots, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        appointmentServiceMock.Setup(service => service.GetStudentAppointmentsAsync(dateWithoutAvailableTimeSlots,
                dateWithoutAvailableTimeSlots.AddHours(23).AddMinutes(59), Config.SenderId.ToString(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.InitialMessage,
            CreateAppointmentStepMessage.CreateChooseTeacherMessage(teachers),
            CreateAppointmentStepMessage.SelectAppointmentDay,
            DefaultStepMessage.ProcessingRequest,
            CreateAppointmentStepMessage.NoAvailableTimeSlots
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);
        await SendUpdate(selectedTeacherIndex);
        await SendUpdate(dateWithoutAvailableTimeSlots.ToShortDateString());

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    [Fact, Order(3)]
    public async Task CreateAppointmentAsync_WhenStudentHasAppouintmentOnThisDate_ShouldReturnErrorMessage()
    {
        const string selectedTeacherIndex = "0";
        var selectedDate = DateTime.Parse("2024.10.14");

        var appointmentServiceMock = new Mock<IAppointmentService>();
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        var studentServiceMock = new Mock<IStudentService>();

        var factory =
            new WepAppFactoryWithTeachersWithoutAvailableTimeSlots(appointmentServiceMock, dateTimeProviderMock,
                studentServiceMock);

        await InitializeAsync(factory);

        var passwordProtector = factory.Services.GetRequiredService<IPasswordProtector>();
        List<Teacher> teachers =
        [
            Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
            Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
        ];

        await SeedTeachers(factory, teachers);

        dateTimeProviderMock.Setup(provider => provider.Now).Returns(selectedDate);

        studentServiceMock.Setup(studentService =>
                studentService.GetStudentAsync(It.Is<string>(userId => userId == Config.SenderId.ToString()),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" });

        appointmentServiceMock.Setup(service => service.GetStudentAppointmentsAsync(selectedDate,
                selectedDate.AddHours(23).AddMinutes(59), Config.SenderId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StudentAppointmentInfo>
                { new() { DueDate = selectedDate, EventId = "test", TeacherName = "test" } });

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.InitialMessage,
            CreateAppointmentStepMessage.CreateChooseTeacherMessage(teachers),
            CreateAppointmentStepMessage.SelectAppointmentDay,
            DefaultStepMessage.ProcessingRequest,
            CreateAppointmentStepMessage.AlreadyHaveAppointmentOnSelectedDay
        ]);

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

    private static async Task SeedTeachers(WepAppFactoryWithTeachersWithoutAvailableTimeSlots factory,
        List<Teacher> teachers)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupernovaSchoolDbContext>();

        await db.Teachers.AddRangeAsync(teachers);
        await db.SaveChangesAsync();
    }
}