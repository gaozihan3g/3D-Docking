using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Xml.Serialization;
using System.Collections;

public class UserStudyManager : MonoBehaviour
{
    public static UserStudyManager Instance;

    const string kPath = "Assets/Output/";
    public string kXmlFileName = "data.xml";
    const string kFileNameFormat = "output_{0}_{1}.txt";
    const string kNeedInit = "Initialization needed. Enter condition # and metric #.";
    public bool practice = true;
    public bool auto = false;
    public int autoConditionCounter = 0;

    public int numOfUsers = 36;
    public int numOfConditions = 6;
    public int numOfTrials = 5;

    public List<UserSession> userSessions;
    public List<string> metricNames = new List<string>();

    [HideInInspector]
    public int currentUser = 0;
    [HideInInspector]
    public int currentCondition = 0;
    [HideInInspector]
    public int currentTrial = 0;
    [HideInInspector]
    public string log;
    [HideInInspector]
    public bool initialized;

    public float taskInterval = 1f;

    private Dictionary<string, int> orderDictionary;
    /// <summary>
    /// min: 1 !!!
    /// </summary>
    /// <value>The order dictionary.</value>
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
        practice = true;
    }

    public void Init()
    {
        currentCondition = 0;
        currentUser = 0;
        auto = false;

        // get num of condition based on condition manager
        GetNumOfCondition();

        InitOrderDict();

        // populate users
        userSessions = new List<UserSession>();

        for (int i = 0; i < numOfUsers; ++i)
        {
            userSessions.Add(new UserSession(numOfConditions, numOfTrials, i));
        }

        initialized = true;
        Log("Initialized.");
    }

    protected void GetNumOfCondition()
    {
        numOfConditions = ConditionManager.Instance.NumOfConditions;
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

        if (noc != 2)
        {
            int n = noc - 1;
            int m = 2;

            while (index.Count < noc)
            {
                index.Add(n--);
                index.Add(m++);
            }
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
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(2);

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

        StartCoroutine(WaitAndDo(taskInterval, () =>
        {
            TaskSetup(currentCondition, currentTrial);
        }));

    }

    IEnumerator WaitAndDo(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }

    public void TaskSetup(int condition, int trial)
    {
        practice = false;
        currentCondition = condition;
        currentTrial = trial;

        if (ConditionManager.Instance != null)
            ConditionManager.Instance.CurrentCondition = condition;

        if (DockingManager.Instance != null)
            DockingManager.Instance.Init(trial);


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

        string fileNameStr = string.Format(kFileNameFormat, k, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
        File.WriteAllText(kPath + fileNameStr, sb.ToString());
        AssetDatabase.Refresh();
    }

    public void ExportData()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("u");
        sb.Append("\t");
        sb.Append("c");
        sb.Append("\t");
        sb.Append("t");
        sb.Append("\n");


        for (int ii = 0; ii < userSessions.Count; ++ii)
        {
            string s;
            s = userSessions[ii].ToString();
            sb.Append(s);
        }

        string fileNameStr = string.Format(kFileNameFormat, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), "all");
        File.WriteAllText(kPath + fileNameStr, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("Data Exported.");
    }

    public void SaveXML()
    {
        var serializer = new XmlSerializer(typeof(List<UserSession>));
        using (var stream = new FileStream(kPath + kXmlFileName, FileMode.Create))
        {
            serializer.Serialize(stream, userSessions);
        }

        AssetDatabase.Refresh();
        Debug.Log("XML Saved.");
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

        // get conditions TODO
        var c = userSessions[0].conditions;
        numOfConditions = c.Count;

        // get trials TODO
        var t = c[0].trials;
        numOfTrials = t.Count;

        auto = false;
        initialized = true;
        Debug.Log("XML Loaded.");
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

        // add meta info
        t.u = currentUser;
        t.c = currentCondition;
        t.t = currentTrial;

        userSessions[currentUser].conditions[currentCondition].trials[currentTrial] = t;
        Debug.Log("[CurrentUser: " + (currentUser + 1) +
            "] [CurrentCondition: " + (currentCondition + 1) +
            "] [CurrentTrial: " + (currentTrial + 1) +
             "] " + t.ToString());

        // save it ?
        SaveXML();

        AutoSetNextTask();
    }

    [Serializable]
    public class UserSession
    {
        public int u;

        public List<Condition> conditions;

        public UserSession()
        { }

        // condition, trial, metrics
        public UserSession(int noc, int not, int iu)
        {
            conditions = new List<Condition>();

            for (int i = 0; i < noc; ++i)
                conditions.Add(new Condition(not, iu, i));
        }

        public Trial GetTask(int c, int t)
        {
            //print("c: " + c + " t: " + t);
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
        public int u;
        public int c;

        public List<Trial> trials;
        public List<float> average;

        public Condition()
        { }

        public Condition(int not, int iu, int ic)
        {
            u = iu;
            c = ic;

            trials = new List<Trial>();
            average = new List<float>();

            for (int i = 0; i < not; ++i)
                trials.Add(new Trial(iu, ic, i));

        }

        public void GetAvgData()
        {
            average = new List<float>();

            // get longest trial size
            int maxSize = 0;
            for (int j = 0; j < trials.Count; j++)
            {
                if (trials[j].data.Count > maxSize)
                    maxSize = trials[j].data.Count;
            }

            for (int i = 0; i < maxSize; i++)
            {
                float avg = 0f;
                int count = 0;

                for (int j = 0; j < trials.Count; j++)
                {
                    if (trials[j].data.Count > i)
                    {
                        avg += trials[j].data[i];
                        count++;
                    }
                }

                avg /= count;
                average.Add(avg);
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
        public int u;
        public int c;
        public int t;

        public List<float> data;

        public Trial()
        { }

        public Trial(int iu, int ic, int it)
        {
            u = iu;
            c = ic;
            t = it;

            data = new List<float>();
        }

        public Trial(List<float> d)
        {
            data = d;
        }

        public string GetDataString(int k)
        {
            return data[k].ToString() + "\t";
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(u + 1);
            sb.Append("\t");
            sb.Append(c + 1);
            sb.Append("\t");
            sb.Append(t + 1);
            sb.Append("\t");

            for (int i = 0; i < data.Count; ++i)
            {
                string s = data[i].ToString();
                sb.Append(s);
                sb.Append("\t");
            }

            sb.Append("\n");
            return sb.ToString();
        }
    }
}