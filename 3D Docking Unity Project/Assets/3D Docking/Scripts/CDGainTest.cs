using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDGainTest : MonoBehaviour
{
    public Transform controller;

    public Transform target;

    public float angularSpeed;


    const float MinS = 5f;
    const float SC = 30f;
    const float MaxS = 90f;

    const float smallRS = 0.3f;
    const float midRS = 1f;
    const float largeRS = 3f;

    public float rotationScaleFactor;

    Quaternion pControllerRot;

    Quaternion pTargetRot;

    float GetRotationFactor(float s)
    {
        // cd = c/d
        // rf = d/c

        float r = 0f;

        if (s < MinS)
            r = 0f;
        else if (s < SC)
            r = smallRS;
        else if (s < MaxS)
            r = midRS;
        else
            r = largeRS;

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
        pTargetRot = target.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        angularSpeed = Quaternion.Angle(controller.rotation, pControllerRot) / Time.deltaTime;

        rotationScaleFactor = GetRotationFactor(angularSpeed);


        Quaternion delta = GetDeltaQuaternion(pControllerRot, controller.rotation);

        Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);

        target.rotation = diff * pTargetRot;

        pControllerRot = controller.rotation;
        pTargetRot = target.rotation;
    }

    static Quaternion GetDeltaQuaternion(Quaternion from, Quaternion to)
    {
        Quaternion d = to * Quaternion.Inverse(from);
        return d;
    }
}
