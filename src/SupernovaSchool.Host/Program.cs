using System.Globalization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Debugging;
using SupernovaSchool;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Application.Security;
using SupernovaSchool.Application.Services;
using SupernovaSchool.Data;
using SupernovaSchool.Data.Repositories;
using SupernovaSchool.Host;
using SupernovaSchool.Host.Configs;
using SupernovaSchool.Telegram;
using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Metrics;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkflowCore.Interface;
using YandexCalendar.Net.Extensions;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File("logs/bootstrap.log", formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting web host");

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.ConfigureOptions<SecurityConfigSetup>();
    builder.Services.ConfigureOptions<TelegramBotConfigSetup>();
    builder.Services.ConfigureTelegramBot<JsonOptions>(options => options.SerializerOptions);
    
    builder.Services.AddControllers();
    
    builder.AddServiceDefaults();
    
    builder.Host.UseSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));
    builder.Services.AddOpenTelemetry()
        .WithTracing()
        .WithMetrics(providerBuilder => providerBuilder
            .AddMeter(WorkflowStaterCounterMetric.MeterName)
            .AddMeter(StepDurationTimeMeter.MeterName)
            .AddPrometheusExporter()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation());

    builder.Services.AddDbContext<SupernovaSchoolDbContext>(optionsBuilder =>
        optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

    builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); 
    
    builder.Services.AddSingleton<IPasswordProtector, PasswordProtector>();
    builder.Services.AddSingleton<ISecurityKeyProvider>(provider =>
    {
        var config = provider.GetRequiredService<IOptions<SecurityConfig>>().Value;
        return new SecurityKeyProvider(config.SecretKey, config.InitVector);
    });

    builder.Services.AddSingleton<IAdminsProvider>(provider => new TelegramAdminsProvider(
        provider.GetRequiredService<IOptions<TelegramBotConfig>>().Value.AdminUserIdsFromTelegram));

    builder.Services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
    builder.Services.AddTransient<IAppointmentService, AppointmentService>();
    builder.Services.AddTransient<ITeacherService, TeacherService>();
    builder.Services.AddTransient<IStudentService, StudentService>();
    builder.Services.AddTransient<IEventService, EventService>();
    builder.Services.AddTransient<ICalendarService, CalendarService>();
    builder.Services.AddTransient<UpdateHandler>();

    builder.Services.AddMemoryCache();
    builder.Services.AddTelegramBot(provider => provider.GetRequiredService<IOptions<TelegramBotConfig>>().Value.Token);

    builder.Services.YandexCalendarClient();

    builder.Services.AddSingleton<WorkflowStaterCounterMetric>();
    builder.Services.AddSingleton<StepDurationTimeMeter>();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHostedService<BackgroundTelegramService>();
    }

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SupernovaSchoolDbContext>();
        db.Database.Migrate();
    }

    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    app.MapDefaultEndpoints();

    app.MapPost("updates", async (UpdateHandler handler, Update update, CancellationToken ct) =>
            TypedResults.Ok(await handler.HandleUpdateAsync(update, ct)));

    var botUrl = app.Services.GetRequiredService<IOptions<TelegramBotConfig>>().Value.WebHookUrl;
    var bot = app.Services.GetRequiredService<ITelegramBotClient>();
    await bot.SetWebhookAsync(string.Empty);
    await bot.SetWebhookAsync(botUrl + "/updates",
        allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery], dropPendingUpdates: true);

    var workflow = app.Services.GetRequiredService<IWorkflowHost>();

    workflow.RegisterWorkflow<CreateAppointmentWorkflow, CreateAppointmentWorkflowData>();
    workflow.RegisterWorkflow<RegisterStudentWorkflow, RegisterStudentWorkflowData>();
    workflow.RegisterWorkflow<DeleteMyAppointmentsWorkflow, DeleteMyAppointmentsWorkflowData>();
    workflow.RegisterWorkflow<CreateTeacherWorkflow, CreateTeacherWorkflowData>();
    workflow.Start();

    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

//need for integration tests
public partial class Program;