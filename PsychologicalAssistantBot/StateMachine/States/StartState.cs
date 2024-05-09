using PsychologicalAssistantBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PsychologicalAssistantBot.StateMachine.States;

public class StartState : ChatStateBase
{
    private readonly ITelegramBotClient _botClient;
    private readonly string _agencyName;
    private readonly IChatGptService _chatGptService;
    
    public StartState(ChatStateMachine stateMachine, ITelegramBotClient botClient, string agencyName, IChatGptService chatGptService) : base(stateMachine)
    {
        _botClient = botClient;
        _agencyName = agencyName;
        _chatGptService = chatGptService;
    }

    public override Task HandleMessage(Message message)
    {
        return Task.CompletedTask;
    }

    private async Task SendGreeting(long chatId)
    {
        var greetingsText = 
            $"Приветствую!\nЯ первый психологический бот с искусственным интелектом, созданный агентством психологической поддержки *{_agencyName}*." +
            "\nЯ могу оказать Вам мгновенную первичную консультацию по любому интересующему Вас вопросу," +
            " либо передать Ваши данные нашему специалисту для более детальной консультации.";
        greetingsText = greetingsText.EscapeMarkdownV2();
        
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Задать вопрос",
                    GlobalData.QUESTION)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Консультация специалиста",
                    GlobalData.SPECIALIST)
            }
        });
        
        await _botClient.SafeSendTextMessageAsync(chatId, greetingsText, replyMarkup: inlineKeyboard,
            parseMode: ParseMode.MarkdownV2);
        await _stateMachine.TransitTo<IdleState>(chatId);
    }

    public override async Task OnEnter(long chatId)
    {
        await base.OnEnter(chatId);
        await SendGreeting(chatId);
    }
    
}