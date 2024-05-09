using PsychologicalAssistantBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PsychologicalAssistantBot.StateMachine.States;

public class DoneState : ChatStateBase
{
    private readonly ITelegramBotClient _botClient;
    
    public DoneState(ChatStateMachine stateMachine, ITelegramBotClient botClient) : base(stateMachine)
    {
        _botClient = botClient;
    }

    public override Task HandleMessage(Message message)
    {
        return Task.CompletedTask;
    }

    public override async Task OnEnter(long chatId)
    {
        await base.OnEnter(chatId);
        var response = "Спасибо, что воспользовались нашими услугами.\n" +
                       "Если потребуется, я могу оказать Вам мгновенную первичную консультацию по любому интересующему" +
                       " Вас вопросу, либо передать Ваши данные нашему специалисту для более детальной консультации.";
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Задать вопрос", GlobalData.QUESTION),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Консультация специалиста", GlobalData.SPECIALIST)
            }
        });

        await _botClient.SafeSendTextMessageAsync(chatId, response, replyMarkup: keyboard);
        await _stateMachine.TransitTo<IdleState>(chatId);
    }
}