using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using System;
using UnityEngine.Events;
using HTC.UnityPlugin.Vive;

public class Manipulatable : MonoBehaviour
, IColliderEventDragStartHandler
, IColliderEventDragUpdateHandler
, IColliderEventDragEndHandler
{

    public bool translationEnabled;
    public bool rotationEnabled;

    [SerializeField]
    private ColliderButtonEventData.InputButton m_manipulateButton = ColliderButtonEventData.InputButton.Trigger;


    [SerializeField]
    private float positionScaleFactor = 1.0f;
    [SerializeField]
    private float rotationScaleFactor = 1.0f;

    private RigidPose m_orgCasterPose = RigidPose.identity;
    private RigidPose m_orgPose = RigidPose.identity;

    public void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        m_orgCasterPose = GetEventPose(eventData);
        m_orgPose = new RigidPose(transform);

        DockingManager.Instance.TouchStart();

    }

    public void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        var curCasterPose = GetEventPose(eventData);

        if (translationEnabled)
        {
            // translation
            Vector3 deltaPos = curCasterPose.pos - m_orgCasterPose.pos;

            Vector3 diffPos = deltaPos * positionScaleFactor;

            transform.position = diffPos + m_orgPose.pos;
        }

        if (rotationEnabled)
        {
            // rotation
            Quaternion delta = GetDeltaQuaternion(m_orgCasterPose.rot, curCasterPose.rot);

            Quaternion diff = Quaternion.SlerpUnclamped(Quaternion.identity, delta, rotationScaleFactor);

            transform.rotation = diff * m_orgPose.rot;
        }

        DockingManager.Instance.TouchUpdate();
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

    void Start()
    {
    }




    //public void OnColliderEventDragStart(ColliderButtonEventData eventData)
    //{
    //    if (eventData.button != m_manipulateButton) { return; }

    //    m_pCasterPose = m_orgCasterPose = GetEventPose(eventData);
    //    m_pPose = m_orgPose = new RigidPose(transform);

    //}

    //public void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    //{
    //    if (eventData.button != m_manipulateButton) { return; }

    //    var casterPose = GetEventPose(eventData);

    //    var offsetPose = RigidPose.FromToPose(m_pCasterPose, m_pPose);

    //    var deltaPose = RigidPose.FromToPose(m_pCasterPose, casterPose);
    //    Quaternion rot = Quaternion.LerpUnclamped(Quaternion.identity, deltaPose.rot, scaleFactor);

    //    var targetPose = m_pPose * deltaPose;

    //    transform.position = targetPose.pos;
    //    transform.rotation = targetPose.rot;

    //    m_pPose = targetPose;
    //    m_pCasterPose = casterPose;
    //}
}
