using System.Collections;
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

    public bool calibrated = false;
    [SerializeField]
    TechType tech;
    public DockingHelper.ManipulationType manipulationType;

    public bool nonIsoRotation = false;
    public bool viewpointControl = false;
    public bool lazyRelease = false;

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

    public float minSpeed = 0.01f;
    public float maxSpeed = 1f;
    float minScale0 = 0f;
    float maxScale0 = 1f;

    [Tooltip("m/sec")]
    public float speed;

    float minAngSpeed = 1f;
    float maxAngSpeed = 90f;
    float minAngScale = 0f;
    float maxAngScale = 3f;
    [Tooltip("degree/sec")]
    public float angSpeed;

    public float prismScaleFactor = 1f;
    public float homerScaleFactor = 1f;
    public float rotationScaleFactor = 1f;

    public float techThreshold = 0.3f;
    public float handTorsoDist;
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
        UIManager.Instance.SetLineColor(0);
        UIManager.Instance.CamZoom(false);
        UIManager.Instance.ShowPointer(transform, true);
        UIManager.Instance.SetCursorColor(viewpointControl ? Color.green : Color.red);
    }

    void OnValidate()
    {
        UpdateTechState();
    }

    void UpdateTechState()
    {
        lazyRelease = tech == TechType.NOVEL;
        nonIsoRotation = tech == TechType.NOVEL;
        viewpointControl = tech == TechType.NOVEL;
    }
    void InputCheck()
    {
        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Grip))
        {
            DockingManager.Instance.Finish(false);
        }

        var r = ViveInput.GetPadTouchAxisEx(HandRole.RightHand);

        if (viewpointControl)
        {
            UIManager.Instance.CamZoom(r != Vector2.zero);
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

        homerScaleFactor = DockingHelper.Map(speed, minSpeed, maxSpeed, minScale0, maxScale0, true);

        if (!nonIsoRotation)
        {
            rotationScaleFactor = 1f;
        }
        else
        {
            // get scale factor based on rotation speed
            angSpeed = Quaternion.Angle(pHandRigidPose.rot, cHandRigidPose.rot) / Time.deltaTime;
            rotationScaleFactor = DockingHelper.Map(angSpeed, minAngSpeed, maxAngSpeed, minAngScale, maxAngScale, true);

            rotationScaleFactor = speed > maxSpeed ? 0f : rotationScaleFactor;
        }
    }


    void Update()
    {
        if (!calibrated)
        {
            Calibrate();
            return;
        }

        ManipulationCheck();

        InputCheck();
    }

    void ManipulationCheck()
    {
        bool bStart;

        if (lazyRelease)
            bStart = pointed || selected;
        else
            bStart = pointed;

        if (bStart && !manipulationStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
            OnManipulationStart();

        if (manipulationStarted)
            OnManipulationUpdate();

        if (manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
            OnManipulationEnd();

        if (lazyRelease && selected && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Menu))
            OnRelease();
    }

    void OnManipulationStart()
    {
        selected = true;
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

        offset = oRigidPose.pos - (origin + (oHandRigidPose.pos - origin).normalized * oObjR);

        DockingManager.Instance.ManipulationStart();
        UIManager.Instance.ShowPointer(transform, false);
        UIManager.Instance.SetLineColor(2);
    }

    void OnManipulationUpdate()
    {
        cHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);

        CalculateScaleFactor();

        if ((manipulationType & DockingHelper.ManipulationType.Translation) == DockingHelper.ManipulationType.Translation)
        {
            // translation
            Vector3 deltaPos = cHandRigidPose.pos - pHandRigidPose.pos;

            Vector3 diffPos = deltaPos * homerScaleFactor;
            scaledHandPos += diffPos;

            Vector3 dir = (scaledHandPos - origin).normalized;
            float cWandR = Vector3.Distance(origin, scaledHandPos);
            transform.position = origin + dir * cWandR * ratio + offset;
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
        UIManager.Instance.ShowPointer(transform, !lazyRelease);
        UIManager.Instance.SetLineColor(lazyRelease ? 1 : 0);
    }
    void OnRelease()
    {
        selected = false;

        ViveInput.TriggerHapticVibration(HandRole.RightHand);
        UIManager.Instance.ShowPointer(transform, true);
        UIManager.Instance.SetLineColor(0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointed = true;
        UIManager.Instance.SetLineColor(1);
        ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointed = false;
        UIManager.Instance.SetLineColor(0);
        ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
    }

    void Start()
    {
        Init();
    }
}



//public float xSpeed;
//public float ySpeed;
//public float zSpeed;
//public float minAxisSpeed;
//public bool X;
//public bool Y;
//public bool Z;

//xSpeed = Mathf.Abs(deltaHandPos.x) / Time.deltaTime;
//ySpeed = Mathf.Abs(deltaHandPos.y) / Time.deltaTime;
//zSpeed = Mathf.Abs(deltaHandPos.z) / Time.deltaTime;

//X = xSpeed > minAxisSpeed;
//Y = ySpeed > minAxisSpeed;
//Z = zSpeed > minAxisSpeed;

//diffPos.x = X ? diffPos.x : 0f;
//diffPos.y = Y ? diffPos.y : 0f;
//diffPos.z = Z ? diffPos.z : 0f;


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

//void CheckSelection()
//{
//    if (pointed && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
//    {
//        selected = true;

//        ViveInput.TriggerHapticVibration(HandRole.RightHand);

//        //ui
//        UIManager.Instance.SetupPointer(transform, false);

//        DockingManager.Instance.ManipulationStart();
//    }

//    if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Menu))
//    {
//        selected = false;

//        ViveInput.TriggerHapticVibration(HandRole.RightHand);

//        //ui
//        UIManager.Instance.SetupPointer(transform, true);
//    }
//}


//void UpdateManipulation()
//{
//    if (!selected)
//        return;

//    // define start and end

//    if (!manipulationStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
//    {
//        OnManipulationStart();
//    }

//    if (manipulationStarted)
//        OnManipulationUpdate();

//    if (manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
//    {
//        OnManipulationEnd();
//    }
//}
