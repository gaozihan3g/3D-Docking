using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UserStudyWindow : EditorWindow
{

    public UserStudyManager usm;
    public ConditionManager cm;

    int gridIndex = 0;
    Vector2 scrollPos;
    bool[] showCondition;
    public string[] options;

    int userPerRow = 4;
    int dataWidth = 30;
    const int kBtnWidth = 40;
    const int kSmallBtnWidth = 20;
    int activeUserId = -1;


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

        activeUserId = -1;

        gridIndex = 0;
        showCondition = new bool[usm.numOfConditions];

        for (int i = 0; i < showCondition.Length; ++i)
            showCondition[i] = true;
    }

    void OnGUI()
    {
        GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal();
        usm = (UserStudyManager)EditorGUILayout.ObjectField("USM", usm, typeof(UserStudyManager), true);
        cm = (ConditionManager)EditorGUILayout.ObjectField("CM", cm, typeof(ConditionManager), true);
        GUILayout.EndHorizontal();

        if (usm == null || cm == null)
        {
            if (GUILayout.Button("Get Managers"))
            {
                if (UserStudyManager.Instance != null)
                {
                    usm = UserStudyManager.Instance;
                    usm.Clear();
                }


                if (ConditionManager.Instance != null)
                    cm = ConditionManager.Instance;
            }
        }
        else
        {
            if ((GUILayout.Button("Remove Managers")))
            {
                usm = null;
                cm = null;
            }
        }



        if (usm == null || cm == null)
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

        //exportIndex = EditorGUILayout.Popup(exportIndex, options);


        //if (GUILayout.Button("Export Data " + exportIndex))
        //{
        //    usm.ExportData(exportIndex);
        //}

        if (GUILayout.Button("Export All Data"))
        {
            usm.ExportData();
            //for (int i = 0; i < usm.numOfMetrics; ++i)
            //usm.ExportData(i);
        }

        if (GUILayout.Button("Export Vis Data"))
        {
            //usm.ExportVisData();
            //for (int i = 0; i < usm.numOfMetrics; ++i)
            //usm.ExportData(i);
        }

        GUILayout.EndHorizontal();
    }

    void SetupGUI()
    {
        usm.numOfUsers = EditorGUILayout.IntField("User #", usm.numOfUsers);
        //usm.numOfConditions = EditorGUILayout.IntField("Condition #", usm.numOfConditions);
        usm.numOfTrials = EditorGUILayout.IntField("Trial #", usm.numOfTrials);
        //usm.numOfMetrics = EditorGUILayout.IntField("Metric #", usm.numOfMetrics);

        GUILayout.FlexibleSpace();
    }

    void ExperimentGUI()
    {
        if (usm.userSessions.Count != 0)
        {
            // auto button
            GUILayout.BeginHorizontal();

            userPerRow = EditorGUILayout.IntSlider("User Per Row", userPerRow, 1, usm.userSessions.Count);

            dataWidth = EditorGUILayout.IntSlider("Data Width", dataWidth, 1, 200);

            GUILayout.EndHorizontal();


            // user buttons
            List<string> gridStrings = new List<string>();
            for (int i = 0; i < usm.userSessions.Count; ++i)
                gridStrings.Add("#" + (i + 1).ToString());

            gridIndex = GUILayout.SelectionGrid(gridIndex, gridStrings.ToArray(), userPerRow);

            usm.currentUser = gridIndex;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Auto", GUILayout.Width(kBtnWidth)))
            {
                activeUserId = usm.currentUser;
                usm.AutoInit();
            }

            usm.auto = EditorGUILayout.ToggleLeft("Auto", usm.auto);


            if (GUILayout.Button("Show", GUILayout.Width(kBtnWidth)))
            {
                for (int i = 0; i < showCondition.Length; ++i)
                    showCondition[i] = true;
            }

            if (GUILayout.Button("Hide", GUILayout.Width(kBtnWidth)))
            {
                for (int i = 0; i < showCondition.Length; ++i)
                    showCondition[i] = false;
            }
            GUILayout.EndHorizontal();



            GUILayout.BeginHorizontal();

            GUILayout.Label("", GUILayout.Width(kBtnWidth));

            GUILayout.Label("", GUILayout.Width(kSmallBtnWidth));

            // metric names
            for (int i = 0; i < usm.metricNames.Count; ++i)
                GUILayout.Label(usm.metricNames[i], GUILayout.Width(dataWidth));

            GUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // each condition
            for (int j = 0; j < usm.numOfConditions; ++j)
            {
                string s = gridIndex % usm.numOfConditions + "_" + j;

                if (usm.OrderDictionary == null)
                    usm.InitOrderDict();

                int c = usm.OrderDictionary[s];

                showCondition[j] = EditorGUILayout.Foldout(showCondition[j], "#" + (j + 1) + " " + cm.GetConditionName(c) + " [" + (c + 1) + "]");

                if (showCondition[j])
                {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("P", GUILayout.Width(kBtnWidth)))
                    {
                        cm.CurrentCondition = c;

                        usm.PracticeMode();
                    }

                    GUILayout.EndHorizontal();

                    if (showCondition[j])
                    {
                        EditorGUILayout.BeginVertical();

                        // each task
                        for (int i = 0; i < usm.numOfTrials; ++i)
                        {
                            TaskGUI(c, i, j);
                        }

                        EditorGUILayout.EndVertical();
                    }

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("AVG", GUILayout.Width(kBtnWidth)))
                    {
                        // calculate mean based on:
                        //user:
                        //condition: c
                        usm.GetCurrentUser().conditions[c].GetAvgData();
                    }

                    GUILayout.Label("", GUILayout.Width(kSmallBtnWidth));

                    var avg = usm.GetCurrentUser().conditions[c].average;

                    for (int i = 0; i < avg.Count; ++i)
                        GUILayout.Label(avg[i].ToString("F2"), GUILayout.Width(dataWidth));

                    GUILayout.EndHorizontal();


                }
            }

            EditorGUILayout.EndScrollView();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(""))
        {
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

    void TaskGUI(int con, int tri, int order)
    {
        var trial = usm.GetCurrentUser().GetTask(con, tri);

        if (usm.currentUser == activeUserId && usm.currentTrial == tri && usm.currentCondition == con)
            GUI.backgroundColor = Color.HSVToRGB(0.25f, 0.5f, 1f);
        else
            GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("#" + (tri + 1), GUILayout.Width(kBtnWidth)))
        {
            usm.TaskSetup(con, tri);
            activeUserId = usm.currentUser;
            usm.autoConditionCounter = order;
        }

        GUI.backgroundColor = Color.HSVToRGB(0f, 0.5f, 1f);

        if (GUILayout.Button("X", GUILayout.Width(kSmallBtnWidth)))
        {
            trial.data = new List<float>();
        }

        GUI.backgroundColor = Color.white;

        for (int i = 0; i < trial.data.Count; ++i)
        {
            trial.data[i] = EditorGUILayout.FloatField(trial.data[i], GUILayout.Width(dataWidth));
        }

        GUILayout.EndHorizontal();
    }
}