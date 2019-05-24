using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DockingManager))]
public class DockingManagerEditor : Editor
{

    DockingManager dm;

    public override void OnInspectorGUI()
    {
        if (!dm)
            dm = (DockingManager)target;


        DrawDefaultInspector();

        if (GUILayout.Button("Initialize"))
        {
            dm.Init();
        }

        if (GUILayout.Button("Send Data"))
        {
            dm.SendData();
        }

    }
}

