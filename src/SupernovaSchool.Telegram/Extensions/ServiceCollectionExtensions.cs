using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Telegram.BackgroundServices;
using SupernovaSchool.Telegram.Extensions.Steps;
using SupernovaSchool.Telegram.Middlewares;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments.Steps;
using SupernovaSchool.Telegram.Workflows.DeleteTeacher.Steps;
using Telegram.Bot;

namespace SupernovaSchool.Telegram.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTelegramBot(this IServiceCollection services, Func<IServiceProvider,string> botTokenFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(botTokenFactory);
        
        services.AddWorkflow();
        services.AddWorkflowMiddleware<DiagnosticMiddleware>();
        services.AddWorkflowStepMiddleware<StepDurationMiddleware>();
        services.AddWorkflowStepMiddleware<StepDurationMiddleware>();

        services.AddBaseSteps();
        services.AddRegisterStudentSteps();

        services.AddTransient<EnsureThatStudentRegisteredStep>();
        services.AddTransient<EnsureThatUserDosentRegisteredOnMeeting>();
        services.AddTransient<SelectTeacherByIndexStep>();
        services.AddTransient<LoadAvailableMeetingDays>();
        services.AddTransient<CreateMeetingStep>();

        services.AddTransient<LoadMyAppointmentsStep>();
        services.AddTransient<SendAppointmentToDelete>();
        services.AddTransient<DeleteAppointmentStep>();

        services.AddTransient<EnsureThatUserIsAdminStep>();
        services.AddTransient<CreateTeacherStep>();
        services.AddTransient<DeleteTeacherStep>();

        services.AddSingleton<IConversationHistory, ConversationHistory>();
        services.AddSingleton<IUserSessionStorage, UserSessionStorage>();
        services.AddSingleton<CommandRegistry>();

        services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(botTokenFactory(provider)));
        services.AddSingleton<ICommandUploader, CommandUploader>();
        services.AddHostedService<TelegramBackgroundService>();
    }
}