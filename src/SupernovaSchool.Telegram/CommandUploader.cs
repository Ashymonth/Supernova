using SupernovaSchool.Abstractions;
using Telegram.Bot.Types;

namespace SupernovaSchool.Telegram;

public interface ICommandUploader
{
    Task UploadUserCommandsAsync(string userId, CancellationToken ct = default);
}

public class CommandUploader : ICommandUploader
{
    private static readonly BotCommand[] AdminCommands =
    [
        new() { Command = Commands.CreateTeacherCommand, Description = "Добавить нового сотрудника в бота" },
        new() { Command = Commands.DeleteTeacherCommand, Description = "Удалить сотрудника" },
    ];

    private static readonly BotCommand[] DefaultCommands =
    [
        new() { Command = Commands.CreateAppointmentCommand, Description = "Записаться к психологу" },
        new() { Command = Commands.DeleteAppointmentCommand, Description = "Удалить запись к психологу" },
        new() { Command = Commands.RegisterAsStudentCommand, Description = "Зарегистрироваться" }
    ];

    private readonly IAdminsProvider _adminsProvider;
    private readonly ITelegramBotClientWrapper _telegramBotClient;

    public CommandUploader(IAdminsProvider adminsProvider, ITelegramBotClientWrapper telegramBotClient)
    {
        _adminsProvider = adminsProvider;
        _telegramBotClient = telegramBotClient;
    }

    public async Task UploadUserCommandsAsync(string userId, CancellationToken ct = default)
    {
        if (!_adminsProvider.IsAdmin(userId))
        {
            await _telegramBotClient.SetMyCommands(DefaultCommands, BotCommandScope.Chat(userId),
                cancellationToken: ct);
            return;
        }

        await _telegramBotClient.SetMyCommands(DefaultCommands.Concat(AdminCommands),
            BotCommandScope.Chat(userId), cancellationToken: ct);
    }
}