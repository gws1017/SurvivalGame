using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AxeController : CloseWeaponController
{
    public static bool isActivate = false;


    void Update()
    {
        if (!IsOwner) return;
        if (isActivate)
            TryAttack();
    }
    protected override IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;

        }
    }

    public override void CloseWeaponChange(CloseWeapon closeWeapon)
    {
        base.CloseWeaponChange(closeWeapon);
        isActivate = true;
    }
}
