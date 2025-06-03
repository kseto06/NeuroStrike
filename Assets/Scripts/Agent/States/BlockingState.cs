using UnityEngine;

public class BlockingState : AgentState
{
    public new string action;

    public BlockingState(SparringAgent agent, string action) : base(agent, action) 
    {
        this.action = action;
    }

    public override void Enter(AgentState fromState)
    {
        base.Enter(fromState);
        Debug.Log($"Entering BlockingState");  
        agent.animationController.Play("Block", overrideAnimation: false);
        agent.animationController.animator.applyRootMotion = false;

        // Set rigidbody constraints to prevent movement
        agent.rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
    }

    public override void Exit(AgentState toState)
    {
        base.Exit(toState);
    }

    public override AgentState Process()
    {
        if (action == "Idle" || !agent.animationController.isAnimating)
        {
            agent.rb.constraints = RigidbodyConstraints.FreezeRotation;
            return new IdleState(agent, "Idle");
        }
        else
        {
            return this;
        }
    }

    public override bool CanBeInterrupted(string action)
    {
        return false;
    }
}