using UnityEngine;
using System.Collections.Generic;

public class AttackingState : AgentState, IMoveTypeState
{
    public new string action;
    private int moveTypeIndex;

    public static readonly Dictionary<string, int> AttackActionMap = new Dictionary<string, int>
    {
        { "LeftJab", 0 },
        { "LeftCross", 1 },
        { "LeftHook", 2 },
        { "RightJab", 3 },
        { "RightCross", 4 },
        { "RightHook", 5 },
        { "LeftUppercut", 6 },
        { "RightUppercut", 7 },
        { "LeadUppercut", 8 },
        { "RearUppercut", 9 },
        { "RightElbow", 10 },
        { "LeftElbow", 11 },
        { "RightUpwardsElbow", 12 },
        { "LeadKnee", 13 },
        { "RearKnee", 14 },
        { "LowKick", 15 },
        { "MidRoundhouseKick", 16 },
        { "HighRoundhouseKick", 17 },
        { "SpinningHookKick", 18 },
        { "SideKick", 19 },
        { "LeadTeep", 20 },
        { "RearTeep", 21 },
        { "ComboPunch", 22 }
    };
    public const int mapLength = 23;

    public AttackingState(SparringAgent agent, string action) : base(agent, action)
    {
        this.action = action;
        this.moveTypeIndex = AttackActionMap.TryGetValue(action, out int index) ? index : 0;
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

    public int GetMoveTypeIndex()
    {
        return moveTypeIndex;
    }

}