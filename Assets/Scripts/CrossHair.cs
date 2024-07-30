using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    [SerializeField]
    private Animator anim;

    // 총 정확도
    private float gunAccuracy;

    //크로스헤어 활성화 여부를 위한 객체
    [SerializeField]
    private GameObject goCrossHairHUD;
    public GunController gunController;

    public void WalkingAnimation(bool val)
    {
        anim.SetBool("Walking",val);
    }
    public void RunningAnimation(bool val)
    {
        anim.SetBool("Running", val);

    }
    public void CrouchingAnimation(bool val)
    {
        anim.SetBool("Crouching", val);
    }

    public void FineSightAnimation(bool val)
    {
        anim.SetBool("FineSight", val);
    }

    public void FireAnimation()
    {
        if (anim.GetBool("Walking"))
            anim.SetTrigger("Walk_Fire");
        else if (anim.GetBool("Crouching")) 
            anim.SetTrigger("Crouch_Fire");
        else 
            anim.SetTrigger("Idle_Fire");
    }

    public float GetAccuracy()
    {
        if (anim.GetBool("Walking"))
            gunAccuracy = 0.06f;
        else if (anim.GetBool("Crouching"))
            gunAccuracy = 0.015f;
        else if (gunController.GetFineSightMode())
            gunAccuracy = 0.001f;
        else
            gunAccuracy = 0.035f;

        return gunAccuracy;
    }
}
