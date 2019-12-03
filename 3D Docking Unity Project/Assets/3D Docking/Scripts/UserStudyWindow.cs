using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UserStudyWindow : EditorWindow
{

    public UserStudyManager usm;

    int gridIndex = 0;
    Vector2 scrollPos;
    bool[] showCondition;
    int exportIndex = 0;
    public string[] options;


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

        exportIndex = 0;

        List<string> s = new List<string>();
        for (int j = 0; j < usm.numOfMetrics; ++j)
            s.Add(j.ToString());
        //options
        options = s.ToArray();
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


        if (GUILayout.Button("Clear"))
        {
            usm.Clear();
        }

        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();

        exportIndex = EditorGUILayout.Popup(exportIndex, options);


        if (GUILayout.Button("Export Data " + exportIndex))
        {
            usm.ExportData(exportIndex);
        }

        if (GUILayout.Button("Export All Data"))
        {
            for (int i = 0; i < usm.numOfMetrics; ++i)
                usm.ExportData(i);
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

            gridIndex = GUILayout.SelectionGrid(gridIndex, gridStrings.ToArray(), 8);

            usm.currentUser = gridIndex;

            // auto button
            GUILayout.BeginHorizontal();

            usm.auto = EditorGUILayout.ToggleLeft("Auto", usm.auto);

            if (GUILayout.Button("Start Auto Test", GUILayout.Width(100)))
            {
                usm.AutoInit();
            }

            GUILayout.EndHorizontal();


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // each condition
            for (int j = 0; j < usm.numOfConditions; ++j)
            {
                showCondition[j] = EditorGUILayout.Foldout(showCondition[j], "Condition #" + (j + 1));

                if (showCondition[j])
                {

                    string s = gridIndex % usm.numOfConditions + "_" + j;

                    if (usm.OrderDictionary == null)
                        usm.InitOrderDict();

                    int c = usm.OrderDictionary[s];

                    EditorGUILayout.BeginHorizontal();
                    // practice
                    if (GUILayout.Button("Practice #" + (c + 1) + "\n" + ((ConditionManager.Condition)c).ToString(), GUILayout.ExpandHeight(true), GUILayout.Width(100)))
                    {
                        //change condition
                        if (ConditionManager.Instance != null)
                            ConditionManager.Instance.SetCondition(c);

                        usm.PracticeMode();
                    }

                    EditorGUILayout.BeginVertical();
                    // each task
                    for (int i = 0; i < usm.numOfTrials; ++i)
                    {
                        TaskGUI(c, i, j);
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
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

    void TaskGUI(int con, int tri, int order)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(((ConditionManager.Condition)con).ToString() + " #" + (tri + 1), GUILayout.Width(120)))
        {
            usm.TaskSetup(con, tri);
            usm.autoConditionCounter = order;
        }

        for (int i = 0; i < usm.numOfMetrics; ++i)
        {
            if (usm.GetCurrentUser() == null)
                return;

            usm.GetCurrentUser().GetTask(con, tri).data[i] = EditorGUILayout.FloatField(usm.GetCurrentUser().GetTask(con, tri).data[i], GUILayout.Width(30));
        }

        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            // clear data
            for (int i = 0; i < usm.numOfMetrics; ++i)
            {
                if (usm.GetCurrentUser() == null)
                    return;

                usm.GetCurrentUser().GetTask(con, tri).data[i] = 0f;
            }
        }

        GUILayout.EndHorizontal();
    }
}
