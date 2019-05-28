﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Xml.Serialization;

[ExecuteInEditMode]
public class UserStudyManager : MonoBehaviour
{
    public static UserStudyManager Instance;

    string path = "Assets/";
    string fileNameFormat = "output_{0}.txt";
    string xmlFilename = "data.xml";
    const string NEEDINIT = "Initialization needed. Enter condition # and metric #.";
    public bool practice = true;

    [HideInInspector]
    public int numOfUsers = 36;
    //[HideInInspector]
    public List<UserSession> userSessions;
    [HideInInspector]
    public int currentUser = 0;

    [HideInInspector]
    public int numOfConditions = 6;
    [HideInInspector]
    public int currentCondition = 0;

    [HideInInspector]
    public int numOfTrials = 5;
    [HideInInspector]
    public int currentTrial = 0;

    [HideInInspector]
    public int numOfMetrics = 20;

    [HideInInspector]
    public string log;
    [HideInInspector]
    public bool initialized;

    private Dictionary<string, int> orderDictionary;
    public Dictionary<string, int> OrderDictionary { get => orderDictionary; }

    public UserSession GetCurrentUser()
    {
        if (userSessions == null || userSessions.Count == 0)
            return null;

        return userSessions[currentUser];
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void PracticeMode()
    {
        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();
        practice = true;
    }

    public void Init()
    {
        currentCondition = 0;
        currentUser = 0;

        InitOrderDict();

        // populate users

        UserSession.Init();
        userSessions = new List<UserSession>();

        for (int i = 0; i < numOfUsers; ++i)
        {
            userSessions.Add(new UserSession(numOfConditions, numOfTrials, numOfMetrics));
        }

        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        initialized = true;
        Log("Initialized.");
    }

    public void InitOrderDict()
    {
        orderDictionary = new Dictionary<string, int>();

        orderDictionary.Add("0_0", 0);
        orderDictionary.Add("0_1", 1);
        orderDictionary.Add("0_2", 5);
        orderDictionary.Add("0_3", 2);
        orderDictionary.Add("0_4", 4);
        orderDictionary.Add("0_5", 3);

        orderDictionary.Add("1_0", 1);
        orderDictionary.Add("1_1", 2);
        orderDictionary.Add("1_2", 0);
        orderDictionary.Add("1_3", 3);
        orderDictionary.Add("1_4", 5);
        orderDictionary.Add("1_5", 4);

        orderDictionary.Add("2_0", 2);
        orderDictionary.Add("2_1", 3);
        orderDictionary.Add("2_2", 1);
        orderDictionary.Add("2_3", 4);
        orderDictionary.Add("2_4", 0);
        orderDictionary.Add("2_5", 5);

        orderDictionary.Add("3_0", 3);
        orderDictionary.Add("3_1", 4);
        orderDictionary.Add("3_2", 2);
        orderDictionary.Add("3_3", 5);
        orderDictionary.Add("3_4", 1);
        orderDictionary.Add("3_5", 0);

        orderDictionary.Add("4_0", 4);
        orderDictionary.Add("4_1", 5);
        orderDictionary.Add("4_2", 3);
        orderDictionary.Add("4_3", 0);
        orderDictionary.Add("4_4", 2);
        orderDictionary.Add("4_5", 1);

        orderDictionary.Add("5_0", 5);
        orderDictionary.Add("5_1", 0);
        orderDictionary.Add("5_2", 4);
        orderDictionary.Add("5_3", 1);
        orderDictionary.Add("5_4", 3);
        orderDictionary.Add("5_5", 2);
    }


    public void SetCurrentTask(int condition, int trial)
    {
        practice = false;
        currentCondition = condition;
        currentTrial = trial;

        // TODO init for a new trial
        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        Log("Current User: " + (currentUser + 1) +
            " Current Condition: " + (currentCondition + 1) +
            " Current Trial: " + (currentTrial + 1));
    }


    public void ExportData(int k)
    {
        StringBuilder sb = new StringBuilder();

        // metric names
        for (int i = 0; i < numOfConditions; ++i)
        {
            for (int j = 0; j < numOfTrials; ++j)
            {
                string s = "c" + i + "t" + j + "v" + k;
                sb.Append(s);
                sb.Append("\t");
            }
        }

        sb.Append("\n");

        // values
        for (int i = 0; i < userSessions.Count; ++i)
        {
            var dataStr = userSessions[i].GetDataString(k);


            sb.Append(dataStr);
            sb.Append("\n");
        }

        string fileNameStr = string.Format(fileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        File.WriteAllText(path + fileNameStr, sb.ToString());
        AssetDatabase.Refresh();
    }

    public void ExportData()
    {
        StringBuilder sb = new StringBuilder();

        // metric names
        for (int i = 0; i < numOfConditions; ++i)
        {
            for (int j = 0; j < numOfTrials; ++j)
            {
                for (int k = 0; k < numOfMetrics; ++k)
                {
                    string s = "c" + i + "t" + j + "v" + k;
                    sb.Append(s);
                    sb.Append("\t");
                }
            }
        }

        sb.Append("\n");

        for (int ii = 0; ii < userSessions.Count; ++ii)
        {
            string s = userSessions[ii].ToString();
            sb.Append(s);
            sb.Append("\n");
        }

        string fileNameStr = string.Format(fileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        File.WriteAllText(path + fileNameStr, sb.ToString());
        AssetDatabase.Refresh();
    }

    public void SaveXML()
    {
        var serializer = new XmlSerializer(typeof(List<UserSession>));
        using (var stream = new FileStream(path + xmlFilename, FileMode.Truncate))
        {
            serializer.Serialize(stream, userSessions);
        }

        AssetDatabase.Refresh();
    }

    public void LoadXML()
    {
        var serializer = new XmlSerializer(typeof(List<UserSession>));
        using (var stream = File.OpenRead(path + xmlFilename))
        {
            userSessions = (List<UserSession>)(serializer.Deserialize(stream));
        }

        // get users
        numOfUsers = userSessions.Count;

        // get conditions
        var c = userSessions[0].conditions;
        numOfConditions = c.Count;

        // get trials
        var t = c[0].trials;
        numOfTrials = t.Count;

        // get metrics
        var m = t[0].data;
        numOfMetrics = m.Count;

        initialized = true;
    }

    public void Clear()
    {
        initialized = false;



        Log(NEEDINIT);
        // to sth else to clear
        userSessions = new List<UserSession>();
    }

    public void Log(string s)
    {
        log = s;
    }

    public void SetTaskResult(Trial t)
    {
        if (practice)
            return;

        userSessions[currentUser].conditions[currentCondition].trials[currentTrial] = t;
        Debug.Log("[CurrentUser: " + (currentUser + 1) +
            "] [CurrentCondition: " + (currentCondition + 1) +
            "] [CurrentTrial: " + (currentTrial + 1) +
             "] " + t.ToString());

        // save it ?
        SaveXML();
    }


    [Serializable]
    public class UserSession
    {
        static int userCount;

        public int id;
        public List<Condition> conditions;

        static public void Init()
        {
            userCount = 0;
        }

        public UserSession()
        { }

        // condition, trial, metrics
        public UserSession(int noc, int not, int nom)
        {
            id = userCount++;

            conditions = new List<Condition>();

            for (int i = 0; i < noc; ++i)
                conditions.Add(new Condition(not, nom));
        }

        public Trial GetTask(int c, int t)
        {
            return conditions[c].trials[t];
        }

        public string GetDataString(int k)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < conditions.Count; ++i)
            {
                string s = conditions[i].GetDataString(k);
                sb.Append(s);
            }
            return sb.ToString();
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < conditions.Count; ++i)
            {
                string s = conditions[i].ToString();
                sb.Append(s);
            }

            return sb.ToString();
        }
    }

    [Serializable]
    public class Condition
    {
        public List<Trial> trials;

        public Condition()
        { }

        public Condition(int not, int nom)
        {
            trials = new List<Trial>();

            for (int i = 0; i < not; ++i)
                trials.Add(new Trial(nom));
        }

        public string GetDataString(int k)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < trials.Count; ++i)
            {
                string s = trials[i].GetDataString(k);
                sb.Append(s);
            }
            return sb.ToString();
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < trials.Count; ++i)
            {
                string s = trials[i].ToString();
                sb.Append(s);
            }
            return sb.ToString();
        }
    }


    [Serializable]
    public class Trial
    {
        public List<float> data;


        public Trial()
        { }

        public Trial(List<float> d)
        {
            data = d;
        }

        public Trial(int n)
        {
            data = new List<float>();

            for (int i = 0; i < n; ++i)
                data.Add(0f);
        }

        public string GetDataString(int k)
        {
            return data[k].ToString() + "\t";
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < data.Count; ++i)
            {
                string s = data[i].ToString();
                sb.Append(s);
                sb.Append("\t");
            }
            return sb.ToString();
        }
    }

}
