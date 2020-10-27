﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

public class HybridManipulatable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum TechType
    {
        HOMER,
        NOVEL
    }



    public enum TranslationTechType
    {
        Homer,
        Prism,
    }


    public bool calibrated = false;

    [SerializeField]
    TechType tech;
    public DockingHelper.ManipulationType manipulationType;
    bool lazySelection = false;
    bool nonIsoRotation = false;
    bool viewpointControl = false;
    bool prismFineTuning = false;

    TranslationTechType transTech = TranslationTechType.Homer;
    bool selected = false;
    bool pointed = false;
    bool manipulationStarted = false;

    RigidPose oHandRigidPose;
    RigidPose pHandRigidPose;
    RigidPose cHandRigidPose;
    RigidPose oRigidPose;
    RigidPose pRigidPose;
    RigidPose cRigidPose;
    RigidPose cHmdPose;

    Vector3 origin;
    float ratio;
    Vector3 scaledHandPos;
    Vector3 offset;
    [SerializeField]
    Vector3 HmdOffset = new Vector3(0f, -0.4f, 0f);
    //float gizmosR = 0.03f;

    float minSpeed = 0.01f;
    float maxSpeed = 1f;
    float minScale = 0f;
    float maxScale = 1f;
    [Tooltip("m/sec")]
    public float speed;

    float minAngSpeed = 1f;
    float maxAngSpeed = 90f;
    float minAngScale = 0f;
    float maxAngScale = 3f;
    [Tooltip("degree/sec")]
    public float angSpeed;

    public float xSpeed;
    public float ySpeed;
    public float zSpeed;
    //public float minAxisSpeed;
    //public bool X;
    //public bool Y;
    //public bool Z;

    public float translationScaleFactor = 1f;
    public float rotationScaleFactor = 1f;

    public float techThreshold = 0.3f;
    public float handTorsoDist;

    public TranslationTechType TranslationTech
    {
        get => transTech;
        set
        {
            transTech = value;
            UpdateCursorState();
        }
    }

    public TechType Tech
    {
        get => tech;
        set
        {
            tech = value;
            UpdateTechState();
        }
    }

    public void Init()
    {
        selected = false;
        pointed = false;
        manipulationStarted = false;
    }


    void Update()
    {
        if (!calibrated)
        {
            Calibrate();
            return;
        }

        if (lazySelection)
        {
            CheckSelection();
            UpdateManipulation();
        }
        else
        {
            NormalSelection();
        }

        InputCheck();
    }

    void OnValidate()
    {
        UpdateTechState();
        UpdateCursorState();
    }

    void UpdateCursorState()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetCursorColor(TranslationTech == TranslationTechType.Homer ? Color.blue : Color.green);
        }

    }
    void UpdateTechState()
    {
        lazySelection = tech == TechType.NOVEL;
        nonIsoRotation = tech == TechType.NOVEL;
        viewpointControl = tech == TechType.NOVEL;
        prismFineTuning = tech == TechType.NOVEL;
    }


    void InputCheck()
    {
        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Grip))
        {
            if (selected)
            {
                ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
                AudioManager.Instance.PlaySound(4);
            }
            else
            {
                DockingManager.Instance.Finish();
            }

            
        }

        //if (ViveInput.GetPressUpEx(HandRole.LeftHand, ControllerButton.Grip))
        //{
        //    DockingManager.Instance.Init();
        //}


        var r = ViveInput.GetPadTouchAxisEx(HandRole.RightHand);

        if (viewpointControl)
        {
            UIManager.Instance.CamZoom(r != Vector2.zero);
        }

        if (prismFineTuning)
        {
            if (manipulationStarted)
                return;

            TranslationTech = (r != Vector2.zero) ? TranslationTechType.Prism : TranslationTechType.Homer;
        }
    }

    /// <summary>
    /// put controller on the chest to calibrate
    /// </summary>
    void Calibrate()
    {
        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Grip))
        {
            var hand = VivePose.GetPoseEx(HandRole.RightHand).pos;
            var head = VivePose.GetPoseEx(DeviceRole.Hmd).pos;

            var handHeadDist = Vector3.Distance(hand, head);

            HmdOffset.y = -handHeadDist;

            calibrated = true;
        }
    }

    void CalculateScaleFactor()
    {
        // get speed

        Vector3 deltaHandPos = cHandRigidPose.pos - pHandRigidPose.pos;

        speed = deltaHandPos.magnitude / Time.deltaTime;

        xSpeed = Mathf.Abs(deltaHandPos.x) / Time.deltaTime;
        ySpeed = Mathf.Abs(deltaHandPos.y) / Time.deltaTime;
        zSpeed = Mathf.Abs(deltaHandPos.z) / Time.deltaTime;

        //X = xSpeed > minAxisSpeed;
        //Y = ySpeed > minAxisSpeed;
        //Z = zSpeed > minAxisSpeed;

        translationScaleFactor = DockingHelper.Map(speed, minSpeed, maxSpeed, minScale, maxScale, true);


        if (!nonIsoRotation)
        {
            rotationScaleFactor = 1f;
        }
        else
        {
            // get scale factor based on rotation speed
            angSpeed = Quaternion.Angle(pHandRigidPose.rot, cHandRigidPose.rot) / Time.deltaTime;
            rotationScaleFactor = DockingHelper.Map(angSpeed, minAngSpeed, maxAngSpeed, minAngScale, maxAngScale, true);
        }
    }

    void NormalSelection()
    {
        // define start and end

        if (pointed && !manipulationStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {

            //ui
            UIManager.Instance.SetupPointer(transform, false);
            //ui

            OnManipulationStart();
        }

        if (manipulationStarted)
            OnManipulationUpdate();

        if (manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            //ui
            UIManager.Instance.SetupPointer(transform, true);

            OnManipulationEnd();
        }
    }



    void CheckSelection()
    {
        if (pointed && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnSelected();
        }

        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Menu))
        {
            OnDeselected();
        }
    }

    void OnSelected()
    {
        selected = true;

        ViveInput.TriggerHapticVibration(HandRole.RightHand);

        //ui
        UIManager.Instance.SetupPointer(transform, false);

        DockingManager.Instance.ManipulationStart();
    }

    void OnDeselected()
    {
        selected = false;

        ViveInput.TriggerHapticVibration(HandRole.RightHand);

        //ui
        UIManager.Instance.SetupPointer(transform, true);

        //DockingManager.Instance.TouchEnd();
    }

    void UpdateManipulation()
    {
        if (!selected)
            return;

        // define start and end

        if (!manipulationStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationStart();
        }

        if (manipulationStarted)
            OnManipulationUpdate();

        if (manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationEnd();
        }
    }

    void OnManipulationStart()
    {
        manipulationStarted = true;

        // record data
        oHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        oRigidPose = new RigidPose(transform);

        pHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        pRigidPose = new RigidPose(transform);

        cHmdPose = VivePose.GetPoseEx(DeviceRole.Hmd);
        origin = cHmdPose.pos + HmdOffset;

        // get ratio
        float oWandR = Vector3.Distance(origin, oHandRigidPose.pos);
        float oObjR = Vector3.Distance(origin, oRigidPose.pos);
        ratio = oObjR / oWandR;

        scaledHandPos = oHandRigidPose.pos;

        if (TranslationTech == TranslationTechType.Homer)
        {
            offset = oRigidPose.pos - (origin + (oHandRigidPose.pos - origin).normalized * oObjR);
        }
        else
        {
            offset = oRigidPose.pos - scaledHandPos;
        }

        DockingManager.Instance.ManipulationStart();
    }

    void OnManipulationUpdate()
    {
        cHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);

        CalculateScaleFactor();

        if ((manipulationType & DockingHelper.ManipulationType.Translation) == DockingHelper.ManipulationType.Translation)
        {
            // translation
            Vector3 deltaPos = cHandRigidPose.pos - pHandRigidPose.pos;

            if (TranslationTech == TranslationTechType.Homer)
            {
                Vector3 diffPos = deltaPos * translationScaleFactor;
                scaledHandPos += diffPos;

                Vector3 dir = (scaledHandPos - origin).normalized;
                float cWandR = Vector3.Distance(origin, scaledHandPos);
                transform.position = origin + dir * cWandR * ratio + offset;
            }
            else
            {
                //diffPos.x = X ? diffPos.x : 0f;
                //diffPos.y = Y ? diffPos.y : 0f;
                //diffPos.z = Z ? diffPos.z : 0f;

                scaledHandPos += deltaPos;
                transform.position = scaledHandPos + offset;
            }
        }

        if ((manipulationType & DockingHelper.ManipulationType.Rotation) == DockingHelper.ManipulationType.Rotation)
        {
            // rotation
            Quaternion delta = DockingHelper.GetDeltaQuaternion(pHandRigidPose.rot, cHandRigidPose.rot);
            Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);
            transform.rotation = diff * pRigidPose.rot;
        }



        cRigidPose = new RigidPose(transform); // for debugging
        pHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        pRigidPose = new RigidPose(transform);

        DockingManager.Instance.ManipulationUpdate();
    }

    void OnManipulationEnd()
    {
        manipulationStarted = false;

        DockingManager.Instance.ManipulationEnd();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointed = true;
        ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointed = false;
        ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
    }

    void Start()
    {

    }
}


