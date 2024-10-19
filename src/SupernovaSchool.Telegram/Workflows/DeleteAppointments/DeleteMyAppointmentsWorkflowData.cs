using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;

namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments;

public class DeleteMyAppointmentsWorkflowData : IUserStep
{
    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointmentInfo { get; set; } = [];

    public DateTime AppointmentDateToDelete { get; set; }
    
    public string UserId { get; set; } = null!;
}