using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraConfig : MonoBehaviour
{
    public Camera cam;
    public StereoTargetEyeMask type;

    // Start is called before the first frame update
    void Start()
    {
        cam.stereoTargetEye = type;
    }

    private void OnValidate()
    {
        cam.stereoTargetEye = type;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
