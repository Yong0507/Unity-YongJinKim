using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    // 활성화 여부.
    public static bool isActivate = true;

    [SerializeField]
    private Gun currentGun; // 현재 소유하고 있는 총 

    private float currentFireRate;  // 연사 속도 계산

    // 상태 변수
    private bool isReload = false;
    [HideInInspector]
    public bool isfineSightMode = false;

    private Vector3 originPos; // 정조준 후 원래 값으로 돌아가기 위해서 만든 변수 -> 본래 포지션 값

    private AudioSource audioSource;   // 효과음 재생

    private RaycastHit hitInfo;  // 레이저 충돌 정보 받아옴

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCam;  // 게임 화면이 카메라 시점, 카메라 시점의 정가운데가 총알 발사되는 지점이 될 것임
    private Crosshair theCrosshair;

    // 피격 이펙트 -> 불똥 튀긴다
    [SerializeField]
    private GameObject hit_effect_prefab;

    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        theCrosshair = FindObjectOfType<Crosshair>();

        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;
    }

    void Update()
    {
        if (isActivate)
        {
            GunFireRateCalc();
            TryFire();
            TryReload();
            TryFineSight();
        }
    }

    // 연사속도 재계산
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;  // 1초에 1씩 감소
           
    }

    // 발사 시도
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)  // 왼쪽 마우스 버튼 누르고 있으면
        {
            Fire();
        }
    }

    // 발사 전 계산
    private void Fire() // 발사 전
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot();
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());  // 총알이 없을 시 재장전
            }
        }
    }

    // 발사 후 계산
    private void Shoot() // 발사 후
    {
        theCrosshair.FireAnimation();
        currentGun.currentBulletCount--; // 현재 총알에서 Shoot 할때마다 1개씩 빼기
        currentFireRate = currentGun.fireRate; // 연사 속도 재계산
        PlaySE(currentGun.fire_Sounds);
        currentGun.muzzleFlash.Play();

        Hit();

        // 총기 반동 코루틴 실행
        StopAllCoroutines();    // while문 2개가 같이 실행될 수 있기 때문에 코루틴을 멈춰준다 
        StartCoroutine(RetroActionCoroutine());

    }

    // 쏘는 족족 맞게 하기
    private void Hit()
    {
        // 충돌 검사
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward +
            new Vector3(Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                        Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                        0)                      // 카메라 현재 위치에서 직진으로 쏘는데 랜덤을 더해서 쏜다
            , out hitInfo, currentGun.range))
        {
            // point -> 충돌한 곳의 실제 좌표를 반환, normal -> 충돌한 객체의 표면을 반환
            // 반환되는 타입 모를 때 var 사용 가능
            GameObject clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }

    }

    // 재장전 시도
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
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

    // 재장전
    IEnumerator ReloadCoroutine()
    {
        // 현재 소유하고 있는 총알이 1발이라도 있으면 재장전, 없으면 장전도 못함
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;

            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.reloadTime);

            // 현재 소유한 초알이 30발 아래일때와 30발 이상일 때 구분 해줘야함
            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount; // 재장전 개수는 전부
                currentGun.carryBulletCount -= currentGun.reloadBulletCount; // 현재 소유한 총알 -= 재장전한 총알 개수
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("소유한 총알이 없습니다.");
        }
    }

    // 정조준 시도
    private void TryFineSight()     // 처음에 False인데 우클릭 누르면 FineSight() 함수 실행
    {
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    // 정조준 취소
    public void CancelFineSight()  // 정조준 상태에서 재장전 이루어 질 시 정조준 상태 취소하는 함수
    {
        if (isfineSightMode)
            FineSight();        // true인 상태에서 false로 바꾼다
    }

    // 정조준 로직 가동
    private void FineSight()    // 처음에 False이므로 true로 바꿔준다. true로 바뀌면서 애니메이션 실행
    {
        isfineSightMode = !isfineSightMode;
        currentGun.anim.SetBool("FineSightMode", isfineSightMode);
        theCrosshair.FineSightAnimation(isfineSightMode);
        
        if (isfineSightMode)
        {
            StopAllCoroutines(); // Nerp는 멈추지 않고 계속 실행되면서 보간하기 때문에 밑에서 2개가 같이 실행되버림 
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines(); // 그렇기 때문에 기존에 실행되던 코루틴을 멈추기 위해 Stop한것임
            StartCoroutine(FineSightDeActivateCoroutine());
        }
    }

    // 정조준 활성화
    IEnumerator FineSightActivateCoroutine()
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos) // 정조준 할때 위치가 될 때까지 반복
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }

    // 정조준 비활성화
    IEnumerator FineSightDeActivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos) // 원래 포지션 값이 될 때 까지 반복
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    // 반동 코루틴
    IEnumerator RetroActionCoroutine()
    {
        // 원래 z축 반동인데 90도 꺽었기 때문에 x축을 이용해 반동을 만들 예정
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z); // 정조준 안했을 때 최대 반동
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); // 정조준 했을 때 최대 반동

        if (!isfineSightMode) // 정조준 상태가 아닐 경우
        {
            currentGun.transform.localPosition = originPos; // 반동을 한번 주면 다시 원래 위치 -> 계속 이어지면 반동처럼 안보임

            // 반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f) // Nerp단점은 끝에 도달하지 못함 그래서 -0.02f 줘서 그쯤 도달하면 멈추게 하는 것임
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }

            // 원위치
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }

        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos; // 정조준 상태의 위치로 되돌린다

            // 반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f) 
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            // 원위치
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos) // 정조준 상태의 원래 값으로 갈 때 까지
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }


    }

    // 사운드 재생
    private void PlaySE(AudioClip _clip)  // 게임 사운드 넣기
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public Gun GetGun()
    {
        return currentGun;
    }

    public bool GetFineSightMode()
    {
        return isfineSightMode;
    }

    public void GunChange(Gun _gun)
    {
        if (WeaponManager.currentWeapon != null) // 뭔가를 들고 있는 경우
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false); // 기존 총이 사라짐
        }

        currentGun = _gun; // 바꿀 무기가 현재 무기
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;
        
        currentGun.transform.localPosition = Vector3.zero; // 무기 교체 될 때 position 바뀔 수도 있으니 0,0,0ㅇ로 초기화
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }
}
