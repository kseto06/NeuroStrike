using UnityEngine;

public class AttackingState : AgentState
{
    public new string action;

    public AttackingState(SparringAgent agent, string action) : base(agent, action)
    {
        this.action = action;
    }

    public override void Enter(AgentState fromState)
    {
        base.Enter(fromState);
        Debug.Log($"Entering AttackingState with action: {action}");
        agent.animationController.animator.applyRootMotion = false;

        // Force override current animations from MovingState
        agent.animationController.Play(this.action, overrideAnimation: true);
    }

    public override void Exit(AgentState toState)
    {
        agent.animationController.animator.applyRootMotion = true;
        base.Exit(toState);
    }

    public override AgentState Process()
    {
        string currentAction = this.GetCurrentAction();

        // Interrupting attack if hit
        if (this.HurtList.Contains(currentAction))
        {
            return new HurtState(agent, currentAction);
        }
        else if (currentAction == "Idle" || !agent.animationController.isAnimating)
        {
            return new IdleState(agent, "Idle");
        }
        else 
        {
            return this;
        }
    }

    public override bool CanBeInterrupted(string action)
    {
        return HurtList.Contains(action);
    }
}