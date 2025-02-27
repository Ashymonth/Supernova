using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.DeleteTeacher;
using SupernovaSchool.Tests.Fixtures;
using Xunit.Extensions.Ordering;

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
        var webApp = _appFactoryBuilder.WithAdditionalConfiguration(builder =>
            builder.AddJsonFile("appsettings-without-admins.json")).Build();

        await InitializeAsync(webApp);

        var expectedMessagesInOrder = new Queue<string>([
            CreateTeacherStepMessage.NotEnoughRightToCreateATeacher,
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.CreateTeacherCommand);

        Assert.Empty(expectedMessagesInOrder);
    }
    
    [Fact, Order(2)]
    public async Task DeleteTeacherTest_WhenUserIsAnAdmin_TeacherExists_ShouldDeleteTeacher()
    {
        var existedTeachers = new List<Teacher>();
        var webApp = _appFactoryBuilder.WithTeachers(provider =>
        {
            var passwordProtector = provider.GetRequiredService<IPasswordProtector>();
            var teachers = new List<Teacher>([
                Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
                Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
            ]);
            
            existedTeachers.AddRange(teachers);

            return teachers;
        });

        await InitializeAsync(webApp.Build());

        var expectedMessagesInOrder = new Queue<string>([
            DeleteTeacherStepMessage.SelectTeacherToDeleteMessage(existedTeachers),
            DeleteTeacherStepMessage.TeacherDeletedMessage
        ]);

        SubscribeOnUpdates(expectedMessagesInOrder);

        await SendUpdate(Telegram.Commands.DeleteTeacherCommand);

        Assert.Empty(expectedMessagesInOrder);
    }

}