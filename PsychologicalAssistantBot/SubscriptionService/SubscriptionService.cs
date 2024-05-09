using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PsychologicalAssistantBot.SubscriptionService;

public class SubscriptionService
{
    public ChatId SubscribeChatId { get; }
    private readonly ITelegramBotClient _botClient;

    private readonly List<ChatMemberStatus> _allowedStatuses = new()
    {
        ChatMemberStatus.Member, ChatMemberStatus.Administrator, ChatMemberStatus.Creator
    };
    
    public SubscriptionService(ITelegramBotClient telegramBotClient, ChatId chatId)
    {
        _botClient = telegramBotClient;
        SubscribeChatId = chatId;
    }

    public async Task<bool> IsSubscribed(long userId)
    {
        try
        {
            var chatMember = await _botClient.GetChatMemberAsync(SubscribeChatId, userId);
            if (_allowedStatuses.Contains(chatMember.Status))
            {
                return true;
            }
        }
        catch (ApiRequestException exception)
        {
            Console.WriteLine(exception);
            return false;
        }

        return false;
    }
}