using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockingManager : MonoBehaviour {

    public Transform focusedObject;
    public Transform targetObject;

    public float distance;
    public float angle;

    public float distThreshold;
    public float angleThreshold;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        UpdateDiffs();
	}


    void UpdateDiffs()
    {
        distance = Vector3.Distance(focusedObject.position, targetObject.position);
        angle = Quaternion.Angle(focusedObject.rotation, targetObject.rotation);
    }
}
