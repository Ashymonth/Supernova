using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps.Common;

public class SendMessageWithOptionsToUser : IStepBody, IUserStep
{
    private const int ItemsPerPage = 5;
    private readonly ITelegramBotClient _telegramBotClient;

    public SendMessageWithOptionsToUser(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public string UserId { get; set; } = default!;

    public string Message { get; set; } = default!;

    public string[] Options { get; set; } = null!;

    public bool ShouldPaginate => Options.Length > 5;

    public int Page { get; set; }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var paginatedOptions = ShouldPaginate ? Options.Skip(Page * ItemsPerPage).Take(ItemsPerPage) : Options;

        var buttons = paginatedOptions.Select(x => new KeyboardButton(x)).ToList();
        if (ShouldPaginate && buttons.Count == ItemsPerPage)
        {
            buttons.Add(new KeyboardButton("Дальше"));
        }

        if (Page >= 1)
        {
            buttons.Add(new KeyboardButton("Назад"));
        }

        var answerOptions = new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };

        var message = await _telegramBotClient.SendTextMessageAsync(UserId, Message, replyMarkup: answerOptions);

        return ExecutionResult.Next();
    }
}