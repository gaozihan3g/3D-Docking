using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UserStudyManager))]
public class UserStudyManagerEditor : Editor
{
    UserStudyManager usm;

    int gridIndex = 0;
    bool showMetricNames = false;


    void Init()
    {
        gridIndex = 0;
        showMetricNames = false;
    }

    public override void OnInspectorGUI()
    {
        if (!usm)
            usm = (UserStudyManager)target;

        if (!usm.initialized)
        {
            SetupGUI();
        }
        else
        {
            ExperimentGUI();
        }

        EditorGUILayout.HelpBox(usm.log, MessageType.Info);

        DrawDefaultInspector();
    }

    void SetupGUI()
    {
        GUILayout.BeginHorizontal();
        usm.numOfConditions = EditorGUILayout.IntField("Total Conditions", usm.numOfConditions);
        usm.numOfMetrics = EditorGUILayout.IntField("Total Metrics", usm.numOfMetrics);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Initialize"))
        {
            Init();
            usm.Init();
        }
    }

    void ExperimentGUI()
    {
        showMetricNames = EditorGUILayout.Foldout(showMetricNames, "Metric Names");

        if (showMetricNames)
        {
            for (int i = 0; i < usm.numOfMetrics; ++i)
            {
                usm.metrics[i] = EditorGUILayout.TextField("" + i, usm.metrics[i]);
            }
        }


        GUILayout.BeginHorizontal();


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



        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Data"))
        {
            usm.SaveData();
        }

        if (GUILayout.Button("Clear"))
        {
            usm.Clear();
        }

        GUILayout.EndHorizontal();
    }

    void TaskGUI(int i)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Condition #" + i))
        {
            usm.SetCurrentTask(i);
        }


        for (int j = 0; j < usm.metrics.Count; ++j)
        {
            GUILayout.Label(usm.metrics[j]);

            string s = "-";

            if (usm.GetCurrentUser() != null)
                s = usm.GetCurrentUser().tasks[i].data[j].ToString();

            EditorGUILayout.TextField(s);
        }

        GUILayout.EndHorizontal();
    }
}
