using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseWeapon : MonoBehaviour
{
    public string closeWeaponName;

    public bool isHand;
    public bool isAxe;
    public bool isPickAxe;

    public float range;
    public int damage;
    public float workSpeed;
    public float attackRecoveryTime; //후딜레이
    public float attackFrontDelay; //선딜레이
    public float attackDelay; //공격판정

    public Animator anim;
    

}
