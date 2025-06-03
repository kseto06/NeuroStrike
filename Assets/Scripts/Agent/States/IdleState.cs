using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class IdleState : AgentState 
{
    public new string action;

    private List<string> MovementList = new List<string>
    {
        "StepBackward",
        "ShortStepForward",
        "MediumStepForward",
        "LongStepForward",
        "ShortRightSideStep",
        "ShortLeftSideStep",
        "MediumRightSideStep",
        "MediumLeftSideStep",
        "LongRightSideStep",
        "LongLeftSideStep",
        "LeftPivot",
        "RightPivot"
    };

    public IdleState(SparringAgent agent, string action) : base(agent, action) 
    {
        this.action = action;
    }

    public override void Enter(AgentState fromState)
    {
        base.Enter(fromState);
        Debug.Log("Entering IdleState");
        agent.animationController.animator.applyRootMotion = true;
    }

    public override void Exit(AgentState toState)
    {
        base.Exit(toState);
    }

    public override AgentState Process()
    {
        if (this.HurtList.Contains(action))
        {
            return new HurtState(agent, action);
        }
        else if (action == "Block")
        {
            return new BlockingState(agent, action);
        } 
        else if (this.AttackList.Contains(action))
        {
            return new AttackingState(agent, action);
        }
        else if (MovementList.Contains(action))
        {
            return new MovingState(agent, action);
        }
        else 
        {
            return this; //Stay in Idle state
        }
    }

    public override bool CanBeInterrupted(string action)
    {
        return true;
    }
}