//public TechSwitching techSwitching;

//if (techSwitching == TechSwitching.Auto)
//{
//    // distance based
//    var hand = VivePose.GetPoseEx(HandRole.RightHand).pos;
//    var torso = VivePose.GetPoseEx(DeviceRole.Hmd).pos + HmdOffset;

//    handTorsoDist = Vector3.Distance(hand, torso);

//    if (handTorsoDist > techThreshold)
//        Tech = ManipulationTech.Homer;
//    else
//        Tech = ManipulationTech.Prism;
//}
//else
//{


//private void OnDrawGizmos()
//{
//    // origin
//    Gizmos.color = Color.white;
//    Gizmos.DrawSphere(origin, gizmosR);
//    // p hand
//    Gizmos.color = Color.red;
//    Gizmos.DrawSphere(pHandRigidPose.pos, gizmosR);
//    // hand to origin
//    Gizmos.DrawLine(origin, pHandRigidPose.pos);

//    Gizmos.color = Color.magenta;
//    Gizmos.DrawSphere(cHandRigidPose.pos, gizmosR);
//    // hand to origin
//    Gizmos.DrawLine(origin, cHandRigidPose.pos);

//    // p object
//    Gizmos.color = Color.blue;
//    Gizmos.DrawSphere(pRigidPose.pos, gizmosR);
//    // object to origin
//    Gizmos.DrawLine(origin, pRigidPose.pos);

//    Gizmos.color = Color.cyan;
//    Gizmos.DrawSphere(cRigidPose.pos, gizmosR);
//    // object to origin
//    Gizmos.DrawLine(origin, cRigidPose.pos);
//}