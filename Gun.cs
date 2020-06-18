using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string gunName; // 총의 이름
    public float range; // 사정 거리
    public float accuracy; // 정확도
    public float fireRate; // 연사속도
    public float reloadTime; // 재장전 속도

    public int damage; // 총의 데미지

    public int reloadBulletCount; // 총알 재장전 개수
    public int currentBulletCount; // 현재 탄알집의 남아있는 총알의 개수
    public int maxBulletCount; // 최대 소유 가능 총알 개수
    public int carryBulletCount; // 현재 소유하고 있는 총알 개수

    public float retroActionForce; // 총 쏠때 반동 세기
    public float retroActionFineSightForce; // 정조준시의 반동 세기

    public Vector3 fineSightOriginPos;  // 정조준할때 총 가운데로 오는데 이것을 위해 만들어 주는 변수
    public Animator anim;
    public ParticleSystem muzzleFlash; // 화염,총알 발사 이펙트 등을 위해서

    public AudioClip fire_Sounds;
}
