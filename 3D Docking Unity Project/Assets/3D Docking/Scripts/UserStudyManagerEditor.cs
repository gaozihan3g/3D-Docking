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

        if (GUILayout.Button("Task " + i))
        {
            usm.SetCurrentTask(i);
        }

        string s1;
        string s2;
        string s3;


        if (usm.GetCurrentUser() == null)
        {
            s1 = s2 = s3 = "-";
        }
        else
        {
            s1 = usm.GetCurrentUser().tasks[i].time.ToString();
            s2 = usm.GetCurrentUser().tasks[i].accuracy.ToString();
            s3 = usm.GetCurrentUser().tasks[i].clutch.ToString();
        }

        GUILayout.Label("Time");
        GUILayout.TextField(s1);
        GUILayout.Label("Accuracy");
        GUILayout.TextField(s2);
        GUILayout.Label("Clutch");
        GUILayout.TextField(s3);

        GUILayout.EndHorizontal();
    }


    public override void OnInspectorGUI()
    {

        if (!usm)
            usm = (UserStudyManager)target;


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

        GUILayout.BeginHorizontal();

        GUILayout.Label("Conditions");
        usm.numOfConditions = (int)(GUILayout.HorizontalSlider(usm.numOfConditions, 1f, 10f));
        GUILayout.Label(usm.numOfConditions.ToString());

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

        if (GUILayout.Button("Save Data"))
        {
            usm.SaveData();
        }


        EditorGUILayout.HelpBox(string.Format("Total User:{0} Current User:{1} Total Condition:{2} Current Condition:{3}",
            usm.userSessions.Count, usm.currentUser, usm.numOfConditions, usm.currentCondition), MessageType.Info);

        EditorGUILayout.HelpBox(usm.log, MessageType.Info);

        DrawDefaultInspector();
    }
}
