﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    //오브젝트 다른경우 생각하기!!!!!!!!!!!!!!!!!
    public GameObject hp;
    

    // Start is called before the first frame update
    void Start()
    {
        hp = GameObject.Find("DDaeng_2").transform.Find("HpBar").transform.Find("Hp").gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        var other = GameObject.Find("DDaeng_2").GetComponent<Move>();

        Vector3 Pos = hp.transform.position;
        
        /*if(other.hp < 100.0f)   //감소 발생
        {
            Pos.x -= 0.1f;
            hp.transform.position = Pos;
        }*/
            

        
    }
}
