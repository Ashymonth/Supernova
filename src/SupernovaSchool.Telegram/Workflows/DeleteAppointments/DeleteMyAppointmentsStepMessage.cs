namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments;

public static class DeleteMyAppointmentsStepMessage
{
    private const string SuccessMessageTemplate = "Запись на {0}-{1} число отменена.";
    
    public const string DontHaveAnyAppointments = "У вас нет ни одной активной записи";
    public const string SelectAppointmentToDelete = "Выберите запись для удаления";

    public static string CreateSuccessMessage(DateTime appointmentDate)
    {
        return string.Format(SuccessMessageTemplate, appointmentDate.ToShortDateString(),
            appointmentDate.ToShortTimeString());
    }
}