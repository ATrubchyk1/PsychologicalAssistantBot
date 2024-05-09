using PsychologicalAssistantBot.Extensions;
using PsychologicalAssistantBot.StateMachine;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PsychologicalAssistantBot;

public class TelegramBotController
{
    private readonly ITelegramBotClient _botClient;
    private readonly SubscriptionService.SubscriptionService _subscriptionService;
    private readonly ChatStateController _chatStateController;

    public TelegramBotController(ITelegramBotClient telegramBotClient,
        SubscriptionService.SubscriptionService subscriptionService, ChatStateController chatStateController)
    {
        _botClient = telegramBotClient;
        _subscriptionService = subscriptionService;
        _chatStateController = chatStateController;
    }

    public void StartBot()
    {
        using var cancellationToken = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            }
        };
        CreateCommandsKeyboard().WaitAsync(cancellationToken.Token);
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken.Token);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type is UpdateType.Message or UpdateType.CallbackQuery)
        {
            if (update.Message == null && update.CallbackQuery == null)
            {
                return;
            }

            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            if (message != null && message.Type != MessageType.Text)
            {
                return;
            }
            var userId = message?.From.Id ?? callbackQuery.From.Id;
            var messageId = message?.MessageId ?? callbackQuery.Message.MessageId;
            var messageText = message?.Text ?? callbackQuery?.Data;
            string response;
            if (messageText == GlobalData.CHECK_SUBSCRIPTION)
            {
                if (!await _subscriptionService.IsSubscribed(userId))
                {
                    response = "Для продолжения работы бота, Вам необходимо быть подписчиком канала";
                    await DeleteMessageAsync(userId, messageId, cancellationToken);
                    await SendSubscriptionMessage(userId, response);
                }

                await DeleteMessageAsync(userId, messageId, cancellationToken);
            }
            if (await _subscriptionService.IsSubscribed(userId))
            {
                await _chatStateController.HandleUpdate(update);
            }
            else
            {
                response = "Безлимитное использование бота доступно подписчикам канала";
                await SendSubscriptionMessage(userId, response);
            }
        }
        
    }

    private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var requestException = exception switch
        {
            ApiRequestException apiRequestException => apiRequestException,
            _ => exception
        };

        var errorText = "Произошла критическая ошибка. Требуется *ПЕРЕЗАПУСК* бота";
        await ErrorNotificationService.Instance.SendTextMessageError(errorText);
        await ErrorNotificationService.Instance.SendErrorNotification(requestException);
    }
    private async Task CreateCommandsKeyboard()
    {
        await _botClient.DeleteMyCommandsAsync();
        
        var commands = new BotCommand[]
        {
            new()
            {
                Command = GlobalData.START,
                Description = "Запуск бота."
            }
        };
         await _botClient.SetMyCommandsAsync(commands);
    }

    private async Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            await _botClient.DeleteMessageAsync(chatId, messageId, cancellationToken: cancellationToken);
        }
        catch (ApiRequestException exception)
        {
            await ErrorNotificationService.Instance.SendErrorNotification(exception);
        }
    }
    
    private async Task SendSubscriptionMessage(long chatId, string response)
    {
        try
        {
            var chanelInfo = await _botClient.GetChatAsync(_subscriptionService.SubscribeChatId);
            var channelName = chanelInfo.Title.EscapeMarkdownV2();
            var userName = chanelInfo.Username;
            var channelLink = $"[{channelName}](https://t.me/{userName})";
            var subscriptionMessage = $"{response} {channelLink}";
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Проверить подписку.",
                        GlobalData.CHECK_SUBSCRIPTION)
                }
            });
            await _botClient.SafeSendTextMessageAsync(chatId, subscriptionMessage, replyMarkup: inlineKeyboard,
                parseMode: ParseMode.MarkdownV2);
        }
        catch (ApiRequestException exception)
        {
            await ErrorNotificationService.Instance.SendErrorNotification(exception);
        }
    }
}