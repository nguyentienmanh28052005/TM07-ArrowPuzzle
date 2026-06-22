using System;
using UnityEngine;

public class MyOpenAdsOb
{
    public int idOb;
    public string linkAds;
    public string gameId;

    public Texture2D texture;

    public override string ToString()
    {
        string re = "";
        re += ("id:" + idOb);
        re += (",link:" + linkAds);
        if (gameId != null && gameId.Length > 0)
        {
            re += (",gid:" + gameId);
        }

        return re;
    }

    public bool fromString(string data)
    {
        if (data != null && data.Length > 10)
        {
            string[] sarrdata = data.Split(new char[] { ',' });
            if (sarrdata != null && sarrdata.Length >= 6)
            {
                for (int i = 0; i < sarrdata.Length; i++)
                {
                    if (sarrdata[i].StartsWith("id:"))
                    {
                        string ss = sarrdata[i].Substring(3);
                        idOb = int.Parse(ss);
                    }
                    else if (sarrdata[i].StartsWith("link:"))
                    {
                        linkAds = sarrdata[i].Substring(5);
                    }
                    else if (sarrdata[i].StartsWith("gid:"))
                    {
                        gameId = sarrdata[i].Substring(4);
                    }
                }
                return true;
            }
        }
        return false;
    }
}

public class MoreGameOb {
    public string gameName;
    public string icon;
    public string gameId;
    public int playOnClient;
    public int isStatic;
    public string playAbleAds;
    public string linkStore;

    public override string ToString()
    {
        string re = "";
        re += ("name:" + gameName);
        re += (",icon:" + icon);
        re += (",gid:" + gameId);
        re += (",play:" + playOnClient);
        re += (",stic:" + isStatic);
        re += (",ads:" + playAbleAds);
        if (linkStore != null && linkStore.Length > 0)
        {
            re += (",link:" + linkStore);
        }

        return re;
    }

    public bool fromString(string data)
    {
        if (data != null && data.Length > 10)
        {
            string[] sarrdata = data.Split(new char[] { ',' });
            if (sarrdata != null && sarrdata.Length >= 5)
            {
                for (int i = 0; i < sarrdata.Length; i++)
                {
                    if (sarrdata[i].StartsWith("name:"))
                    {
                        gameName = sarrdata[i].Substring(5);
                    }
                    else if (sarrdata[i].StartsWith("icon:"))
                    {
                        icon = sarrdata[i].Substring(5);
                    }
                    else if (sarrdata[i].StartsWith("gid:"))
                    {
                        gameId = sarrdata[i].Substring(4);
                    }
                    else if (sarrdata[i].StartsWith("play:"))
                    {
                        string ss = sarrdata[i].Substring(5);
                        playOnClient = int.Parse(ss);
                    }
                    else if (sarrdata[i].StartsWith("stic:"))
                    {
                        string ss = sarrdata[i].Substring(5);
                        isStatic = int.Parse(ss);
                    }
                    else if (sarrdata[i].StartsWith("ads:"))
                    {
                        playAbleAds = sarrdata[i].Substring(4);
                    }
                    else if (sarrdata[i].StartsWith("link:"))
                    {
                        linkStore = sarrdata[i].Substring(5);
                    }
                }
                return true;
            }
        }
        return false;
    }
}