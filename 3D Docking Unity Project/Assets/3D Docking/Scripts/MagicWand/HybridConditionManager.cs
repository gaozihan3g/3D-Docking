using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridConditionManager : ConditionManager
{
    public HybridManipulatable manipulatable;
    public DockingManager dm;

    public float targetDistS = 3f;
    public float targetDistL = 10f;

    public float moveDistS = 5f;
    public float moveDistL = 10f;

    public float angleS = 0f;
    public float angleL = 135f;


    void Start()
    {
    }

    void DofSetup(bool is6Dof)
    {
        manipulatable.manipulationType = is6Dof ? DockingHelper.ManipulationType.Translation | DockingHelper.ManipulationType.Rotation : DockingHelper.ManipulationType.Translation;
        dm.initAngle = is6Dof ? angleL : angleS;
    }


    protected override void UpdateCondition()
    {
        if (manipulatable == null)
            return;
        if (dm == null)
            return;

        for (int i = 0; i < conditionNames.Count; i++)
        {
            conditionNames[i] = ((i & 1 << 3) == 1 << 3 ? "N" : "H")
                + ((i & 1 << 2) == 1 << 2 ? "6" : "3")
                + ((i & 1 << 1) == 1 << 1 ? "L" : "S")
                + ((i & 1 << 0) == 1 << 0 ? "L" : "S");
        }


        manipulatable.Tech = (CurrentCondition & 1 << 3) == 1 << 3 ? HybridManipulatable.TechType.NOVEL : HybridManipulatable.TechType.HOMER;
        DofSetup((CurrentCondition & 1 << 2) == 1 << 2);
        dm.targetDist = (CurrentCondition & 1 << 1) == 1 << 1 ? targetDistL : targetDistS;
        dm.initDist = (CurrentCondition & 1 << 0) == 1 << 0 ? moveDistL : moveDistS;
    }


}

