using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using Serilog;
using SupernovaSchool;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Application.Security;
using SupernovaSchool.Application.Services;
using SupernovaSchool.Data;
using SupernovaSchool.Data.Repositories;
using SupernovaSchool.Host;
using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Metrics;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using WorkflowCore.Interface;
using YandexCalendar.Net.Extensions;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File("logs/bootstrap.log", formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting web host");

var builder = WebApplication.CreateBuilder();

try
{
    Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
    builder.Services.AddControllers();

    builder.Host.UseSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));

    builder.AddServiceDefaults();

    builder.Services.AddOpenTelemetry()
        .WithTracing()
        .WithMetrics(providerBuilder => providerBuilder
            .AddMeter(WorkflowStaterCounterMetric.MeterName)
            .AddPrometheusExporter()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation());

    builder.Services.AddDbContext<SupernovaSchoolDbContext>(optionsBuilder =>
        optionsBuilder.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));

    builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    builder.Services.AddSingleton<IPasswordProtector, PasswordProtector>();
    builder.Services.AddSingleton<ISecurityKeyProvider>(_ =>
        new SecurityKeyProvider(builder.Configuration.GetValue<string>("SecurityConfig:SecretKey")!,
            builder.Configuration.GetValue<string>("SecurityConfig:InitVector")!));

    builder.Services.AddSingleton<IAdminsProvider>(_ => new TelegramAdminsProvider(
        builder.Configuration.GetSection("AdminUserIdsFromTelegram").Get<HashSet<string>>() ??
        throw new InvalidOperationException("Admin user ids are not provided")));

    builder.Services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
    builder.Services.AddTransient<IAppointmentService, AppointmentService>();
    builder.Services.AddTransient<ITeacherService, TeacherService>();
    builder.Services.AddTransient<IStudentService, StudentService>();
    builder.Services.AddTransient<IEventService, EventService>();
    builder.Services.AddTransient<ICalendarService, CalendarService>();

    builder.Services.AddMemoryCache();

    builder.Services.AddTelegramBot(builder.Configuration.GetValue<string>("Token")!);

    builder.Services.YandexCalendarClient();

    builder.Services.AddSingleton<WorkflowStaterCounterMetric>();

    var app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    app.MapControllers();

    app.UseOpenTelemetryPrometheusScrapingEndpoint("metrics");

    app.MapDefaultEndpoints();

    using (var scope = app.Services.CreateScope())
    {
        var workflow = scope.ServiceProvider.GetRequiredService<IWorkflowHost>();

        workflow.RegisterWorkflow<CreateAppointmentWorkflow, CreateAppointmentWorkflowData>();
        workflow.RegisterWorkflow<RegisterStudentWorkflow, RegisterStudentWorkflowData>();
        workflow.RegisterWorkflow<DeleteMyAppointmentsWorkflow, DeleteMyAppointmentsWorkflowData>();
        workflow.RegisterWorkflow<CreateTeacherWorkflow, CreateTeacherWorkflowData>();
        workflow.Start();
    }

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