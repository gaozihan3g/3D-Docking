using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDGainTest : MonoBehaviour
{
    public Transform controller;

    public Transform target;

    public float angularSpeed;

    public float minSpeed = 1f;

    public float rotationScaleFactor;

    public float ratio = 2f;

    public Quaternion pControllerRot;

    public Quaternion oTargetRot;

    float GetRotationFactor(float s)
    {
        // cd = c/d
        // rf = d/c

        float r = 0f;

        if (s < minSpeed)
            r = 1f;
        else
            r = ratio * s;

        return r;
    }

    private void OnValidate()
    {
        rotationScaleFactor = GetRotationFactor(angularSpeed);
    }

    // Start is called before the first frame update
    void Start()
    {
        pControllerRot = controller.rotation;
        oTargetRot = target.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        angularSpeed = Quaternion.Angle(controller.rotation, pControllerRot);

        rotationScaleFactor = GetRotationFactor(angularSpeed);


        Quaternion delta = GetDeltaQuaternion(pControllerRot, controller.rotation);

        Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);

        target.rotation = diff * oTargetRot;

        pControllerRot = controller.rotation;
    }

    static Quaternion GetDeltaQuaternion(Quaternion from, Quaternion to)
    {
        Quaternion d = to * Quaternion.Inverse(from);
        return d;
    }
}
