using UnityEngine;
using System.Collections.Generic;

public class HurtState : AgentState, IMoveTypeState
{
    public new string action;
    private int moveTypeIdx;
    public static readonly Dictionary<string, int> HurtActionMap = new Dictionary<string, int>
    {
        { "BodyHit", 0 },
        { "HeadHit", 1 },
        { "RightSideHit", 2 },
        { "LeftSideHit", 3 }
    };
    public const int mapLength = 4;

    public HurtState(SparringAgent agent, string action) : base(agent, action) 
    {
        this.action = action;
        this.moveTypeIdx = HurtActionMap.TryGetValue(action, out int index) ? index : 0;
    }

    public override void Enter(AgentState fromState)
    {
        base.Enter(fromState);
        agent.animationController.animator.applyRootMotion = false;
        agent.animationController.Play(this.action, overrideAnimation: true); //Overriding animations on any hit
        agent.hitsReceived++;
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

    public int GetMoveTypeIndex()
    {
        return moveTypeIdx;
    }
}