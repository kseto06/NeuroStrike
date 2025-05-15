using UnityEngine;
using System.Collections;
using System.Linq;

public class ManualAnimationTester : MonoBehaviour
{
    private Animator animator;
    private bool isAnimating = false;
    private Coroutine resetCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("No animation controller assigned");
            return;
        }
    }

    void Update()
    {
        if (isAnimating) return;

        // Movement:
        if (Input.GetKeyDown(KeyCode.W)) 
            PlayAnimation("MediumStepForward");

        if (Input.GetKeyDown(KeyCode.S)) 
            PlayAnimation("StepBackward");

        if (Input.GetKeyDown(KeyCode.A)) 
            PlayAnimation("LeftPivot");

        if (Input.GetKeyDown(KeyCode.D)) 
            PlayAnimation("RightPivot");

        // Combat:
        if (Input.GetKeyDown(KeyCode.I)) 
            PlayAnimation("Block");

        if (Input.GetKeyDown(KeyCode.J))
            PlayAnimation("LeftJab");

        if (Input.GetKeyDown(KeyCode.K)) 
            PlayAnimation("HighRoundhouseKick");

        if (Input.GetKeyDown(KeyCode.L)) 
            PlayAnimation("LeadTeep");
    }

    void PlayAnimation(string animationName)
    {
        animator.Play(animationName, 0, 0f);
        isAnimating = true;

        // Get the animation length
        float clipLength = 1.0f; 
        var clip = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name == animationName);

        if (clip != null)
        {
            clipLength = clip.length;
            Debug.Log($"Animation: {animationName}, Length: {clipLength}s");
        }

        // Reset back to idle after the clip ends
        if (resetCoroutine != null) 
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetIdle(clipLength));
    }

    IEnumerator ResetIdle(float delay) 
    {
        yield return new WaitForSeconds(delay);
        
        // Ensure not already in Idle
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            animator.CrossFade("Idle", 0.1f);
        }
        
        // Reset back to idle logic
        isAnimating = false;
        resetCoroutine = null;
    }
}