using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace mygame.sdk
{
    public class AdmobMediationTool
    {
        public static void setupMediation(IDictionary<string, object> dicMediation)
        {
#if USE_ADMOB_MEDIATON
            Dictionary<string, string> dicNamedependency = new Dictionary<string, string>();
            Dictionary<string, string> dicNameNet = new Dictionary<string, string>();

            string[] arrName = {
                 "VerAdmob", "Admob", "GoogleMobileAdsDependencies"
                ,"VerApplovin", "AppLovin", "AppLovinMediationDependencies"
                ,"VerChartboost", "Chartboost", "ChartboostMediationDependencies"
                ,"VerFyber", "DTExchange", "DTExchangeMediationDependencies"
                ,"VerIronsource", "IronSource", "IronSourceMediationDependencies"
                ,"VerFacebook", "MetaAudienceNetwork", "MetaAudienceNetworkMediationDependencies"
                ,"VerMintegral", "Mintegral", "MintegralMediationDependencies"
                ,"VerMytarget", "MyTarget", "MyTargetMediationDependencies"
                ,"VerPangle", "Pangle", "PangleMediationDependencies"
                ,"VerUnity", "UnityAds", "UnityMediationDependencies"
                ,"VerVungle", "Vungle", "VungleMediationDependencies"
                ,"VerYandex", "Yandex", "YandexMediationDependencies" };

            for (int i = 0; i < arrName.Length / 3; i++)
            {
                dicNameNet.Add(arrName[3 * i], arrName[3 * i + 1]);
                dicNamedependency.Add(arrName[3 * i], arrName[3 * i + 2]);
            }
            foreach (var net in dicMediation)
            {
                if (dicNameNet.ContainsKey(net.Key))
                {
                    doChangeVerNet((string)net.Value, dicNameNet[net.Key], dicNamedependency[net.Key]);
                }
            }
#endif
        }

        public static void clear()
        {
            delFolder(Application.dataPath + $"/MyAds/Admob");
        }

        static void doChangeVerNet(string vers, string namenet, string namedependency)
        {
            string[] cfVer = vers.Split(';');
            if (cfVer.Length >= 2 && cfVer[0].CompareTo("1") == 0)
            {
                string path = Application.dataPath + $"/MyAds/Admob/Editor/{namenet}/{namedependency}.xml";
                if (!File.Exists(path))
                {
                    doCoppyMediation(namenet, namedependency);
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
            }
            else
            {
                delFolder(Application.dataPath + $"/MyAds/Admob/Editor/{namenet}");
            }
        }

        static void doCoppyMediation(string nameNet, string namedependency)
        {
            string pathSrc = Application.dataPath + $"/GamePlugin/Ads/AdmobPlugin/EditorData/{nameNet}";
            string pathdst = Application.dataPath + $"/MyAds/Admob/Editor/{nameNet}";
            SettingBuildAndroid.coppyFolder(pathSrc, pathdst, 1, ".meta", "data");
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
    }
}