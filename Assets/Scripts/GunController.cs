using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField]
    private Gun currentGun;

    private float currentFireRate;

    private bool isReload = false;
    private bool isFineSightMode = false; //조준 상태

    [SerializeField]
    private Vector3 originPos;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFineSight();
    }
    private void GunFireRateCalc()
    {
        if(currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    private void TryFire()
    {
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCnt > 0)
                Shoot();
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCorutine());
            }
        }
    }

    private void Shoot()
    {
        currentGun.currentBulletCnt--;
        currentFireRate = currentGun.fireRate;
        PlaySE(currentGun.fireSound);
        currentGun.muzzleFlash.Play();

        StopAllCoroutines();
        StartCoroutine(RetroActionCorutine());

        Debug.Log("Shoot");
    }
    private void TryReload()
    {
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCnt < currentGun.reloadBulletCnt)
        {
            CancelFineSight();
            StartCoroutine(ReloadCorutine());
        }
    }
    IEnumerator ReloadCorutine()
    {
        if(currentGun.carryBulletCnt > 0)
        {
            isReload = true;
            currentGun.anim.SetTrigger("Reload");

            yield return new WaitForSeconds(currentGun.reloadTime);

            if(currentGun.carryBulletCnt >= currentGun.reloadBulletCnt)
            {
                currentGun.currentBulletCnt = currentGun.reloadBulletCnt;
                currentGun.carryBulletCnt -= currentGun.reloadBulletCnt;
            }
            else
            {
                currentGun.currentBulletCnt = currentGun.carryBulletCnt;
                currentGun.carryBulletCnt = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("bullet is nothing");
        }
    }

    private void TryFineSight()
    {
        if(Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);

        if(isFineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCorutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCorutine());
        }
    }

    IEnumerator FineSightActivateCorutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos,0.2f);
            yield return null;
        }
    }

    IEnumerator FineSightDeActivateCorutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    IEnumerator RetroActionCorutine()
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce,originPos.y,originPos.z); //90도 돌려서 z축이아닌 x축사용
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        if (!isFineSightMode)
        {
            currentGun.transform.localPosition = originPos;

            while(currentGun.transform.localPosition.x <=currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null; 
            }

            while(currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }
        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
