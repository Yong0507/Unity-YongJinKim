using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GunController))]

public class WeaponManager : MonoBehaviour
{
    // 무기 중복 교체 실행 방지. 
    // 공유 자원. 클래스 변수 = 정적 변수.
    public static bool isChangeWeapon = false;
    
    // 현재 무기와 현재 무기의 애니메이션.
    public static Transform currentWeapon;  // 모든 객체는 기본 Transform 이 있기 때문에
    public static Animator currentWeaponAnim;

    // 현재 무기의 타입
    [SerializeField]
    private string currentWeaponType;


    // 무기 교체 딜레이 타임, 무기 교체가 완전히 끝난 시점.
    [SerializeField]
    private float changeWeaponDelayTime;
    [SerializeField]
    private float changeWeaponEndDeplayTime;


    // 무기 종류들 전부 관리
    [SerializeField]
    private Gun[] guns;
    [SerializeField]
    private Hand[] hands;

    // 관리 차원에서 쉽게 무기 접근이 가능하도록 만듦
    private Dictionary<string, Gun> gunDictionary = new Dictionary<string, Gun>();
    private Dictionary<string, Hand> handDictionary = new Dictionary<string, Hand>();

    // 필요한 컴포넌트 
    [SerializeField]
    private GunController theGunController;
    [SerializeField]
    private HandController theHandController;
    
        

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            gunDictionary.Add(guns[i].gunName, guns[i]);
        }

        for (int i = 0; i < hands.Length; i++)
        {
            handDictionary.Add(hands[i].handName, hands[i]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!isChangeWeapon)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                // 무기 교체 실행 (서브머신건)
                StartCoroutine(ChangeWeaponCoroutine("HAND", "맨손"));
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                // 무기 교체 실행 (맨손)
                StartCoroutine(ChangeWeaponCoroutine("GUN", "SubMachineGun1"));
        }
    }

    public IEnumerator ChangeWeaponCoroutine(string _type,string _name)
    {
        isChangeWeapon = true;
        currentWeaponAnim.ResetTrigger("Weapon_Out");

        yield return new WaitForSeconds(changeWeaponDelayTime);

        CancelPreWeaponAction(); // 정조준 상태 해제
        WeaponChange(_type, _name);  // 원하는 다음 무기 꺼내기

        yield return new WaitForSeconds(changeWeaponEndDeplayTime);

        currentWeaponType = _type;
        isChangeWeapon = false;
     }

    private void CancelPreWeaponAction()
    {
        switch (currentWeaponType)
        {
            case "GUN":
                theGunController.CancelFineSight();
                theGunController.CancelReload();
                break;
            case "HAND":

                break;
        }
    }

    private void WeaponChange(string _type, string _name)
    {
        if (_type == "GUN")
            theGunController.GunChange(gunDictionary[_name]);

        else if (_type == "HANDA")
            theHandController.HandChange(handDictionary[_name]);               
    }
}
