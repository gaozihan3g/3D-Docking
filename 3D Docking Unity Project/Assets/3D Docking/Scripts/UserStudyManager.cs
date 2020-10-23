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

    const string kPath = "Assets/Output/";
    const string kXmlFileName = "data.xml";
    const string kFileNameFormat = "output_{0}_{1}.txt";
    const string kNeedInit = "Initialization needed. Enter condition # and metric #.";
    public bool practice = true;
    public bool auto = false;
    public int autoConditionCounter = 0;

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
    public int numOfTrials = 10;
    [HideInInspector]
    public int currentTrial = 0;

    [HideInInspector]
    public int numOfMetrics = 20;

    public List<string> metricNames = new List<string>();

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
        auto = false;

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
        InitOrderDict(numOfConditions);
    }

    void InitOrderDict(int noc)
    {
        orderDictionary = new Dictionary<string, int>();

        List<int> index = new List<int>();

        index.Add(0);
        index.Add(1);

        int n = noc - 1;
        int m = 2;

        while (index.Count < noc)
        {
            index.Add(n--);
            index.Add(m++);
        }

        for (int i = 0; i < noc; i++)
        {
            for (int j = 0; j < noc; j++)
            {
                orderDictionary.Add(i + "_" + j, (index[j] + i) % noc);
            }
        }
    }


    public void AutoInit()
    {
        auto = true;
        autoConditionCounter = 0;
        currentTrial = 0;

        string s = currentUser % numOfConditions + "_" + autoConditionCounter;

        int c = OrderDictionary[s];

        currentCondition = c;

        TaskSetup(currentCondition, currentTrial);
    }

    public void AutoSetNextTask()
    {
        if (!auto)
            return;

        currentTrial++;

        if (currentTrial == numOfTrials)
        {
            AudioManager.Instance.PlaySound(2);

            // get avg based on currentCondition
            GetCurrentUser().conditions[currentCondition].GetAvgData();

            currentTrial = 0;

            autoConditionCounter++;

            // check count, if all done, return
            if (autoConditionCounter >= numOfConditions)
            {
                return;
            }

            // get new condition
            string s = currentUser % numOfConditions + "_" + autoConditionCounter;

            int c = OrderDictionary[s];

            currentCondition = c;
        }

        TaskSetup(currentCondition, currentTrial);
    }


    public void TaskSetup(int condition, int trial)
    {
        practice = false;
        currentCondition = condition;
        currentTrial = trial;

        // TODO init for a new trial
        if (ConditionManager.Instance != null)
            ConditionManager.Instance.CurrentCondition = condition;

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
        sb.Append("u");
        sb.Append("\t");

        sb.Append("c");
        sb.Append("\t");

        for (int j = 0; j < numOfTrials; ++j)
        {
            string s = "t" + j + "v" + k;
            sb.Append(s);
            sb.Append("\t");
        }

        sb.Append("\n");



        for (int i = 0; i < numOfUsers; ++i)
        {
            for (int ii = 0; ii < numOfConditions; ++ii)
            {
                sb.Append(i + 1);
                sb.Append("\t");

                sb.Append(ii + 1);
                sb.Append("\t");

                for (int iii = 0; iii < numOfTrials; ++iii)
                {
                    sb.Append(userSessions[i].conditions[ii].trials[iii].data[k].ToString("F3"));
                    sb.Append("\t");
                }

                sb.Append("\n");
            }
        }

        // values


        string fileNameStr = string.Format(kFileNameFormat, k, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        File.WriteAllText(kPath + fileNameStr, sb.ToString());
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

        string fileNameStr = string.Format(kFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        File.WriteAllText(kPath + fileNameStr, sb.ToString());
        AssetDatabase.Refresh();
    }

    public void SaveXML()
    {
        var serializer = new XmlSerializer(typeof(List<UserSession>));
        using (var stream = new FileStream(kPath + kXmlFileName, FileMode.Create))
        {
            serializer.Serialize(stream, userSessions);
        }

        AssetDatabase.Refresh();
    }

    public void LoadXML()
    {
        var serializer = new XmlSerializer(typeof(List<UserSession>));
        using (var stream = File.OpenRead(kPath + kXmlFileName))
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

        auto = false;
        initialized = true;
    }

    public void Clear()
    {
        initialized = false;



        Log(kNeedInit);
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

    // user * condition * metric
    // 24 * 12 * 10

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
        public List<float> average;
        public Condition()
        { }

        public Condition(int not, int nom)
        {
            trials = new List<Trial>();
            average = new List<float>();

            for (int i = 0; i < not; ++i)
                trials.Add(new Trial(nom));

            for (int i = 0; i < nom; i++)
                average.Add(0f);
        }

        public void GetAvgData()
        {
            for (int i = 0; i < average.Count; i++)
            {
                float avg = 0f;

                for (int j = 0; j < trials.Count; j++)
                {
                    avg += trials[j].data[i];
                }

                avg /= trials.Count;
                average[i] = avg;
            }
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
