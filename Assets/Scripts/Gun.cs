 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string gunName;
    public float range;
    public float accuracy;
    public float fireRate;
    public float reloadTime;

    public int damage;
    public int reloadBulletCnt; //재장전
    public int currentBulletCnt; //장전된 탄알
    public int maxBulletCnt;
    public int carryBulletCnt; //전체 보유 수량

    public float retroActionForce; //반동 세기
    public float retroActionFineSightForce; //정조준 반동세기

    public Vector3 fineSightOriginPos; //정조준 위치
    public Animator anim; 
    public ParticleSystem muzzleFlash; //총구 화염 이펙트

    public AudioClip fireSound;

}
