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
, IColliderEventDragUpdateHandler
, IColliderEventDragEndHandler
{
    public bool translationEnabled;
    public bool rotationEnabled;
    public bool showHighlight = false;
    public bool dynamicScale = false;
    public GameObject highlight;

    [SerializeField]
    private ColliderButtonEventData.InputButton m_manipulateButton = ColliderButtonEventData.InputButton.Trigger;

    [SerializeField]
    private float positionScaleFactor = 1.0f;
    [SerializeField]
    private float rotationScaleFactor = 1.0f;

    private RigidPose pCasterPose = RigidPose.identity;
    private RigidPose pObjPose = RigidPose.identity;

    public float angularSpeed;

    public float MinS = 1f;

    /// <summary>
    /// threshold between iso and non-iso
    /// </summary>
    public float SC = 100f;

    public float smallRS = 1f;

    public float largeRS = 3f;

    public void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        pCasterPose = GetEventPose(eventData);
        pObjPose = new RigidPose(transform);

        DockingManager.Instance.TouchStart();

    }

    public void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
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
            Quaternion delta = DockingHelper.GetDeltaQuaternion(pCasterPose.rot, curCasterPose.rot);

            // get scale factor based on rotation speed
            angularSpeed = Quaternion.Angle(pCasterPose.rot, curCasterPose.rot) / Time.deltaTime;

            rotationScaleFactor = dynamicScale ? DockingHelper.Map(angularSpeed, MinS, SC, smallRS, largeRS, true) : rotationScaleFactor;

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

        print(Time.deltaTime);
        print(Time.fixedDeltaTime);

        if (highlight == null)
            return;

        if (showHighlight)
            highlight.SetActive(false);
    }
}
