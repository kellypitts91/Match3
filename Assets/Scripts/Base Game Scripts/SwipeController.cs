﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Vector2 firstPosition;
    private Vector2 secondPosition;
    public Vector2 moveDirection;
    public bool isControlling = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            isControlling = true;
            firstPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if(Input.GetMouseButtonUp(0)){
            isControlling = false;
            secondPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            moveDirection = new Vector2(secondPosition.x - firstPosition.x, secondPosition.y - firstPosition.y);
}
    }
}
