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
        { "RightCross", 3 },
        { "RightHook", 4 },
        { "LeadUppercut", 5 },
        { "RearUppercut", 6 },
        { "RightElbow", 7 },
        { "RightUpwardsElbow", 8 },
        { "LeadKnee", 9 },
        { "RearKnee", 10 },
        { "LowKick", 11 },
        { "MidRoundhouseKick", 12 },
        { "HighRoundhouseKick", 13 },
        { "SpinningHookKick", 14 },
        { "SideKick", 15 },
        { "LeadTeep", 16 },
        { "RearTeep", 17 },
        { "ComboPunch", 18 }
    };
    public const int mapLength = 19;

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
        agent.hitRegistered = false; //Reset hit registered flag finishing attack animation
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