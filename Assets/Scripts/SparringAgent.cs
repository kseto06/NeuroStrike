using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public enum Team
{
    Player = 0,
    Opponent = 1
}

public class SparringAgent : Agent
{
    // Agent setup
    public SparringAgent opponent;
    [HideInInspector] public Team team;
    private BehaviorParameters behaviorParameters;
    private const int MAX_STEPS = 2500; // Maximum steps per episode (20s)

    //Env
    [SerializeField] private GameObject ground;
    [SerializeField] public GameObject area;
    [HideInInspector] private Bounds areaBounds;

    public SparringEnvController envController
    {
        get
        {
            if (m_envController)
            {
                return m_envController;
            }
            else
            {
                m_envController = transform.root.GetComponentInChildren<SparringEnvController>();
                return m_envController;
            }
        }
    }
    private SparringEnvController m_envController;
    SparringEnvController.AgentInfo m_agentInfo;

    // Moveset
    private List<string> Moveset = new List<string>();

    private List<string> HurtList = new List<string>
    {
        "BodyHit",
        "HeadHit",
        "RightSideHit",
        "LeftSideHit"
    };

    private List<string> MoveList = new List<string> {
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

    private List<string> AttackList = new List<string>
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

    [Header("Animations")]
    public AnimationController animationController;
    private Animator animator;
    public bool isAnimating = false; //Keeping track of animating state
    private Coroutine resetCoroutine;
    public bool hitRegistered = false;

    [Header("Rigidbody")]
    public Rigidbody rb;

    [Header("Hurtboxes & Hitboxes")]
    [SerializeField] private Hurtbox hurtbox;
    public int hitsReceived = 0;
    [SerializeField] private Hitbox hitbox;

    [Header("FSM")]
    //Current state of the agent
    private AgentState currentState;
    public string inputAction;
    public bool doingMove = false;

    // Init all possible agent states
    private IdleState idleState;
    private BlockingState blockState;
    private AttackingState attackState;
    private HurtState hurtState;
    private MovingState moveState;

    // State Mapping
    private Dictionary<string, int> stateMapping = new Dictionary<string, int> {
        { "IdleState", 0 },
        { "AttackingState", 1 },
        { "BlockingState", 2 },
        { "MovingState", 3 },
        { "HurtState", 4 }
    };


    public override void Initialize()
    {
        // Get hitbox and hurtbox components
        hitbox = GetComponentInChildren<Hitbox>();
        hurtbox = GetComponentInChildren<Hurtbox>();

        // Construct full moveset (action space)
        Moveset.Add("Idle");
        Moveset.Add("Block");
        Moveset.AddRange(MoveList);
        Moveset.AddRange(AttackList);

        // Behaviour params and team setup for self-play
        behaviorParameters = GetComponent<BehaviorParameters>();
        team = (Team)behaviorParameters.TeamId;

        if (behaviorParameters.TeamId == (int)Team.Player)
        {
            team = Team.Player;
        }
        else
        {
            team = Team.Opponent;
        }

        //Area bounds
        ground.SetActive(true);
        areaBounds = ground.GetComponent<Collider>().bounds;

        if (ground == null)
        {
            Debug.LogError("Ground reference not set in inspector");
        }
        else if (ground.GetComponent<Collider>() == null)
        {
            Debug.LogError("Ground object has no collider: ", ground);
        }
    }

    void Start()
    {
        // Animation setup
        animator = GetComponent<Animator>();
        animationController = GetComponent<AnimationController>();

        if (animationController == null)
        {
            Debug.LogError("Missing AnimationController on " + gameObject.name);
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("No animation controller assigned");
            return;
        }

        // Rigidbody setup
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // FSM setup
        // Initialize agent states
        idleState = new IdleState(this, "");
        blockState = new BlockingState(this, "");
        attackState = new AttackingState(this, "");
        hurtState = new HurtState(this, "");
        moveState = new MovingState(this, "");

        currentState = idleState; //Start in idle state
        currentState.Enter(null);
    }

    void Update()
    {
        //Update FSM state
        currentState.action = inputAction;

        AgentState nextState = currentState.Process();
        if (nextState != currentState)
        {
            currentState.Exit(nextState);
            nextState.Enter(currentState);
            currentState = nextState;
            return;
        }

        if (!string.IsNullOrEmpty(inputAction))
        {
            // Allow interrupt only if current state allows it
            if (currentState.CanBeInterrupted(inputAction))
            {
                AgentState stateFromInput = GetStateFromAction(inputAction);
                currentState.Exit(stateFromInput);
                stateFromInput.Enter(currentState);
                currentState = stateFromInput;
            }

            inputAction = null;
        }
    }

    void OnAnimatorMove()
    {
        if (animator == null || rb == null)
        {
            Debug.LogError("Animator or Rigidbody is not initialized on: " + gameObject.name);
            return;
        }

        if (animator.applyRootMotion)
        {
            // Apply animator-driven movement to the Rigidbody
            rb.MovePosition(rb.position + animator.deltaPosition);
            rb.MoveRotation(rb.rotation * animator.deltaRotation);
        }
    }

    private AgentState GetStateFromAction(string action)
    {
        if (this.HurtList.Contains(action))
        {
            return new HurtState(this, action);
        }
        else if (this.MoveList.Contains(action))
        {
            return new MovingState(this, action);
        }
        else if (this.AttackList.Contains(action))
        {
            return new AttackingState(this, action);
        }
        else if (action == "Block")
        {
            return new BlockingState(this, action);
        }
        else
        {
            return new IdleState(this, action);
        }
    }

    // Observations
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the player's own data to observations
        VisibleState playerVisibleState = GetVisibleState();
        playerVisibleState.AddObservations(sensor);

        // Add the opponent's data to observations
        VisibleState opponentVisibleState = opponent.GetVisibleState();
        opponentVisibleState.AddObservations(sensor);
    }

