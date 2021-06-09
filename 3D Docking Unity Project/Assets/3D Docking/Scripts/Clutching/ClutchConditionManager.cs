using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClutchConditionManager : ConditionManager
{
    public DockingManager dm;

    public float[] initDists = { 1f, 10f };
    public float[] initAngles = { 45f, 135f };
    public float[] init6DofDists = { 1f, 10f };
    public float[] init6DofAngles = { 1f, 10f };
    public float[] targetDists = { 2f, 8f };

    public List<ConditionConfig> config;

    [Serializable]
    public class ConditionConfig
    {
        public string name;
        public ClutchManipulatable.FlowType flowType;
        public DockingHelper.ManipulationType manipulationType;
        public float initDist;
        public float initAngle;
        public float targetDist;
    }

    // Start is called before the first frame update
    void Start()
    {
        ConditionSetup();
    }

    void ConditionSetup()
    {
        config = new List<ConditionConfig>();
        conditionNames = new List<string>();
        int maxNumOfID = Mathf.Max(initDists.Length, initAngles.Length, init6DofDists.Length, init6DofAngles.Length);


        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int m = 0; m < maxNumOfID; m++)
                {
                    for (int n = 0; n < targetDists.Length; n++)
                    {
                        ConditionConfig cc = new ConditionConfig();

                        cc.flowType = (ClutchManipulatable.FlowType)i;

                        switch (j)
                        {
                            case 0:
                                if (m >= initDists.Length)
                                    continue;
                                cc.manipulationType = DockingHelper.ManipulationType.Translation;
                                cc.initDist = initDists[m];
                                break;
                            case 1:
                                if (m >= initAngles.Length)
                                    continue;
                                cc.manipulationType = DockingHelper.ManipulationType.Rotation;
                                cc.initAngle = initAngles[m];
                                break;
                            case 2:
                                if (m >= init6DofDists.Length)
                                    continue;
                                cc.manipulationType = DockingHelper.ManipulationType.Translation | DockingHelper.ManipulationType.Rotation;
                                cc.initDist = init6DofDists[m];
                                cc.initAngle = init6DofAngles[m];
                                break;
                        }

                        cc.targetDist = targetDists[n];

                        string str = "";

                        switch (i)
                        {
                            case 0:
                                str += "Norm";
                                break;
                            case 1:
                                str += "Lazy";
                                break;
                        }

                        str += "_";

                        switch (j)
                        {
                            case 0:
                                str += "T";
                                break;
                            case 1:
                                str += "R";
                                break;
                            case 2:
                                str += "6";
                                break;
                        }

                        str += "_" + m + "_" + n;
                        cc.name = str;

                        //TODO remove condition names
                        conditionNames.Add(str);
                        config.Add(cc);
                    }
                }

            }
        }
    }

    protected override void UpdateCondition()
    {
        if (dm == null)
            return;
        if (config == null || config.Count == 0)
            return;

        // get config based on current condition
        ConditionConfig cc = config[CurrentCondition];
        var manipulatable = dm.manipulatable;
        manipulatable.flow = cc.flowType;
        manipulatable.manipulationType = cc.manipulationType;
        dm.initDist = cc.initDist;
        dm.initAngle = cc.initAngle;
        dm.targetDist = cc.targetDist;
    }

    // Update is called once per frame
    void Update()
    {

    }
}


//void ConditionSetup()
//{
//    config = new List<ConditionConfig>();
//    conditionNames = new List<string>();

//    for (int i = 0; i < 4; i++)
//    {
//        for (int j = 0; j < 3; j++)
//        {
//            for (int k = 0; k < 3; k++)
//            {
//                for (int l = 0; l < 2; l++)
//                {
//                    ConditionConfig cc = new ConditionConfig();

//                    cc.flowType = (ClutchManipulatable.FlowType)i;

//                    switch (j)
//                    {
//                        case 0:
//                            cc.manipulationType = DockingHelper.ManipulationType.Translation;
//                            cc.initDist = initDists[k];
//                            break;
//                        case 1:
//                            cc.manipulationType = DockingHelper.ManipulationType.Rotation;
//                            cc.initAngle = initAngles[k];
//                            break;
//                        case 2:
//                            cc.manipulationType = DockingHelper.ManipulationType.Translation | DockingHelper.ManipulationType.Rotation;
//                            cc.initDist = initDists[k];
//                            cc.initAngle = initAngles[k];
//                            break;
//                    }

//                    cc.targetDist = targetDists[l];

//                    string str = "";

//                    switch (i)
//                    {
//                        case 0:
//                            str += "DRG_ONE";
//                            break;
//                        case 1:
//                            str += "CLK_ONE";
//                            break;
//                        case 2:
//                            str += "DRG_TWO";
//                            break;
//                        case 3:
//                            str += "CLK_TWO";
//                            break;
//                    }

//                    str += "_";

//                    switch (j)
//                    {
//                        case 0:
//                            str += "T";
//                            break;
//                        case 1:
//                            str += "R";
//                            break;
//                        case 2:
//                            str += "6";
//                            break;
//                    }

//                    str += "_" + k + "_" + l;
//                    cc.name = str;

//                    //TODO remove condition names
//                    conditionNames.Add(str);
//                    config.Add(cc);
//                }
//            }
//        }
//    }