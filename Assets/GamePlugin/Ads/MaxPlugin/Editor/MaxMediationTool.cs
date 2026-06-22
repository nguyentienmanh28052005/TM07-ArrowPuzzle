using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MaxMediationTool
{
    public static void SetupMediation(IDictionary<string, object> dicMediation)
    {
#if USE_MYMAX_MEDIATON
        string[] arrMediation = {
            "VerAdmob", "Google",
            "VerBidMachine", "BidMachine",
            "VerChartboost", "Chartboost",
            "VerFyber", "Fyber",
            "VerIronsource", "IronSource",
            "VerFacebook", "Facebook",
            "VerMintegral", "Mintegral",
            "VerMytarget", "MyTarget",
            "VerPangle", "ByteDance",
            "VerUnity", "UnityAds",
            "VerVungle", "Vungle",
            "VerYandex", "Yandex"
        };
        Dictionary<string, string> dicNamedependency = new Dictionary<string, string>();
        for (int i = 0; i < arrMediation.Length / 2; i++)
        {
            dicNamedependency.Add(arrMediation[2 * i], arrMediation[2 * i + 1]);
        }
        foreach (var net in dicMediation)
        {
            if (dicNamedependency.ContainsKey(net.Key))
            {
                doChangeVerNet((string)net.Value, dicNamedependency[net.Key]);
            }
        }
#endif
    }

    public static void clear()
    {
        delFolder(Application.dataPath + $"/MyAds/Max");
    }

    static void doChangeVerNet(string vers, string namenet)
    {
        string[] cfVer = vers.Split(';');
        if (cfVer.Length >= 2 && cfVer[0].CompareTo("1") == 0)
        {
            string path = Application.dataPath + $"/MyAds/Max/Editor/{namenet}/Dependencies.xml";
            if (!File.Exists(path))
            {
                doCoppyMediation(namenet);
            }

            if (File.Exists(path))
            {
#if UNITY_ANDROID
                string key1 = "<androidPackage spec=";
                string key2 = ":";
#else
                string key1 = "<iosPod name=";
                string key2 = "version=\"";
#endif
                bool isw = false;
                int idxver = 1;
                string[] alline = File.ReadAllLines(path);
                for (int i = 0; i < alline.Length; i++)
                {
                    if (alline[i].Contains(key1))
                    {
                        string namever = "";
                        int n = alline[i].LastIndexOf(key2);
                        if (n > 0)
                        {
                            int n1 = alline[i].IndexOf("\"", n + key2.Length);
                            if (n1 > 0)
                            {
                                namever = alline[i].Substring(n + key2.Length, n1 - n - key2.Length);
                                if (namever.StartsWith("["))
                                {
                                    namever = namever.Replace("[", "");
                                }
                                if (namever.EndsWith("]"))
                                {
                                    namever = namever.Replace("]", "");
                                }
                                if (cfVer[idxver].Length > 2 && cfVer[idxver].Contains(".") && namever.CompareTo(cfVer[idxver]) != 0)
                                {
                                    isw = true;
                                    alline[i] = alline[i].Replace(namever, cfVer[idxver]);
                                }
                                idxver++;
                                if (idxver >= cfVer.Length)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (isw)
                {
                    File.WriteAllLines(path, alline);
                }
            }
            else
            {
                Debug.LogError($"mysdk: MaxMediationTool doAddNet err net={namenet} ver={vers}");
            }
        }
        else
        {
            delFolder(Application.dataPath + $"/MyAds/Max/Editor/{namenet}");
        }
    }

    static void doCoppyMediation(string nameNet)
    {
        string pathSrc = Application.dataPath + $"/GamePlugin/Ads/MaxPlugin/EditorData/{nameNet}";
        string pathdst = Application.dataPath + $"/MyAds/Max/Editor/{nameNet}";
        SettingBuildAndroid.coppyFolder(pathSrc, pathdst, 2, ".meta", "data");
    }

    static void delFolder(string path)
    {
        if (Directory.Exists(path))
        {
            string[] listfiles = Directory.GetFiles(path);
            if (listfiles != null && listfiles.Length > 0)
            {
                if (listfiles != null && listfiles.Length > 0)
                {
                    for (int i = 0; i < listfiles.Length; i++)
                    {
                        File.Delete(listfiles[i]);
                    }
                }
            }
            string[] listd = Directory.GetDirectories(path);
            if (listd != null && listd.Length > 0)
            {
                for (int i = 0; i < listd.Length; i++)
                {
                    delFolder(listd[i]);
                }
            }
            File.Delete(path + ".meta");
            Directory.Delete(path);
        }
    }

    private static string isContainKeyMediation(string line, IDictionary<string, object> dicMediation)
    {
        foreach (var item in dicMediation)
        {
            if (line.Contains(item.Key))
            {
                return item.Key;
            }
        }
        return "";
    }

    //
    // change ver adapter
    public static void SetupUnityMediation(IDictionary<string, object> dicMediation)
    {
        Dictionary<string, string> dicNameFile = new Dictionary<string, string>();
        dicNameFile.Add("ver", "AppLovin");
        dicNameFile.Add("bidMachine", "Mediation/BidMachine");
        dicNameFile.Add("bigo", "Mediation/BigoAds");
        dicNameFile.Add("pangle", "Mediation/ByteDance");
        dicNameFile.Add("chartboost", "Mediation/Chartboost");
        dicNameFile.Add("facebook", "Mediation/Facebook");
        dicNameFile.Add("fyber", "Mediation/Fyber");
        dicNameFile.Add("admob", "Mediation/Google");
        dicNameFile.Add("ggadmanager", "Mediation/GoogleAdManager");
        dicNameFile.Add("inmobi", "Mediation/InMobi");
        dicNameFile.Add("iron", "Mediation/IronSource");
        dicNameFile.Add("mintegral", "Mediation/Mintegral");
        dicNameFile.Add("moloco", "Mediation/Moloco");
        dicNameFile.Add("mytarget", "Mediation/MyTarget");
        dicNameFile.Add("pubmatic", "Mediation/PubMatic");
        dicNameFile.Add("unity", "Mediation/UnityAds");
        dicNameFile.Add("verve", "Mediation/Verve");
        dicNameFile.Add("vungle", "Mediation/Vungle");
        dicNameFile.Add("yandex", "Mediation/Yandex");
        foreach (var net in dicMediation)
        {
            if (dicNameFile.ContainsKey(net.Key))
            {
                changeVerAdapter((string)net.Value, dicNameFile[net.Key]);
            }
        }
    }

    static void changeVerAdapter(string vers, string namedependency)
    {
        string[] cfVer = vers.Split(';');
        if (cfVer.Length > 0)
        {
            string path = Application.dataPath + $"/MaxSdk/{namedependency}/Editor/Dependencies.xml";
            if (File.Exists(path))
            {
#if UNITY_ANDROID
                string key1 = "<androidPackage spec=";
                string key2 = ":";
#else
                string key1 = "<iosPod name=";
                string key2 = "version=\"";
#endif
                bool isw = false;
                int idxver = 0;
                string[] alline = File.ReadAllLines(path);
                for (int i = 0; i < alline.Length; i++)
                {
                    if (alline[i].Contains(key1))
                    {
                        string namever = "";
                        int n = alline[i].LastIndexOf(key2);
                        if (n > 0)
                        {
                            int n1 = alline[i].IndexOf("\"", n + key2.Length);
                            if (n1 > 0)
                            {
                                namever = alline[i].Substring(n + key2.Length, n1 - n - key2.Length);
                                if (namever.StartsWith("["))
                                {
                                    namever = namever.Replace("[", "");
                                }
                                if (namever.EndsWith("]"))
                                {
                                    namever = namever.Replace("]", "");
                                }
                                if (cfVer[idxver].Length > 2 && cfVer[idxver].Contains(".") && namever.CompareTo(cfVer[idxver]) != 0)
                                {
                                    isw = true;
                                    alline[i] = alline[i].Replace(namever, cfVer[idxver]);
                                }
                                idxver++;
                                if (idxver >= cfVer.Length)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (isw)
                {
                    File.WriteAllLines(path, alline);
                }
            }
        }
    }

    public static string getVerMediation(string GameName)
    {
        string passet = Application.dataPath;
        string ppro = passet.Replace("/Assets", "");
#if UNITY_IOS || UNITY_IPHONE
        string pathFileConfig = "/iOS/GameConfigiOS.json";
#else
        string pathFileConfig = "/Android/GameConfigAndroid.json";
#endif
        string pptt = ppro + $"/Mem{GameName}" + pathFileConfig;
        if (!File.Exists(pptt))
        {
            pathFileConfig = ppro + $"/Mem" + pathFileConfig;
        }
        else
        {
            pathFileConfig = pptt;
        }
        string[] allLines = File.ReadAllLines(pathFileConfig);
        string datacf = "";
        string re = "";
        foreach (var line in allLines)
        {
            datacf += line;
        }
        var dictmp = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(datacf);
        foreach (KeyValuePair<string, object> item in dictmp)
        {
            if (item.Key.Equals("max_mediation"))
            {
                IDictionary<string, object> IronMe = (IDictionary<string, object>)item.Value;
                if (IronMe.ContainsKey("mediation_ver"))
                {
                    re = IronMe["mediation_ver"].ToString();
                    if (re.StartsWith("0;"))
                    {
                        re = "";
                    }
                    else
                    {
                        re = re.Substring(2);
                    }
                }
                break;
            }
        }
        return re;
    }
}