using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    private Vector3 originPos;

    private Vector3 currentPos;

    [SerializeField]
    private Vector3 limitPos;
    [SerializeField]
    private Vector3 fineSightLimitPos;

    [SerializeField]
    private Vector3 smoothSway;

    //ÄÄÆ÷³ÍÆ®
    [SerializeField]
    private GunController gunController;
    void Start()
    {
        originPos = Vector3.zero;
    }

    void Update()
    {
        TrySway();
    }

    private void TrySway()
    {
        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
            Swaying();
        else
            BackToOriginPos();
    }

    private void Swaying()
    {
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

         // 

        if(!gunController.isFineSightMode)
        {
            mx = Mathf.Lerp(currentPos.x, -mx, smoothSway.x);
            my = Mathf.Lerp(currentPos.y, -my, smoothSway.x);
            float x = Mathf.Clamp(mx, -limitPos.x, limitPos.x);
            float y = Mathf.Clamp(my, -limitPos.y, limitPos.y);
            currentPos.Set(x, y, originPos.z);

        }
        else
        {
            mx = Mathf.Lerp(currentPos.x, -mx, smoothSway.y);
            my = Mathf.Lerp(currentPos.y, -my, smoothSway.y);
            float x = Mathf.Clamp(mx, -fineSightLimitPos.x, fineSightLimitPos.x);
            float y = Mathf.Clamp(my, -fineSightLimitPos.y, fineSightLimitPos.y); 
            currentPos.Set(x, y, originPos.z);
        }

        transform.localPosition = currentPos;
    }

    private void BackToOriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}
