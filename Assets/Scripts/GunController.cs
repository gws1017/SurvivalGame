using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public static bool isActivate = true;
    [SerializeField]
    private Gun currentGun;

    //���� �ӵ�
    private float currentFireRate;

    private bool isReload = false; //������ ���ΰ�?
    [HideInInspector]
    public bool isFineSightMode = false; //���� ����

    private Vector3 originPos;

    private AudioSource audioSource;

    private RaycastHit hitInfo;

    //������Ʈ
    [SerializeField]
    private Camera Cam;
    private CrossHair crossHair;

    //�ǰ�����Ʈ
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

    //�߻� �ӵ� ����
    private void GunFireRateCalc()
    {
        if(currentFireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    //�߻� �õ�
    private void TryFire()
    {
        if(Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }

    //�߻��� ó��
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

    //�߻� �� ó��
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

    //������ �õ�
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
    //������ �õ�
    private void TryFineSight()
    {
        if(Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    //������ ���
    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    //������ ����
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

    //������ Ȱ��ȭ
    IEnumerator FineSightActivateCorutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos,0.2f);
            yield return null;
        }
    }

    //������ ��Ȱ��ȭ
    IEnumerator FineSightDeActivateCorutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    //�ݵ� ����
    IEnumerator RetroActionCorutine()
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce,originPos.y,originPos.z); //90�� ������ z���̾ƴ� x����
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
