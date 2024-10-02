using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class SendTeacherListStep : IStepBody
{
    private readonly IRepository<Teacher> _teacherRepository;
    private readonly ITelegramBotClient _telegramBotClient;

    public SendTeacherListStep(IRepository<Teacher> teacherRepository, ITelegramBotClient telegramBotClient)
    {
        _teacherRepository = teacherRepository;
        _telegramBotClient = telegramBotClient;
    }

    public string UserId { get; set; } = null!;
    
    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var teachers = await _teacherRepository.ListAsync();

        var buttons = teachers.Select(teacher =>
            InlineKeyboardButton.WithCallbackData(teacher.Name, teacher.Id.ToString()));

        await _telegramBotClient.SendTextMessageAsync(UserId, "Список сотрудников",
            replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: context.CancellationToken);
        
        return ExecutionResult.Next();
    }
}