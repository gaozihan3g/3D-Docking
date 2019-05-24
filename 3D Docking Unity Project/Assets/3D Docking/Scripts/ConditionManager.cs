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
        VR_fAST,
        AR_SLOW,
        AR_NORMAL,
        AR_FAST
    };

    public Condition curCondition;
    public static ConditionManager Instance;

    public MeshRenderer camRenderer;
    public GameObject environment;
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
                ToggleEnvironment(true);
                ToggleCam(false);
                break;
            case Condition.VR_NORMAL:
                SetSpeedLevel(1);
                ToggleEnvironment(true);
                ToggleCam(false);
                break;
            case Condition.VR_fAST:
                SetSpeedLevel(2);
                ToggleEnvironment(true);
                ToggleCam(false);
                break;
            case Condition.AR_SLOW:
                SetSpeedLevel(0);
                ToggleEnvironment(false);
                ToggleCam(true);
                break;
            case Condition.AR_NORMAL:
                SetSpeedLevel(1);
                ToggleEnvironment(false);
                ToggleCam(true);
                break;
            case Condition.AR_FAST:
                SetSpeedLevel(2);
                ToggleEnvironment(false);
                ToggleCam(true);
                break;
        }
    }

    void ToggleCam(bool b)
    {
        if (camRenderer == null)
            return;

        camRenderer.enabled = b;
    }

    void ToggleEnvironment(bool b)
    {
        if (environment == null)
            return;

        environment.SetActive(b);
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
