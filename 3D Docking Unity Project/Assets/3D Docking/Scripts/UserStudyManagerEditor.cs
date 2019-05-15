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
    bool[] showCondition = new bool[4];


    void Init()
    {
        gridIndex = 0;
        showMetricNames = false;
        showCondition = new bool[usm.numOfConditions];
    }

    public override void OnInspectorGUI()
    {
        if (!usm)
            usm = (UserStudyManager)target;

        ConsoleGUI();

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

    void ConsoleGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Initialize"))
        {
            usm.Init();
            Init();
        }

        if (GUILayout.Button("Save XML"))
        {
            usm.SaveXML();
        }

        if (GUILayout.Button("Load XML"))
        {
            usm.LoadXML();
        }

        if (GUILayout.Button("Export Data"))
        {
            usm.ExportData();
        }

        if (GUILayout.Button("Clear"))
        {
            usm.Clear();
        }

        GUILayout.EndHorizontal();
    }

    void SetupGUI()
    {
        GUILayout.BeginHorizontal();
        usm.numOfUsers = EditorGUILayout.IntField("User #", usm.numOfUsers);
        usm.numOfConditions = EditorGUILayout.IntField("Condition #", usm.numOfConditions);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        usm.numOfTrials = EditorGUILayout.IntField("Trial #", usm.numOfTrials);
        usm.numOfMetrics = EditorGUILayout.IntField("Metric #", usm.numOfMetrics);
        GUILayout.EndHorizontal();
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


        if (usm.userSessions.Count != 0)
        {
            // user buttons
            List<string> gridStrings = new List<string>();
            for (int i = 0; i < usm.userSessions.Count; ++i)
                gridStrings.Add("User #" + (i + 1).ToString());

            gridIndex = GUILayout.SelectionGrid(gridIndex, gridStrings.ToArray(), 10);

            usm.currentUser = gridIndex;



            // each condition
            for (int j = 0; j < usm.numOfConditions; ++j)
            {
                showCondition[j] = EditorGUILayout.Foldout(showCondition[j], "Condition #" + (j + 1));

                if (showCondition[j])
                {
                    // each task
                    for (int i = 0; i < usm.numOfTrials; ++i)
                    {
                        TaskGUI(j, i);
                    }
                }
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("User");
        EditorGUILayout.TextField(string.Format("{0}/{1}", usm.currentUser + 1, usm.userSessions.Count));
        GUILayout.Label("Condition");
        EditorGUILayout.TextField(string.Format("{0}/{1}", usm.currentCondition + 1, usm.numOfConditions));
        GUILayout.Label("Trial");
        EditorGUILayout.TextField(string.Format("{0}/{1}", usm.currentTrial + 1, usm.numOfTrials));
        GUILayout.EndHorizontal();
    }

    void TaskGUI(int con, int tri)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Trial #" + (tri + 1)))
        {
            usm.SetCurrentTask(con, tri);
        }


        for (int i = 0; i < usm.metrics.Count; ++i)
        {
            if (usm.GetCurrentUser() == null)
                return;

            GUILayout.Label(usm.metrics[i].ToString());
            EditorGUILayout.TextField(usm.GetCurrentUser().GetTask(con, tri).data[i].ToString());
        }

        GUILayout.EndHorizontal();
    }
}
