using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SparringEnvController : MonoBehaviour
{
    [Header("Env Params")]
    private const int MAX_STEPS = 2000;

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

    public int episodeCount = 0;

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
    public AgentInfo PlayerAgentInfo => m_playerAgent;
    private AgentInfo m_opponentAgent;
    public AgentInfo OpponentAgentInfo => m_opponentAgent;

    void Awake()
    {
        //Initializing default values
        foreach (var agentInfo in AgentList)
        {
            agentInfo.StartingPos = agentInfo.agent.transform.position;
            agentInfo.StartingRot = agentInfo.agent.transform.rotation;
            agentInfo.Rb = agentInfo.agent.GetComponent<Rigidbody>();
        }
    }

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
            episodeCount++;
        }
    }

    public void AngleReward(Team team)
    {
        /*
            Function to handle rewards based on the angle to the opponent
            This function should be called from the SparringAgent script when the angle is calculated
        */
        float angleReward = 0f;

        if (team == Team.Player)
        {
            // angleReward = Math.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(m_opponentAgent.agent.transform.forward, Vector3.up),
            //                                   Vector3.ProjectOnPlane(m_opponentAgent.agent.transform.localPosition - m_playerAgent.agent.transform.localPosition, Vector3.up).normalized,
            //                                   Vector3.up) / 180f);
            angleReward = ComputeNormalizedAngle(m_playerAgent.agent.transform, m_opponentAgent.agent.transform) / 180f;
            angleReward /= MAX_STEPS; // Normalize by max steps
            m_playerAgent.AddReward(angleReward);
            //m_opponentAgent.AddReward(-angleReward);
        }
        else
        {
            // angleReward = Math.Abs(Vector3.SignedAngle(Vector3.ProjectOnPlane(m_playerAgent.agent.transform.forward, Vector3.up),
            //                                    Vector3.ProjectOnPlane(m_playerAgent.agent.transform.localPosition - m_opponentAgent.agent.transform.localPosition, Vector3.up).normalized,
            //                                    Vector3.up) / 180f);
            angleReward = ComputeNormalizedAngle(m_opponentAgent.agent.transform, m_playerAgent.agent.transform) / 180f;
            angleReward /= MAX_STEPS; // Normalize by max steps
            m_opponentAgent.AddReward(angleReward);
            //m_playerAgent.AddReward(-angleReward);
        }
    }

    private float ComputeNormalizedAngle(Transform playerTransform, Transform opponentTransform) 
    {
        Vector3 forwardDir = playerTransform.forward;
        forwardDir.y = 0; //ignore vertical component
        forwardDir.Normalize();

        Vector3 direction = opponentTransform.position - playerTransform.position;
        direction.y = 0;
        direction.Normalize();

        // Return negative angle to punish the agent for facing away
        float angle = -Vector3.Angle(
            forwardDir,
            direction
        );
        return angle / 180f;
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
            reward = -5.5f; // Reward for head hit
        }
        else if (hitType == "LeftSideHit" || hitType == "RightSideHit")
        {
            reward = -4.5f; // Reward for side hits
        }
        else if (hitType == "BodyHit")
        {
            reward = -4.0f; // Reward for body hit
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
                m_playerAgent.AddReward(4.0f);
                m_opponentAgent.AddReward(-0.5f);
            }
            else
            {
                m_playerAgent.AddReward(-0.5f);
                m_opponentAgent.AddReward(4.0f);
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
            agentInfo.agent.Respawn();
        }
    }
}