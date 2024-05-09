using System.Text.RegularExpressions;
using PsychologicalAssistantBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PsychologicalAssistantBot.StateMachine.States;

public class WaitingForPhoneState : ChatStateBase
{
    private readonly UsersDataProvider _usersDataProvider;
    private readonly ITelegramBotClient _botClient;

    public WaitingForPhoneState(ChatStateMachine stateMachine, ITelegramBotClient telegramBotClient, UsersDataProvider usersDataProvider) : base(stateMachine)
    {
        _botClient = telegramBotClient;
        _usersDataProvider = usersDataProvider;
    }

    public override async Task HandleMessage(Message message)
    {
        var chatId = message.Chat.Id;
        if (!Regex.IsMatch(message.Text, @"^[+\-()\d\s]*$"))
        {
            var response = "Номер телефона введен некорректно.\nПожалуйста, введите номер без букв.";
            await _botClient.SafeSendTextMessageAsync(chatId, response);
            return;
        }
        _usersDataProvider.SetUserPhone(chatId, message.Text);
        await _stateMachine.TransitTo<UserDataSubmissionState>(chatId);
    }

    public override async Task OnEnter(long chatId)
    {
        await base.OnEnter(chatId);
        var response = "Пожалуйста, введите ваш номер телефона";
        await _botClient.SafeSendTextMessageAsync(chatId, response);
    }
}