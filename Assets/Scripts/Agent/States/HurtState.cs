using UnityEngine;

public class HurtState : AgentState
{
    public new string action;

    public HurtState(SparringAgent agent, string action) : base(agent, action) 
    {
        this.action = action;
    }

    public override void Enter(AgentState fromState)
    {
        base.Enter(fromState);
        agent.animationController.animator.applyRootMotion = false;
        agent.animationController.Play(this.action, overrideAnimation: true); //Overriding animations on any hit
        Debug.Log($"Entering HurtState with action: {action}");
    }

    public override void Exit(AgentState toState)
    {
        base.Exit(toState);
    }

    public override AgentState Process()
    {   
        if (action == "Idle" || !agent.animationController.isAnimating)
        {
            return new IdleState(agent, "Idle");
        }

        // Stay in HurtState until the animation completes
        return this;
    }

    public override bool CanBeInterrupted(string action)
    {
        return false;
    }
}