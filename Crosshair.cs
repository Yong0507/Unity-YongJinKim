using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    // 크로스헤어 상태에 따른 총의 정확도
    private float gunAccuracy;

    // 크로스헤어 비활성화를 위한 부모 객체 -> 도끼를 들어야 할 수도 있음
    [SerializeField]
    private GameObject go_CrosshairHUD;
    [SerializeField]
    private GunController theGunController;

    public void WalkingAnimation(bool _flag)
    {
        animator.SetBool("Walking", _flag);
    }

    public void RunningAnimation(bool _flag)
    {
        animator.SetBool("Running", _flag);
    }

    public void CrouchingAnimation(bool _flag)
    {
        animator.SetBool("Crouching", _flag);
    }

    public void FineSightAnimation(bool _flag)
    {
        animator.SetBool("FineSight", _flag);
    }

    public void FireAnimation()
    {
        if (animator.GetBool("Walking"))
            animator.SetTrigger("Walk_Fire");
        else if (animator.GetBool("Crouching"))
            animator.SetTrigger("Crouch_Fire");
        else
            animator.SetTrigger("Idle_Fire");

    }

    public float GetAccuracy()
    {
        if (animator.GetBool("Walking"))
            gunAccuracy = 0.06f;
        else if (animator.GetBool("Crouching"))
            gunAccuracy = 0.015f;
        else if (theGunController.GetFineSightMode())
            gunAccuracy = 0.001f;
        else
            gunAccuracy = 0.035f;

        return gunAccuracy;
    }
}
