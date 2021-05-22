using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DockingManager : MonoBehaviour
{
    public static DockingManager Instance;
    public ClutchManipulatable manipulatable;
    public Transform anchor;
    public Transform fromObject;
    public Transform toObject;
    public Transform head;
    public Transform[] hands;
    public bool IsRightHand = true;

    public float targetSize = 1f;
    public float targetDist = 5f;
    public float randomOffsetRadius = 1f;
    public float initDist = 1f;
    public float initAngle;

    //[Range(0f, 0.1f)]
    public float easyDistThreshold = 0.03f;
    //[Range(0f, 30f)]
    public float easyAngleThreshold = 30f;
    //[Range(0f, 0.1f)]
    public float distThreshold = 0.01f;
    //[Range(0f, 30f)]
    public float angleThreshold = 5f;

    public int randomSeedBase = 10000;
    public int randomSeed;

    bool isFirstTouch = true;
    bool isTouching = false;
    [SerializeField]
    bool isTimeCounting = false;
    bool easyThresholdMet = false;
    bool hardThresholdMet = false;
    float startTime;
    float maniStartTime;
    float lastReleaseTime;
    Transform hand;

    Vector3 preHandPos;
    Quaternion preHandRot;
    Vector3 preFromObjPos;
    Quaternion preFromObjRot;
    Vector3 orgFromObjPos;
    Quaternion orgFromObjRot;

    public float selectionTimer = 0f;
    public float manipulationTimer = 0f;
    public float clutchingTimer = 0f;
    //public float reactionTimer = 0f;

    [SerializeField]
    int clutch = -1;
    [SerializeField]
    float distance;
    [SerializeField]
    float angle;

    float totalHandDistance = 0f;
    float totalHandAngle = 0f;
    float totalObjDistance = 0f;
    float totalObjAngle = 0f;
    float translationEfficiency = 0f;
    float rotationEfficiency = 0f;

    float timer_0 = 0f;
    int clutch_0 = 0;
    float distance_0;
    float angle_0;

    float totalHandDistance_0 = 0f;
    float totalHandAngle_0 = 0f;
    float totalObjDistance_0 = 0f;
    float totalObjAngle_0 = 0f;
    float translationEfficiency_0 = 0f;
    float rotationEfficiency_0 = 0f;
    int numOfAttempts = 0;

    const float kMinStatDist = 0.0001f;
    const float kMinStatAngle = 0.001f;

    public bool accFeedback = false;
    public bool easyRotation = false;

    public float tSpeed;
    public float rSpeed;

    public LogData logData;

    //Vector3 preHeadPos;
    //Quaternion preHeadRot;
    //public float totalHeadDistance = 0f;
    //public float totalHeadAngle = 0f;
    //public float totalHeadDistance_0 = 0f;
    //public float totalHeadAngle_0 = 0f;


    public void Init(int t)
    {
        randomSeed = randomSeedBase + (t + 1) * randomSeedBase / 100;

        Init();
    }

    public void Init()
    {
        isFirstTouch = true;
        isTimeCounting = false;
        easyThresholdMet = false;
        hardThresholdMet = false;

        UpdateHand();

        // start timer
        startTime = Time.time;

        manipulationTimer = 0f;
        selectionTimer = 0f;
        clutchingTimer = 0f;

        // clutch = 0 means no clutch
        clutch = -1;

        totalHandDistance = 0f;
        totalHandAngle = 0f;
        totalObjDistance = 0f;
        totalObjAngle = 0f;

        timer_0 = 0f;
        clutch_0 = 0;
        distance_0 = 0f;
        angle_0 = 0f;

        totalHandDistance_0 = 0f;
        totalHandAngle_0 = 0f;
        totalObjDistance_0 = 0f;
        totalObjAngle_0 = 0f;
        translationEfficiency_0 = 0f;
        rotationEfficiency_0 = 0f;

        numOfAttempts = 0;

        //totalHeadDistance = 0f;
        //totalHeadAngle = 0f;
        //totalHeadDistance_0 = 0f;
        //totalHeadAngle_0 = 0f;

        manipulatable.Init();

        fromObject.localScale = Vector3.one * targetSize;
        toObject.localScale = Vector3.one * targetSize;

        anchor.position = head.position + Vector3.forward * targetDist;

        Random.InitState(randomSeed);
        // reset pos
        RandomPosition();

        // reset rot
        RandomRotation();

        // register org data
        orgFromObjPos = fromObject.position;
        orgFromObjRot = fromObject.rotation;

        logData = new LogData();

        // reset UI
        //if (UIManager.Instance != null)
        //    UIManager.Instance.SetText("Ready");
    }

    void OnValidate()
    {
        UpdateHand();
    }

    void UpdateHand()
    {
        if (IsRightHand)
            hand = hands[0];
        else
            hand = hands[1];
    }


    float[] rValues = { .25f, .75f };

    public void RandomPosition()
    {
        toObject.position = anchor.position + Random.onUnitSphere * randomOffsetRadius;

        // r, polar theta, azimuth phi
        //var theta = Random.Range(0.25f * Mathf.PI, 0.5f * Mathf.PI);
        var theta = 0.5f * Mathf.PI;
        //var phi = Random.Range(0f, 0.25f * Mathf.PI);
        var phi = 0.75f * Mathf.PI;
        //var phi = rValues[Random.Range(0, rValues.Length)] * Mathf.PI;

        Vector3 d = new Vector3(
            Mathf.Sin(theta) * Mathf.Cos(phi),
            Mathf.Cos(theta),
            Mathf.Sin(theta) * Mathf.Sin(phi)
            );

        fromObject.position = toObject.position + d * initDist;
        //fromObject.position = head.position + d * initDist;


    }

    public void RandomRotation()
    {

        Vector3 axis0 = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        int s = Random.Range(1, 3);
        Quaternion a = Quaternion.AngleAxis(22.5f * (2 * s + 1), axis0);

        //var a = Random.rotationUniform;
        var axis1 = easyRotation ? Vector3.up : Random.onUnitSphere;
        var b = Quaternion.AngleAxis(initAngle, axis1);
        //Debug.Log("#" + axis0 + " " + axis1);
        toObject.rotation = a;
        fromObject.rotation = b * a;
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

    void FixedUpdate()
    {
        UpdateDiffs();
        UpdateAccuracy();
        UpdateTime();
        UpdateStats();
    }

    void UpdateAccuracy()
    {
        float angleAccuracy = 0f;
        angleAccuracy = DockingHelper.Map(angle, initAngle, angleThreshold, 0f, 1f, true);

        float distAccuracy = 0f;
        distAccuracy = DockingHelper.Map(distance, initDist, distThreshold, 0f, 1f, true);

        float acc = 0f;

        switch (manipulatable.manipulationType)
        {
            case DockingHelper.ManipulationType.Translation:
                acc = distAccuracy;
                break;
            case DockingHelper.ManipulationType.Rotation:
                acc = angleAccuracy;
                break;
            case DockingHelper.ManipulationType.Translation | DockingHelper.ManipulationType.Rotation:
                acc = Mathf.Min(angleAccuracy, distAccuracy);
                break;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateColor(acc);
    }

    void UpdateDiffs()
    {
        if (fromObject == null || toObject == null)
            return;

        distance = Vector3.Distance(fromObject.position, toObject.position);

        angle = Quaternion.Angle(fromObject.rotation, toObject.rotation);
    }

    void UpdateTime()
    {
        if (isTimeCounting)
        {
            manipulationTimer += Time.fixedDeltaTime;

            if (!isTouching)
                clutchingTimer += Time.fixedDeltaTime;
        }
    }

    void UpdateStats()
    {
        if (head == null)
            return;

        if (fromObject == null)
            return;

        if (isTimeCounting)
        {
            // head
            //float deltaHD = Vector3.Distance(head.position, preHeadPos);

            //if (deltaHD > minStatDist)
            //    totalHeadDistance += deltaHD;

            //float deltaHA = Quaternion.Angle(head.rotation, preHeadRot);
            //totalHeadAngle += deltaHA;

            // hand
            // only touch
            if (isTouching)
            {
                float deltaHandD = Vector3.Distance(hand.position, preHandPos);

                if (deltaHandD >= kMinStatDist)
                    totalHandDistance += deltaHandD;

                float deltaHandA = Quaternion.Angle(hand.rotation, preHandRot);
                if (deltaHandA >= kMinStatAngle)
                    totalHandAngle += deltaHandA;
            }

            // obj
            float deltaOD = Vector3.Distance(fromObject.position, preFromObjPos);
            if (deltaOD >= kMinStatDist)
                totalObjDistance += deltaOD;

            float deltaOA = Quaternion.Angle(fromObject.rotation, preFromObjRot);
            if (deltaOA >= kMinStatAngle)
                totalObjAngle += deltaOA;

            tSpeed = Vector3.Distance(fromObject.position, preFromObjPos) / Time.fixedDeltaTime;
            rSpeed = Quaternion.Angle(fromObject.rotation, preFromObjRot) / Time.fixedDeltaTime;

            logData.Add(manipulationTimer, tSpeed, rSpeed, distance, angle);
        }

        //preHeadPos = head.position;
        //preHeadRot = head.rotation;
        preHandPos = hand.position;
        preHandRot = hand.rotation;
        preFromObjPos = fromObject.position;
        preFromObjRot = fromObject.rotation;
    }

    public void ManipulationStart()
    {
        isTouching = true;

        // clutch +1
        clutch++;

        // touch check
        if (isFirstTouch)
        {
            maniStartTime = Time.time;
            selectionTimer = maniStartTime - startTime;
            isTimeCounting = true;
            isFirstTouch = false;
        }

        logData.AddManiStart(Time.time - maniStartTime);
    }

    public void ManipulationUpdate()
    {
        // update sths
        EasyAccuracyCheck();
        HardAccuracyCheck();
    }

    public void ManipulationEnd()
    {
        isTouching = false;
        lastReleaseTime = Time.time;

        logData.AddManiEnd(Time.time - maniStartTime);

        // accuracy check
    }




    void EasyAccuracyCheck()
    {
        if (easyThresholdMet)
            return;

        // check easy

        var checkT = (manipulatable.manipulationType & DockingHelper.ManipulationType.Translation) == DockingHelper.ManipulationType.Translation;
        var checkR = (manipulatable.manipulationType & DockingHelper.ManipulationType.Rotation) == DockingHelper.ManipulationType.Rotation;

        var tNotMet = distance > easyDistThreshold;
        var rNotMet = angle > easyAngleThreshold;


        if (checkT && checkR) // 6DOF
        {
            if (tNotMet && rNotMet)
                return;
        }
        else // 3DOF
        {
            if (checkT)
                if (tNotMet)
                    return;

            if (checkR)
                if (rNotMet)
                    return;
        }



        // if met, record stats
        timer_0 = manipulationTimer;
        clutch_0 = clutch;
        distance_0 = distance;
        angle_0 = angle;

        totalHandDistance_0 = totalHandDistance;
        totalHandAngle_0 = totalHandAngle;
        totalObjDistance_0 = totalObjDistance;
        totalObjAngle_0 = totalObjAngle;
        // calculate efficiency
        translationEfficiency_0 = Vector3.Distance(orgFromObjPos, fromObject.position) / totalObjDistance_0;
        rotationEfficiency_0 = Quaternion.Angle(orgFromObjRot, fromObject.rotation) / totalObjAngle_0;

        // record sth
        orgFromObjPos = fromObject.position;
        orgFromObjRot = fromObject.rotation;

        // audio feedback
        if (accFeedback)
        {
            AudioManager.Instance.PlaySound(0);
            ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
        }


        easyThresholdMet = true;

        //totalHeadDistance_0 = totalHeadDistance;
        //totalHeadAngle_0 = totalHeadAngle;
    }

    void HardAccuracyCheck()
    {
        if (!easyThresholdMet)
            return;

        if (hardThresholdMet)
            return;

        // check hard
        //if (distance > distThreshold || angle > angleThreshold)
        //    return;


        var checkT = (manipulatable.manipulationType & DockingHelper.ManipulationType.Translation) == DockingHelper.ManipulationType.Translation;
        var checkR = (manipulatable.manipulationType & DockingHelper.ManipulationType.Rotation) == DockingHelper.ManipulationType.Rotation;

        var tNotMet = distance > distThreshold;
        var rNotMet = angle > angleThreshold;


        if (checkT)
            if (tNotMet)
                return;

        if (checkR)
            if (rNotMet)
                return;


        // audio feedback
        if (accFeedback)
        {
            AudioManager.Instance.PlaySound(1);
            ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
        }

        hardThresholdMet = true;
    }

    public void Attempt()
    {
        numOfAttempts++;
    }

    public void Finish(bool selected = false)
    {
        if (!isTimeCounting)
        {
            ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
            AudioManager.Instance.PlaySound(4);
            return;
        }

        numOfAttempts++;

        if (selected)
        {
            ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
            AudioManager.Instance.PlaySound(4);
            return;
        }


        var checkT = (manipulatable.manipulationType & DockingHelper.ManipulationType.Translation) == DockingHelper.ManipulationType.Translation;
        var checkR = (manipulatable.manipulationType & DockingHelper.ManipulationType.Rotation) == DockingHelper.ManipulationType.Rotation;

        var tNotMet = distance > distThreshold;
        var rNotMet = angle > angleThreshold;

        if (checkT)
            if (tNotMet)
            {
                ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
                AudioManager.Instance.PlaySound(4);
                return;
            }

        if (checkR)
            if (rNotMet)
            {
                ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
                AudioManager.Instance.PlaySound(4);
                return;
            }

        // stop timer
        isTimeCounting = false;

        //reactionTimer = Time.time - lastReleaseTime;

        // calculate efficiency
        translationEfficiency = Vector3.Distance(orgFromObjPos, fromObject.position) / (totalObjDistance - totalObjDistance_0);
        rotationEfficiency = Quaternion.Angle(orgFromObjRot, fromObject.rotation) / (totalObjAngle - totalObjAngle_0);

        // send data
        SendData();

        // audio feedback - sucess
        AudioManager.Instance.PlaySound(2);
    }


    public void SendData()
    {
        if (UserStudyManager.Instance == null)
        {
            Debug.LogWarning("GAME NOT RUNNING!");
            return;
        }

        List<float> data = new List<float>();

        // fill data

        //data.Add(selectionTimer);

        //data.Add(timer_0);
        //data.Add(manipulationTimer - timer_0);
        data.Add(manipulationTimer);

        data.Add(clutchingTimer);
        //data.Add(reactionTimer);

        //data.Add(distance_0);
        data.Add(distance);

        //data.Add(angle_0);
        data.Add(angle);

        //data.Add(clutch_0);
        //data.Add(clutch - clutch_0);
        data.Add(clutch);



        //data.Add(totalHandDistance_0);
        //data.Add(totalHandDistance - totalHandDistance_0);

        //data.Add(totalHandAngle_0);
        //data.Add(totalHandAngle - totalHandAngle_0);

        //data.Add(translationEfficiency_0);
        //data.Add(translationEfficiency);

        //data.Add(rotationEfficiency_0);
        //data.Add(rotationEfficiency);

        //data.Add(numOfAttempts);

        // data for on and off

        // user input log e.g. speed




        // set completion time
        //UIManager.Instance.SetText(string.Format(timeFormatStr, timer));

        // send to user study manager
        UserStudyManager.Instance.SetTaskResult(new UserStudyManager.Trial(data, logData));

    }
}
