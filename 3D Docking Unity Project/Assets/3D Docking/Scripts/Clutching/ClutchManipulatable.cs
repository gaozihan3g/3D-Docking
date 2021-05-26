using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;


public class ClutchManipulatable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public enum FlowType
    {
        DragRelease,
        //ClickRelease,
        DragRetention,
        //ClickRetention,
    }

    public FlowType flow;
    public bool calibrated = false;
    public DockingHelper.ManipulationType manipulationType;

    public bool nonIsoRotation = false;
    public bool viewpointControl = false;
    public bool lazyRelease = false;

    [SerializeField]
    bool selected = false;
    [SerializeField]
    bool pointed = false;
    [SerializeField]
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

    public GameObject highlight;

    Renderer[] m_Renderers;
    public Bounds bounds;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointed = true;

        highlight.SetActive(true);
        UIManager.Instance.SetLineColor(1);
        ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
        // 0 -> 1
        DockingManager.Instance.LogStateChange(0, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointed = false;
        highlight.SetActive(false);
        UIManager.Instance.SetLineColor(0);
        ViveInput.TriggerHapticVibrationEx(HandRole.RightHand);
        // 1 -> 0
        DockingManager.Instance.LogStateChange(1, 0);
    }

    public void Init()
    {
        selected = false;
        pointed = false;
        manipulationStarted = false;
        UIManager.Instance.SetLineColor(0);
        UIManager.Instance.ShowPointer(transform, true);
        highlight.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        m_Renderers = GetComponentsInChildren<Renderer>();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        switch (flow)
        {
            case FlowType.DragRelease:
                DragLogic();
                break;
            //case FlowType.ClickRelease:
            //    ClickLogic();
                //break;
            case FlowType.DragRetention:
                DragEscLogic();
                break;
            //case FlowType.ClickRetention:
            //    ClickEscLogic();
                //break;
        }

        //InputCheck();
    }



    void InputCheck()
    {
        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Grip))
        {
            DockingManager.Instance.Finish();
        }




        var v = ViveInput.GetPadTouchAxisEx(HandRole.RightHand);

        if (viewpointControl)
        {
            if (v != Vector2.zero)
            {
                if (pointed)
                {
                    bounds = new Bounds(transform.position, Vector3.zero);
                    foreach (var r in m_Renderers)
                    {
                        bounds.Encapsulate(r.bounds);
                    }

                    UIManager.Instance.CamZoom(true, transform, bounds.size.y);
                }
            }
            else
            {
                UIManager.Instance.CamZoom(false);
            }

        }
    }

    void DragLogic()
    {
        // select
        if (pointed && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnSelect();
            OnManipulationStart();
            // 1 -> 2
            DockingManager.Instance.LogStateChange(1, 2);
        }

        // manipulate
        if (manipulationStarted)
            OnManipulationUpdate();

        // release
        if (selected && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationEnd();
            OnRelease();
            // 2 -> 0
            DockingManager.Instance.LogStateChange(2, 0);
        }
    }

    void DragEscLogic()
    {
        // select
        if ((pointed || selected) && !manipulationStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            if (selected)
                // 3 -> 2
                DockingManager.Instance.LogStateChange(3, 2);
            else
                // 1 -> 2
                DockingManager.Instance.LogStateChange(1, 2);

            OnSelect();
            OnManipulationStart();


        }

        // manipulate
        if (manipulationStarted)
            OnManipulationUpdate();

        // release
        if (manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationEnd();
            // 2 -> 3
            DockingManager.Instance.LogStateChange(2, 3);
        }

        if (selected && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Pad))
        {
            OnRelease();
            // 3 -> 0
            DockingManager.Instance.LogStateChange(3, 0);
        }
    }

    void ClickLogic()
    {
        // select
        if (pointed && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnSelect();
            return;
        }

        if (selected && !manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationStart();
            return;
        }


        // manipulate
        if (manipulationStarted)
            OnManipulationUpdate();

        // release
        if (selected && manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationEnd();
            OnRelease();
        }
    }

    void ClickEscLogic()
    {
        // select
        if (pointed && !manipulationStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnSelect();
            return;
        }

        if (selected && !manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationStart();
            return;
        }


        // manipulate
        if (manipulationStarted)
            OnManipulationUpdate();


        // release
        if (manipulationStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnManipulationEnd();
        }

        if (selected && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Pad))
        {
            OnRelease();
        }
    }




    void OnManipulationStart()
    {
        Debug.Log("OnManipulationStart" + Time.frameCount);
        //selected = true;
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
    }

    void OnManipulationUpdate()
    {
        //Debug.Log("OnManipulationUpdate" + Time.frameCount);
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
        Debug.Log("OnManipulationEnd" + Time.frameCount);
        manipulationStarted = false;

        DockingManager.Instance.ManipulationEnd();

        DockingManager.Instance.Finish();

        //UIManager.Instance.ShowPointer(transform, !lazyRelease);
        //UIManager.Instance.SetLineColor(lazyRelease ? 1 : 0);
    }

    void OnSelect()
    {
        selected = true;
        UIManager.Instance.ShowPointer(transform, false);
        UIManager.Instance.SetLineColor(2);
        highlight.SetActive(false);
    }
    void OnRelease()
    {
        selected = false;

        ViveInput.TriggerHapticVibration(HandRole.RightHand);
        UIManager.Instance.ShowPointer(transform, true);
        UIManager.Instance.SetLineColor(0);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, bounds.size);
    }
}
