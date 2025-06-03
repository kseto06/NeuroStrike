using System.Collections.Generic;
using UnityEngine;

public class HitboxActivator : StateMachineBehaviour
{
    public string animationName;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hitbox = animator.GetComponent<Hitbox>();
        if (hitbox != null) 
        {
            hitbox.ActivateHitboxes(animationName);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var hitbox = animator.GetComponent<Hitbox>();
        if (hitbox != null) 
        {
            hitbox.DeactivateHitboxes();
        }
    }
}