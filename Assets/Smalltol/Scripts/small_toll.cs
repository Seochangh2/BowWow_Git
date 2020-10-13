﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class small_toll : MonoBehaviour
{
    public GameObject fireballPrefab;//파이어볼 프리팹
    public GameObject DDaeng;//땡이
    public GameObject smalltoll;//스몰톨
    public GameObject DamageText;

    public Transform head;//데미지 텍스트 뜨는 위치 
    Transform st;

    Vector3 target;//땡이 위치
    Vector3 me;//스몰톨 위치 

    public float d = 30f;//범위 거리 설정  
    float movePower = 5f;//움직이는 속력

    float RateMin = 0.5f;//최소 생성 주기 
    float RateMax = 3f;//최대 생성 주기
    float Rate;//파이어볼 생성 주기
    float Ypos;

    private float timeAfter;//발사 후 지난 시간
    float timeball;//파이어볼 생성에 필요한 시간 

    int movementFlag = 0;//0: 정지, 1: 왼쪽, 2: 오른쪽
    string dist = "";//이동 방향 

    bool isTracing = false;//거리 내에 들어와서 유지 중인 상태 
    bool Enter = false;//거리 내에 들어오면 (처음)
    bool isStop = false;//멈췄다가 파이어볼 쏘기
    bool isHeart = false;//플레이어에게 공격 받음 여부
    bool isBall = false;//공 한번만
    bool isAttack_once = false;//근접 공격 한번만 적용
    bool isAttack = false;//근접 공격 여부
    bool isY = false;//y값 비교, 추적, 공격 여부
    bool isWall = false;//벽 파악 

    public float HPMax = 100.0f;//최대 체력  
    public float HP;//현재 체력

    //h
    GameObject hp_bar;  //hp바
    float hpbar_sx;     //hp바 스케일 x값
    float hpbar_tx;     //hp바 위치 x값
    float hpbar_tmp;    //hp바 감소 정도
    string tag_name;    //hp바 태그
    //

    public int Power_run;//런크래쉬 공격력
    public int Power_fireball;//파이어볼 공격력

    // Start is called before the first frame update
    void Start()
    {
        //h
        HP = HPMax;//체력 설정 
        tag_name = transform.Find("HpBar").transform.Find("Hp").tag;
        hp_bar = GameObject.FindWithTag(tag_name);

        hpbar_sx = hp_bar.transform.localScale.x;
        hpbar_tx = hp_bar.transform.localPosition.x;
        hpbar_tmp = hpbar_sx / HPMax;
        //

        DDaeng = GameObject.Find("DDaeng");//하이라키 내에서 찾기 위함 

        timeAfter = 0f;//파이어볼 생성 시간 초기화 
        Rate = Random.Range(RateMin, RateMax);//처음 파이어볼 생성 주기 설정

        st = smalltoll.transform.Find("warning");//warning 활성/비활성화 위함
        st.gameObject.SetActive(false);

        /*if()//floor0 인 경우와 아닌 경우 나누기 
        {

        }*/

        StartCoroutine("ChangeMovement");
    }

    IEnumerator ChangeMovement()
    {
        movementFlag = Random.Range(1, 3);//움직임 설정 랜덤

        yield return new WaitForSeconds(3f);//3초동안 실행

        StartCoroutine("ChangeMovement");//다른 움직임 또 하게 호출 
    }

    IEnumerator MoveStop()//멈춘 후 파이어볼 발사 
    {
        yield return new WaitForSeconds(0.25f);
        if (isBall == false)//0.25초 지나 정지했다가. 공이 한개만 있는지 체크  
        {
            FireballMake();//파이어볼 발사

            timeAfter = 0;
            isStop = false;
        }
    }

    //스몰톨이 카메라 벗어나지 않게 제한 
    IEnumerator ClipMovementleft()//왼쪽으로 가는 코루틴 실행
    {
        movementFlag = 1;
        //Debug.Log("코루틴 left");

        yield return new WaitForSeconds(3f);

        StartCoroutine("ChangeMovement");
    }

    IEnumerator ClipMovementright()//오른쪽으로 가는 코루틴 실행 
    {
        movementFlag = 2;
        //Debug.Log("코루틴 right");

        yield return new WaitForSeconds(3f);

        StartCoroutine("ChangeMovement");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeAfter += Time.deltaTime;//시간 갱신

        Distance();//거리 파악. 트리거 대신 
        Move();//거리 파악 후 움직임, 파이어볼 발사 
    }

    //h
    public void hpMove(float hp_delta)
    {
        float move = ((HPMax - HP) + hp_delta) * hpbar_tmp;
        HP -= hp_delta;

        Vector3 Scale = hp_bar.transform.localScale;
        hp_bar.transform.localScale = new Vector3(hpbar_sx - move, Scale.y, Scale.z);

        Vector3 Pos = hp_bar.transform.localPosition;
        hp_bar.transform.localPosition = new Vector3(hpbar_tx - move / 2.0f, Pos.y, Pos.z);
    }
    //

    void Distance()
    {
        target = DDaeng.transform.position;

        float distance = Vector3.Distance(target, transform.position);
        Ypos = Mathf.Abs(target.y - transform.position.y);//절댓값(땡이의 y값 - 스몰 톨의 y값)

        if (Ypos <= 5)//y값 비교 
        {
            isY = true;//같은 층에 있음. 공격, 추적 가능 
        }
        else
        {
            isY = false;
        }

        if (distance <= d && isY &&!isWall)//범위 내에 처음 들어오면
        {
            Enter = true;
            StopCoroutine("ChangeMovement");//이동하던 거 멈추고 추적 시작 
        }

        if (Enter == true && distance <= d &&isY && distance>9 &&isY)//들어 온 상태이고 범위 내에 계속 있으면 (닿진 않았고)
        {
            st.gameObject.SetActive(true);

            isTracing = true;//추적 시작 
            isAttack = false;//근접 공격 
        }

        if (isTracing == true && distance > d &&isY)//거리 벗어나면 
        {
            Enter = false;
            isTracing = false;
            isY = false;
            StartCoroutine("ChangeMovement");//다시 랜덤 이동 시작 
        }

        if (distance <= 10 && isY)//빠르게 움직여서 근접 공격 
        {
            isAttack = true;//근접 공격 플래그 -> 속도 빠르게 
        }

        if (distance<=6 &&isY) //공격 후 닿은 시점 ->근접 공격 
        {
            isAttack = false;//공격 했으니까 Attack 플래그 꺼줌 -> 속도 느리게 
            isTracing = false;//추적 그만

            Move dd = GameObject.Find("DDaeng").GetComponent<Move>();

            if (target.x < me.x)
            {
                StartCoroutine("ClipMovementright");//오른쪽으로
            }
            else if (target.x > me.x)
            {
                StartCoroutine("ClipMovementleft");
            }

            //데미지 텍스트 설정 
            if (target.x > me.x)//땡이가 오른쪽이면 
            {
                dd.head.position = DDaeng.GetComponent<Move>().headleft.position;
            }
            else
            {
                dd.head.position = DDaeng.GetComponent<Move>().headright.position;//기본 head
            }

            if(isAttack_once)//한 번 만 공격 
            {
                dd.TakeDamage(5);//텍스트 데미지 
                dd.hpMove(5.0f);
            }
            if (dd.HP <= 0)
            {
                Destroy(DDaeng);
            }
            isAttack_once = false;
        }
    }

    void Move()
    {
        me = transform.position;
        Vector3 moveVelocity = Vector3.zero;

        if(isStop ==false)
        {
            if (isTracing && isY &&!isWall|| isHeart)//일정 거리 내 이거나 공격 받으면 플레이어 쪽으로 이동  
            {
                //파이어볼 발사 
                if (timeAfter >= Rate)
                {
                    isStop = true;//멈춤 후
                    isBall = false;
                }

                //추격 중에 Y값 조건 체크 
                if (Ypos <= 5)
                {
                    isY = true;
                }
                else
                {
                    isY = false;
                }

                //근접 공격 속도 설정 
                if (isAttack)
                {
                    if (timeAfter >= Rate)
                    {
                        isStop = true;//멈춤 후 공격
                        isBall = false;
                    }
                    movePower = 50;
                    isAttack = false;
                }
                else
                {
                    movePower = 13;//추적 시에 속도 빠르게
                }

                if (target.x < me.x)//땡이가 왼쪽이면
                {
                    if (isHeart)
                    {
                        StartCoroutine("ClipMovementleft");//3초동안 왼쪽으로 
                    }
                    else
                    {
                        dist = "Left";//왼쪽으로 가라 
                    }
                }

                else if (target.x > me.x)//땡이가 오른쪽이면
                {
                    if(isHeart)
                    {
                        StartCoroutine("ClipMovementright");//3초동안 오른쪽으로 
                    }
                    else
                    {
                        dist = "Right";
                    }
                }
            }
            else//거리 밖이면 (평소)
            {
                st.gameObject.SetActive(false);

                movePower = 5;

                if (me.x >= 40)
                {
                    StopCoroutine("ChangeMovement");
                    StartCoroutine("ClipMovementleft");
                }
                else if (me.x <= -40)
                {
                    StopCoroutine("ChangeMovement");
                    StartCoroutine("ClipMovementright");
                }

                if (movementFlag == 1)
                    dist = "Left";
                else if (movementFlag == 2)
                    dist = "Right";
            }

            //좌우 이동 
            if (dist == "Left")
            {
                moveVelocity = Vector3.left;
            }
            else if (dist == "Right")
            {
                moveVelocity = Vector3.right;
            }
            transform.position += moveVelocity * movePower * Time.deltaTime;
        }
        else if(isStop ==true)//정지 상태인 경우 isStop =true 인 경우 
        {
            movePower = 0;
            StartCoroutine("MoveStop");//멈추고
        }
    }
    
    public void TakeDamage(int damage)//땡이한테 맞기위함 
    {
        GameObject damageText = Instantiate(DamageText);
        damageText.transform.position = head.position;
        damageText.GetComponent<DamageText>().damage = damage;
    }

    void FireballMake()
    {
        GameObject ball = GameObject.Instantiate(fireballPrefab); //파이어볼 생성
        isBall = true;
        
        ball.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, 16.5f);//파이어볼 초기 위치 z:15
        Debug.Log("파이어볼 생성 시 스몰 톨 위치 : " + transform.position);
        ball.transform.parent = null;

        Rate = Random.Range(RateMin, RateMax);//다음 번 파이어볼 생성 주기 설정 

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "miniwall")
        {
            isWall = true;
            if (other.gameObject.transform.position.x <= transform.position.x)//벽이 왼쪽이면 
            {
                StartCoroutine("ClipMovementright");
            }
            else if (other.gameObject.transform.position.x > transform.position.x)
            {
                StartCoroutine("ClipMovementleft");
            }
        }
        if(other.gameObject.tag =="DDaeng")
        {
            isAttack_once = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "miniwall")
        {
            isWall = true;
            if (other.gameObject.transform.position.x <= transform.position.x)//벽이 왼쪽이면 
            {
                StartCoroutine("ClipMovementright");
            }
            else if (other.gameObject.transform.position.x > transform.position.x)//벽이 오른쪽이면 
            {
                StartCoroutine("ClipMovementleft");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "miniwall")
        {
            //Debug.Log("벽 트리거 끝");
            isWall = false;//벽이 없음 
        }
        if(other.gameObject.tag =="DDaeng")
        {
            isAttack_once = false;
        }
    }
}