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

        public UserSession(int noc)
        {
            id = userCount++;
            tasks = new List<Task>();

            for (int i = 0; i < noc; ++i)
                tasks.Add(new Task());
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
        public float time;
        public float accuracy;
        public int clutch;

        public Task()
        {
            time = 0f;
            accuracy = 0f;
            clutch = 0;
        }

        public Task(float t, float a, int c)
        {
            time = t;
            accuracy = a;
            clutch = c;
        }

        override public string ToString()
        {
            return string.Format("{0}\t{1}\t{2}", time, accuracy, clutch);
        }
    }


    public static UserStudyManager Instance;

    public string path = "Assets/";
    public string filename = "data.txt";

    [HideInInspector]
    public string log = "User Study Manager";

    [SerializeField]
    public List<UserSession> userSessions;

    //[HideInInspector]
    //public bool initialized = false;

    public int numOfConditions = 1;
    public int currentCondition = 0;
    public int currentUser = 0;



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
        numOfConditions = 1;
        currentCondition = 0;
        currentUser = 0;

        UserSession.Init();
        userSessions = new List<UserSession>();

        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        //initialized = true;
        Log("Initialized.");
    }

    public void NewUser()
    {
        var us = new UserSession(numOfConditions);

        userSessions.Add(us);
        Log("User Added. " + userSessions.Count);
    }

    public void SetCurrentTask(int i)
    {
        currentCondition = i;
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

    public void Log(string s)
    {
        log = s;
    }

    public void SetTaskResult(Task t)
    {
        userSessions[currentUser].tasks[currentCondition] = t;
    }

}
