using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetAnimatorBool : StateMachineBehaviour
{
    public string isInteractingBool = "isInteracting";
    public bool isInteractingStatus = false;

    public string isRotatingWithRootMotionBool = "isRotatingWithRootMotion";
    public bool isRotatingWithRootMotionStatus = false;

    public string canRotateBool = "canRotate";
    public bool canRotateStatus = true;

    public string canMoveBool = "canMove";
    public bool canMoveStatus = true;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetBool(isInteractingBool, isInteractingStatus);
        animator.SetBool(isRotatingWithRootMotionBool, isRotatingWithRootMotionStatus);
        animator.SetBool(canRotateBool, canRotateStatus);
        animator.SetBool(canMoveBool, canMoveStatus);
    }
}
