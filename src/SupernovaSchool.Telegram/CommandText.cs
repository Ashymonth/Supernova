namespace SupernovaSchool.Telegram;

public class CommandText
{
    public const string StartCommandMessage = $"""
                                        Привет! Я бот, с помощью которого можно удобно записаться к психологу.
                                        Первым делом тебе нужно зарегистрироваться, чтобы психолог мог видеть, кто к нему записался.
                                        Сделать это можно с помощью команды: {Commands.RegisterAsStudentCommand}.
                                        После регистрации ты сможешь записаться с помощью команды: {Commands.CreateAppointmentCommand}.
                                        Важно: На 1 день доступна только 1 запись. 
                                        Если ты передумал или захотел перенести запись, то с помощью команды: {Commands.DeleteAppointmentCommand} ты сможешь отменить свою запись.
                                        Любую команду можно преравть, если ты напишешь 'Выйти'
                                        """;

    public const string CommandCanceled = "Команда отменена";
    public const string CannotInvokeCommandUntilActiveCommandExist = "Нельзя вызвать новую команду, пока вы не завершили старую";
}