    public struct VisibleState
    {
        // Fields
        public Vector3 localPosition;
        public Vector3 facingDirection;
        public Vector3 opponentPosition;

        public AgentState currentState;
        public AgentState idleState;
        public AgentState hurtState;

        public int stateIdx;
        public int actionIdx;
        public bool doingMove;
        public int hitsReceived;

        // Constructor
        public VisibleState(
            //Spatial variables
            Vector3 localPosition,
            Vector3 facingDirection,
            Vector3 opponentPosition,

            //State
            AgentState currentState,
            AgentState idleState,
            AgentState hurtState,
            int stateIdx,

            //Actions
            int actionIdx,
            bool doingMove,

            //Hits
            int hitsReceived
        )
        {
            this.localPosition = localPosition;
            this.facingDirection = facingDirection;
            this.opponentPosition = opponentPosition;
            this.currentState = currentState;
            this.idleState = idleState;
            this.hurtState = hurtState;
            this.stateIdx = stateIdx;
            this.actionIdx = actionIdx;
            this.doingMove = doingMove;
            this.hitsReceived = hitsReceived;
        }

        public void AddObservations(VectorSensor sensor)
        {
            // Agent position 
            sensor.AddObservation(this.localPosition.x);
            // sensor.AddObservation(this.localPosition.y);
            sensor.AddObservation(this.localPosition.z);

            // Perception params -- distance and angle to opponent
            Vector3 forwardDir = this.facingDirection;
            forwardDir.y = 0; //ignore vertical component
            forwardDir = forwardDir.normalized;

            Vector3 direction = this.opponentPosition - this.localPosition;
            direction.y = 0;
            direction = direction.normalized;

            // Normalized signed angle to opponent for rotational and directional context
            float signedAngle = Vector3.SignedAngle(
                forwardDir,
                direction,
                Vector3.up
            );
            sensor.AddObservation(signedAngle / 180f);

            // Distance to opponent
            sensor.AddObservation(Vector3.Distance(this.localPosition, this.opponentPosition)); //Distance to opponent

            // Agent state
            sensor.AddObservation(this.stateIdx);

            // Actions (within each of the agent states)
            if (this.currentState is IMoveTypeState moveTypeState)
            {
                sensor.AddObservation((float)moveTypeState.GetMoveTypeIndex());
            }
            else
            {
                //NOTE: Blocking and Idle states only have one action (no further move types)
                sensor.AddObservation(0f);
            }

            if (this.currentState != this.idleState && this.currentState != this.hurtState)
            {
                this.doingMove = true;
            }
            else
            {
                this.doingMove = false;
            }
            sensor.AddObservation(this.doingMove);

            // Hits
            sensor.AddObservation(this.hitsReceived);
        }
    }

    public VisibleState GetVisibleState()
    {
        return new VisibleState
        (
            transform.localPosition,
            transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head").forward,
            opponent.transform.localPosition,
            currentState,
            idleState,
            hurtState,
            stateMapping.TryGetValue(currentState?.GetType().Name ?? "", out int stateIdx) ? stateIdx : 0,
            currentState is IMoveTypeState moveTypeState ? moveTypeState.GetMoveTypeIndex() : 0,
            doingMove,
            hitsReceived
        );
    }

    //Heuristic for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // Default action is Idle

        // Movement:
        if (Input.GetKeyDown(KeyCode.W)) 
            discreteActionsOut[0] = Moveset.IndexOf("MediumStepForward");

        if (Input.GetKeyDown(KeyCode.S)) 
            discreteActionsOut[0] = Moveset.IndexOf("StepBackward");

        if (Input.GetKeyDown(KeyCode.A)) 
            discreteActionsOut[0] = Moveset.IndexOf("MediumLeftSideStep");

        if (Input.GetKeyDown(KeyCode.D)) 
            discreteActionsOut[0] = Moveset.IndexOf("MediumRightSideStep");

        // Combat:
        if (Input.GetKeyDown(KeyCode.I)) 
            discreteActionsOut[0] = Moveset.IndexOf("Block");

        if (Input.GetKeyDown(KeyCode.J))
            discreteActionsOut[0] = Moveset.IndexOf("LeftJab");

        if (Input.GetKeyDown(KeyCode.K)) 
            discreteActionsOut[0] = Moveset.IndexOf("HighRoundhouseKick");

