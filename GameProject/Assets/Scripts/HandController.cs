﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // 활성화 여부.
    public static bool isActivate = false;

    // 현재 장착된 Hand형 타입 무기
    [SerializeField]
    private Hand currentHand;

    // 공격중??
    private bool isAttack = false;
    private bool isSwing = false;

    private RaycastHit hitInfo;

    void Update()
    {
        if (isActivate)
            TryAttack();

    }

    private void TryAttack()
    {
        if (Input.GetButton("Fire1"))    // Fire1은 좌클릭 총발사 할때 쓰게 되는 문법-> Fire1은 왼쪽 Control 키 눌러도 같이 적용됨. Edit에서 컨트롤 부분 삭제했음 앉기버튼이 컨트롤 이기 때문
        {
            if (!isAttack)
            {
                // 코루틴 실행, 마우스 좌클릭 순간 -> 코루틴 실행 -> 바로 isAttack = true 실행
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentHand.anim.SetTrigger("Attack"); // currentHand에 있는 애니메이션, 그 안에 있는 상태 변수 Trigger 발동

        yield return new WaitForSeconds(currentHand.attackDelayA);
        isSwing = true;

        // 공격 활성화 시점
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB);
        isSwing = false;

        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false;
    }

    IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                // 충돌했음
                isSwing = false; // 이렇게 하는 이유? -> 하나 충돌했으면 2번..3번 실행안되게 하려고
                //Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }

    private bool CheckObject()
    {
        // 충돌한게 있다면 true 없으면 false
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range))
        {
            return true;
        }
        return false;
    }

    public void HandChange(Hand _hand)
    {
        if (WeaponManager.currentWeapon != null) // 뭔가를 들고 있는 경우
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false); // 기존 총이 사라짐
        }

        currentHand = _hand; // 바꿀 무기가 현재 무기
        WeaponManager.currentWeapon = currentHand.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentHand.anim;

        currentHand.transform.localPosition = Vector3.zero; // 무기 교체 될 때 position 바뀔 수도 있으니 0,0,0ㅇ로 초기화
        currentHand.gameObject.SetActive(true);
        isActivate = true;
    }
}
