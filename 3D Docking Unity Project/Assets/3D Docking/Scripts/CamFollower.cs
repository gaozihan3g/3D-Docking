using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollower : MonoBehaviour
{
    public Camera cam;
    public Transform target;
    public Vector3 offset;
    public bool b;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            b = !b;
            if (cam != null)
                cam.enabled = b;
        }

        transform.position = target.position + offset;

        transform.forward = Vector3.ProjectOnPlane(target.forward, Vector3.up);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}
