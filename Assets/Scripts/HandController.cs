using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandController : NetworkBehaviour
{
    [SerializeField]
    private Hand currentWeapon;

    private bool isAttack = false;
    private bool isSwing = false;

    private RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        TryAttack();
    }

    private void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if(!isAttack)
            {
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentWeapon.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentWeapon.attackFrontDelay);
        //충돌 판정 활성화
        isSwing = true;
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentWeapon.attackDelay);
        //충돌 판정 비활성화
        isSwing = false;

        yield return new WaitForSeconds(currentWeapon.attackRecoveryTime);
        isAttack = false;
    }

    IEnumerator HitCoroutine()
    {
        while(isSwing)
        {
            if(CheckObject())
            {
                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
            
        }
    }

    private bool CheckObject()
    {
        if(Physics.Raycast(transform.position,transform.forward, out hitInfo,currentWeapon.range)) 
        {
            return true;
        }
        return false;
    }
}
