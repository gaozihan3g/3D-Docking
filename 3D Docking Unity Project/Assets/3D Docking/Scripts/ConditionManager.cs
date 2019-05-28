using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ConditionManager : MonoBehaviour
{
    public enum Condition
    {
        VR_SLOW,
        VR_NORMAL,
        VR_FAST,
        AR_SLOW,
        AR_NORMAL,
        AR_FAST
    };

    public Condition curCondition;
    public static ConditionManager Instance;

    public GameObject[] highResObjects;
    public GameObject[] lowResObjects;
    public Manipulatable obj;

    public float slow = 0.33f;
    public float normal = 1f;
    public float fast = 3f;



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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnValidate()
    {
        UpdateCondition();
    }

    public void SetCondition(int c)
    {
        curCondition = (Condition)c;
        UpdateCondition();
    }

    void UpdateCondition()
    {
        switch (curCondition)
        {
            case Condition.VR_SLOW:
                SetSpeedLevel(0);
                ToggleLowRes(true);
                ToggleHighRes(false);
                break;
            case Condition.VR_NORMAL:
                SetSpeedLevel(1);
                ToggleLowRes(true);
                ToggleHighRes(false);
                break;
            case Condition.VR_FAST:
                SetSpeedLevel(2);
                ToggleLowRes(true);
                ToggleHighRes(false);
                break;
            case Condition.AR_SLOW:
                SetSpeedLevel(0);
                ToggleLowRes(false);
                ToggleHighRes(true);
                break;
            case Condition.AR_NORMAL:
                SetSpeedLevel(1);
                ToggleLowRes(false);
                ToggleHighRes(true);
                break;
            case Condition.AR_FAST:
                SetSpeedLevel(2);
                ToggleLowRes(false);
                ToggleHighRes(true);
                break;
        }
    }

    void ToggleHighRes(bool b)
    {
        if (highResObjects == null)
            return;

        foreach (var o in highResObjects)
        {
            if (o != null)
                o.SetActive(b);
        }
    }

    void ToggleLowRes(bool b)
    {
        if (lowResObjects == null)
            return;

        foreach (var o in lowResObjects)
        {
            if (o != null)
                o.SetActive(b);
        }

    }

    void SetSpeedLevel(int i)
    {
        if (obj == null)
            return;

        switch (i)
        {
            case 0:
                obj.SetRotationFactor(slow);
                break;
            case 1:
                obj.SetRotationFactor(normal);
                break;
            case 2:
                obj.SetRotationFactor(fast);
                break;
        }
    }
}
