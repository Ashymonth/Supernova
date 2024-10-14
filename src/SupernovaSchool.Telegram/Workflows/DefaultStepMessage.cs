namespace SupernovaSchool.Telegram.Workflows;

public static class DefaultStepMessage
{
    private const string InitialMessageTemplate = """
                                                  {0}
                                                  Для завершения команды введите 'Выйти'"
                                                  """;

    public const string ProcessingRequest = "Обработка запроса...";

    public static string CreateInitialMessage(string message)
    {
        return string.Format(InitialMessageTemplate, message);
    }
}