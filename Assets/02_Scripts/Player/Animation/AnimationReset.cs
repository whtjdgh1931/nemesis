using UnityEngine;

public class AnimationReset : StateMachineBehaviour
{
    [SerializeField] string _triggerName;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(_triggerName);
    }
}