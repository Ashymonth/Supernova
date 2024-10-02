using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using SupernovaSchool.Telegram.Workflows.RegisterStudent.Steps;
using Telegram.Bot;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTelegramBot(this IServiceCollection services, string botToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(botToken);
        services.AddWorkflow();
        
        services.AddTransient<SendMessageToUser>();
        services.AddTransient<SendMessageWithOptionsToUser>();
        services.AddTransient<SendInitialMessageToUserStep>();
        
        services.AddTransient<SendTeacherListStep>();
        services.AddTransient<SendAvailableTimeSlotsStep>();
        services.AddTransient<RegisterStudentStep>();

        services.AddTransient<EnsureThatStudentRegisteredStep>();
        services.AddTransient<EnsureThatUserDosentRegisteredOnMeeting>();
        services.AddTransient<LoadAvailableMeetingDays>();
        services.AddTransient<CreateMeetingStep>();
        
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        services.AddHostedService<TelegramHostedService>();
    }
 
}