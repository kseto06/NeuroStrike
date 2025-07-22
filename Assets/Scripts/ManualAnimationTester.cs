using UnityEngine;
using System.Collections;
using System.Linq;

public class ManualAnimationTester : MonoBehaviour
{
    private AnimationController animationController;
    private Animator animator;

    private Rigidbody rb;

    private SparringAgent agent;

    void Start()
    {
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

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        agent = GetComponent<SparringAgent>();
    }

    void Update()
    {
        // Movement:
        if (Input.GetKeyDown(KeyCode.W)) 
            agent.inputAction = "MediumStepForward";

        if (Input.GetKeyDown(KeyCode.S)) 
            agent.inputAction = "StepBackward";

        if (Input.GetKeyDown(KeyCode.A)) 
            agent.inputAction = "MediumLeftSideStep";

        if (Input.GetKeyDown(KeyCode.D)) 
            agent.inputAction = "MediumRightSideStep";

        // Combat:
        if (Input.GetKeyDown(KeyCode.I)) 
            agent.inputAction = "Block";

        if (Input.GetKeyDown(KeyCode.J))
            agent.inputAction = "LeftJab";

        if (Input.GetKeyDown(KeyCode.K)) 
            agent.inputAction = "HighRoundhouseKick";

        if (Input.GetKeyDown(KeyCode.L)) 
            agent.inputAction = "LeadTeep";

        if (Input.GetKeyDown(KeyCode.M)) 
            agent.inputAction = "SpinningHookKick";

        if (Input.GetKeyDown(KeyCode.N)) 
            agent.inputAction = "ComboPunch";

        if (Input.GetKeyDown(KeyCode.P)) 
            agent.inputAction = "LowKick";

    }
}