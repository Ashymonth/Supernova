using Microsoft.EntityFrameworkCore;
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
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.MyAppointments;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using WorkflowCore.Interface;
using YandexCalendar.Net.Extensions;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

var builder = WebApplication.CreateBuilder();

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

var app = builder.Build();

var workflow = app.Services.GetRequiredService<IWorkflowHost>();

workflow.RegisterWorkflow<CreateAppointmentWorkflow, CreateAppointmentWorkflowData>();
workflow.RegisterWorkflow<RegisterStudentWorkflow, RegisterStudentWorkflowData>();
workflow.RegisterWorkflow<DeleteMyAppointmentsWorkflow, DeleteMyAppointmentsWorkflowData>();
workflow.RegisterWorkflow<CreateTeacherWorkflow, CreateTeacherWorkflowData>();
workflow.Start();

app.Run();