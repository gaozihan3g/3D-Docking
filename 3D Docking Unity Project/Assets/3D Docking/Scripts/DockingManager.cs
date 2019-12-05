using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DockingManager : MonoBehaviour
{
    public Transform anchor;
    public Transform fromObject;
    public Transform toObject;
    public Transform head;
    public Transform[] hands;
    public bool IsRightHand = true;
    Transform hand;

    public float randomOffsetRadius = 1f;
    public float initDist = 1f;
    public float initAngle;

    [Range(0f, 0.1f)]
    public float easyDistThreshold = 0.03f;
    [Range(0f, 30f)]
    public float easyAngleThreshold = 30f;

    [Range(0f, 0.1f)]
    public float distThreshold = 0.01f;
    [Range(0f, 30f)]
    public float angleThreshold = 5f;

    //public bool updateDistance = false;

    bool isFirstTouch = true;
    bool isTouching = false;
    bool isTimeCounting = false;

    bool easyThresholdMet = false;

    float startTime;

    public float timer = 0f;
    public int clutch = -1;

    public float distance;
    public float angle;

    public float totalHeadDistance = 0f;
    public float totalHeadAngle = 0f;

    public float totalHandDistance = 0f;
    public float totalHandAngle = 0f;

    const float minStatDist = 0.001f;
    const float minStatAngle = 0.001f;

    public float totalObjDistance = 0f;
    public float totalObjAngle = 0f;

    public float translationEfficiency = 0f;
    public float rotationEfficiency = 0f;

    public float timer_0 = 0f;
    public int clutch_0 = 0;

    public float distance0;
    public float angle0;

    public float totalHeadDistance_0 = 0f;
    public float totalHeadAngle_0 = 0f;

    public float totalHandDistance_0 = 0f;
    public float totalHandAngle_0 = 0f;

    public float totalObjDistance_0 = 0f;
    public float totalObjAngle_0 = 0f;

    public float translationEfficiency_0 = 0f;
    public float rotationEfficiency_0 = 0f;

    Vector3 preHeadPos;
    Quaternion preHeadRot;

    Vector3 preHandPos;
    Quaternion preHandRot;

    Vector3 preFromObjPos;
    Quaternion preFromObjRot;

    Vector3 orgFromObjPos;
    Quaternion orgFromObjRot;

    public static DockingManager Instance;

    

    public void Init()
    {
        isFirstTouch = true;
        isTimeCounting = false;
        easyThresholdMet = false;

        UpdateHand();

        startTime = 0f;
        timer = 0f;

        // clutch = 0 means no clutch
        clutch = -1;

        totalHeadDistance = 0f;
        totalHeadAngle = 0f;

        totalHandDistance = 0f;
        totalHandAngle = 0f;

        totalObjDistance = 0f;
        totalObjAngle = 0f;

        timer_0 = 0f;
        clutch_0 = 0;

        distance0 = 0f;
        angle0 = 0f;

        totalHeadDistance_0 = 0f;
        totalHeadAngle_0 = 0f;

        totalHandDistance_0 = 0f;
        totalHandAngle_0 = 0f;

        totalObjDistance_0 = 0f;
        totalObjAngle_0 = 0f;

        translationEfficiency_0 = 0f;
        rotationEfficiency_0 = 0f;


        // reset pos
        RandomPosition();

        // reset rot
        RandomRotation();

        // register org data
        orgFromObjPos = fromObject.position;
        orgFromObjRot = fromObject.rotation;


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

    public void RandomPosition()
    {
        fromObject.position = anchor.position + Random.onUnitSphere * randomOffsetRadius;

        var theta = Random.Range(0f, 2 * Mathf.PI);
        Vector3 d = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta));
        toObject.position = fromObject.position + d * initDist;
    }

    public void RandomRotation()
    {
        var a = Random.rotation;
        var x = Random.onUnitSphere;
        var b = Quaternion.AngleAxis(initAngle, x);

        fromObject.rotation = a;
        toObject.rotation = b * a;
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
        float angleAccuracy = Map(angle, initAngle, 0f, 0f, 1f, true);

        if (UIManager.Instance != null)
            UIManager.Instance.SetColor(angleAccuracy);
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
            timer = Time.time - startTime;
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
            float deltaHD = Vector3.Distance(head.position, preHeadPos);

            if (deltaHD > minStatDist)
                totalHeadDistance += deltaHD;

            float deltaHA = Quaternion.Angle(head.rotation, preHeadRot);
            totalHeadAngle += deltaHA;

            // hand
            // only touch
            if (isTouching)
            {
                float deltaHandD = Vector3.Distance(hand.position, preHandPos);

                if (deltaHandD >= minStatDist)
                    totalHandDistance += deltaHandD;

                float deltaHandA = Quaternion.Angle(hand.rotation, preHandRot);
                print("deltaHandA " + deltaHandA.ToString("F3"));
                if (deltaHandA >= minStatAngle)
                    totalHandAngle += deltaHandA;
            }

            // obj
            float deltaOD = Vector3.Distance(fromObject.position, preFromObjPos);
            if (deltaOD >= minStatDist)
                totalObjDistance += deltaOD;

            float deltaOA = Quaternion.Angle(fromObject.rotation, preFromObjRot);
            if (deltaOA >= minStatAngle)
                totalObjAngle += deltaOA;

        }

        preHeadPos = head.position;
        preHeadRot = head.rotation;

        preHandPos = hand.position;
        preHandRot = hand.rotation;

        preFromObjPos = fromObject.position;
        preFromObjRot = fromObject.rotation;
    }

    public void TouchStart()
    {
        isTouching = true;

        // clutch +1
        clutch++;

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
        EasyAccuracyCheck();
    }

    public void TouchEnd()
    {
        isTouching = false;
        // accuracy check
        HardAccuracyCheck();
    }

    void EasyAccuracyCheck()
    {
        // check easy
        if (distance > easyDistThreshold)
            return;

        if (angle > easyAngleThreshold)
            return;

        // change flag
        if (!easyThresholdMet)
        {
            // if met, record stats
            timer_0 = timer;
            clutch_0 = clutch;

            distance0 = distance;
            angle0 = angle;

            totalHeadDistance_0 = totalHeadDistance;
            totalHeadAngle_0 = totalHeadAngle;

            totalHandDistance_0 = totalHandDistance;
            totalHandAngle_0 = totalHandAngle;

            totalObjDistance_0 = totalObjDistance;
            totalObjAngle_0 = totalObjAngle;

            // calculate efficiency
            translationEfficiency_0 = Vector3.Distance(orgFromObjPos, fromObject.position) / totalObjDistance_0;
            rotationEfficiency_0 = (initAngle - easyAngleThreshold) / totalObjAngle_0;

            // record sth


            // audio feedback
            AudioManager.Instance.PlaySound(0);

            easyThresholdMet = true;
        }
    }

    void HardAccuracyCheck()
    {
        // check hard
        if (distance > distThreshold)
            return;

        if (angle > angleThreshold)
            return;

        // stop timer
        isTimeCounting = false;

        // audio feedback
        // if last task
        AudioManager.Instance.PlaySound(1);

        // calculate efficiency
        translationEfficiency = Vector3.Distance(orgFromObjPos, fromObject.position) / totalObjDistance;
        rotationEfficiency = (easyAngleThreshold - angleThreshold) / (totalObjAngle - totalObjAngle_0);

        // send data
        SendData();

        // auto start next trial
        AutoStartNextTrial();
    }

    void AutoStartNextTrial()
    {
        UserStudyManager.Instance.AutoSetNextTask();
    }

    public void SendData()
    {
        if (UserStudyManager.Instance == null)
        {
            Debug.Log("GAME NOT RUNNING!");
            return;
        }



        List<float> data = new List<float>();

        // fill data

        data.Add(timer_0);
        // get fine-tuning time
        var timer_1 = timer - timer_0;
        data.Add(timer_1);

        data.Add(angle0);
        data.Add(angle);

        //data.Add(distance0);
        //data.Add(distance);

        data.Add(clutch_0);
        // get fine-tuning clutch
        var clutch_1 = clutch - clutch_0;
        data.Add(clutch_1);

        //data.Add(totalHeadDistance_0);
        //data.Add(totalHeadDistance);

        //data.Add(totalHeadAngle_0);
        //data.Add(totalHeadAngle);

        //data.Add(totalHandDistance_0);
        //data.Add(totalHandDistance);

        data.Add(totalHandAngle_0);
        // get fine-tuning angle
        var totalHandAngle_1 = totalHandAngle - totalHandAngle_0;
        data.Add(totalHandAngle_1);

        //data.Add(translationEfficiency_0);
        //data.Add(translationEfficiency);

        data.Add(rotationEfficiency_0);
        data.Add(rotationEfficiency);

        // set completion time
        //UIManager.Instance.SetText(string.Format(timeFormatStr, timer));

        // send to user study manager
        UserStudyManager.Instance.SetTaskResult(new UserStudyManager.Trial(data));

    }

    public static float Map(float v, float fmin, float fmax, float tmin, float tmax, bool clamp = false)
    {
        float fd = fmax - fmin;
        float t = (v - fmin) / fd;
        float td = tmax - tmin;
        float r = tmin + t * td;
        if (clamp)
            return Mathf.Clamp(r, tmin, tmax);
        return r;
    }

}
