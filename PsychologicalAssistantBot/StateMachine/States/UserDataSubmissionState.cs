using PsychologicalAssistantBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PsychologicalAssistantBot.StateMachine.States;

public class UserDataSubmissionState : ChatStateBase
{
    private readonly UsersDataProvider _usersDataProvider;
    private readonly ITelegramBotClient _botClient;
    private readonly ChatId _managerChannelId;
    
    public UserDataSubmissionState(ChatStateMachine stateMachine, ITelegramBotClient telegramBotClient, UsersDataProvider usersDataProvider, ChatId managerChannelId) : base(stateMachine)
    {
        _botClient = telegramBotClient;
        _usersDataProvider = usersDataProvider;
        _managerChannelId = managerChannelId;
    }

    public override Task HandleMessage(Message message)
    {
        return Task.CompletedTask;
    }

    public override async Task OnEnter(long chatId)
    {
        await base.OnEnter(chatId);
        var response = "Спасибо за обращение. Данные переданы нашему специалисту, он свяжется с Вами в скором времени.";
        await _botClient.SafeSendTextMessageAsync(chatId, response);
        await _stateMachine.TransitTo<IdleState>(chatId);
    }

    private async Task SendUserInfoToManager(long chatId)
    {
        try
        {
            var userProfile = _usersDataProvider.GetUserData(chatId);
            var userInfo = BuildUserInfo(userProfile);
            await _botClient.SafeSendTextMessageAsync(_managerChannelId, userInfo);
            
            _usersDataProvider.ClearUserData(chatId);
        }
        catch (Exception exception)
        {
            await ErrorNotificationService.Instance.SendErrorNotification(exception);
        }
    }
    
    private string BuildUserInfo(UserData userProfile)
    {
        var userInfo = "Информация о клиенте:" +
                       $"\nФИО: {userProfile.Name}" +
                       $"\nНомер телефона: {userProfile.Phone}";
        if (!string.IsNullOrEmpty(userProfile.Telegram))
        {
            userInfo += $"\nTelegram: @{userProfile.Telegram}";
        }
        if (!string.IsNullOrEmpty(userProfile.LastQuestion))
        {
            userInfo += $"\nПоследний отправленный вопрос боту: {userProfile.LastQuestion}";
        }

        return userInfo;
    }

    public override async Task OnExit(long chatId)
    {
        await base.OnExit(chatId);
        await SendUserInfoToManager(chatId);
    }
}