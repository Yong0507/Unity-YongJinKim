using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minitaur : MonoBehaviour
{

    [SerializeField] private string animalName; // 동물의 이름
    [SerializeField] private int hp; // 동물의 체력.

    [SerializeField] private float walkSpeed; // 걷기 스피드.

    private Vector3 direction; // 방향.


    // 상태변수
    private bool isAction; // 행동중인지 아닌지 판별.
    private bool isWalking; // 걷는지 안 걷는지 판별.

    [SerializeField] private float walkTime; // 걷기 시간
    [SerializeField] private float waitTime; // 대기 시간.
    private float currentTime;


    // 필요한 컴포넌트
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private BoxCollider boxCol;

    // Use this for initialization
    void Start()
    {
        currentTime = waitTime;
        isAction = true;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotation();
        ElapseTime();
    }

    private void Move()
    {
        if (isWalking)
            rigid.MovePosition(transform.position + (transform.forward * walkSpeed * Time.deltaTime));
    }

    private void Rotation()
    {
        if (isWalking)
        {
            Vector3 _rotation = Vector3.Lerp(transform.eulerAngles, direction, 0.01f);
            rigid.MoveRotation(Quaternion.Euler(_rotation));
        }
    }

    private void ElapseTime()
    {
        if (isAction)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
                ReSet();
        }
    }

    private void ReSet()
    {
        isWalking = false; isAction = true;
        anim.SetBool("Walking", isWalking);
        direction.Set(0f, Random.Range(0f, 360f), 0f);
        RandomAction();
    }

    private void RandomAction()
    {
        int _random = Random.Range(0, 4); // 대기, 풀뜯기, 두리번, 걷기.

        if (_random == 0)
            Wait();
        else if (_random == 1)
            Shout();
        else if (_random == 2)
            Jump();
        else if (_random == 3)
            TryWalk();
    }

    private void Wait()
    {
        currentTime = waitTime;
        Debug.Log("대기");
    }
    private void Shout()
    {
        currentTime = waitTime;
        anim.SetTrigger("Shout");
        Debug.Log("소리쳐");
    }
    private void Jump()
    {
        currentTime = waitTime;
        anim.SetTrigger("Jump");
        Debug.Log("점프");
    }
    private void TryWalk()
    {
        isWalking = true;
        anim.SetBool("Walking", isWalking);
        currentTime = walkTime;
        Debug.Log("걷기");
    }
}
