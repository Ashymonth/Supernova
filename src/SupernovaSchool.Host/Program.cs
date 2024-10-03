using Microsoft.EntityFrameworkCore;
using SupernovaSchool;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Application.Services;
using SupernovaSchool.Data;
using SupernovaSchool.Data.Repositories;
using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
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

using (var scope = app.Services.CreateScope())
{
    var workflow = scope.ServiceProvider.GetRequiredService<IWorkflowHost>();

    workflow.RegisterWorkflow<CreateAppointmentWorkflow, CreateAppointmentWorkflowData>();
    workflow.RegisterWorkflow<RegisterStudentWorkflow, RegisterStudentWorkflowData>();
    workflow.RegisterWorkflow<DeleteMyAppointmentsWorkflow, DeleteMyAppointmentsWorkflowData>();
    workflow.Start();
}

app.Run();