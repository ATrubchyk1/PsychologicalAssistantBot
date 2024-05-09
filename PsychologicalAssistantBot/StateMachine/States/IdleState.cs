using PsychologicalAssistantBot.StateMachine.States;
using Telegram.Bot.Types;

namespace PsychologicalAssistantBot.StateMachine;

public class IdleState : ChatStateBase
{
    public IdleState(ChatStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override Task HandleMessage(Message message)
    {
        return Task.CompletedTask;
    }
}