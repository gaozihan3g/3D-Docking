using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MagicManager))]
public class MagicManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MagicManager mm = (MagicManager)target;

        //var v1 = mm.scale;
        //var v2 = mm.controller.rotation;
        //var v3 = Quaternion.SlerpUnclamped(Quaternion.identity, v2, v1);



        //// a * from = to

        //// a = to * ~from

        //// a
        //var v11 = mm.t1.rotation;
        //// b
        //var v22 = mm.t2.rotation;
        //// c
        //var v33 = v11 * v22;


        //var v111 = v33 * Quaternion.Inverse(v22);


        //mm.t3.rotation = v33;

        //var v11i = Quaternion.Inverse(v11);

        //var v4 = v11 * v11i;

        //EditorGUILayout.HelpBox(string.Format("org:{1:F3} | scale:{0} | target:{2:F3}", v1, v2, v3), MessageType.Info);

        //EditorGUILayout.HelpBox(string.Format("1:{0:F3} | 2:{1} | 3:{2:F3}", v11, v22, v33), MessageType.Info);

        //EditorGUILayout.HelpBox(string.Format("{0} {1} {2}", v11, v11i, v4), MessageType.Info);



        //EditorGUILayout.HelpBox(string.Format("{0} * {1} = {2}", v11, v22, v33), MessageType.Info);

        //EditorGUILayout.HelpBox(string.Format("{0} = {1} * ~ {2}", v111, v33, v22), MessageType.Info);

    }
}
