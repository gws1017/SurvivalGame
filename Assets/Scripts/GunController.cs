using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public static bool isActivate = true;
    [SerializeField]
    private Gun currentGun;

    //연사 속도
    private float currentFireRate;

    private bool isReload = false; //재장전 중인가?
    [HideInInspector]
    public bool isFineSightMode = false; //조준 상태

    private Vector3 originPos;

    private AudioSource audioSource;

    private RaycastHit hitInfo;

    //컴포넌트
    [SerializeField]
    private Camera Cam;
    private CrossHair crossHair;

    //피격이펙트
    [SerializeField]
    private GameObject hitEffectPrefab;

    private void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        crossHair = FindObjectOfType<CrossHair>();

        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;
    }

    // Update is called once per frame
    void Update()
    {
        if(isActivate)
        {
            GunFireRateCalc();
            TryFire();
            TryReload();
            TryFineSight();
        }
    }

    //발사 속도 재계산
    private void GunFireRateCalc()
    {
        if(currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    //발사 시도
    private void TryFire()
    {
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    //발사전 처리
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

    //발사 후 처리
    private void Shoot()
    {
        crossHair.FireAnimation();
        currentGun.currentBulletCnt--;
        currentFireRate = currentGun.fireRate;
        PlaySE(currentGun.fireSound);
        currentGun.muzzleFlash.Play();
        Hit();
        StopAllCoroutines();
        StartCoroutine(RetroActionCorutine());
    }

    private void Hit()
    {
        float Accur = crossHair.GetAccuracy()+ crossHair.GetAccuracy();
        float randAccurX = Random.Range(-Accur, Accur);
        float randAccurY = Random.Range(-Accur, Accur);

        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward + 
            new Vector3(randAccurX,randAccurY, 0f)
            ,out hitInfo, currentGun.range))
        {
            GameObject clone = Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
        
    }

    //재장전 시도
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

    public void CancelReload()
    {
        if (isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }
    //정조준 시도
    private void TryFineSight()
    {
        if(Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    //정조준 취소
    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    //정조준 로직
    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);
        crossHair.FineSightAnimation(isFineSightMode);

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

    //정조준 활성화
    IEnumerator FineSightActivateCorutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos,0.2f);
            yield return null;
        }
    }

    //정조준 비활성화
    IEnumerator FineSightDeActivateCorutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    //반동 적용
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

    public Gun GetGun()
    {
        return currentGun;
    }

    public bool GetFineSightMode() { return isFineSightMode; }

    public void GunChange(Gun gun)
    {
        if(WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        }

        currentGun = gun;
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;

        currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }
}
