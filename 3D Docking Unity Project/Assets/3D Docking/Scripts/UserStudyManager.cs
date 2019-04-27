using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

[ExecuteInEditMode]
public class UserStudyManager : MonoBehaviour
{
    [Serializable]
    public class UserSession
    {
        static int userCount;

        public int id;
        public List<Task> tasks;

        static public void Init()
        {
            userCount = 0;
        }

        // condition, metrics
        public UserSession(int noc, int nom)
        {
            id = userCount++;
            tasks = new List<Task>();

            for (int i = 0; i < noc; ++i)
                tasks.Add(new Task(nom));
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(id + "\t");

            for (int i = 0; i < tasks.Count; ++i)
            {
                string s = tasks[i].ToString();
                sb.Append(s);
                sb.Append("\t");
            }

            return sb.ToString();
        }
    }

    [Serializable]
    public class Task
    {
        //public float time;
        //public float accuracy;
        //public int clutch;
        //public float initAngle;
        //public float angleThreshold;

        public List<float> data;

        public Task (List<float> d)
        {
            data = d;
        }

        public Task(int n)
        {
            data = new List<float>();

            for (int i = 0; i < n; ++i)
                data.Add(0f);

            //time = 0f;
            //accuracy = 0f;
            //clutch = 0;
        }

        //public Task(float t, float a, int c, float ia, float th)
        //{
        //    time = t;
        //    accuracy = a;
        //    clutch = c;
        //    initAngle = ia;
        //    angleThreshold = th;
        //}

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

        //override public string ToString()
        //{
        //    return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", time, accuracy, clutch, initAngle, angleThreshold);
        //}
    }


    public static UserStudyManager Instance;

    public string path = "Assets/";
    public string filename = "data.txt";

    const string NEEDINIT = "Initialization needed. Enter condition # and metric #.";

    [HideInInspector]
    public string log;
    [HideInInspector]
    public bool initialized;
    [HideInInspector]
    public List<UserSession> userSessions;
    [HideInInspector]
    public int numOfConditions = 2;
    [HideInInspector]
    public int currentCondition = 0;
    [HideInInspector]
    public int currentUser = 0;
    [HideInInspector]
    public int numOfMetrics;
    [HideInInspector]
    public List<string> metrics = new List<string>();



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

    public void Init()
    {
        currentCondition = 0;
        currentUser = 0;

        UserSession.Init();
        userSessions = new List<UserSession>();

        metrics = new List<string>();

        for (int i = 0; i < numOfMetrics; ++i)
	        metrics.Add("");

        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        initialized = true;
        Log("Initialized.");
    }

    public void NewUser()
    {
        var us = new UserSession(numOfConditions, metrics.Count);

        userSessions.Add(us);
        Log("User Added. " + userSessions.Count);
    }

    public void SetCurrentTask(int i)
    {
        currentCondition = i;

        // TODO init for a new trial
        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        Log("Current User: " + currentUser + " Current Condition: " + currentCondition);
    }


    public void SaveData()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < userSessions.Count; ++i)
        {
            string s = userSessions[i].ToString();
            sb.Append(s);
            sb.Append("\n");
        }

        File.AppendAllText(path + filename, sb.ToString());

        AssetDatabase.Refresh();
    }

    public void Clear()
    {
    	initialized = false;

        Log(NEEDINIT);
        // to sth else to clear
    }

    public void Log(string s)
    {
        log = s;
    }

    public void SetTaskResult(Task t)
    {
        userSessions[currentUser].tasks[currentCondition] = t;
        Debug.Log("currentUser: " + currentUser + "currentCondition " + currentCondition + " " + t.ToString());
    }

}
