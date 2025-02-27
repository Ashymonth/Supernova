using Microsoft.Extensions.DependencyInjection;
using Moq;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.DeleteTeacher;
using SupernovaSchool.Tests.Extensions;
using SupernovaSchool.Tests.Fixtures;
using TL;
using Xunit.Extensions.Ordering;
using ReplyKeyboardMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup;
using ReplyMarkup = Telegram.Bot.Types.ReplyMarkups.ReplyMarkup;

namespace SupernovaSchool.Tests.Commands;

[Collection("CommandsCollection"), Order(5)]
public class DeleteTeacherCommandTest : BaseCommandTest, IClassFixture<WebAppFactoryBuilder>
{
    private readonly WebAppFactoryBuilder _appFactoryBuilder;

    public DeleteTeacherCommandTest(WebAppFactoryBuilder appFactoryBuilder)
    {
        _appFactoryBuilder = appFactoryBuilder;
    }

    [Fact, Order(1)]
    public async Task CreateTeacherTest_WhenUserIsNotAnAdmin_ReturnErrorMessage()
    {
        var mock = new Mock<ITelegramBotClientWrapper>();
        mock.SetupSendMessage<ReplyMarkup>(Config.SenderId, CreateTeacherStepMessage.NotEnoughRightToCreateATeacher);

        var adminsMock = new Mock<IAdminsProvider>();
        adminsMock.Setup(provider => provider.IsAdmin(Config.SenderId.ToString()))
            .Returns(false)
            .Verifiable(Times.Once);

        var webApp = _appFactoryBuilder
            .WithReplacedService(mock.Object)
            .WithReplacedService(adminsMock.Object)
            .Build();

        await InitializeAsync(webApp);

        await SendUpdate(Telegram.Commands.DeleteTeacherCommand);

        await Task.Delay(500);

        mock.VerifyAll();
        adminsMock.VerifyAll();
    }

    [Fact, Order(2)]
    public async Task DeleteTeacherTest_WhenUserIsAnAdmin_TeacherExists_ShouldDeleteTeacher()
    {
        var tgMock = new Mock<ITelegramBotClientWrapper>();

        var adminsMock = new Mock<IAdminsProvider>();
        adminsMock.Setup(provider => provider.IsAdmin(Config.SenderId.ToString()))
            .Returns(true)
            .Verifiable(Times.Once);

        var existedTeachers = new List<Teacher>();
        var webApp = _appFactoryBuilder
            .WithTeachers(provider =>
            {
                var passwordProtector = provider.GetRequiredService<IPasswordProtector>();
                var teachers = new List<Teacher>([
                    Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
                    Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
                ]);

                existedTeachers.AddRange(teachers);

                tgMock.SetupSendMessage<ReplyKeyboardMarkup>(Config.SenderId,
                    DeleteTeacherStepMessage.SelectTeacherToDeleteMessage(teachers));
                return teachers;
            })
            .WithReplacedService(tgMock.Object)
            .WithReplacedService(adminsMock.Object);

        
        tgMock.SetupSendMessage(Config.SenderId, DeleteTeacherStepMessage.TeacherDeletedMessage);

        await InitializeAsync(webApp.Build());
 
        await SendUpdate(Telegram.Commands.DeleteTeacherCommand);

        await Task.Delay(500);
        
        await SendUpdate("0"); // teacher to delete index
        
        await Task.Delay(500);
        
        adminsMock.VerifyAll();
        
        tgMock.VerifyAll();
    }
}