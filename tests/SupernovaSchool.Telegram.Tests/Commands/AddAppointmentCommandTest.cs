using Microsoft.EntityFrameworkCore;
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
    [Fact]
    public async Task CreateAppointmentAsync_WhenStudentNotRegistered_ShouldReturnErrorMessage()
    {
        var mock = new Mock<IStudentService>();

        var userIsNotRegisteredFactory = new WebAppFactoryWhenUserIsNotRegistered(mock);

        await InitializeAsync(userIsNotRegisteredFactory);

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.ChooseTeacherFromListTemplate,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);

        mock.VerifyAll();

        Assert.True(expectedMessagesInOrder.Count == 0);
    }

    [Fact]
    public async Task CreateAppointmentAsync_WhenForDayNoTimeSlots_ShouldReturnErrorMessage()
    {
        const string selectedTeacherIndex = "0";
        const string dateWithoutAvailableTimeSlots = "2024.10.14";
        
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
        
        dateTimeProviderMock.Setup(provider => provider.Now).Returns(DateTime.Parse("2024.10.14"));
        
        studentServiceMock.Setup(studentService =>
                studentService.GetStudentAsync(It.Is<string>(userId => userId == Config.SenderId.ToString()),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = Config.SenderId.ToString(), Name = "Test student", Class = "7" });
        appointmentServiceMock.Setup(service =>
                service.FindTeacherAvailableAppointmentSlotsAsync(teachers[int.Parse(selectedTeacherIndex)].Id,
                    DateTime.Parse(dateWithoutAvailableTimeSlots), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var expectedMessagesInOrder = new Queue<string>([
            CreateAppointmentStepMessage.InitialMessage,
            CreateAppointmentStepMessage.CreateChooseTeacherMessage(teachers),
            CreateAppointmentStepMessage.SelectAppointmentDay,
            CreateAppointmentStepMessage.NoAvailableTimeSlots
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateAppointmentCommand);
        await SendUpdate(selectedTeacherIndex);
        await SendUpdate(dateWithoutAvailableTimeSlots);

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