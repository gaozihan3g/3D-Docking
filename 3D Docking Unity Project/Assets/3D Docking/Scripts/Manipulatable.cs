﻿using System.Collections;
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

    [SerializeField]
    private ColliderButtonEventData.InputButton m_manipulateButton = ColliderButtonEventData.InputButton.Trigger;


    [SerializeField]
    private float scaleFactor = 1.0f;

    private RigidPose m_orgCasterPose = RigidPose.identity;
    private RigidPose m_orgPose = RigidPose.identity;

    private RigidPose m_pCasterPose = RigidPose.identity;
    private RigidPose m_pPose = RigidPose.identity;

    public void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        m_pCasterPose = m_orgCasterPose = GetEventPose(eventData);
        m_pPose = m_orgPose = new RigidPose(transform);

    }

    public void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_manipulateButton) { return; }

        var casterPose = GetEventPose(eventData);

        var offsetPose = RigidPose.FromToPose(m_pCasterPose, m_pPose);

        var deltaPose = RigidPose.FromToPose(m_pCasterPose, casterPose);
        Quaternion rot = Quaternion.LerpUnclamped(Quaternion.identity, deltaPose.rot, scaleFactor);

        var targetPose = m_pPose * deltaPose;

        transform.position = targetPose.pos;
        transform.rotation = targetPose.rot;

        m_pPose = targetPose;
        m_pCasterPose = casterPose;
    }


    public void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private RigidPose GetEventPose(ColliderButtonEventData eventData)
    {
        var grabberTransform = eventData.eventCaster.transform;
        return new RigidPose(grabberTransform);
    }
}
