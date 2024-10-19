namespace SupernovaSchool.Telegram.Workflows.CreateTeacher;

public static class CreateTeacherStepMessage
{
    private const string SuccessMessageTemplate = """
                                                  Учитель успешно добавлен. Данные учителя:
                                                  ФИО: {0}
                                                  Логин в Яндекс.Календарь:{1}"
                                                  """;

    public const string NotEnoughRightToCreateATeacher = "У вас недостаточно прав для этой команды";
    public const string InputName = "Введите Фио";
    public const string InputLoginFromYandexCalendar = "Введите логин от яндекс календаря";

    public const string InputPasswordFromYandexCalendar = """
                                                          Введите пароль от яндекс календаря\n Как получить пароль: https://id.yandex.ru/security/app-passwords"
                                                          """;


    public static string CreateSuccessMessage(string teacherName, string loginFromYandexCalendar)
    {
        return string.Format(SuccessMessageTemplate, teacherName, loginFromYandexCalendar);
    }
}