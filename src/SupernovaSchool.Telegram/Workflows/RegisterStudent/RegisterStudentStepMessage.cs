namespace SupernovaSchool.Telegram.Workflows.RegisterStudent;

public static class RegisterStudentStepMessage
{
    public const string CommandStartMessage = "Для того, чтобы записаться к психологу, вам нужно указать свои данные.";
    public const string InputName = "Введите имя";
    public const string InputClass = "Укажите ваш поток";

    private const string SuccessResultTemplate =
        "Вы успешно зарегистрировались. Теперь вы можете записаться к психологу.\nВаши данные:{0}-{1}";

    public static string CreateSuccessMessage(string studentName, string studentClass)
    {
        return string.Format(SuccessResultTemplate, studentName, studentClass);
    }
}