using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SparringAgent : Agent
{
    // Agent setup
    public SparringAgent opponent;

    //Env
    private GameObject ground;
    private GameObject area;
    [HideInInspector] private Bounds areaBounds;

    // Moveset
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

    [Header("Rigidbody")]
    public Rigidbody rb;

    [Header("Hurtboxes & Hitboxes")]
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private Hitbox hitbox;
    private Dictionary<string, int> hitboxRewards;

    [Header("FSM")]
    //Current state of the agent
    private AgentState currentState;
    public string inputAction;
    
    // Init all possible agent states
    private IdleState idleState;
    private BlockingState blockState;
    private AttackingState attackState;
    private HurtState hurtState;
    private MovingState moveState;


    public override void Initialize() 
    {
        hitbox = GetComponentInChildren<Hitbox>();
        hurtbox = GetComponentInChildren<Hurtbox>();
    }

    void Start()
    {
        // Animation setup
        animator = GetComponent<Animator>();
        animationController = GetComponent<AnimationController>();

        if (animationController == null) {
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

        // RL setup
        hitboxRewards = new Dictionary<string, int>
        {
            { "LeftJab", 5 },
            { "LeftCross", 6 },
            { "LeftHook", 7 },
            { "RightJab", 5 },
            { "RightCross", 6 },
            { "RightHook", 7 },
            { "LeftUppercut", 8 },
            { "RightUppercut", 8 },
            { "LeadUppercut", 8 },
            { "RearUppercut", 8 },
            { "RightElbow", 10 },
            { "LeftElbow", 10 },
            { "RightUpwardsElbow", 12 },
            { "LeadKnee", 9 },
            { "RearKnee", 9 },
            { "LowKick", 10 },
            { "MidRoundhouseKick", 12 },
            { "HighRoundhouseKick", 15 },
            { "SpinningHookKick", 18 },
            { "SideKick", 14 },
            { "LeadTeep", 8 },
            { "RearTeep", 9 },
            { "ComboPunch", 20 },
            { "Block", 0 },
            { "StepBackward", 0 },
            { "ShortStepForward", 0 },
            { "MediumStepForward", 0 },
            { "LongStepForward", 0 },
            { "ShortRightSideStep", 0 },
            { "ShortLeftSideStep", 0 },
            { "MediumRightSideStep", 0 },
            { "MediumLeftSideStep", 0 },
            { "LongRightSideStep", 0 },
            { "LongLeftSideStep", 0 },
            { "LeftPivot", 0 },
            { "RightPivot", 0 },
            { "Idle", 0 }
        };
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

    // //RL Functions:
    // public struct VisibleState(
    //     //Spatial awareness
    //     public Vector3 localPosition;
    //     public Vector3 localRotation;
    //     public float distanceToOpponent;
    //     public float angleToOpponent;

    //     // Combat state
    //     public bool isBlocking;
    //     public bool isAttacking;
    //     public bool isHurt;
    //     public bool isMoving;
    //     public string currentAnimation;

    //     //Timing, Cooldowns
        
    // )

    public Vector3 GetRandomSpawnPos()
    {
        bool foundSpawn = false;
        var randomSpawn = Vector3.zero;
        while (!foundSpawn) {
            randomSpawn = ground.transform.position + new Vector3(
                UnityEngine.Random.Range(-areaBounds.extents.x * 0.9f, areaBounds.extents.x * 0.9f),
                0f,
                UnityEngine.Random.Range(-areaBounds.extents.z * 0.9f, areaBounds.extents.z * 0.9f)
            );

            if (Vector3.Distance(randomSpawn, opponent.transform.position) > 2f)
            {
                foundSpawn = true;
            }
        }

        return randomSpawn;
    }

    public void AddObservations(VectorSensor sensor) {}

    public void MoveAgent(ActionSegment<int> action) {
        if (isAnimating)
            return;

        // Action
    }

    private void ResetState() {}

    public void Respawn() {
        transform.position = GetRandomSpawnPos();
        ResetState();
    }

    public override void OnEpisodeBegin() {}

    public override void CollectObservations(VectorSensor sensor) {}

    // public VisibleState GetVisibleState() {}


}