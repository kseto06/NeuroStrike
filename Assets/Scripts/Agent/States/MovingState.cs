using UnityEngine;
using System.Collections.Generic;

public class MovingState : AgentState, IMoveTypeState
{
    public new string action;

    private int moveTypeIndex;

    public static readonly Dictionary<string, int> MoveActionMap = new Dictionary<string, int>
    {
        { "StepBackward", 0 },
        { "ShortStepForward", 1 },
        { "MediumStepForward", 2 },
        { "LongStepForward", 3 },
        { "ShortRightSideStep", 4 },
        { "ShortLeftSideStep", 5 },
        { "MediumRightSideStep", 6 },
        { "MediumLeftSideStep", 7 },
        { "LongRightSideStep", 8 },
        { "LongLeftSideStep", 9 },
        { "LeftPivot", 10 },
        { "RightPivot", 11 }
    };
    public const int mapLength = 12;

    public MovingState(SparringAgent agent, string action) : base(agent, action)
    {
        this.action = action;
        this.moveTypeIndex = MoveActionMap.TryGetValue(action, out int index) ? index : 0;
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

    public int GetMoveTypeIndex()
    {
        return moveTypeIndex;
    }
}