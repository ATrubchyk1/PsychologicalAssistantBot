using Newtonsoft.Json;
using PsychologicalAssistantBot.StateMachine;
using Telegram.Bot;

namespace PsychologicalAssistantBot;

public static class Program
{
    private const string APP_SETINGS = "AppSettings/app_settings.json";
    private const string SECRETS = "AppSettings/secrets.json";
    private static async Task Main()
    {
        var secretsJson = await File.ReadAllTextAsync(SECRETS);
        var secrets = JsonConvert.DeserializeObject<Secrets>(secretsJson);
        
        var settingsJson = await File.ReadAllTextAsync(APP_SETINGS);
        var settings = JsonConvert.DeserializeObject<AppSettings>(settingsJson);
        
        var botClient = new TelegramBotClient(secrets.ApiKeys.TelegramKey);
        
        ErrorNotificationService.Initialize(botClient, settings.ErrorsLogChannelId, settings.ErrorsFilePath);
        
        var subscriptionService = new SubscriptionService.SubscriptionService(botClient, settings.SubscribeChannelId);
        IChatGptService chatGptService = new ChatGptService(secrets.ApiKeys.OpenAiKey, settings);
        var userDataProvider = new UsersDataProvider();
        
        var chatStateMachine = new ChatStateMachine(botClient, settings, chatGptService, userDataProvider);
        
        var chatStateController = new ChatStateController(chatStateMachine);
        
        var telegramBot = new TelegramBotController(botClient, subscriptionService, chatStateController);
        telegramBot.StartBot(); 
        await Task.Delay(Timeout.Infinite);
    }
}