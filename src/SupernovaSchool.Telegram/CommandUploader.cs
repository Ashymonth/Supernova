using SupernovaSchool.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SupernovaSchool.Telegram;

public interface ICommandUploader
{
    Task UploadUserCommandsAsync(string userId, CancellationToken ct = default);
}

public class CommandUploader : ICommandUploader
{
    private static readonly BotCommand CreateTeacherCommand = new()
        { Command = Commands.CreateTeacherCommand, Description = "Добавить нового сотрудника в бота" };

    private static readonly BotCommand[] DefaultCommands =
    [
        new() { Command = Commands.CreateAppointmentCommand, Description = "Записаться к психологу" },
        new() { Command = Commands.DeleteAppointmentCommand, Description = "Удалить запись к психологу" },
        new() { Command = Commands.RegisterAsStudentCommand, Description = "Зарегестрироваться" }
    ];

    private readonly IAdminsProvider _adminsProvider;
    private readonly ITelegramBotClient _telegramBotClient;

    public CommandUploader(IAdminsProvider adminsProvider, ITelegramBotClient telegramBotClient)
    {
        _adminsProvider = adminsProvider;
        _telegramBotClient = telegramBotClient;
    }

    public async Task UploadUserCommandsAsync(string userId, CancellationToken ct = default)
    {
        if (!_adminsProvider.IsAdmin(userId))
        {
            await _telegramBotClient.SetMyCommandsAsync(DefaultCommands, BotCommandScope.Chat(userId),
                cancellationToken: ct);
            return;
        }

        await _telegramBotClient.SetMyCommandsAsync(DefaultCommands.Concat([CreateTeacherCommand]),
            BotCommandScope.Chat(userId), cancellationToken: ct);
    }
}