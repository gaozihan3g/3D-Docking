using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Text;

[Serializable]
public class LogData
{
    public List<float> time;
    public List<float> tSpeeds;
    public List<float> rSpeeds;
    public List<float> tDists;
    public List<float> rDists;
    public List<float> maniStartTimes;
    public List<float> maniEndTimes;

    public LogData()
    {
        time = new List<float>();
        tSpeeds = new List<float>();
        rSpeeds = new List<float>();
        tDists = new List<float>();
        rDists = new List<float>();
        maniStartTimes = new List<float>();
        maniEndTimes = new List<float>();
    }

    public bool HasData()
    {
        return time.Count != 0;
    }

    public void Add(float i, float t, float r, float td, float rd)
    {
        time.Add(i);
        tSpeeds.Add(t);
        rSpeeds.Add(r);
        tDists.Add(td);
        rDists.Add(rd);
    }

    public void AddManiStart(float t)
    {
        maniStartTimes.Add(t);
    }

    public void AddManiEnd(float t)
    {
        maniEndTimes.Add(t);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < time.Count; ++i)
        {
            sb.Append(time[i]);
            sb.Append("\t");
            sb.Append(tSpeeds[i]);
            sb.Append("\t");
            sb.Append(rSpeeds[i]);
            sb.Append("\t");
            sb.Append(tDists[i]);
            sb.Append("\t");
            sb.Append(rDists[i]);

            if (i < maniStartTimes.Count)
            {
                sb.Append("\t");
                sb.Append(maniStartTimes[i]);
                sb.Append("\t");
                sb.Append(maniEndTimes[i]);
            }

            sb.Append("\n");
        }

        return sb.ToString();
    }
}
