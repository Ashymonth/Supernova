using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SupernovaSchool.Telegram.Middlewares;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments.Steps;
using SupernovaSchool.Telegram.Workflows.RegisterStudent.Steps;
using Telegram.Bot;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.LifeCycleEvents;

namespace SupernovaSchool.Telegram.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTelegramBot(this IServiceCollection services, string botToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(botToken);
        
        services.AddWorkflow();
        services.AddWorkflowMiddleware<DiagnosticMiddleware>();
        services.AddWorkflowStepMiddleware<StepDurationMiddleware>();

        services.AddTransient<SendMessageToUser>();
        services.AddTransient<SendMessageWithOptionsToUser>();
        services.AddTransient<SendInitialMessageToUserStep>();
        services.AddTransient<SendInlineDataMessageToUser>();
        services.AddTransient<CleanupStep>();

        services.AddTransient<SendTeacherListStep>();
        services.AddTransient<SendAvailableTimeSlotsStep>();
        services.AddTransient<RegisterStudentStep>();

        services.AddTransient<EnsureThatStudentRegisteredStep>();
        services.AddTransient<EnsureThatUserDosentRegisteredOnMeeting>();
        services.AddTransient<LoadAvailableMeetingDays>();
        services.AddTransient<CreateMeetingStep>();

        services.AddTransient<LoadMyAppointmentsStep>();
        services.AddTransient<SendAppointmentToDelete>();
        services.AddTransient<DeleteAppointmentStep>();

        services.AddTransient<EnsureThatUserIsAdminStep>();
        services.AddTransient<CreateTeacherStep>();

        services.AddSingleton<IConversationHistory, ConversationHistory>();
        services.AddSingleton<IUserSessionStorage, UserSessionStorage>();
        services.AddSingleton<CommandRegistry>();

        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        services.AddHostedService<TelegramHostedService>();
    }
}