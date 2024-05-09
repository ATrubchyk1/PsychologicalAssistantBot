using PsychologicalAssistantBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace PsychologicalAssistantBot;

public class ErrorNotificationService
{
    public static ErrorNotificationService Instance => _instance;
    private static ErrorNotificationService _instance;
    private readonly ITelegramBotClient _botClient;
    private readonly ChatId _errorsLogChatId;
    private readonly string? _botUsername;
    private readonly string _errorLogFilePath;

    private ErrorNotificationService(ITelegramBotClient botClient, ChatId errorsLogChatId, string errorLogFilePath)
    {
        _botClient = botClient;
        _errorsLogChatId = errorsLogChatId;
        _errorLogFilePath = errorLogFilePath;
        _botUsername = botClient.GetMeAsync().Result.Username;
    }
    
    public static void Initialize(ITelegramBotClient botClient, ChatId errorsLogChatId, string errorLogFilePath)
    {
        _instance ??= new ErrorNotificationService(botClient, errorsLogChatId, errorLogFilePath);
    }
    
    public async Task SendErrorNotification(ApiRequestException apiRequestException)
    {
        try
        {
            var response = $"В боте @{_botUsername.EscapeMarkdownV2()} произошла ошибка: ";
            var message =
                $"{response} \nКод ошибки: {apiRequestException.ErrorCode}\nСообщение об ошибке:" +
                $" {apiRequestException.Message.EscapeMarkdownV2()}\nТрассировка стека:" +
                $" ’ ’ ’\n{apiRequestException.StackTrace.EscapeMarkdownV2()}\n’ ’ ’";
            await _botClient.SendTextMessageAsync(_errorsLogChatId, message, parseMode: ParseMode.MarkdownV2);
        }
        catch (Exception exception)
        {
            WriteToLogFile(apiRequestException);
            WriteToLogFile(exception);
        }
    }
    
    public async Task SendErrorNotification(Exception exception)
    {
        try
        {
            var response = $"В боте @{_botUsername.EscapeMarkdownV2()} произошла ошибка: ";
            var message =
                $"{response} \nСообщение об ошибке: {exception.Message.EscapeMarkdownV2()}" +
                $"\nТрассировка стека: ’ ’ ’\n{exception.StackTrace.EscapeMarkdownV2()}\n’ ’ ’";
            await _botClient.SendTextMessageAsync(_errorsLogChatId, message, parseMode: ParseMode.MarkdownV2);
        }
        catch (Exception ex)
        {
            WriteToLogFile(exception);
            WriteToLogFile(ex);
        }
    }
    
    public async Task SendTextMessageError(string message)
    {
        try
        {
            var response = $"В боте @{_botUsername.EscapeMarkdownV2()} произошла ошибка: {message.EscapeMarkdownV2()}";
            await _botClient.SendTextMessageAsync(_errorsLogChatId, response, parseMode: ParseMode.MarkdownV2);
        }
        catch (Exception exception)
        {
            WriteToLogFile(exception);
        }
    }

    private void WriteToLogFile(Exception exception)
    {
        try
        {
            using StreamWriter writer = File.AppendText(_errorLogFilePath);
            writer.WriteLine($"Дата: {DateTime.Now}");
            writer.WriteLine($"Ошибка: {exception.Message}");
            writer.WriteLine($"Стек трейс: +{exception.StackTrace}");
            writer.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось записать информацию об ошибке в файл: {ex.Message}");
        }
    }
}