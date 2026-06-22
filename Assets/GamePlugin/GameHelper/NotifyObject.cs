using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyJson;
using System;

[Serializable]
public class NotifyObject
{
    public int ver;
    public List<NotiData> data;

    public string data2String()
    {
        string re = "";

        if (data != null && data.Count > 0)
        {
            re = "[";
            for (int i = 0; i < data.Count; i++)
            {
                re += JsonUtility.ToJson(data[i]);
                if (i < (data.Count - 1))
                {
                    re += ",";
                }
            }
            re += "]";
        }

        return re;
    }

    public Dictionary<string, NotiData> getGroupNoti()
    {
        Dictionary<string, NotiData> re = new Dictionary<string, NotiData>();
        for (int i = 0; i < data.Count; i++)
        {
            string keynoti = string.Format("{0:d2}:{1:d2}", data[i].hour, data[i].minus);
            if (!re.ContainsKey(keynoti))
            {
                re.Add(keynoti, data[i]);
            }
        }
        return re;
    }
}

[Serializable]
public class NotiData
{
    public int id;
    public int hour;
    public int minus;
    public string repeat;
    public string titleNoti;
    public string msgNoti;
}
