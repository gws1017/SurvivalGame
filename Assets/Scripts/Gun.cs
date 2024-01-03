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
    public int reloadBulletCnt; //������
    public int currentBulletCnt; //������ ź��
    public int maxBulletCnt;
    public int carryBulletCnt; //��ü ���� ����

    public float retroActionForce; //�ݵ� ����
    public float retroActionFineSightForce; //������ �ݵ�����

    public Vector3 fineSightOriginPos; //������ ��ġ
    public Animator anim; 
    public ParticleSystem muzzleFlash; //�ѱ� ȭ�� ����Ʈ

    public AudioClip fireSound;

}
