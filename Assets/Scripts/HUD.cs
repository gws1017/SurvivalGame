using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    //ÄÄÆ÷³ÍÆ®
    public GunController gunController;
    private Gun currentGun;

    [SerializeField]
    private GameObject go_BulletHUD;

    [SerializeField]
    private Text[] textBullet;
    

    // Update is called once per frame
    void Update()
    {
        CheckBullet();
    }

    private void CheckBullet()
    {
        currentGun = gunController.GetGun();
        textBullet[0].text = currentGun.carryBulletCnt.ToString();
        textBullet[1].text = currentGun.reloadBulletCnt.ToString();
        textBullet[2].text = currentGun.currentBulletCnt.ToString();
    }
}
