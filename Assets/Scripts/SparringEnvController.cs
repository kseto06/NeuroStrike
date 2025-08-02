using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SparringEnvController : MonoBehaviour
{
    [Header("Env Params")]
    private const int MAX_STEPS = 2500;

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

        // Partial rewards for logging
        [HideInInspector] public float distanceReward = 0f;
        [HideInInspector] public float angleReward = 0f;
        [HideInInspector] public float hitsReward = 0f;
        [HideInInspector] public float hurtReward = 0f;
        [HideInInspector] public float blockReward = 0f;

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

    //Stats recorder for logging
    private StatsRecorder statsRecorder;

    void Awake()
    {
        //Initializing default values
        foreach (var agentInfo in AgentList)
        {
            agentInfo.StartingPos = agentInfo.agent.transform.position;
            agentInfo.StartingRot = agentInfo.agent.transform.rotation;
            agentInfo.Rb = agentInfo.agent.GetComponent<Rigidbody>();
        }

        //Initialize stats recorder
        statsRecorder = Academy.Instance.StatsRecorder;
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
        else 
        {
            float distReward = DistanceReward(
                Vector3.Distance(m_playerAgent.agent.transform.localPosition, m_opponentAgent.agent.transform.localPosition)
            );
            m_playerAgent.distanceReward += distReward;
            m_opponentAgent.distanceReward += distReward;
            m_playerAgent.AddReward(distReward);
            m_opponentAgent.AddReward(distReward);
        }

        // Debug.Log($"Environment {m_playerAgent.agent.area.name} -- Distance: {Vector3.Distance(m_playerAgent.agent.transform.localPosition, m_opponentAgent.agent.transform.localPosition)}");
    }

    private float DistanceReward(float distance, float optimal_dist = 0.85f, float a = 1.4f)
    {
        /*
            Function to compute a piecewise log function based on distance between agents
            Will penalize agents for being too far or too close, aiming to keep them at the most optimal striking distance
            Hyperparameters:
            - optimal_dist: Set optimal distance for striking
            - a: Vertical stretching/compressing factor for the log function
        */
        distance = Mathf.Max(distance, 0.01f);  //avoid log(0)
        float reward = distance / optimal_dist; //compute ratio

        if (distance < optimal_dist)
        {
            reward = a * Mathf.Log(reward);
        }
        else if (distance > optimal_dist)
        {
            reward = -a * Mathf.Log(reward);
        }
        else
        {
            //Optimal distance, neutral reward
            reward = 0f;
        }

        // Return normalized reward by max steps for existential reward
        // Debug.Log($"Env: {m_playerAgent.agent.area.name} -- Player Distance Reward: {reward / MAX_STEPS}");
        return reward / MAX_STEPS; 
    }

    public void AngleReward(Team team)
    {
        /*
            Function to handle existential rewards based on the angle to the opponent
            This function should be called from the SparringAgent script when the angle is calculated
        */
        float angleReward = 0f;

        if (team == Team.Player)
        {
            angleReward = ComputeNormalizedAngle(m_playerAgent.agent.transform, m_opponentAgent.agent.transform);
            //Debug.Log($"Env: {m_playerAgent.agent.area.name} -- Player Angle Reward: {angleReward / 1000f}");
            m_playerAgent.angleReward += angleReward / 1000f;
            m_playerAgent.AddReward(angleReward / 1000f);
        }
        else
        {
            angleReward = ComputeNormalizedAngle(m_opponentAgent.agent.transform, m_playerAgent.agent.transform);
            //Debug.Log($"Env: {m_opponentAgent.agent.area.name} -- Opponent Angle Reward: {angleReward / MAX_STEPS * 10f}");
            m_opponentAgent.angleReward += angleReward / 1000f;
            m_opponentAgent.AddReward(angleReward / 1000f);
        }
    }

    private float ComputeNormalizedAngle(Transform playerTransform, Transform opponentTransform) 
    {
        Vector3 forwardDir = playerTransform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head").forward;
        forwardDir.y = 0; //ignore vertical component
        forwardDir = forwardDir.normalized;

        Vector3 direction = opponentTransform.localPosition - playerTransform.localPosition;
        direction.y = 0;
        direction = direction.normalized;

        // Return normalized [-1, 1] negative angle to punish the agent for facing away
        float angle = Vector3.Angle(
            forwardDir,
            direction
        );
        return -(angle / 180f);
    }

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
            m_playerAgent.hurtReward += reward;
            m_opponentAgent.hitsReward -= reward;
            m_playerAgent.AddReward(reward);
            m_opponentAgent.AddReward(-reward);
        }
        else
        {
            m_playerAgent.hitsReward -= reward;
            m_opponentAgent.hurtReward += reward;
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
                m_playerAgent.blockReward += 4.0f;
                m_playerAgent.AddReward(4.0f);

                m_opponentAgent.hitsReward += 1.0f;
                m_opponentAgent.AddReward(1.0f);
            }
            else
            {
                m_playerAgent.hitsReward += 1.0f;
                m_playerAgent.AddReward(1.0f);

                m_opponentAgent.blockReward += 4.0f;
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
            // Stats recorder logging -- partial rewards
            statsRecorder.Add("Rewards/CumulativeDistanceReward", agentInfo.distanceReward);
            statsRecorder.Add("Rewards/CumulativeAngleReward", agentInfo.angleReward);
            statsRecorder.Add("Rewards/CumulativeHitsReward", agentInfo.hitsReward);
            statsRecorder.Add("Rewards/CumulativeHurtReward", agentInfo.hurtReward);
            statsRecorder.Add("Rewards/CumulativeBlockReward", agentInfo.blockReward);

            // Performance metrics -- resetting rewards
            agentInfo.agent.hitsReceived = 0;
            agentInfo.totalReward = 0f;
            agentInfo.distanceReward = 0f;
            agentInfo.angleReward = 0f;
            agentInfo.hitsReward = 0f;
            agentInfo.blockReward = 0f;

            // Reset agent position and rotation
            agentInfo.agent.SetAgentInfo(agentInfo);
            agentInfo.agent.ResetAgent();
            agentInfo.agent.ResetState();
            agentInfo.agent.Respawn();
        }
    }
}