using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SparringEnvController : MonoBehaviour
{
    [Header("Env Params")]
    private const int MAX_STEPS = 1000;

    [SerializeField]
    public int totalSteps
    {
        get
        {
            return m_totalSteps;
        }
        set
        {
            m_totalSteps = value;
            m_stepsRemaining = MAX_STEPS - value;
        }
    }
    public int m_totalSteps;

    [SerializeField]
    public int stepsRemaining
    {
        get
        {
            return m_stepsRemaining;
        }
        set
        {
            m_stepsRemaining = value;
            m_totalSteps = MAX_STEPS - value;
        }
    }
    public int m_stepsRemaining;

    [System.Serializable]
    public class AgentInfo
    {
        public SparringAgent agent;
        [HideInInspector] public Vector3 StartingPos;
        [HideInInspector] public Quaternion StartingRot;
        [HideInInspector] public Rigidbody Rb;
        public float totalReward;

        public void AddReward(float reward)
        {
            agent.AddReward(reward);
            totalReward += reward;
        }
    }

    public List<AgentInfo> AgentList = new List<AgentInfo>();

    private AgentInfo m_playerAgent;
    private AgentInfo m_opponentAgent;

    void Start()
    {
        foreach (var agentInfo in AgentList)
        {
            if (agentInfo.agent == null)
            {
                Debug.LogError("Missing Agent in AgentList");
                continue;
            }

            //Logging agent properties
            agentInfo.StartingPos = agentInfo.agent.transform.position;
            agentInfo.StartingRot = agentInfo.agent.transform.rotation;
            agentInfo.Rb = agentInfo.agent.GetComponent<Rigidbody>();

            if (agentInfo.Rb == null)
            {
                Debug.LogError("Missing Rigidbody on " + agentInfo.agent.name);
            }

            //Assigning agents
            if (agentInfo.agent.team == Team.Player)
            {
                m_playerAgent = agentInfo;
            }
            else if (agentInfo.agent.team == Team.Opponent)
            {
                m_opponentAgent = agentInfo;
            }
            else
            {
                Debug.LogError("Unknown team for agent: " + agentInfo.agent.name);
            }
        }

        ResetEnv();
    }

    void FixedUpdate()
    {
        //Stepping the env
        totalSteps++;

        //Reset if time limit exceeded
        if (totalSteps >= MAX_STEPS && MAX_STEPS > 0)
        {
            //NOTE: EpisodeInterrupted indicates episode ended but not due to Agent's "fault"
            m_playerAgent.agent.EpisodeInterrupted();
            m_opponentAgent.agent.EpisodeInterrupted();
            ResetEnv();
        }
    }

    // public void ComputeGlobalRewards()
    // {
    //     //Add reward functions here and provide cumulative reward
    //     float reward = 0f;

    //     // Shape: Angle to opponent (higher angle => more advantageous position)
    //     reward += Math.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up),
    //                         Vector3.ProjectOnPlane(m_opponentAgent.agent.transform.localPosition - m_playerAgent.agent.transform.localPosition, Vector3.up).normalized,
    //                         Vector3.up) / 180f);

    //     // Shape: Hits received (aim is to get more aggressive as the agent gets hit more)
    //     reward -= hitsReceived * 0.1f;

    //     m_playerAgent.AddReward(reward);
    //     m_opponentAgent.AddReward(-reward);
    // }

    public void AngleReward(Team team)
    {
        /*
            Function to handle rewards based on the angle to the opponent
            This function should be called from the SparringAgent script when the angle is calculated
        */
        float angleReward = 0f;

        if (team == Team.Player)
        {
            angleReward = Math.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(m_opponentAgent.agent.transform.forward, Vector3.up),
                                              Vector3.ProjectOnPlane(m_opponentAgent.agent.transform.localPosition - m_playerAgent.agent.transform.localPosition, Vector3.up).normalized,
                                              Vector3.up) / 180f);
            m_playerAgent.AddReward(angleReward);
            m_opponentAgent.AddReward(-angleReward);
        }
        else
        {
            angleReward = Math.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.forward, Vector3.up),
                                               Vector3.ProjectOnPlane(m_playerAgent.agent.transform.localPosition - m_opponentAgent.agent.transform.localPosition, Vector3.up).normalized,
                                               Vector3.up) / 180f);
            m_opponentAgent.AddReward(angleReward);
            m_playerAgent.AddReward(-angleReward);
        }
    }

    // public void HitsReceivedReward(Team team)
    // {
    //     /*
    //         Function to handle rewards based on hits received
    //         Goal is to influence agents to be more aggressive as they take more hits
    //         This function should be called from the SparringAgent script when hits are received
    //     */
    //     float hitsReceivedReward = 0f;

    //     if (team == Team.Player)
    //     {
    //         hitsReceivedReward = -m_playerAgent.agent.hitsReceived * (1 / MAX_STEPS);
    //         m_playerAgent.AddReward(hitsReceivedReward);
    //     }
    //     else
    //     {
    //         hitsReceivedReward = -m_opponentAgent.agent.hitsReceived * (1 / MAX_STEPS);
    //         m_opponentAgent.AddReward(hitsReceivedReward);
    //     }
    // }

    public void AttackLandedReward(Team hitTeam, string hitType)
    {
        /*
            Function to handle rewards for attacks that land on a target
            hitTeam: Team that was hit (Player or Opponent)
            hitType: Type of hit (e.g. "HeadHit", "LegHit")
            This function should be called from the Hitbox script when a hit is detected
        */
        float reward = 0f;

        if (hitType == "HeadHit")
        {
            reward = -5.0f; // Reward for head hit
        }
        else if (hitType == "LeftSideHit" || hitType == "RightSideHit")
        {
            reward = -3.5f; // Reward for body hit
        }
        else if (hitType == "LegHit")
        {
            reward = -4.0f; // Reward for leg hit
        }

        if (hitTeam == Team.Player)
        {
            m_playerAgent.AddReward(reward);
            m_opponentAgent.AddReward(-reward);
        }
        else
        {
            m_playerAgent.AddReward(-reward);
            m_opponentAgent.AddReward(reward);
        }
    }

    public void AttackBlockedReward(Team hitTeam, bool isBlocking)
    {
        /*
            Function to handle rewards for blocked attacks
            hitTeam: Team that was hit (Player or Opponent)
            isBlocking: Whether the attack was blocked
            This function should be called from the Hitbox script when a block is detected
        */

        if (isBlocking)
        {
            if (hitTeam == Team.Player)
            {
                m_playerAgent.AddReward(1.0f);
                m_opponentAgent.AddReward(-0.1f);
            }
            else
            {
                m_playerAgent.AddReward(-0.1f);
                m_opponentAgent.AddReward(1.0f);
            }
        }
    }

    public void ResetEnv()
    {
        //Reset env steps
        totalSteps = 0;

        //Reset agents
        foreach (AgentInfo agentInfo in AgentList)
        {
            agentInfo.agent.hitsReceived = 0;
            agentInfo.totalReward = 0f;
            agentInfo.agent.SetAgentInfo(agentInfo);
            agentInfo.agent.ResetAgent();
        }
    }
}