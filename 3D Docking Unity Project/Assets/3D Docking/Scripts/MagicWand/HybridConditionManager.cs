using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridConditionManager : ConditionManager
{
    //public HomerManipulatable homer;
    public HybridManipulatable hybrid;
    protected override void UpdateCondition()
    {
        switch (CurrentCondition)
        {
            case 0:
                hybrid.selectionMode = HybridManipulatable.SelectionMode.Normal;
                hybrid.rotationMode = HybridManipulatable.RotationMode.ISO;
                hybrid.viewpointControl = false;
                hybrid.prismFineTuning = false;
                break;
            case 1:
                hybrid.selectionMode = HybridManipulatable.SelectionMode.Lazy;
                hybrid.rotationMode = HybridManipulatable.RotationMode.NON_ISO;
                hybrid.viewpointControl = true;
                hybrid.prismFineTuning = true;
                break;
        }
    }
}
