using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.IO;
#endif

public class IrMediationTool
{
    public static void SetupMediation(IDictionary<string, object> dicMediation)
    {
        Dictionary<string, string> dicNameFile = new Dictionary<string, string>();
        dicNameFile.Add("ver", "IronSourceSDKDependencies");
        dicNameFile.Add("admob", "ISAdMobAdapterDependencies");
        dicNameFile.Add("max", "ISAppLovinAdapterDependencies");
        dicNameFile.Add("bidMachine", "ISBidMachineAdapterDependencies");
        dicNameFile.Add("bigo", "ISBigoAdapterDependencies");
        dicNameFile.Add("chartboost", "ISChartboostAdapterDependencies");
        dicNameFile.Add("facebook", "ISFacebookAdapterDependencies");
        dicNameFile.Add("fyber", "ISFyberAdapterDependencies");
        dicNameFile.Add("inmobi", "ISInMobiAdapterDependencies");
        dicNameFile.Add("mintegral", "ISMintegralAdapterDependencies");
        dicNameFile.Add("moloco", "ISMolocoAdapterDependencies");
        dicNameFile.Add("mytarget", "ISMyTargetAdapterDependencies");
        dicNameFile.Add("pangle", "ISPangleAdapterDependencies");
        dicNameFile.Add("unity", "ISUnityAdsAdapterDependencies");
        dicNameFile.Add("vungle", "ISVungleAdapterDependencies");
        dicNameFile.Add("yandex", "ISYandexAdapterDependencies");
        foreach (var net in dicMediation)
        {
            if (dicNameFile.ContainsKey(net.Key))
            {
                doChangeVerNet((string)net.Value, dicNameFile[net.Key]);
            }
        }
    }

    static void doChangeVerNet(string vers, string namedependency)
    {
        string[] cfVer = vers.Split(';');
        if (cfVer.Length > 0)
        {
            string path = Application.dataPath + $"/LevelPlay/Editor/{namedependency}.xml";
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
                    if (alline[i].Contains("<unityversion>"))
                    {
                        int idx = alline[i].IndexOf(">");
                        int n = alline[i].IndexOf("</");
                        n = n - idx - 1;
                        if (n > 0)
                        {
                            string currver = alline[i].Substring(idx + 1, n);
                            isw = true;
                            alline[i] = alline[i].Replace(currver, cfVer[idxver]);
                        }
                        idxver++;
                        if (idxver >= cfVer.Length)
                        {
                            break;
                        }
                    }
                    else if (alline[i].Contains(key1))
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
            if (item.Key.Equals("iron_mediation"))
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