using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

public class MagicManipulatable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    //public Transform wand;
    public GameObject pointer;
    public ConnectionLine line;
    public Transform wand;

    [System.Flags]
    public enum ManipulationType
    {
        Translation = 4,
        Rotation = 2,
        Scale = 1,
    }

    public enum TranslationType
    {
        Spherical = 2,
        Cylindrical = 1,
        Cartesian = 0,
    }

    public ManipulationType manipulationType;
    public TranslationType translationType;

    bool selected = false;
    bool pointed = false;
    bool magicStarted = false;

    public float minSpeed;
    public float maxSpeed;
    public float minScale;
    public float maxScale;
    public float speed;

    public float translationScaleFactor = 1f;
    public float rotationScaleFactor = 1f;

    RigidPose pWandRigidPose;
    RigidPose pRigidPose;
    RigidPose cWandRigidPose;
    RigidPose cRigidPose;
    RigidPose cHmdPose;

    Vector3 HmdOffset;
    public Vector3 origin;
    Vector3 wandO;
    Vector3 objO;
    public float ratio;
    public float gizmosR = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CheckSelection();
        Manipulation();
    }

    void CheckSelection()
    {
        if (pointed && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            OnSelected();
            selected = true;
        }

        if (ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Menu))
        {
            OnDeselected();
            selected = false;
        }

    }

    void OnSelected()
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

    }

    void OnDeselected()
    {
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

    void OnMagicStart()
    {
        magicStarted = true;
        // record data
        //Debug.Log("# OnMagicStart");
        pWandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        pRigidPose = new RigidPose(transform);

        cHmdPose = VivePose.GetPoseEx(DeviceRole.Hmd);
        origin = cHmdPose.pos;
        //origin.y = pWandRigidPose.pos.y;
    }

    void OnMagicUpdate()
    {
        // update based on C-D
        //Debug.Log("OnMagicUpdate");
        cWandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        cRigidPose = new RigidPose(transform);

        if ((manipulationType & ManipulationType.Translation) == ManipulationType.Translation)
        {
            // get speed
            speed = Vector3.Distance(pWandRigidPose.pos, cWandRigidPose.pos) / Time.deltaTime;

            translationScaleFactor = DockingHelper.Map(speed, minSpeed, maxSpeed, minScale, maxScale, true);


            switch (translationType)
            {
                case TranslationType.Cartesian:

                    Vector3 deltaPos = cWandRigidPose.pos - pWandRigidPose.pos;

                    Vector3 diffPos = deltaPos * translationScaleFactor;

                    transform.position = diffPos + pRigidPose.pos;

                    break;
                case TranslationType.Cylindrical:

                    Vector3 cWandCyl = DockingHelper.CartesianToCylindrical(cWandRigidPose.pos, origin);
                    Vector3 pWandCyl = DockingHelper.CartesianToCylindrical(pWandRigidPose.pos, origin);

                    Vector3 deltaCyl = cWandCyl - pWandCyl;

                    Vector3 pCyl = DockingHelper.CartesianToCylindrical(pRigidPose.pos, origin);

                    deltaCyl.y *= translationScaleFactor;
                    deltaCyl.z *= translationScaleFactor;

                    Vector3 finalCyl = pCyl + deltaCyl;

                    transform.position = DockingHelper.CylindricalToCartesian(finalCyl, origin);


                    break;
                case TranslationType.Spherical:

                    wandO = new Vector3(origin.x, pWandRigidPose.pos.y, origin.z);
                    Vector3 cWandSph = DockingHelper.CartesianToShpherical(cWandRigidPose.pos, wandO);
                    Vector3 pWandSph = DockingHelper.CartesianToShpherical(pWandRigidPose.pos, wandO);

                    Vector3 deltaSph = cWandSph - pWandSph;

                    objO = new Vector3(origin.x, pRigidPose.pos.y, origin.z);
                    Vector3 pSph = DockingHelper.CartesianToShpherical(pRigidPose.pos, objO);


                    ratio = pSph.x / pWandSph.x;


                    deltaSph.x *= ratio;

                    Vector3 finalSph = pSph + deltaSph;

                    transform.position = DockingHelper.ShphericalToCartesian(finalSph, objO);

                    break;
            }
        }

        if ((manipulationType & ManipulationType.Rotation) == ManipulationType.Rotation)
        {
            // rotation
            Quaternion delta = DockingHelper.GetDeltaQuaternion(pWandRigidPose.rot, cWandRigidPose.rot);

            // get scale factor based on rotation speed
            //angularSpeed = Quaternion.Angle(pCasterPose.rot, curCasterPose.rot) / Time.deltaTime;

            //rotationScaleFactor = dynamicScale ? DockingHelper.Map(angularSpeed, MinS, SC, smallRS, largeRS, true) : rotationScaleFactor;

            Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);

            transform.rotation = diff * pRigidPose.rot;

        }

        //pWandRigidPose = VivePose.GetPoseEx(HandRole.RightHand);
        //pRigidPose = new RigidPose(transform);

    }


    void OnMagicEnd()
    {
        magicStarted = false;
        //Debug.Log("# OnMagicEnd");
    }

    private void OnDrawGizmos()
    {
        // origin
        Gizmos.color = Color.white;
        Gizmos.DrawLine(origin + Vector3.up, origin + Vector3.down);
        //Gizmos.DrawSphere(origin, gizmosR);

        // hand
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cWandRigidPose.pos, gizmosR);
        // hand to origin
        Gizmos.DrawLine(wandO, cWandRigidPose.pos);

        // object
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(cRigidPose.pos, gizmosR);
        // object to origin
        Gizmos.DrawLine(objO, cRigidPose.pos);

        // hand to object
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(cWandRigidPose.pos, cRigidPose.pos);
    }
}



//Vector3 cWandSph = DockingHelper.CartesianToShpherical(cWandRigidPose.pos, origin);
//Vector3 pWandSph = DockingHelper.CartesianToShpherical(pWandRigidPose.pos, origin);

//Vector3 deltaSph = cWandSph - pWandSph;

//Vector3 pSph = DockingHelper.CartesianToShpherical(pRigidPose.pos, origin);

//deltaSph.x *= translationScaleFactor;

//                    Vector3 finalSph = pSph + deltaSph;

//transform.position = DockingHelper.ShphericalToCartesian(finalSph, origin);
