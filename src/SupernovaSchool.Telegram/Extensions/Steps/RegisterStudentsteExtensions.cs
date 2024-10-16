using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using SupernovaSchool.Telegram.Workflows.RegisterStudent.Steps;

namespace SupernovaSchool.Telegram.Extensions.Steps;

public static class RegisterStudentStepExtensions
{
    public static void AddRegisterStudentSteps(this IServiceCollection services)
    {
        services.AddTransient<LoadTeachersStep>();
        services.AddTransient<LoadTeacherAvailableTimeSlotsStep>();
        services.AddTransient<RegisterStudentStep>();

    }
}