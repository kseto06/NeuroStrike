using UnityEngine;
using System.Collections.Generic;

public abstract class AgentState
{
    // Only allow agent and HurtList to be accessed by extending classes
    protected SparringAgent agent;
    protected float StateEnterTime;

    protected List<string> HurtList = new List<string>
    {
        "BodyHit",
        "HeadHit",
        "RightSideHit",
        "LeftSideHit"
    };

    protected List<string> MoveList = new List<string> { 
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

    protected List<string> AttackList = new List<string>
    {
        "LeftJab",
        "LeftCross",
        "LeftHook",
        "RightCross",
        "RightHook",
        "LeadUppercut",
        "RearUppercut",
        "RightElbow",
        "RightUpwardsElbow",
        "LeadKnee",
        "RearKnee",
        "LowKick",
        "MidRoundhouseKick",
        "HighRoundhouseKick",
        "SpinningHookKick",
        "SideKick",
        "LeadTeep",
        "RearTeep",
        "ComboPunch"
    };

    public string action;

    public AgentState(SparringAgent agent, string action)
    {
        this.agent = agent;
        this.action = action;
    }

    public virtual void Enter(AgentState fromState) {}
    public virtual void Exit(AgentState toState) {}
    public virtual AgentState Process() 
    {
        return this;
    }

    public virtual string GetCurrentAction() {
        if (agent.animationController != null)
        {
            return agent.animationController.GetCurrentAnimatorStateName();
        }
        else
        {
            Debug.LogError("AnimationController is not assigned on " + agent.name);
            return null;
        }
    }

    public virtual bool CanBeInterrupted(string action) 
    {
        return true;
    }

    public virtual bool IsAnimating()
    {
        if (agent.animationController != null)
        {
            return agent.animationController.isAnimating;
        }
        else
        {
            Debug.LogError("AnimationController is not assigned on " + agent.name);
            return false;
        }
    }
}