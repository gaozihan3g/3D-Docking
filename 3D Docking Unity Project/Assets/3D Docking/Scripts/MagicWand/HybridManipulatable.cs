using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

public class HybridManipulatable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SelectionMode selectionMode;
    public ManipulationType manipulationType = ManipulationType.Translation & ManipulationType.Rotation;
    [SerializeField]
    private ManipulationTech tech;

    //public ReferenceFrame refFrame;
    //public bool velocityBased = false;

    public GameObject pointer;
    // move to UI
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
    Vector3 scaledHandPos;
    Vector3 offset;
    Vector3 HmdOffset = new Vector3(0f, -0.4f, 0f);
    float gizmosR = 0.03f;

    float minSpeed = 0f;
    float maxSpeed = 0.1f;
    float minScale = 0f;
    float maxScale = 2f;
    [Tooltip("m/sec")]
    public float speed;

    float minAngSpeed = 0f;
    float maxAngSpeed = 90f;
    float minAngScale = 0f;
    float maxAngScale = 3f;
    [Tooltip("degree/sec")]
    public float angSpeed;

    //public float xSpeed;
    //public float ySpeed;
    //public float zSpeed;
    //public float minAxisSpeed;
    //public bool X;
    //public bool Y;
    //public bool Z;

    public float translationScaleFactor = 1f;
    public float rotationScaleFactor = 1f;

    public ManipulationTech Tech
    {
        get => tech;
        set
        {
            tech = value;
            UpdateCursorState();
        }
    }

    public enum SelectionMode
    {
        Magical,
        Normal,
    }

    public enum ManipulationTech
    {
        Homer,
        Prism,
    }

    //public enum ReferenceFrame
    //{
    //    User,
    //    World,
    //}


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

        UpdateState();
    }

    void OnValidate()
    {
        UpdateCursorState();
    }

    void UpdateCursorState()
    {
        Color c;

        if (Tech == ManipulationTech.Homer)
            c = Color.blue;
        else
            c = Color.green;

        if (UIManager.Instance != null)
            UIManager.Instance.SetCursorColor(c);
    }

    void UpdateState()
    {
        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Grip))
        {
            Tech = 1 - Tech;
        }
    }

    void CalculateVelocity()
    {
        // get speed

        Vector3 deltaHandPos = cHandRigidPose.pos - pHandRigidPose.pos;

        speed = deltaHandPos.magnitude / Time.deltaTime;

        //xSpeed = Mathf.Abs(deltaHandPos.x) / Time.deltaTime;
        //ySpeed = Mathf.Abs(deltaHandPos.y) / Time.deltaTime;
        //zSpeed = Mathf.Abs(deltaHandPos.z) / Time.deltaTime;

        //X = xSpeed > minAxisSpeed;
        //Y = ySpeed > minAxisSpeed;
        //Z = zSpeed > minAxisSpeed;


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


        if (Tech == ManipulationTech.Homer)
        {
            cHmdPose = VivePose.GetPoseEx(DeviceRole.Hmd);
            origin = cHmdPose.pos + HmdOffset;

            // get ratio
            float oWandR = Vector3.Distance(origin, oHandRigidPose.pos);
            float oObjR = Vector3.Distance(origin, oRigidPose.pos);
            ratio = oObjR / oWandR;

            // get offset
            scaledHandPos = oHandRigidPose.pos;
            offset = oRigidPose.pos - (origin + (oHandRigidPose.pos - origin).normalized * oObjR);

        }
        else
        {
            scaledHandPos = oHandRigidPose.pos;
            offset = oRigidPose.pos - scaledHandPos;
        }
    }

    void OnMagicUpdate()
    {
        cHandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);

        if (Tech == ManipulationTech.Prism)
            CalculateVelocity();
        else
        {
            translationScaleFactor = 1f;
            rotationScaleFactor = 1f;
        }

        if ((manipulationType & ManipulationType.Translation) == ManipulationType.Translation)
        {

            Vector3 deltaPos = cHandRigidPose.pos - pHandRigidPose.pos;
            Vector3 diffPos = deltaPos * translationScaleFactor;


            if (Tech == ManipulationTech.Homer)
            {
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

                scaledHandPos += diffPos;
                transform.position = scaledHandPos + offset;
            }


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
