using SupernovaSchool.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment;

public static class CreateAppointmentStepMessage
{
    private const string SuccessMessageTemplate = """
                                                  Вы успешно записаны. Данные записи:
                                                  Сотрудник - {0}
                                                  День - {1}
                                                  Время - {2}
                                                  """;

    public static readonly string InitialMessage = DefaultStepMessage.CreateInitialMessage("Следуйте указаным шагам.");

    public const string UserNotRegistered =
        $"Сначала вы должна зарегистрироваться с помощью команды {Commands.RegisterAsStudentCommand}";

    public const string ChooseTeacherFromListTemplate = """
                                                        Для записи выберите сотрудника из списка.
                                                        {0}"
                                                        """;

    public const string SelectAppointmentDay = "Выберите дату для записи";

    public const string NoAvailableTimeSlots =
        "На выбранный день нет доступх мест для записи. Выберите другой день или другого сотрудника.";

    public const string AlreadyHaveAppointmentOnSelectedDay =
        "У вас уже есть запись на этот день. На 1 день можно записать не больше 1 раза";

    public const string SelectTimeSlot = "Выберите время для записи.";

    public static string CreateTimeSlotMessage(TimeRange timeRange)
    {
        return $"{timeRange.Start} - {timeRange.End}";
    }

    public static string CreateChooseTeacherMessage(IReadOnlyCollection<Teacher> teachers)
    {
        var teachersList = string.Join("\n", teachers.Select((teacher, index) => $"{index}. {teacher.Name}"));
      
        return string.Format(ChooseTeacherFromListTemplate, teachersList);
    }

    public static string CreateSuccessMessage(string teacherName, DateTime day, string time) =>
        string.Format(SuccessMessageTemplate, teacherName, day.ToShortDateString(), time);
}