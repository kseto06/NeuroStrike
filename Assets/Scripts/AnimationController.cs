using System.Collections;
using System.Linq;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody rb;
    private Coroutine resetCoroutine;
    public bool isAnimating = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Plays a specified animation on an Agent
    /// </summary>
    /// <param name="animationName">Animation to be played</param>
    /// <param name="overrideAnimation">Boolean to control override a current animation on punishes / interrupted hits</param>
    public void Play(string animationName, bool overrideAnimation = false)
    {
        if (animator == null)
            return;

        if (isAnimating && !overrideAnimation)
            return;

        if (overrideAnimation)
        {
            if (GetCurrentAnimatorStateName() != animationName)
                CancelCurrentAnimation();
        }

        animator.Play(animationName, 0, 0f);
        isAnimating = true;

        float clipLength = GetAnimationLength(animationName);

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        resetCoroutine = StartCoroutine(ResetToIdle(clipLength));
    }

    private IEnumerator ResetToIdle(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (GetCurrentAnimatorStateName() != "Idle")
            animator.CrossFade("Idle", 0.1f);

        isAnimating = false;
        resetCoroutine = null;
    }

    private float GetAnimationLength(string animationName)
    {
    
        var clip = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name == animationName);

        return clip != null ? clip.length : 0.5f;
    }

    private void CancelCurrentAnimation()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        isAnimating = false;
    }

    public string GetCurrentAnimatorStateName()
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned on " + gameObject.name);
            return null;
        }

        //Return current animation name
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        if (clips.Length > 0)
        {
            return clips[0].clip.name;
        }
        else
        {
            return "Idle";
        }
    }
}
