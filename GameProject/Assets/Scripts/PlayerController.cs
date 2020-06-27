using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;       // 보호수준을 지키면서 인스펙터에 나오게 된다    
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;
    
    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    // 상태 변수
    private bool isWalk = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;   // 땅에 있는지 없는지 -> 점프 구현을 위해 쓴다

    // 움직임 체크 변수
    private Vector3 lastPos;    // 전 프레임의 현재 위치 vs 현 프레임의 현재 위치를 통해 움직이는 지 검사

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 땅 착지 여부
    private CapsuleCollider capsuleCollider;   


    // 카메라의 민감도
    [SerializeField]
    private float lookSensitivity; 

    // 카메라 한계
    [SerializeField]
    private float cameraeRotationLimit; // 마우스를 위로 올리면 고개를 위로 돈다,// 360도 넘어가면 한바퀴 돈다. 각도 설정을 위해 한다.
    private float currentCameraRotationX = 0;  // 카메라는 X이기 때문에 그리고 일단 정면 봐야하므로 0으로 설정

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;   // 이번에는 일부로 Serialized 씀.   이유는 -> Player에는 Camera 컴포넌트가 없음, Player의 자식 객체에 있음


    private Rigidbody myRigid;     // 플레이어의 실체 육체적인 몸
                                   // Collider로 충돌 영역 설정하고, Rigibody는 Collider에 물리학을 입힌다 
    private GunController theGunController;
    private Crosshair theCrosshair;


    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();   // Rigidbody 컴포넌트를 myRigid에 넣는다.    Serialized 써도 되는데 이 방법을 권장 -> 더 빠르다
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();
        
        // 초기화
        applySpeed = walkSpeed;     // 달리기 전에는 무조건 걷는 상태이기 때문
        originPosY = theCamera.transform.localPosition.y; // 캐릭터의 y가 아니라 카메라의 y를 내린다. 
                                                          // localPosition 쓴 이유 -> Camera는 월드 기준과 플레이어 기준에서 다르다 -> 상대적인 기준을 쓴 것이다
        applyCrouchPosY = originPosY;
        
    }


    void Update()       // 1초에 60번 정도 실행된다.
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        MoveCheck();
        CameraRotation();
        CharacterRotation();
    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // 앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch; // == if(isCrouch) 일 경우 isCrouch = false --> 서로 반전 시켜주는걸 1줄로 완성
        theCrosshair.CrouchingAnimation(isCrouch);

        if (isCrouch)
        {
            applySpeed = crouchSpeed;   // 앉을 때 스피드 달라짐
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;   // 일어섰을 때 스피드 달라짐
            applyCrouchPosY = originPosY;
        }

        // 코루틴으로 변경한다. theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        //                      벡터 값은 카메라는 현재 x값, 현재 z값, 바뀔 y값을 가지게 된다
        StartCoroutine(CrouchCorotuine());
    }

    // 부드러운 동작 실행      
    IEnumerator CrouchCorotuine()
    {
        float _posY = theCamera.transform.localPosition.y;  // 카메라는 플레이어 자식이므로 상대적인 위치 사용
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);  // 자연스럽게 보일 수 있도록 보간하는 함수
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null;   // 1프레임 대기, 즉 1프레임 마다 while문을 실행해서 자연스럽게 보인다
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f); // 15번 반복하다 목적지에 도달하고 끝남
    }
    // 지면 체크
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        // 우선 transform은 up과 right 밖에 없는데 왜 -transform.up이 아니고 Vector3.down을 쓰는가?
        // 고정되게 아래를 향해야 하기 때문에 trasnform이 아닌 고정좌표인 Vector3을 사용
        // 캡슐 컬라이더 바운드는 플레이어의 영역이고, extents는 y의 절반 사이즈
        // 0.1을 더한 이유는 약간의 여유를 준 것이다. 계단이나 대각선, 여러 지형 때문에
        theCrosshair.RunningAnimation(!isGround);
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)     //GetKey -> 계속 눌렸을 때, GetKeyDown -> 한번 눌렀을 때
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        // 앉은 상태에서 점프시 앉은 상태 해제
        if (isCrouch)
            Crouch(); 

        myRigid.velocity = transform.up * jumpForce;  // myRigid가 어느 방향으로 움직이고 있는 속도 = 위로 * 점프포스. -> 순간적으로 velocity를 바꿔서 점프한다

    }

    // 달리기 시도  
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))  // 왼쪽 쉬프트를 떼었을 때
        {
            RunningCancel();           
        }
    }

    // 달리기 실행
    private void Running()
    {
        // 앉은 상태에서 점프시 앉은 상태 해제
        if (isCrouch)
            Crouch();

        theGunController.CancelFineSight(); // 뛸 때 정조준 모드 해제된다
        
        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;  // 처음엔 applySpeed가 walkSpeed 였다. Move에서 applySpeed는 달리기 속도로 바뀐다
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    // 움직임 실행
    private void Move()
    {

        float _moveDirX = Input.GetAxisRaw("Horizontal");   // 좌우 화살표, 오른쪽 = 1 ,왼쪽 = -1 리턴 
        float _moveDirZ = Input.GetAxisRaw("Vertical");     // 유니티에서는 x가 좌우 z가 정면과 뒤


        Vector3 _moveHorizontal = transform.right * _moveDirX;   // Vector3 (1,0,0) * -1 = (-1,0,0) -> 왼쪽
                                                                 // Vector3 (1,0,0) * -1 = (1,0,0) -> 오른쪽         
        Vector3 _moveVertical = transform.forward * _moveDirZ;   // Vector (0,0,1) *1 or * -1 

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed; 
        // 방향 * 속도
        // (1,0,0) + (0,0,1) = (1,0,1)   -> 2개 합이 2가 된다
        // normalized 하는 이유 -> (0.5,0,0.5) -> 합을 1로 만들어 준다.
        // 어차피 방향은 같아서 똑같지만 합이 1이면 1초에 얼마나 이동시킬 지 계산이 편함 

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime); // 한번에 움직이면 순간이동 함 -> deltaTime 사용
    }

    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Vector3.Distance(lastPos,transform.position) >= 0.01f) // 경사면의 미끄러짐을 위해 여유를 준다
                isWalk = true;          // 이전 프레임 위치와 현재 프레임 위치가 다르면 걷는다 true
            else
                isWalk = false;

            theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }
    }

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X"); // 마우스 좌우로 움직이는 경우, y값 각도가 좌우를 움직인다
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        // 유니티에서 회전은 쿼터니언, MovePosition은 Vector3, MoveRotation은 쿼터니언
        // 우리가 구한 벡터값은 오일러 값이다. 
        // 오일러 값을 쿼터니언으로 바꾼 후 myRigid 쿼터니언 로테이션 값과 곱해주면 실제 Unity 내부 회전 적용됨
    }

    // 상하 카메라 회전
    private void CameraRotation()
    {   
        float _xRotation = Input.GetAxisRaw("Mouse Y"); // 왜 x인데 마우스는 y인가? -> 마우스는 2차원이라 x,y밖에 없다 -> 위 아래로 고개를 든다
        float _cameraRotationX = _xRotation * lookSensitivity;  // 한방에 확 안올라가고 천천히 올라가게 하려고 lookS 곱함
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraeRotationLimit, cameraeRotationLimit);
        // limit 만큼 가두기하자, currentCameraRotationX가 -45~45사이에 고정되게 가둔다
   
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f); // 오일러는 rotation이라 생각하면 된다.
    }
}
