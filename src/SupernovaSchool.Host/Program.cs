using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
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
using SupernovaSchool.Host.Configs;
using SupernovaSchool.Host.Extensions;
using SupernovaSchool.Telegram;
using SupernovaSchool.Telegram.Extensions;
using Telegram.Bot.Types;
using YandexCalendar.Net.Extensions;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File("logs/bootstrap.log", formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting web host");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddSerilogAndOpenTelemetry();
    builder.Services.ConfigureOptions<SecurityConfigSetup>();
    builder.Services.ConfigureOptions<TelegramBotConfigSetup>();
    builder.Services.ConfigureTelegramBotMvc();

    builder.Services.AddHealthChecks()
        .AddCheck("self", () => new HealthCheckResult(HealthStatus.Healthy))
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

    // Configure the Health Checks UI Client
    builder.Services.AddHealthChecksUI(options =>
    {
        options.SetEvaluationTimeInSeconds(10);
        options.MaximumHistoryEntriesPerEndpoint(50);
    }).AddInMemoryStorage();

    builder.Services.AddDbContext<SupernovaSchoolDbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

    var app = builder.Build();

    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    app.MapHealthChecks("/health");
    app.MapHealthChecksUI(options => options.ApiPath = "/health-ui");
    app.UseHealthChecksPrometheusExporter(new PathString("/health-prometheus"));
    
    app.MapPost("updates",
        async (UpdateHandler handler, Update update, CancellationToken ct) =>
            TypedResults.Ok(await handler.HandleUpdateAsync(update, ct)));

    app.UserWorkflowsAndStartHost();

    app.Run();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Log.Information(ex.Message);
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