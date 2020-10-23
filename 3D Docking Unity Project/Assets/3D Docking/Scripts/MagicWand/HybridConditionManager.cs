using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridConditionManager : ConditionManager
{
    public HomerManipulatable homer;
    public HybridManipulatable hybrid;
    protected override void UpdateCondition()
    {
        switch (CurrentCondition)
        {
            case 0:
                homer.enabled = true;
                hybrid.enabled = false;
                //hybrid.selectionMode = HybridManipulatable.SelectionMode.Normal;
                //hybrid.manipulationMode = HybridManipulatable.ManipulationMode.Scaled_HOMER;
                //hybrid.viewpointControl = false;
                break;
            case 1:
                homer.enabled = false;
                hybrid.enabled = true;
                //hybrid.selectionMode = HybridManipulatable.SelectionMode.Lazy;
                //hybrid.manipulationMode = HybridManipulatable.ManipulationMode.Hybrid_HOMER;
                //hybrid.viewpointControl = true;
                break;
        }
    }
}
