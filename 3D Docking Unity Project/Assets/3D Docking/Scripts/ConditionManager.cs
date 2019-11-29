using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ConditionManager : MonoBehaviour
{
    //public enum Condition
    //{
    //    VR_SLOW,
    //    VR_NORMAL,
    //    VR_FAST,
    //    AR_SLOW,
    //    AR_NORMAL,
    //    AR_FAST
    //};

    public enum Condition
    {
        VR_ISO_N,
        VR_ISO_Y,
        VR_NON_N,
        VR_NON_Y,
        AR_ISO_N,
        AR_ISO_Y,
        AR_NON_N,
        AR_NON_Y,
    };


    public Condition curCondition;
    public static ConditionManager Instance;

    public Camera cam;
    public GameObject[] hands;
    public GameObject grid;

    //public GameObject[] highResObjects;
    //public GameObject[] lowResObjects;
    public Manipulatable obj;

    //public float slow = 0.33f;
    public float normal = 1f;
    public float fast = 3f;

    public float initDist = 0.3f;

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
        if (curCondition == (Condition)c)
            return;

        curCondition = (Condition)c;
        UpdateCondition();
    }

    void UpdateCondition()
    {
        switch (curCondition)
        {
            case Condition.VR_ISO_N:
                SetMR(0);
                SetSpeedLevel(0);
                SetTranslation(0);
                break;
            case Condition.VR_ISO_Y:
                SetMR(0);
                SetSpeedLevel(0);
                SetTranslation(1);
                break;
            case Condition.VR_NON_N:
                SetMR(0);
                SetSpeedLevel(1);
                SetTranslation(0);
                break;
            case Condition.VR_NON_Y:
                SetMR(0);
                SetSpeedLevel(1);
                SetTranslation(1);
                break;
            case Condition.AR_ISO_N:
                SetMR(1);
                SetSpeedLevel(0);
                SetTranslation(0);
                break;
            case Condition.AR_ISO_Y:
                SetMR(1);
                SetSpeedLevel(0);
                SetTranslation(1);
                break;
            case Condition.AR_NON_N:
                SetMR(1);
                SetSpeedLevel(1);
                SetTranslation(0);
                break;
            case Condition.AR_NON_Y:
                SetMR(1);
                SetSpeedLevel(1);
                SetTranslation(1);
                break;
        }
    }

    void SetMR(int i)
    {
        if (cam == null)
            return;

        if (hands == null)
            return;

        switch (i)
        {
            case 0:
                // cam
                cam.clearFlags = CameraClearFlags.SolidColor;
                // hand
                foreach (var h in hands)
                    h.SetActive(false);

                grid.SetActive(true);
                break;
            case 1:
                // cam
                cam.clearFlags = CameraClearFlags.Skybox;
                // hand
                foreach (var h in hands)
                    h.SetActive(true);

                grid.SetActive(false);
                break;
        }
    }

    void SetSpeedLevel(int i)
    {
        if (obj == null)
            return;

        switch (i)
        {
            case 0:
                obj.RotationSetup(normal);
                break;
            case 1:
                obj.RotationSetup(fast);
                break;
            case 2:
                obj.RotationSetup(0f, true);
                break;
        }
    }

    void SetTranslation(int i)
    {
        if (obj == null)
            return;

        if (DockingManager.Instance == null)
            return;

        switch (i)
        {
            case 0:
                obj.translationEnabled = false;
                DockingManager.Instance.initDist = 0f;
                DockingManager.Instance.Init();
                break;
            case 1:
                obj.translationEnabled = true;
                DockingManager.Instance.initDist = initDist;
                DockingManager.Instance.Init();
                break;
        }
    }


}
