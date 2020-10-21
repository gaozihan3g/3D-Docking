using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

public class HomerManipulatable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SelectionMode selectionMode;
    public ManipulationType manipulationType;
    public bool velocityBased = false;

    public GameObject pointer;
    public ConnectionLine line;
    public Transform wand;

    bool selected = false;
    bool pointed = false;
    bool magicStarted = false;

    RigidPose oHandRigidPose;
    RigidPose pHandRigidPose;
    RigidPose cHandRigidPose;

    RigidPose oRigidPose;
    RigidPose pRigidPose;
    RigidPose cRigidPose;

    RigidPose cHmdPose;

    Vector3 origin;
    float ratio;
    Vector3 virtualHandPos;
    Vector3 wandPos;
    Vector3 offset;
    float gizmosR = 0.03f;
    public Vector3 HmdOffset;

    public float minSpeed;
    public float maxSpeed;
    public float minScale;
    public float maxScale;
    public float speed;

    public float minAngSpeed;
    public float maxAngSpeed;
    public float minAngScale;
    public float maxAngScale;
    public float angSpeed;

    public float translationScaleFactor = 1f;
    public float rotationScaleFactor = 1f;


    public enum SelectionMode
    {
        Magical,
        Normal,
    }


    [System.Flags]
    public enum ManipulationType
    {
        Translation = 4,
        Rotation = 2,
        //Scale = 1,
    }


    // Start is called before the first frame update
    void Start()
    {

    }


    void Update()
    {
        if (selectionMode == SelectionMode.Magical)
        {
            CheckSelection();
            Manipulation();
        }
        else
        {
            NormalSelection();
        }
    }

    void CalculateVelocity()
    {
        // get speed
        speed = Vector3.Distance(pHandRigidPose.pos, cHandRigidPose.pos) / Time.deltaTime;
        translationScaleFactor = DockingHelper.Map(speed, minSpeed, maxSpeed, minScale, maxScale, true);

        // get scale factor based on rotation speed
        angSpeed = Quaternion.Angle(pHandRigidPose.rot, cHandRigidPose.rot) / Time.deltaTime;
        rotationScaleFactor = DockingHelper.Map(angSpeed, minAngSpeed, maxAngSpeed, minAngScale, maxAngScale, true);


    }

    void NormalSelection()
    {
        // define start and end

        if (pointed && !magicStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            if (pointer != null)
                pointer.SetActive(false);

            if (line != null)
            {
                List<Transform> t = new List<Transform>();
                t.Add(wand);
                t.Add(transform);
                line.Setup(t);
            }

            OnMagicStart();
        }

        if (magicStarted)
            OnMagicUpdate();

        if (magicStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            if (pointer != null)
                pointer.SetActive(true);

            if (line != null)
            {
                List<Transform> t = new List<Transform>();
                line.Setup(t);
            }

            OnMagicEnd();
        }
    }



    void CheckSelection()
    {
        if (pointed && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
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

        if (pointer != null)
            pointer.SetActive(false);

        if (line != null)
        {
            List<Transform> t = new List<Transform>();
            t.Add(wand);
            t.Add(transform);
            line.Setup(t);
        }

    }

    void OnDeselected()
    {
        selected = false;

        if (pointer != null)
            pointer.SetActive(true);

        if (line != null)
        {
            List<Transform> t = new List<Transform>();
            line.Setup(t);
        }
    }

    void Manipulation()
    {
        if (!selected)
            return;

        // define start and end

        if (!magicStarted && ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnMagicStart();
        }

        if (magicStarted)
            OnMagicUpdate();

        if (magicStarted && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnMagicEnd();
        }
    }

    void OnMagicStart()
    {
        magicStarted = true;

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

        // get offset
        virtualHandPos = origin + (oHandRigidPose.pos - origin).normalized * oObjR;
        offset = oRigidPose.pos - virtualHandPos;

        wandPos = oHandRigidPose.pos;
    }

    void OnMagicUpdate()
    {
        cHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);

        if (velocityBased)
            CalculateVelocity();

        if ((manipulationType & ManipulationType.Translation) == ManipulationType.Translation)
        {
            Vector3 deltaPos = cHandRigidPose.pos - pHandRigidPose.pos;
            Vector3 diffPos = deltaPos * translationScaleFactor;

            wandPos += diffPos;

            Vector3 dir = (wandPos - origin).normalized;
            float cWandR = Vector3.Distance(origin, wandPos);
            transform.position = origin + dir * cWandR * ratio + offset;

            //Vector3 dir = (cHandRigidPose.pos - origin).normalized;
            //float cWandR = Vector3.Distance(origin, cHandRigidPose.pos);
            //transform.position = origin + dir * cWandR * ratio + offset;
        }

        if ((manipulationType & ManipulationType.Rotation) == ManipulationType.Rotation)
        {
            // rotation
            Quaternion delta = DockingHelper.GetDeltaQuaternion(pHandRigidPose.rot, cHandRigidPose.rot);
            Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);
            transform.rotation = diff * pRigidPose.rot;
        }

        cRigidPose = new RigidPose(transform); // for debugging

        pHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        pRigidPose = new RigidPose(transform);
    }

    void OnMagicEnd()
    {
        magicStarted = false;

        //Debug.Log("# OnMagicEnd");
    }




    public void OnPointerEnter(PointerEventData eventData)
    {
        pointed = true;
        //Debug.Log("OnPointerEnter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointed = false;
        //Debug.Log("OnPointerExit");
    }


    private void OnDrawGizmos()
    {
        // origin
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(origin, gizmosR);



        // p hand
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pHandRigidPose.pos, gizmosR);
        // hand to origin
        Gizmos.DrawLine(origin, pHandRigidPose.pos);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(cHandRigidPose.pos, gizmosR);
        // hand to origin
        Gizmos.DrawLine(origin, cHandRigidPose.pos);

        // v object
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(vPos, gizmosR);
        //Gizmos.DrawLine(pRigidPose.pos, vPos);
        //Gizmos.DrawLine(origin, vPos);

        // p object
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pRigidPose.pos, gizmosR);
        // object to origin
        Gizmos.DrawLine(origin, pRigidPose.pos);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(cRigidPose.pos, gizmosR);
        // object to origin
        Gizmos.DrawLine(origin, cRigidPose.pos);

    }

}
