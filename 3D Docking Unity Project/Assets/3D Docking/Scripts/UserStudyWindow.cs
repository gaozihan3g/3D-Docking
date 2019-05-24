using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UserStudyWindow : EditorWindow
{

    public UserStudyManager usm;

    int gridIndex = 0;
    bool[] showCondition = new bool[4];


    [MenuItem("Window/User Study Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        UserStudyWindow window = (UserStudyWindow)EditorWindow.GetWindow(typeof(UserStudyWindow));
        window.Show();
    }

    private void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (usm == null)
            return;

        gridIndex = 0;
        showCondition = new bool[usm.numOfConditions];

        for (int i = 0; i < showCondition.Length; ++i)
            showCondition[i] = true;
    }

    void OnGUI()
    {
        if (usm == null)
        {
            if (GUILayout.Button("GetUserStudyManager"))
            {
                if (UserStudyManager.Instance != null)
                    usm = UserStudyManager.Instance;
            }
        }

        if (usm == null)
            return;

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
    }

    void ConsoleGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Initialize"))
        {
            usm.Init();
            Initialize();
        }

        if (GUILayout.Button("Save XML"))
        {
            usm.SaveXML();
        }

        if (GUILayout.Button("Load XML"))
        {
            usm.LoadXML();
            Initialize();
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
        usm.numOfUsers = EditorGUILayout.IntField("User #", usm.numOfUsers);
        usm.numOfConditions = EditorGUILayout.IntField("Condition #", usm.numOfConditions);
        usm.numOfTrials = EditorGUILayout.IntField("Trial #", usm.numOfTrials);
        usm.numOfMetrics = EditorGUILayout.IntField("Metric #", usm.numOfMetrics);

        GUILayout.FlexibleSpace();
    }

    void ExperimentGUI()
    {
        if (usm.userSessions.Count != 0)
        {
            // user buttons
            List<string> gridStrings = new List<string>();
            for (int i = 0; i < usm.userSessions.Count; ++i)
                gridStrings.Add("#" + (i + 1).ToString());

            gridIndex = GUILayout.SelectionGrid(gridIndex, gridStrings.ToArray(), 10);

            usm.currentUser = gridIndex;



            // each condition
            for (int j = 0; j < usm.numOfConditions; ++j)
            {
                showCondition[j] = EditorGUILayout.Foldout(showCondition[j], "Condition #" + (j + 1));

                if (showCondition[j])
                {
                    // practice
                    if (GUILayout.Button("Practice Condition #" + (j + 1)))
                    {
                        //change condition
                        if (ConditionManager.Instance != null)
                            ConditionManager.Instance.SetCondition(j);

                        usm.PracticeMode();
                    }
                    // each task
                    for (int i = 0; i < usm.numOfTrials; ++i)
                    {
                        TaskGUI(j, i);
                    }
                }
            }
        }

        GUILayout.FlexibleSpace();

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

        if (GUILayout.Button("Trial #" + (tri + 1), GUILayout.Width(100)))
        {
            //change condition
            ConditionManager.Instance.SetCondition(con);

            usm.SetCurrentTask(con, tri);
        }


        for (int i = 0; i < usm.numOfMetrics; ++i)
        {
            if (usm.GetCurrentUser() == null)
                return;

            usm.GetCurrentUser().GetTask(con, tri).data[i] = EditorGUILayout.FloatField(usm.GetCurrentUser().GetTask(con, tri).data[i], GUILayout.Width(30));
        }

        GUILayout.EndHorizontal();
    }
}
