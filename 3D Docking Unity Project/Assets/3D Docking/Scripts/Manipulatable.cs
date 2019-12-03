using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using System;
using UnityEngine.Events;
using HTC.UnityPlugin.Vive;

public class Manipulatable : MonoBehaviour
, IColliderEventHoverEnterHandler
, IColliderEventHoverExitHandler
, IColliderEventDragStartHandler
, IColliderEventDragFixedUpdateHandler
, IColliderEventDragEndHandler
{
    public bool translationEnabled;
    public bool rotationEnabled;
    public bool showHighlight = false;
    public bool dynamicScale = false;
    public GameObject highlight;


    const float MinS = 1f;
    const float SC = 30f;
    const float MaxS = 45f;

    const float smallRS = 0.3f;
    const float midRS = 1f;
    const float largeRS = 3f;

    [SerializeField]
    private ColliderButtonEventData.InputButton m_manipulateButton = ColliderButtonEventData.InputButton.Trigger;


    [SerializeField]
    private float positionScaleFactor = 1.0f;
    [SerializeField]
    private float rotationScaleFactor = 1.0f;

    private RigidPose pCasterPose = RigidPose.identity;
    private RigidPose pObjPose = RigidPose.identity;

    public void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        pCasterPose = GetEventPose(eventData);
        pObjPose = new RigidPose(transform);

        DockingManager.Instance.TouchStart();

    }

    public void OnColliderEventDragFixedUpdate(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        var curCasterPose = GetEventPose(eventData);

        if (translationEnabled)
        {
            // translation
            Vector3 deltaPos = curCasterPose.pos - pCasterPose.pos;

            Vector3 diffPos = deltaPos * positionScaleFactor;

            transform.position = diffPos + pObjPose.pos;
        }

        if (rotationEnabled)
        {
            // rotation
            Quaternion delta = GetDeltaQuaternion(pCasterPose.rot, curCasterPose.rot);

            // get scale factor based on rotation speed
            float angularSpeed = Quaternion.Angle(pCasterPose.rot, curCasterPose.rot) / Time.fixedDeltaTime;

            rotationScaleFactor = dynamicScale ? GetRotationFactor(angularSpeed) : rotationScaleFactor;

            Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);

            transform.rotation = diff * pObjPose.rot;
        }

        DockingManager.Instance.TouchUpdate();

        pCasterPose = curCasterPose;
        pObjPose = new RigidPose(transform);
    }

    public void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        DockingManager.Instance.TouchEnd();
    }


    private RigidPose GetEventPose(ColliderButtonEventData eventData)
    {
        var grabberTransform = eventData.eventCaster.transform;
        return new RigidPose(grabberTransform);
    }

    static Quaternion GetDeltaQuaternion(Quaternion from, Quaternion to)
    {
        Quaternion d = to * Quaternion.Inverse(from);
        return d;
    }



    public void RotationSetup(float f, bool b = false)
    {
        rotationScaleFactor = f;
        dynamicScale = b;
    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (highlight == null)
            return;

        if (showHighlight)
            highlight.SetActive(true);
    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        if (highlight == null)
            return;

        if (showHighlight)
            highlight.SetActive(false);
    }
    void Start()
    {
        if (highlight == null)
            return;

        if (showHighlight)
            highlight.SetActive(false);
    }

    float GetRotationFactor(float s)
    {
        // cd = c/d
        // rf = d/c
        // TODO tweak it

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
}
