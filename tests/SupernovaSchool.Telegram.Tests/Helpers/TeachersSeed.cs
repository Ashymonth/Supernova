using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Models;

namespace SupernovaSchool.Telegram.Tests.Helpers;

public class TeachersSeed
{
    public static List<Teacher> CreateTeachers(IServiceProvider services)
    {
        var passwordProtector = services.GetRequiredService<IPasswordProtector>();
        List<Teacher> teachers =
        [
            Teacher.Create("teacher 1", "login 1", Password.Create("123", passwordProtector)),
            Teacher.Create("teacher 2", "login 2", Password.Create("123", passwordProtector))
        ];
 
        return teachers;
    }
}