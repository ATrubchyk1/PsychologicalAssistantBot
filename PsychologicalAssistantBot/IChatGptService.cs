using Telegram.Bot.Types;

namespace PsychologicalAssistantBot;

public interface IChatGptService
{
    public Task<string?> GetAnswerFromChatGpt(string question);
}