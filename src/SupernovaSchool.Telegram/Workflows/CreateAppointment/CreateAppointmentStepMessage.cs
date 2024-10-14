namespace SupernovaSchool.Telegram.Workflows.CreateAppointment;

public static class CreateAppointmentStepMessage
{
    private const string SuccessMessageTemplate = """
                                                  Вы успешно записаны. Данные записи:
                                                  Сотрудник - {0}
                                                  День - {1}
                                                  Время - {2}
                                                  """;

    public const string UserNotRegistered =
        $"Сначала вы должна зарегистрироваться с помощью команды {Commands.RegisterAsStudentCommand}";

    public const string ChooseTeacherFromList = "Для записи выберите сотрудника из списка.";

    public const string NoAvailableTimeSlots =
        "На выбранный день нет доступх мест для записи. Выберите другой день или другого сотрудника.";

    public const string AlreadyHaveAppointmentOnSelectedDay =
        "У вас уже есть запись на этот день. На 1 день можно записать не больше 1 раза";

    public const string SelectTimeSlot = "Выберите время для записи.";

    public static string CreateSuccessMessage(string teacherName, string day, string time) =>
        string.Format(SuccessMessageTemplate, teacherName, day, time);
}