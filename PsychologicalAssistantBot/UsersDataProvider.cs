using System.Collections.Concurrent;

namespace PsychologicalAssistantBot;

public class UsersDataProvider
{
    private readonly ConcurrentDictionary<long, UserData> _userDatas = new();

    public void SetUserName(long chatId, string name)
    {
        var userState = _userDatas.GetOrAdd(chatId, new UserData());
        userState.Name = name;
    }

    public void SetUserPhone(long chatId, string phone)
    {
        var userState = _userDatas.GetOrAdd(chatId, new UserData());
        userState.Phone = phone;
    }

    public void SetTelegramName(long chatId, string? telegram)
    {
        var userState = _userDatas.GetOrAdd(chatId, new UserData());
        userState.Telegram = telegram;
    }

    public UserData GetUserData(long chatId)
    {
        return _userDatas[chatId];
    }

    public void SaveLastQuestion(long chatId, string? question)
    {
        var data = _userDatas.GetOrAdd(chatId, new UserData());
        data.LastQuestion = question;
    }

    public void ClearUserData(long chatId)
    {
        _userDatas.TryRemove(chatId, out _);
    }
}