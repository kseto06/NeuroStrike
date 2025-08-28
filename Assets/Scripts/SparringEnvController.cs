using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SparringEnvController : MonoBehaviour
{
    [Header("Env Params")]
    private const int MAX_STEPS = 5000;
    [SerializeField] private GameObject area;

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
        [HideInInspector] public float distanceToCenterReward = 0f;
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
            m_playerAgent.agent.EndEpisode();
            m_opponentAgent.agent.EndEpisode();
            ResetEnv();

            //Add/Decrease reward for ELO recording
            /*
            Referencing: 
            The ELO is calculated using the final reward. If an agent wins, the final reward must be positive. If an agent loses, the final reward must be negative. Otherwise, 0 indicates a draw. 
            Final reward is the very last reward that the agent receives
            */
            if (m_playerAgent.totalReward > m_opponentAgent.totalReward)
            {
                m_playerAgent.AddReward(1.0f);
                m_opponentAgent.AddReward(-1.0f);
            }
            else if (m_playerAgent.totalReward < m_opponentAgent.totalReward)
            {
                m_playerAgent.AddReward(-1.0f);
                m_opponentAgent.AddReward(1.0f);
            }
            else
            {
                m_playerAgent.AddReward(0.0f);
                m_opponentAgent.AddReward(0.0f);
            }

            episodeCount++;
        }

        // Debug.Log($"Environment {m_playerAgent.agent.area.name} -- Distance: {Vector3.Distance(m_playerAgent.agent.transform.localPosition, m_opponentAgent.agent.transform.localPosition)}");
    }

    public float DistanceReward(SparringAgent currentAgent, SparringAgent opponentAgent)
    {
        /*
            Function to compute the modulo-existential reward based on whether the agent is moving toward the opponent.
            The reward is calculated by taking the dot product of the agent's normalized velocity
            with the normalized direction vector toward the opponent.

            Args:
            - currentAgent: The SparringAgent for which we check to see if it is moving/hurt
        */

        // Calculate delta position and direction to opponent
        Vector3 deltaPosition = currentAgent.transform.localPosition - currentAgent.previousPosition;
        Vector3 directionToOpponent = opponentAgent.transform.localPosition - currentAgent.transform.localPosition;

        // Prevent division by zero or very small values
        float deltaPositionNorm = deltaPosition.magnitude;
        float directionToOpponentNorm = directionToOpponent.magnitude;

        // If norms are too small or animation is not moving/hurt, return 0 reward
        if (deltaPositionNorm < 1e-6 || directionToOpponentNorm < 1e-6 ||
            !(currentAgent.currentState is MovingState || currentAgent.currentState is HurtState))
        {
            return 0f;
        }

        deltaPosition /= deltaPositionNorm;
        directionToOpponent /= directionToOpponentNorm;
        return Vector3.Dot(deltaPosition, directionToOpponent) * 0.5f;
    }

    public float DistanceToCenterReward(SparringAgent currentAgent)
    {
        /*
            Function to compute the reward based on an agent's distance to the center of the arena.
            Encourages agents to stay near the center

            Args:
            - currentAgent: The individual SparringAgent for which we reference positions and states
        */
        //Calculating distances to the center from current and previous positions
        Vector3 center = currentAgent.area.transform.InverseTransformPoint(currentAgent.ground.GetComponent<Collider>().bounds.center); //convert to local space

        Vector3 currentPos = currentAgent.transform.localPosition;
        Vector3 previousPos = currentAgent.previousPosition;

        float currentDistance = Mathf.Abs(Vector2.Distance(new Vector2(currentPos.x, currentPos.z), new Vector2(center.x, center.z)));
        float previousDistance = Mathf.Abs(Vector2.Distance(new Vector2(previousPos.x, previousPos.z), new Vector2(center.x, center.z)));

        if (currentDistance < previousDistance && (currentAgent.currentState is MovingState || currentAgent.currentState is HurtState))
        {
            //Moving towards center, return positive reward
            return (previousDistance - currentDistance) * 1.5f;
        }
        else if (currentDistance > previousDistance && (currentAgent.currentState is MovingState || currentAgent.currentState is HurtState))
        {
            //Moving away from center, return negative reward
            return -0.1f;
        }
        else
        {
            //No movement detected
            return 0f;
        }
    }

    public float AngleReward(SparringAgent currentAgent, SparringAgent opponentAgent)
    {
        /*
            Function to compute the angle reward based on the agent's facing direction and the direction towards the opponent.
            Encourages agents to face their opponent.

            Args:
            - currentAgent: The SparringAgent for which we compute the angle reward
        */
        if (currentAgent.currentState is not MovingState)
        {
            return 0f;
        }

        //Calculate angle based on forward direction and distance to opponent
        Vector3 forwardDir = currentAgent.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head").forward;
        forwardDir.y = 0;
        forwardDir.Normalize();

        Vector3 directionToOpponent = opponentAgent.transform.position - currentAgent.transform.position;
        directionToOpponent.y = 0;
        directionToOpponent.Normalize();

        if (forwardDir.magnitude < 1e-6f || directionToOpponent.magnitude < 1e-6f)
        {
            return 0f;
        }

        float reward = Vector3.Dot(forwardDir, directionToOpponent);
        return reward;
    }

    public void AttackLandedReward(Team hitTeam, string hitType)
    {
        /*
            Function to handle rewards for attacks that land on a target
            Args:
            - hitTeam: Team that was hit (Player or Opponent)
            - hitType: Type of hit (e.g. "HeadHit", "LegHit")
            This function should be called from Hitbox.cs when a hit is detected
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
            hitTeam: Team -- Team that was hit (Player or Opponent)
            isBlocking: bool -- Whether the attack was blocked
            This function is called from the Hitbox script when a block is detected
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
            statsRecorder.Add("Rewards/CumulativeDistanceToCenterReward", agentInfo.distanceToCenterReward);
            statsRecorder.Add("Rewards/CumulativeHitsReward", agentInfo.hitsReward);
            statsRecorder.Add("Rewards/CumulativeHurtReward", agentInfo.hurtReward);
            statsRecorder.Add("Rewards/CumulativeBlockReward", agentInfo.blockReward);

            // Performance metrics -- resetting rewards
            agentInfo.agent.hitsReceived = 0;
            agentInfo.totalReward = 0f;
            agentInfo.distanceReward = 0f;
            agentInfo.distanceToCenterReward = 0f;
            //agentInfo.angleReward = 0f;
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