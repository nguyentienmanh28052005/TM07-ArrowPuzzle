using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


namespace ads.myeditor
{
    public class IronYandexAdsUtil
    {
        public static void addRemoveAds(string stateAds)
        {
            try
            {
                int isAdd = -1;
#if !ENABLE_ADS_YANDEX
                isAdd = 0;
#endif
                string[] arrsss = stateAds.Split(';');
                string vernetAdap = "7.1.0.0";
                if (arrsss.Length >= 2)
                {
                    isAdd = int.Parse(arrsss[0]);
                    vernetAdap = arrsss[1];
                }
                if (isAdd != 0)
                {
                    YandexPostProcess.enableOrDisableYandexios(true);
                    if (isAdd == 1)
                    {
                        string pathIronMedia = Application.dataPath + "/IronSource/Plugins.meta";
                        string path = "";
                        if (File.Exists(pathIronMedia))
                        {
                            path = Application.dataPath + "/IronSource/Editor/YandexAds/YandexMediationDependencies.xml";
                            if (!File.Exists(path))
                            {
                                string pathSrc = Application.dataPath + "/GamePlugin/Ads/IronPlugin/EditorData/YandexAds";
                                string pathdst = Application.dataPath + "/IronSource/Editor/YandexAds";
                                SettingBuildAndroid.coppyFolder(pathSrc, pathdst, 1, ".meta", "data");
                            }
                        }
                        else
                        {
                            pathIronMedia = Application.dataPath + "/LevelPlay/Editor.meta";
                            path = Application.dataPath + "/LevelPlay/Editor/YandexAds/YandexMediationDependencies.xml";
                            if (!File.Exists(path) && File.Exists(pathIronMedia))
                            {
                                string pathSrc = Application.dataPath + "/GamePlugin/Ads/IronPlugin/EditorData/YandexAds";
                                string pathdst = Application.dataPath + "/LevelPlay/Editor/YandexAds";
                                SettingBuildAndroid.coppyFolder(pathSrc, pathdst, 1, ".meta", "data");
                            }
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
                                            if (namever.CompareTo(vernetAdap) != 0)
                                            {
                                                isw = true;
                                                alline[i] = alline[i].Replace(namever, vernetAdap);
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
                            Debug.LogError($"mysdk: IronYandexAdsUtil doAddNet err");
                        }
                    }
                    else
                    {

                        string pathIronMedia = Application.dataPath + "/IronSource/Plugins.meta";
                        if (File.Exists(pathIronMedia))
                        {
                            delFolder(Application.dataPath + "/IronSource/Editor/YandexAds");
                        }
                        else
                        {
                            pathIronMedia = Application.dataPath + "/LevelPlay/Editor.meta";
                            if (File.Exists(pathIronMedia))
                            {
                                delFolder(Application.dataPath + "/LevelPlay/Editor/YandexAds");
                            }
                        }
                    }
                }
                else
                {
                    YandexPostProcess.enableOrDisableYandexios(false);
                    string pathIronMedia = Application.dataPath + "/IronSource/Plugins.meta";
                    if (File.Exists(pathIronMedia))
                    {
                        delFolder(Application.dataPath + "/IronSource/Editor/YandexAds");
                    }
                    else
                    {
                        pathIronMedia = Application.dataPath + "/LevelPlay/Editor.meta";
                        if (File.Exists(pathIronMedia))
                        {
                            delFolder(Application.dataPath + "/LevelPlay/Editor/YandexAds");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("mysdk: IronYandexAdsUtil ex=" + ex.ToString());
            }
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
    }
}