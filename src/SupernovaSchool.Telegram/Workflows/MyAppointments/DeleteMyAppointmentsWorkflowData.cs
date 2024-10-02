using System.Globalization;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupernovaSchool.Telegram.Workflows.MyAppointments;

public class DeleteMyAppointmentsWorkflowData : IUserStep
{
    public string UserId { get; set; } = null!;

    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointmentInfo { get; set; } = [];

    public DateTime AppointmentDateToDelete { get; set; }

    public InlineKeyboardButton[] CreateButtonsToDeleteAppointment()
    {
        return StudentAppointmentInfo.Select(info =>
            InlineKeyboardButton.WithCallbackData($"{info.TeacherName} - {info.DueDate.ToShortDateString()}",
                info.DueDate.ToString(CultureInfo.CreateSpecificCulture("ru-Ru")))).ToArray();
    }
}