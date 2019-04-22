﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockingManager : MonoBehaviour
{

    public Transform focusedObject;
    public Transform targetObject;

    public float distance;
    public float angle;

    public float distThreshold;
    public float angleThreshold;


    public bool isFirstTouch = true;
    public bool isTimeCounting = false;
    public float startTime;
    public float timer = 0f;
    public int clutch = 0;


    public static DockingManager Instance;


    public void Init()
    {
        isFirstTouch = true;
        isTimeCounting = false;
        startTime = 0f;
        timer = 0f;
        clutch = 0;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateDiffs();
        UpdateTime();
    }


    void UpdateDiffs()
    {
        distance = Vector3.Distance(focusedObject.position, targetObject.position);
        angle = Quaternion.Angle(focusedObject.rotation, targetObject.rotation);
    }

    void UpdateTime()
    {
        if (isTimeCounting)
        {
            timer = Time.time - startTime;
        }
    }

    public void TouchStart()
    {
        // touch check
        if (isFirstTouch)
        {
            // start timer
            startTime = Time.time;
            isTimeCounting = true;
            isFirstTouch = false;
        }
    }

    public void TouchUpdate()
    {
        // update sths 
    }

    public void TouchEnd()
    {
        // accuracy check
        AccuracyCheck();
        // clutch +1
        clutch++;
    }


    void AccuracyCheck()
    {
        // get angle

        // compare with threshold
        // if smaller, then stop timer
        if (angle < angleThreshold)
        {
            isTimeCounting = false;
        }


        // do sth to stop this task
        // send to user study manager
        UserStudyManager.Instance.SetTaskResult(new UserStudyManager.Task(timer, angle, clutch));

    }

}