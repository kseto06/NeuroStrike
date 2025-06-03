using UnityEngine;

public class MovingState : AgentState
{
    public new string action;

    public MovingState(SparringAgent agent, string action) : base(agent, action)  
    {
        this.action = action;
    }

    public override void Enter(AgentState fromState)
    {
        base.Enter(fromState);
        agent.animationController.animator.applyRootMotion = true; //Enable movement by animation 
        agent.animationController.Play(this.action, overrideAnimation: false);
        Debug.Log($"Entering MovingState with action: {this.action}");
    }

    public override void Exit(AgentState toState)
    {
        base.Exit(toState);
    }

    public override AgentState Process()
    {
        string currentAction = agent.inputAction;
        agent.inputAction = null;

        // Interrupting movement if hit
        if (this.HurtList.Contains(currentAction))
        {
            return new HurtState(agent, currentAction);
        }
        else if (this.AttackList.Contains(currentAction))
        {
            return new AttackingState(agent, currentAction);
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
        return this.AttackList.Contains(action) || this.HurtList.Contains(action);
    }
}