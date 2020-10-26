using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridConditionManager : ConditionManager
{
    public HybridManipulatable tech;
    protected override void UpdateCondition()
    {
        switch (CurrentCondition)
        {
            case 0:
                tech.lazySelection = false;
                tech.nonIsoRotation = false;
                tech.viewpointControl = false;
                tech.prismFineTuning = false;
                break;
            case 1:
                tech.lazySelection = true;
                tech.nonIsoRotation = false;
                tech.viewpointControl = false;
                tech.prismFineTuning = false;
                break;
            case 2:
                tech.lazySelection = false;
                tech.nonIsoRotation = true;
                tech.viewpointControl = false;
                tech.prismFineTuning = false;
                break;
            case 3:
                tech.lazySelection = false;
                tech.nonIsoRotation = false;
                tech.viewpointControl = true;
                tech.prismFineTuning = false;
                break;
            case 4:
                tech.lazySelection = false;
                tech.nonIsoRotation = false;
                tech.viewpointControl = false;
                tech.prismFineTuning = true;
                break;
            case 5:
                tech.lazySelection = true;
                tech.nonIsoRotation = true;
                tech.viewpointControl = true;
                tech.prismFineTuning = true;
                break;
            case 6:
                tech.lazySelection = true;
                tech.nonIsoRotation = true;
                tech.viewpointControl = true;
                tech.prismFineTuning = false;
                break;
        }
    }
}
