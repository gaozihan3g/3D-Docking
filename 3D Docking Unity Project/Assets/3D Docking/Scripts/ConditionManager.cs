using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConditionManager : MonoBehaviour
{
    public static ConditionManager Instance;

    protected void Awake()
    {
        Instance = this;
    }

    public List<string> conditionNames;

    [SerializeField]
    protected int currentCondition;

    public int NumOfConditions
    {
        get
        {
            return conditionNames.Count;
        }
    }

    public int CurrentCondition
    {
        get => currentCondition;
        set
        {
            if (value == currentCondition)
                return;

            currentCondition = value;
            UpdateCondition();
        }
    }

    public string GetConditionName(int c)
    {
        if (conditionNames == null || conditionNames[c] == "")
            return c.ToString();

        return conditionNames[c];
    }

    public string GetCurrentConditionName()
    {
        return conditionNames[CurrentCondition];
    }

    protected virtual void UpdateCondition()
    { }

    protected void OnValidate()
    {
        UpdateCondition();
    }
}
