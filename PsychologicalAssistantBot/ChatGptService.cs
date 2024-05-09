using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;

namespace PsychologicalAssistantBot;

public class ChatGptService : IChatGptService
{
    private readonly OpenAIService _openAiService;
    private readonly string _promt;

    public ChatGptService(string openAiApiKey, AppSettings appSettings)
    {
        _promt = appSettings.GptPrompt;
        _openAiService = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = openAiApiKey
        });
    }
    
    public async Task<string?> GetAnswerFromChatGpt(string question)
    {
        try
        {
            var completionResult = await _openAiService.ChatCompletion.CreateCompletion(
                new ChatCompletionCreateRequest
            {
                Model = Models.Gpt_3_5_Turbo_0301,
                Messages = new List<ChatMessage>()
                {
                    new(StaticValues.ChatMessageRoles.System, _promt),
                    new(StaticValues.ChatMessageRoles.User, question)
                },
                Temperature = 0.1f
            });

            if (completionResult.Successful)
            {
                var request = completionResult.Choices.First().Message.Content;
                return request;
            }
            
            var errorMessage = $"OpenAi API Error:\n{completionResult.Error.Code}\n{completionResult.Error.Message}";
            Console.WriteLine(errorMessage);
            return null;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return null;
        }
    }
}