        if (Input.GetKeyDown(KeyCode.L)) 
            discreteActionsOut[0] = Moveset.IndexOf("LeadTeep");

        if (Input.GetKeyDown(KeyCode.M)) 
            discreteActionsOut[0] = Moveset.IndexOf("SpinningHookKick");

        if (Input.GetKeyDown(KeyCode.N)) 
            discreteActionsOut[0] = Moveset.IndexOf("ComboPunch");

        if (Input.GetKeyDown(KeyCode.P))
            discreteActionsOut[0] = Moveset.IndexOf("LowKick"); 
    }

    // Sampling Actions and Getting Rewards
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Short delay to allow env processing to prevent immediate action
        if (envController.totalSteps < 50)
        {
            return;
        }

        // Moving agent -- change action animation based on chosen animation index
        MoveAgent(actionBuffers.DiscreteActions);

        // Add angle reward 
        envController.AngleReward(this.team);
    }

    public void MoveAgent(ActionSegment<int> action)
    {
        // Get the action from the action buffer
        int actionIndex = action[0];
        if (actionIndex < 0 || actionIndex >= Moveset.Count)
        {
            Debug.LogError("Invalid action index: " + actionIndex);
            this.inputAction = "Idle"; // Default to Idle if invalid action
        }
        else
        {
            this.inputAction = Moveset[actionIndex];
        }
    }

    public Vector3 GetRandomSpawnPos()
    {
        bool foundSpawn = false;
        var randomSpawn = Vector3.zero;
        const int maxAttempts = 50;
        const float spawnAreaMarginMultipler = 0.65f;

        //error checking
        if (ground == null)
        {
            Debug.LogError("ground is null in GetRandomSpawnPos");
        }

        // Ensure bounds are calculated
        if (areaBounds.size == Vector3.zero)
        {
            Debug.LogWarning("areaBounds not set, calculating from ground collider");
            var collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
                ground.SetActive(true);
                areaBounds = collider.bounds;
                Debug.Log($"AreaBounds info {areaBounds.ToString("F4")}");
            }
            else
            {
                Debug.LogError("collider not found on area");
                return Vector3.zero;
            }
        }

        //Getting the spawn position within max attempts
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            randomSpawn = ground.transform.position + new Vector3(
                UnityEngine.Random.Range(-areaBounds.extents.x * spawnAreaMarginMultipler, areaBounds.extents.x * spawnAreaMarginMultipler),
                0.1f,
                UnityEngine.Random.Range(-areaBounds.extents.z * spawnAreaMarginMultipler, areaBounds.extents.z * spawnAreaMarginMultipler)
            );

            if (randomSpawn != null)
            {
                foundSpawn = true;
                break;
            }
        }

        //Report invalid spawn point and set default spawn positions for player and opponent
        if (!foundSpawn)
        {
            if (behaviorParameters != null && behaviorParameters.TeamId == (int)Team.Player)
            {
                //Player spawn point
                randomSpawn = area.transform.position + new Vector3(2.3f, 0.1f, 0.371f);
                Debug.LogWarning("Invalid spawn point, resetting Player default spawn");
            }
            else if (behaviorParameters != null && behaviorParameters.TeamId == (int)Team.Opponent)
            {
                //Opponent spawn point
                randomSpawn = area.transform.position + new Vector3(-1f, 0.1f, 0.371f);
                Debug.LogWarning("Invalid spawn point, resetting Opponent default spawn");
            }

            // Transform local offset to world space
            randomSpawn = area.transform.TransformPoint(randomSpawn);

            // Clamp to area bounds using ClosestPoint
            randomSpawn = areaBounds.ClosestPoint(randomSpawn);
        }

        return randomSpawn;
    }

    public Quaternion GetRandomSpawnRot()
    {
        // Randomly choose a Y-axis rotation between 0 and 360 degrees
        float randomY = UnityEngine.Random.Range(0f, 360f);
        return Quaternion.Euler(0f, randomY, 0f);
    }

    //Domain randomization respawning
    public void Respawn()
    {
        transform.position = m_agentInfo.StartingPos;
        Vector3 pos = GetRandomSpawnPos();
        pos.y += 0.2f; // Ensure the agent is above the ground
        transform.position = pos;

        transform.rotation = GetRandomSpawnRot();
        ResetState();
    }

    public void SetAgentInfo(SparringEnvController.AgentInfo agentInfo)
    {
        m_agentInfo = agentInfo;
    }

    public void ResetAgent()
    {
        /*
            This function resets the agent's default physical properties 
        */

        // Reset hits received
        hitsReceived = 0;

        // Reset rigidbody
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void ResetState()
    {
        /*
            This function resets the agent's default state and animation (i.e. Idle)
        */

        // Reset agent state
        currentState = idleState;
        currentState.Enter(null);
        inputAction = "Idle";
        isAnimating = false;
        hitbox.DeactivateHitboxes();

        // Reset triggers
        doingMove = false;
        hitRegistered = false;

        // Hard reset to idle animation
        // animationController.ResetToIdle(0f);
        animationController.Play("Idle", overrideAnimation: true);
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
    }
}