using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentConditionManager : ConditionManager
{
    public HybridManipulatable manipulatable;
    public DockingManager dm;


    void DofSetup(bool is6Dof)
    {
        manipulatable.manipulationType = is6Dof ? DockingHelper.ManipulationType.Translation | DockingHelper.ManipulationType.Rotation : DockingHelper.ManipulationType.Translation;
    }

    protected override void UpdateCondition()
    {
        if (manipulatable == null)
            return;
        if (dm == null)
            return;

        for (int i = 0; i < conditionNames.Count; i++)
        {
            conditionNames[i] = ((i & 1 << 3) == 1 << 3 ? "N" : "I")
                + ((i & 1 << 2) == 1 << 2 ? "P" : "H")
                + ((i & 1 << 1) == 1 << 1 ? "V" : "N")
                + ((i & 1 << 0) == 1 << 0 ? "L" : "N");
        }

        manipulatable.nonIsoRotation = (CurrentCondition & 1 << 3) == 1 << 3;
        manipulatable.prismFineTuning = (CurrentCondition & 1 << 2) == 1 << 2;
        manipulatable.viewpointControl = (CurrentCondition & 1 << 1) == 1 << 1;
        manipulatable.lazyRelease = (CurrentCondition & 1 << 0) == 1 << 0;

        //DofSetup((CurrentCondition & 1 << 2) == 1 << 2);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
