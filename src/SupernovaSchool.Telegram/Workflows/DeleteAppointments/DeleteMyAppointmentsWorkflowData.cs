using System.Globalization;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments;

public class DeleteMyAppointmentsWorkflowData : IUserStep
{
    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointmentInfo { get; set; } = [];

    public DateTime AppointmentDateToDelete { get; set; }
    public string UserId { get; set; } = null!;

    public InlineKeyboardButton[] CreateButtonsToDeleteAppointment()
    {
        return StudentAppointmentInfo.Select(info =>
            InlineKeyboardButton.WithCallbackData($"{info.TeacherName} - {info.DueDate.ToShortDateString()}",
                info.DueDate.ToString(CultureInfo.CreateSpecificCulture("ru-Ru")))).ToArray();
    }
}