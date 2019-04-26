using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UserStudyManager))]
public class UserStudyManagerEditor : Editor
{
    UserStudyManager usm;

    int gridIndex = 0;


    void Init()
    {
        gridIndex = 0;

    }

    void TaskGUI(int i)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Condition #" + i))
        {
            usm.SetCurrentTask(i);
        }

        string s1;
        string s2;
        string s3;
        string s4;
        string s5;

        if (usm.GetCurrentUser() == null)
        {
            s1 = s2 = s3 = s4 = s5 = "-";
        }
        else
        {
            s1 = usm.GetCurrentUser().tasks[i].time.ToString();
            s2 = usm.GetCurrentUser().tasks[i].accuracy.ToString();
            s3 = usm.GetCurrentUser().tasks[i].clutch.ToString();
            s4 = usm.GetCurrentUser().tasks[i].initAngle.ToString();
            s5 = usm.GetCurrentUser().tasks[i].angleThreshold.ToString();
        }

        GUILayout.Label("Time");
        EditorGUILayout.TextField(s1);
        GUILayout.Label("Accuracy");
        EditorGUILayout.TextField(s2);
        GUILayout.Label("Clutch");
        EditorGUILayout.TextField(s3);
        GUILayout.Label("InitAngle");
        EditorGUILayout.TextField(s4);
        GUILayout.Label("Threshold");
        EditorGUILayout.TextField(s5);

        GUILayout.EndHorizontal();
    }


    public override void OnInspectorGUI()
    {

        if (!usm)
            usm = (UserStudyManager)target;


        GUILayout.BeginHorizontal();

        GUILayout.Label("Conditions"); 
        usm.numOfConditions = (int)(EditorGUILayout.Slider(usm.numOfConditions, 2f, 10f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Initialize"))
        {
            Init();
            usm.Init();
        }

        if (GUILayout.Button("New User"))
        {
            usm.NewUser();
        }

        GUILayout.EndHorizontal();

        if (usm.userSessions.Count != 0)
        {
            List<string> gridStrings = new List<string>();
            for (int i = 0; i < usm.userSessions.Count; ++i)
                gridStrings.Add("#" + i.ToString());

            gridIndex = GUILayout.SelectionGrid(gridIndex, gridStrings.ToArray(), 10);

            usm.currentUser = gridIndex;

            // each task
            for (int i = 0; i < usm.numOfConditions; ++i)
            {
                TaskGUI(i);
            }
        }

        GUILayout.BeginHorizontal();

        GUILayout.Label("Users");
        EditorGUILayout.TextField(string.Format("{0}/{1}", usm.currentUser, usm.userSessions.Count));
        GUILayout.Label("Conditions");
        EditorGUILayout.TextField(string.Format("{0}/{1}", usm.currentCondition, usm.numOfConditions));

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Save Data"))
        {
            usm.SaveData();
        }

        EditorGUILayout.HelpBox(usm.log, MessageType.Info);

        DrawDefaultInspector();
    }
}
