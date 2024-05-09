using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PsychologicalAssistantBot.Extensions;

public static class TelegramBotClientExtensions
{
    public static async Task SafeSendTextMessageAsync(
        this ITelegramBotClient botClient,
        ChatId chatId,
        string text,
        int? messageThreadId = default,
        ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default,
        bool? disableWebPagePreview = default,
        bool? disableNotification = default,
        bool? protectContent = default,
        int? replyToMessageId = default,
        bool? allowSendingWithoutReply = default,
        IReplyMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendTextMessageAsync(
                chatId,
                text,
                messageThreadId,
                parseMode,
                entities,
                disableWebPagePreview,
                disableNotification,
                protectContent,
                replyToMessageId,
                allowSendingWithoutReply,
                replyMarkup,
                cancellationToken);
        }
        catch (ApiRequestException exception)
        {
            await ErrorNotificationService.Instance.SendErrorNotification(exception);
        }
    }
    public static async Task SafeSendChatActionAsync(
        this ITelegramBotClient botClient,
        ChatId chatId,
        ChatAction chatAction,
        int? messageThreadId = default,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);
        }
        catch (ApiRequestException exception)
        {
            await ErrorNotificationService.Instance.SendErrorNotification(exception);
        }
    }
}