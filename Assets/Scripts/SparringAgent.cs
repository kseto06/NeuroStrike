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
    private const int MAX_STEPS = 2000; // Maximum steps per episode (20s)

    //Env
    [SerializeField] private GameObject ground;
    [SerializeField] private GameObject area;
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
                m_envController = FindFirstObjectByType<SparringEnvController>();
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
        "RightJab",
        "RightCross",
        "RightHook",
        "LeftUppercut",
        "RightUppercut",
        "LeadUppercut",
        "RearUppercut",
        "RightElbow",
        "LeftElbow",
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
    private Dictionary<string, int> hitboxRewards;

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
    }

    void Start()
    {
        // Animation setup
        animator = GetComponent<Animator>();
        animationController = GetComponent<AnimationController>();

        //Area bounds
        areaBounds = ground.GetComponent<Collider>().bounds;

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
            sensor.AddObservation(this.localPosition.y);

            // Perception params -- distance and angle to opponent
            sensor.AddObservation(Vector3.SignedAngle(Vector3.ProjectOnPlane(this.facingDirection, Vector3.up),
                                Vector3.ProjectOnPlane(this.opponentPosition - this.localPosition, Vector3.up).normalized,
                                Vector3.up) / 180f);

            sensor.AddObservation(Vector3.Distance(this.localPosition, this.opponentPosition)); //Distance to opponent

            // Agent state
            sensor.AddObservation(this.stateIdx);

            // Actions (within each of the agent states)
            if (this.currentState is IMoveTypeState moveTypeState)
            {
                //sensor.AddObservation((float)moveTypeState.GetMoveTypeIndex() / moveTypeState.mapLength); //Normalized move type index
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

            // Maybe add time later

            // // Add distance to opponent
            // float distanceToOpponent = Vector3.Distance(transform.position, opponent.transform.position);
            // sensor.AddObservation(distanceToOpponent);

            // // Add angle to opponent
            // Vector3 directionToOpponent = (opponent.transform.position - transform.position).normalized;
            // float angleToOpponent = Vector3.Angle(transform.forward, directionToOpponent);
            // sensor.AddObservation(angleToOpponent);

            // // Add current animation state
            // sensor.AddObservation(animationController.GetCurrentAnimation());
        }
    }

    public VisibleState GetVisibleState()
    {
        return new VisibleState
        (
            transform.localPosition,
            transform.forward,
            opponent.transform.localPosition,
            currentState,
            idleState,
            hurtState,
            stateMapping.TryGetValue(currentState.GetType().Name, out int stateIdx) ? stateIdx : 0,
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
        // Moving agent -- change action animation based on chosen animation index
        MoveAgent(actionBuffers.DiscreteActions);

        // Add angle reward 
        envController.AngleReward(this.team / MAX_STEPS * 10f);

        // Timestep penalty
        m_agentInfo.AddReward(-1 / MAX_STEPS);
    }

    public void MoveAgent(ActionSegment<int> action)
    {
        // Get the action from the action buffer
        int actionIndex = action[0];
        if (actionIndex < 0 || actionIndex >= stateMapping.Count)
        {
            Debug.LogError("Invalid action index: " + actionIndex);
            this.inputAction = "Idle"; // Default to Idle if invalid action
        }
        else
        {
            this.inputAction = Moveset[actionIndex];
        }
    }

    //Respawning -- set back default positions
    public void Respawn()
    {
        transform.position = m_agentInfo.StartingPos;
        Vector3 pos = transform.position;
        pos.y += 0.2f; // Ensure the agent is above the ground
        transform.position = pos;
        transform.rotation = m_agentInfo.StartingRot;
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
        // hitsReceived = 0;

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

        // Reset triggers
        doingMove = false;
        hitRegistered = false;

        // Reset to idle animation
        animationController.ResetToIdle(0f);
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
    }
}