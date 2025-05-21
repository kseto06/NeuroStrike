using UnityEngine;
using System.Collections;
using System.Linq;

public class ManualAnimationTester : MonoBehaviour
{
    private AnimationController animationController;
    private Animator animator;
    private bool isAnimating = false;
    private Coroutine resetCoroutine;

    private Rigidbody rb;

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
    }

    void Update()
    {
        if (isAnimating)
            return;

        // Movement:
        if (Input.GetKeyDown(KeyCode.W)) 
            animationController.Play("MediumStepForward");

        if (Input.GetKeyDown(KeyCode.S)) 
            animationController.Play("StepBackward");

        if (Input.GetKeyDown(KeyCode.A)) 
            animationController.Play("MediumLeftSideStep");

        if (Input.GetKeyDown(KeyCode.D)) 
            animationController.Play("MediumRightSideStep");

        // Combat:
        if (Input.GetKeyDown(KeyCode.I)) 
            animationController.Play("Block");

        if (Input.GetKeyDown(KeyCode.J))
            animationController.Play("LeftJab");

        if (Input.GetKeyDown(KeyCode.K)) 
            animationController.Play("HighRoundhouseKick");

        if (Input.GetKeyDown(KeyCode.L)) 
            animationController.Play("LeadTeep");

        if (Input.GetKeyDown(KeyCode.M)) 
            animationController.Play("SpinningHookKick");
    }
}