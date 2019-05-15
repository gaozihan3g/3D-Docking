using System;
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

    public string path = "Assets/";
    public string filename = "output.txt";
    public string xmlFilename = "data.xml";
    const string NEEDINIT = "Initialization needed. Enter condition # and metric #.";

    [HideInInspector]
    public int numOfUsers = 40;
    //[HideInInspector]
    public List<UserSession> userSessions;
    [HideInInspector]
    public int currentUser = 0;

    [HideInInspector]
    public int numOfConditions = 2;
    [HideInInspector]
    public int currentCondition = 0;

    [HideInInspector]
    public int numOfTrials = 5;
    [HideInInspector]
    public int currentTrial = 0;

    [HideInInspector]
    public int numOfMetrics;
    //[HideInInspector]
    //public List<string> metrics = new List<string>();


    //TODO
    public string dataString;

    [HideInInspector]
    public string log;
    [HideInInspector]
    public bool initialized;



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

        // populate users

        UserSession.Init();
        userSessions = new List<UserSession>();

        for (int i = 0; i < numOfUsers; ++i) 
        {
            userSessions.Add(new UserSession(numOfConditions, numOfTrials, numOfMetrics));
        }

        //metrics = new List<string>();

        //for (int i = 0; i < numOfMetrics; ++i)
	        //metrics.Add("metric" + i);

        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        initialized = true;
        Log("Initialized.");
    }


    public void SetCurrentTask(int condition, int trial)
    {
        currentCondition = condition;
        currentTrial = trial;

        // TODO init for a new trial
        if (DockingManager.Instance != null)
            DockingManager.Instance.Init();

        Log("Current User: " + (currentUser + 1) +
            " Current Condition: " + (currentCondition + 1) + 
            " Current Trial: " + (currentTrial + 1));
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

        File.WriteAllText(path + filename, sb.ToString());
        AssetDatabase.Refresh();
    }

    public void SaveXML()
    {
        var serializer = new XmlSerializer(typeof(List<UserSession>));
        using (var stream = File.OpenWrite(path + xmlFilename))
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
        userSessions[currentUser].conditions[currentCondition].trials[currentTrial] = t;
        Debug.Log("currentUser: " + currentUser + 
            "currentCondition " + currentCondition +
            "currentTrial " + currentTrial +
             " " + t.ToString());
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

        public Task GetTask(int c, int t)
        {
            return conditions[c].trials[t];
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
        public List<Task> trials;

        public Condition()
        { }

        public Condition(int not, int nom)
        {
            trials = new List<Task>();

            for (int i = 0; i < not; ++i)
                trials.Add(new Task(nom));
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
    public class Task
    {
        public List<float> data;


        public Task()
        { }

        public Task(List<float> d)
        {
            data = d;
        }

        public Task(int n)
        {
            data = new List<float>();

            for (int i = 0; i < n; ++i)
                data.Add(0f);
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
