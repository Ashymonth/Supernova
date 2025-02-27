using Moq;
using SupernovaSchool.Telegram;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupernovaSchool.Tests.Extensions;

public static class MockExtensions
{
    public static void SetupSendMessage(this Mock<ITelegramBotClientWrapper> mock, long senderId, string message)
    {
        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == senderId),
                    message, It.IsAny<ReplyMarkup>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Text = message })
            .Verifiable(Times.Once);
    }
    
    public static void SetupSendMessage<TKeyboard>(this Mock<ITelegramBotClientWrapper> mock, long senderId, string message)
        where TKeyboard : ReplyMarkup
    {
        mock.Setup(
                wrapper => wrapper.SendMessage(It.Is<ChatId>(id => id.Identifier == senderId),
                    message, It.IsAny<TKeyboard>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Text = message })
            .Verifiable(Times.Once);
    }
}