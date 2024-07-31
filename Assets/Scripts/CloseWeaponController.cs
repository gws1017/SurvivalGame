using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public abstract class CloseWeaponController : NetworkBehaviour
{

    [SerializeField]
    protected CloseWeapon currentCloseWeapon;

    protected bool isAttack = false;
    protected bool isSwing = false;

    protected RaycastHit hitInfo;

    protected void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack)
            {
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentCloseWeapon.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentCloseWeapon.attackFrontDelay);
        //충돌 판정 활성화
        isSwing = true;
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentCloseWeapon.attackDelay);
        //충돌 판정 비활성화
        isSwing = false;

        yield return new WaitForSeconds(currentCloseWeapon.attackRecoveryTime);
        isAttack = false;
    }

    protected abstract IEnumerator HitCoroutine();
    
    protected bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range))
        {
            return true;
        }
        return false;
    }

    public virtual void CloseWeaponChange(CloseWeapon closeWeapon)
    {
        if (WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        }

        currentCloseWeapon = closeWeapon;
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;

        currentCloseWeapon.transform.localPosition = Vector3.zero;
        currentCloseWeapon.gameObject.SetActive(true);
    }
}
