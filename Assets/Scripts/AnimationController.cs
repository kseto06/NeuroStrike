using System.Collections;
using System.Linq;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private Coroutine resetCoroutine;
    private bool isAnimating = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public void Play(string animationName)
    {
        if (isAnimating || animator == null) return;

        animator.Play(animationName, 0, 0f);
        isAnimating = true;

        float clipLength = 1.0f;
        var clip = animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name == animationName);

        if (clip != null)
        {
            clipLength = clip.length;
            Debug.Log($"Animation: {animationName}, Length: {clipLength}s");

            if (animationName == "Block")
                rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        }

        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetToIdle(clipLength));
    }

    private IEnumerator ResetToIdle(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            animator.CrossFade("Idle", 0.1f);

        isAnimating = false;
        resetCoroutine = null;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
