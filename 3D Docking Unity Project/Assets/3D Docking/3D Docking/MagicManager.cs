using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicManager : MonoBehaviour {


    public bool On = false;

    public Transform controller;
    public Transform obj;

    public float scale;

    Quaternion orgControllerRot;
    Quaternion orgObjRot;


    static Quaternion GetDeltaQuaternion(Quaternion from, Quaternion to)
    {
        Quaternion d = to * Quaternion.Inverse(from);
        return d;
    }

    private void OnValidate()
    {
        if (On)
            RegisterOrg();
    }


    // Use this for initialization
    void Awake ()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (On)
            UpdateManipulation();
    }

    void RegisterOrg()
    {
        orgControllerRot = controller.rotation;
        orgObjRot = obj.rotation;
    }


    void UpdateManipulation()
    {
        Quaternion delta = GetDeltaQuaternion(orgControllerRot, controller.rotation);

        // ideal: if a = b, then diff should be identity, no matter the scale

        Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, scale);

        obj.rotation = diff * orgObjRot;
    }


}
