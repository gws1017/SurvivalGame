using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunController))]
public class WeaponManager : MonoBehaviour
{
    //중복 교체 방지
    public static bool isChangeWeapon = false;

    public static Transform currentWeapon;
    public static Animator currentWeaponAnim;

    [SerializeField]
    private string currentWeaponType;

    [SerializeField]
    private float changeWeaponDelayTime;
    [SerializeField]
    private float changeWeaponEndDelayTime;

    [SerializeField]
    private Gun[] guns;
    [SerializeField]
    private Hand[] hands;

    private Dictionary<string, Gun> gunDict = new Dictionary<string, Gun>();
    private Dictionary<string, Hand> handDict = new Dictionary<string, Hand>();

    //컴포넌트
    [SerializeField]
    private GunController gunController;
    [SerializeField]
    private HandController handController;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < guns.Length; ++i)
        {
            gunDict.Add(guns[i].gunName, guns[i]);
        }
        for (int i = 0; i < hands.Length; ++i)
        {
            handDict.Add(hands[i].handName, hands[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isChangeWeapon)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) StartCoroutine(ChangeWeaponCorutine("HAND","맨손"));
            else if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(ChangeWeaponCorutine("GUN", "SubMachineGun"));
        }
    }

    public IEnumerator ChangeWeaponCorutine(string type, string name)
    {
        isChangeWeapon = true;

        currentWeaponAnim.SetTrigger("Weapon_Out");

        yield return new WaitForSeconds(changeWeaponDelayTime);

        CancelPrevWeaponAction();
        WeaponChange(type,name);

        yield return new WaitForSeconds(changeWeaponEndDelayTime);

        currentWeaponType = type;
        isChangeWeapon = false;
    }
    
    private void CancelPrevWeaponAction()
    {
        switch(currentWeaponType)
        {
            case "GUN":
                gunController.CancelFineSight();
                gunController.CancelReload();
                GunController.isActivate = false;
                break;
            case "HAND":
                HandController.isActivate = false;
                break;
        }
    }

    private void WeaponChange(string type, string name)
    {
        if(type == "GUN")
            gunController.GunChange(gunDict[name]);
        else if(type == "HAND")
            handController.HandChange(handDict[name]);
    }
}
