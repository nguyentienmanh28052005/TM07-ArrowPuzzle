//#define Test_lluf2
using System;
using System.Collections;
using System.Collections.Generic;
using MyJson;
using UnityEngine;
using UnityEngine.Analytics;

#if FIRBASE_ENABLE
using Firebase.RemoteConfig;
#elif ENABLE_GETCONFIG
using Firebase.RemoteConfig;
#endif

namespace mygame.sdk
{
    partial class FIRhelper
    {
        void parserConfig()
        {
            try
            {
                Debug.Log($"mysdk: fir parserConfig0");
                isFetchConfig = 1;
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
                ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue("map_media_source");
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    AppsFlyerHelperScript.Instance.mapNewMediaSource(v.StringValue);
                }
                bool isConfigAds = false;
#if UNITY_ANDROID
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("ads_region_android");
#else
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("ads_region_ios");
#endif
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    var listads = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    if (listads != null || listads.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> item in listads)
                        {
                            ObjectAdsCf obads = null;
                            bool willrcvCf = false;
                            string campar = "";
                            if (item.Key.CompareTo("default") == 0)
                            {
                                obads = AdsHelper.Instance.currConfig;
                                isConfigAds = true;
                                willrcvCf = true;
                                Debug.Log($"mysdk: fir parserConfig ads curr cf country1={item.Key}");
                            }
                            else if (GameHelper.Instance.countryCode != null && GameHelper.Instance.countryCode.Length > 0 && item.Key.Contains(GameHelper.Instance.countryCode))
                            {
                                obads = AdsHelper.Instance.currConfig;
                                obads.countrycode = GameHelper.Instance.countryCode;
                                isConfigAds = true;
                                willrcvCf = true;
                                Debug.Log($"mysdk: fir parserConfig ads curr cf country2={item.Key} GameHelper_countryCode={GameHelper.Instance.countryCode}");
                            }
                            if (SDKManager.Instance.mediaCampain != null)
                            {
                                string[] arrik = item.Key.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                for (int it = 0; it < arrik.Length; it++)
                                {
                                    string[] arrPks = arrik[it].ToLower().Split('_');
                                    if (arrPks != null && arrPks.Length > 0)
                                    {
                                        string camName = arrPks[0];
                                        if (arrPks.Length > 1 && arrPks[1].CompareTo("any") != 0)
                                        {
                                            camName += $"_{arrPks[1]}";
                                        }
                                        if (SDKManager.Instance.mediaCampain.StartsWith(camName + "_") || camName.CompareTo("cany") == 0)
                                        {
                                            if (arrPks.Length > 2)
                                            {
                                                if (AppsFlyerHelperScript.Instance.checkSameSource(arrPks[2]))
                                                {
                                                    campar = arrik[it];
                                                    isConfigAds = true;
                                                    willrcvCf = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                campar = arrik[it];
                                                isConfigAds = true;
                                                willrcvCf = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (campar.Length > 0)
                            {
                                obads = AdsHelper.Instance.currConfig;
                                obads.mediaCampain = SDKManager.Instance.mediaCampain;
                                isConfigAds = true;
                                willrcvCf = true;
                                Debug.Log($"mysdk: fir parserConfig ads curr cf country3={item.Key} campar={campar}");
                            }
                            if (obads == null && !AppsFlyerHelperScript.Instance.isGetData)
                            {
                                obads = new ObjectAdsCf();
                                obads.coppyFromOther(AdsHelper.Instance.currConfig);
                                Debug.Log($"mysdk: fir parserConfig ads curr cf country4={item.Key} obads=null");
                            }
                            if (obads != null)
                            {
                                IDictionary<string, object> oitm = (IDictionary<string, object>)item.Value;
                                Debug.Log($"mysdk: fir parserConfig ads willrcvCf={willrcvCf} country={item.Key} sdk_campain={obads.mediaCampain} campar={obads.mediaCampain} obads.countrycode={obads.countrycode}");
                                if (oitm.ContainsKey("maskAdsStatus"))
                                {
                                    obads.maskAdsStatus = Convert.ToInt32(oitm["maskAdsStatus"]);
                                }
                                if (oitm.ContainsKey("bnReload"))
                                {
                                    obads.timeReloadBanner = Convert.ToInt32(oitm["bnReload"]);
                                }
                                if (oitm.ContainsKey("bnChangeCl"))
                                {
                                    obads.timeChangeCl2Banner = Convert.ToInt32(oitm["bnChangeCl"]);
                                }
                                if (oitm.ContainsKey("bnAutoReload"))
                                {
                                    obads.timeAutoReloadBanner = Convert.ToInt32(oitm["bnAutoReload"]);
                                }
                                if (oitm.ContainsKey("bnclAutoReload"))
                                {
                                    obads.timeAutoReloadBannerCl = Convert.ToInt32(oitm["bnclAutoReload"]);
                                }
                                if (oitm.ContainsKey("bnclTypeAutoReload"))
                                {
                                    obads.typeAutoReloadBannerCl = Convert.ToInt32(oitm["bnclTypeAutoReload"]);
                                }
                                if (oitm.ContainsKey("Sdk_version_show_banner"))
                                {
                                    obads.verShowBanner = Convert.ToInt32(oitm["Sdk_version_show_banner"]);
                                }
                                if (oitm.ContainsKey("step_show_banner"))
                                {
                                    obads.stepShowBanner = (string)oitm["step_show_banner"];
                                    obads.parSerStepBN(obads.stepShowBanner);
                                }

                                var ss = "show_gift_config";
                                if (oitm.ContainsKey(ss))
                                {
                                    obads.activeRewardLevel = (string)oitm[ss];
                                    Debug.Log("-----------------------" + (string)oitm[ss]);
                                    if (willrcvCf)
                                    {
                                        AdsRewardConfig.CF_REWARD_LEVEL = obads.activeRewardLevel;
                                        AdsRewardConfig.SetConfigAll();
                                    }
                                }

                                var ss2 = "gold_complete_level";
                                if (oitm.ContainsKey(ss2))
                                {
                                    obads.goldCompleteLevel = (string)oitm[ss2];
                                    Debug.Log("-----------------------" + (string)oitm[ss2]);
                                    if (willrcvCf)
                                    {
                                        PlayerPrefsUtil.CFGoldCompleteLevel= obads.goldCompleteLevel;
                                    }
                                }
                                
                                if (oitm.ContainsKey("bn_session_start"))
                                {
                                    obads.bnSessionShow = Convert.ToInt32(oitm["bn_session_start"]);
                                }
                                if (oitm.ContainsKey("bn_time_start"))
                                {
                                    obads.bnTimeStartShow = Convert.ToInt32(oitm["bn_time_start"]);
                                }
                                if (oitm.ContainsKey("interval_showcl"))
                                {
                                    obads.clIntervalnumover = (string)oitm["interval_showcl"];
                                    obads.parserIntervalnumoverShow(obads.clIntervalnumover, false);
                                }
                                //native
                                if (oitm.ContainsKey("step_show_native"))
                                {
                                    string step = (string)oitm["step_show_native"];
                                    obads.parSerStepNative(step);
                                }
                                //full
                                if (oitm.ContainsKey("fullTotalOfday"))
                                {
                                    obads.fullTotalOfday = Convert.ToInt32(oitm["fullTotalOfday"]);
                                }
                                if (oitm.ContainsKey("fullLevelStart"))
                                {
                                    obads.fullLevelStart = Convert.ToInt32(oitm["fullLevelStart"]);
                                }
                                if (oitm.ContainsKey("fullSessionStart"))
                                {
                                    obads.fullSessionStart = Convert.ToInt32(oitm["fullSessionStart"]);
                                }
                                if (oitm.ContainsKey("fullTimeStart"))
                                {
                                    obads.fullTimeStart = Convert.ToInt32(oitm["fullTimeStart"]);
                                }
                                if (oitm.ContainsKey("fullFlagFor2vscl"))
                                {
                                    obads.fullFlagFor2vscl = Convert.ToInt32(oitm["fullFlagFor2vscl"]);
                                }
                                if (oitm.ContainsKey("fullNtIsIc"))
                                {
                                    obads.fullNtIsIc = Convert.ToInt32(oitm["fullNtIsIc"]);
#if UNITY_IOS || UNITY_IPHONE
                                    obads.fullNtIsIc = 0;
#endif
                                }
                                if (oitm.ContainsKey("defaultFullNumover"))
                                {
                                    obads.fullDefaultNumover = Convert.ToInt32(oitm["defaultFullNumover"]);
                                }
                                if (oitm.ContainsKey("fullShowPlaying"))
                                {
                                    obads.fullShowPlaying = Convert.ToInt32(oitm["fullShowPlaying"]);
                                }
                                if (oitm.ContainsKey("fullExcluseRun"))
                                {
                                    obads.excluseFullrunning = (string)oitm["fullExcluseRun"];
                                    obads.parSerExcluseFull(obads.excluseFullrunning);
                                }
                                if (oitm.ContainsKey("fullDeltatime"))
                                {
                                    obads.fullDeltatime = 1000 * Convert.ToInt32(oitm["fullDeltatime"]);
                                }
                                if (oitm.ContainsKey("fullImgDeltatime"))
                                {
                                    obads.fullImgDeltatime = 1000 * Convert.ToInt32(oitm["fullImgDeltatime"]);
                                }
                                if (oitm.ContainsKey("loadMaxLow"))
                                {
                                    obads.isLoadMaxLow = Convert.ToInt32(oitm["loadMaxLow"]);
                                }
                                if (oitm.ContainsKey("interval_showfull"))
                                {
                                    obads.fullIntervalnumover = (string)oitm["interval_showfull"];
                                    obads.parserIntervalnumoverShow(obads.fullIntervalnumover, true);
                                }
                                if (oitm.ContainsKey("steplevel_showfull"))
                                {
                                    obads.fullStepLevel = (string)oitm["steplevel_showfull"];
                                    obads.parserFullStepLevelShow(obads.fullStepLevel);
                                }
                                if (oitm.ContainsKey("step_show_full"))
                                {
                                    obads.stepShowFull = (string)oitm["step_show_full"];
                                    obads.parSerStepFull(obads.stepShowFull);
                                }
                                if (oitm.ContainsKey("logic_fullrw"))
                                {
                                    obads.cfFullRw = (string)oitm["logic_fullrw"];
                                    obads.parserFullRw(obads.cfFullRw);
                                }
                                if (oitm.ContainsKey("deltaLose"))
                                {
                                    obads.fullDeltaLose = Convert.ToInt32(oitm["deltaLose"]);
                                }
                                if (oitm.ContainsKey("deltaTimeFullGift"))
                                {
                                    obads.fullDeltaTime4Gift = 1000 * Convert.ToInt32(oitm["deltaTimeFullGift"]);
                                }
                                if (oitm.ContainsKey("cf_day_activen"))
                                {
                                    IDictionary<string, object> dicdayac = (IDictionary<string, object>)oitm["cf_day_activen"];
                                    obads.parserDayActive(dicdayac);
                                }
                                //gift
                                if (oitm.ContainsKey("giftTotalOfday"))
                                {
                                    obads.giftTotalOfday = Convert.ToInt32(oitm["giftTotalOfday"]);
                                }
                                if (oitm.ContainsKey("giftDeltatime"))
                                {
                                    obads.giftDeltatime = 1000 * Convert.ToInt32(oitm["giftDeltatime"]);
                                }
                                if (oitm.ContainsKey("step_show_gift"))
                                {
                                    obads.stepShowGift = (string)oitm["step_show_gift"];
                                    obads.parSerStepGift(obads.stepShowGift);
                                }
                                if (oitm.ContainsKey("special_con"))
                                {
                                    IDictionary<string, object> dicSpec = (IDictionary<string, object>)oitm["special_con"];
                                    obads.parSerSpecial(dicSpec);
                                }
                                //openads
                                if (oitm.ContainsKey("cf_open_ad_typen"))
                                {
                                    obads.OpenAdShowtype = Convert.ToInt32(oitm["cf_open_ad_typen"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_showat"))
                                {
                                    obads.OpenAdShowat = Convert.ToInt32(oitm["cf_open_ad_showat"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_show_firstopen"))
                                {
                                    obads.OpenAdIsShowFirstOpen = Convert.ToInt32(oitm["cf_open_ad_show_firstopen"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_deltime"))
                                {
                                    obads.OpenAdDelTimeOpen = Convert.ToInt32(oitm["cf_open_ad_deltime"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_timestart"))
                                {
                                    obads.OpenAdTimeShowStart = Convert.ToInt32(oitm["cf_open_ad_timestart"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_level_show"))
                                {
                                    obads.OpenadLvshow = Convert.ToInt32(oitm["cf_open_ad_level_show"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_wait_first"))
                                {
                                    obads.OpenAdTimeWaitShowFirst = Convert.ToInt32(oitm["cf_open_ad_wait_first"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_timebg"))
                                {
                                    obads.OpenAdTimeBg = Convert.ToInt32(oitm["cf_open_ad_timebg"]);
                                }
                                if (oitm.ContainsKey("cf_open_ad_formatopen"))
                                {
                                    obads.OpenAdFlagOpenFormat = Convert.ToInt32(oitm["cf_open_ad_formatopen"]);
                                }

                                if (willrcvCf && oitm.ContainsKey("cf_open_newlogicn"))
                                {
                                    int flagcf = Convert.ToInt32(oitm["cf_open_newlogicn"]);
                                    PlayerPrefs.SetInt("cf_open_newlogicn", flagcf);
                                    if (AdsHelper.Instance != null)
                                    {
                                        AdsHelper.Instance.isNewlogFirtOpen = flagcf;
                                    }
                                }
                                if (oitm.ContainsKey("cf_type_myopenad"))
                                {
                                    obads.typeMyopenAd = Convert.ToInt32(oitm["cf_type_myopenad"]);
                                }

                                if (willrcvCf && oitm.ContainsKey("cf_logic_iron"))
                                {
                                    int lvban = Convert.ToInt32(oitm["cf_logic_iron"]);
                                    if (!AppsFlyerHelperScript.Instance.checkSameSource("ir"))
                                    {
                                        PlayerPrefs.SetInt("mem_cf_login_iron", 0);
                                        AdsHelper.Instance.statusLogicIron = 0;
                                    }
                                    else
                                    {
                                        PlayerPrefs.SetInt("mem_cf_login_iron", lvban);
                                        AdsHelper.Instance.statusLogicIron = lvban;
                                    }
                                }

                                if (item.Key.CompareTo("default") == 0)
                                {
                                    if (oitm.ContainsKey("is_log_admob_va2af"))
                                    {
                                        isLogAdmobRevenueAppsFlyer = Convert.ToInt32(oitm["is_log_admob_va2af"]);
                                        PlayerPrefs.SetInt("is_log_admob_va2af", isLogAdmobRevenueAppsFlyer);
                                    }

                                    if (oitm.ContainsKey("is_log_admob_va2af_me"))
                                    {
                                        isLogAdmobRevenueAppsFlyerinMe = Convert.ToInt32(oitm["is_log_admob_va2af_me"]);
                                        PlayerPrefs.SetInt("is_log_admob_va2af_me", isLogAdmobRevenueAppsFlyerinMe);
                                    }

                                    if (oitm.ContainsKey("ad_va_divide"))
                                    {
                                        int tmpva = Convert.ToInt32(oitm["ad_va_divide"]);
                                        PlayerPrefs.SetInt("ad_va_divide", tmpva);
                                        AdmobRevenewDivide = tmpva;
                                    }
                                }

                                if (AppsFlyerHelperScript.Instance.isGetData)
                                {
                                    if (campar.Length > 0)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (AdsHelper.Instance.dicRegionWait4Campain == null && item.Key.CompareTo("default") != 0)
                                    {
                                        AdsHelper.Instance.dicRegionWait4Campain = new Dictionary<string, ObjectAdsCf>();
                                    }
                                    if (AdsHelper.Instance.dicRegionWait4Campain != null)
                                    {
                                        AdsHelper.Instance.dicRegionWait4Campain.Add(item.Key, obads);
                                    }
                                }
                            }
                        }
                    }
                }

#if UNITY_ANDROID
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("ads_region2_android");
#else
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("ads_region2_ios");
#endif
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    var listads = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    if (listads != null || listads.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> item in listads)
                        {
                            ObjectAdsCf obads = null;
                            if (item.Key.CompareTo("default") == 0)
                            {
                                obads = AdsHelper.Instance.currConfig;
                                isConfigAds = true;
                            }
                            else if (item.Key.Contains(GameHelper.Instance.countryCode))
                            {
                                obads = AdsHelper.Instance.currConfig;
                                obads.countrycode = GameHelper.Instance.countryCode;
                                isConfigAds = true;
                            }
                            if (obads != null)
                            {
                                Debug.Log($"mysdk: fir parserfull2 country=" + item.Key);
                                IDictionary<string, object> oitm = (IDictionary<string, object>)item.Value;
                                if (oitm.ContainsKey("full2LevelStartn"))
                                {
                                    obads.full2LevelStart = Convert.ToInt32(oitm["full2LevelStartn"]);
#if Test_lluf2
                                    obads.full2LevelStart = 0;
#endif
                                }
                                if (oitm.ContainsKey("full2StartTime"))
                                {
                                    obads.full2Starttime = Convert.ToInt32(oitm["full2StartTime"]);
                                }
                                if (oitm.ContainsKey("full2SessionTime"))
                                {
                                    obads.full2Sessiontime = Convert.ToInt32(oitm["full2SessionTime"]);
                                }
#if Test_lluf2
                                obads.full2Sessiontime = 0;
#endif
                                if (oitm.ContainsKey("full2CountFull"))
                                {
                                    obads.full2CountFull = Convert.ToInt32(oitm["full2CountFull"]);
                                }
                                if (oitm.ContainsKey("full2Deltatime"))
                                {
                                    obads.full2Deltatime = 1000 * Convert.ToInt32(oitm["full2Deltatime"]);
                                }
                                if (oitm.ContainsKey("full2TypeShow"))
                                {
                                    obads.full2TypeShow = Convert.ToInt32(oitm["full2TypeShow"]);
#if Test_lluf2
                                    obads.full2TypeShow = 3;
#endif
                                }
                                if (oitm.ContainsKey("full2FlagVn"))
                                {
                                    obads.full2Flag4VN = Convert.ToInt32(oitm["full2FlagVn"]);
                                }
#if Test_lluf2
                                obads.full2Flag4VN = 0;
                                isAdSkip = 97;
#endif

                                if (oitm.ContainsKey("full2Numover"))
                                {
                                    obads.full2Numover = Convert.ToInt32(oitm["full2Numover"]);
                                }
                                if (oitm.ContainsKey("full2Step"))
                                {
                                    obads.full2StepShow = (string)oitm["full2Step"];
#if Test_lluf2
                                    obads.full2StepShow = "cir:20,70#re:10";
#endif
                                    obads.parSerStepFull2(obads.full2StepShow);
                                }
                                if (oitm.ContainsKey("cf_ntfull_click"))
                                {
                                    string cfntfullclick = (string)oitm["cf_ntfull_click"];
                                    PlayerPrefs.SetString("mem_cf_ntfull_lic", cfntfullclick);
                                    if (AdsHelper.Instance != null)
                                    {
                                        ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).setCfNtFull(cfntfullclick);
                                        if (AdsHelper.Instance.adsApplovinMaxMy != null)
                                        {
                                            ((AdsMaxMy)AdsHelper.Instance.adsApplovinMaxMy).setCfNtFull(cfntfullclick);
                                        }
                                    }
                                }
                                if (oitm.ContainsKey("cf_ntfbfull_click"))
                                {
                                    string cfntfullclick = (string)oitm["cf_ntfbfull_click"];
                                    PlayerPrefs.SetString("mem_cf_ntfbfull_lic", cfntfullclick);
                                    if (AdsHelper.Instance != null)
                                    {
                                        if (AdsHelper.Instance.adsFb != null)
                                        {
                                            ((AdsAFbMy)AdsHelper.Instance.adsFb).setCfNtFull(cfntfullclick);
                                        }
                                    }
                                }
                                if (oitm.ContainsKey("cf_nt_dayclick"))
                                {
                                    string cfnatDaclick = (string)oitm["cf_nt_dayclick"];
                                    PlayerPrefs.SetString("mem_cf_nt_dayflic", cfnatDaclick);
                                    if (AdsHelper.Instance != null)
                                    {
                                        ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).setCfNtdayClick(cfnatDaclick);
                                    }
                                }
                            }
                        }
                    }
                }

#if UNITY_ANDROID
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("admob_defaultid_android");
#else
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("admob_defaultid_ios");
#endif

                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    IDictionary<string, object> cfdfid = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    foreach (KeyValuePair<string, object> adsnet in cfdfid)
                    {
                        if (adsnet.Key.Equals("admob"))
                        {
                            IDictionary<string, object> cfadsType = (IDictionary<string, object>)adsnet.Value;
                            foreach (KeyValuePair<string, object> adstype in cfadsType)
                            {
                                if (adstype.Key.Contains("cf_banner"))
                                {
                                    AdsHelper.Instance.adsAdmobMy.setBannerId((string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_open"))
                                {
                                    PlayerPrefs.SetString($"mem_df0_open_id", (string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_full_img"))
                                {
                                    PlayerPrefs.SetString($"mem_df_full_img_id", (string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_full"))
                                {
                                    AdsHelper.Instance.adsAdmobMy.setFullId((string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_gift"))
                                {
                                    AdsHelper.Instance.adsAdmobMy.setGiftId((string)adstype.Value);
                                }
                            }
                            isConfigAds = true;
                        }
                        else if (adsnet.Key.Equals("applovin"))
                        {
                            IDictionary<string, object> cfadsType = (IDictionary<string, object>)adsnet.Value;
                            foreach (KeyValuePair<string, object> adstype in cfadsType)
                            {
                                if (adstype.Key.Contains("cf_banner"))
                                {
                                    AdsHelper.Instance.adsApplovinMax.setBannerId((string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_open"))
                                {
                                    PlayerPrefs.SetString($"mem_df6_open_id", (string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_full"))
                                {
                                    AdsHelper.Instance.adsApplovinMax.setFullId((string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_gift"))
                                {
                                    AdsHelper.Instance.adsApplovinMax.setGiftId((string)adstype.Value);
                                }
                            }
                            isConfigAds = true;
                        }
                        else if (adsnet.Key.Equals("iron"))
                        {
                            IDictionary<string, object> cfadsType = (IDictionary<string, object>)adsnet.Value;
                            foreach (KeyValuePair<string, object> adstype in cfadsType)
                            {
                                if (adstype.Key.Contains("cf_banner"))
                                {
                                    AdsHelper.Instance.adsIron.setBannerId((string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_open"))
                                {
                                    PlayerPrefs.SetString($"mem_df3_open_id", (string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_full"))
                                {
                                    AdsHelper.Instance.adsIron.setFullId((string)adstype.Value);
                                }
                                else if (adstype.Key.Contains("cf_gift"))
                                {
                                    AdsHelper.Instance.adsIron.setGiftId((string)adstype.Value);
                                }
                            }
                            isConfigAds = true;
                        }
                    }
                }

                v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ids_placement");
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    var adPlacement = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    if (adPlacement != null || adPlacement.Count > 0)
                    {
                        ObjectAdsCf obads = AdsHelper.Instance.currConfig;
                        foreach (KeyValuePair<string, object> item in adPlacement)
                        {
                            isConfigAds = true;
                            string scf = "";
                            string slist = "";
                            IDictionary<string, object> cfPl = (IDictionary<string, object>)item.Value;
                            foreach (KeyValuePair<string, object> plItem in cfPl)
                            {
                                if (scf.Length > 0)
                                {
                                    scf += "#" + plItem.Key;
                                }
                                else
                                {
                                    scf = plItem.Key;
                                }
                                IDictionary<string, object> plcf = (IDictionary<string, object>)plItem.Value;
                                int prio = -1;
                                if (plcf.ContainsKey("idx_high_priority"))
                                {
                                    prio = Convert.ToInt32(plcf["idx_high_priority"]);
                                }
                                scf += "," + prio;
                                slist = (string)plcf["list"];
                                scf += "," + slist;
                            }
                            if (item.Key.CompareTo("cf_openad") == 0)
                            {
                                obads.adCfPlacementOpen = scf;
                            }
                            else if (item.Key.CompareTo("cf_banner") == 0)
                            {
                                obads.adCfPlacementBanner = scf;
                            }
                            else if (item.Key.CompareTo("cf_native_bn") == 0)
                            {
                                obads.adCfPlacementBnNt = scf;
                            }
                            else if (item.Key.CompareTo("cf_native_cl") == 0)
                            {
                                obads.adCfPlacementNattiveCl = scf;
                            }
                            else if (item.Key.CompareTo("cf_banner_collapse") == 0)
                            {
                                obads.adCfPlacementCollapse = scf;
                            }
                            else if (item.Key.CompareTo("cf_banner_rect") == 0)
                            {
                                obads.adCfPlacementRect = scf;
                            }
                            else if (item.Key.CompareTo("cf_rect_native") == 0)
                            {
                                obads.adCfPlacementRectNt = scf;
                            }
                            else if (item.Key.CompareTo("cf_native") == 0)
                            {
                                obads.adCfPlacementNative = scf;
                            }
                            else if (item.Key.CompareTo("cf_nativefull") == 0)
                            {
                                obads.adCfPlacementNtFull = scf;
                            }
                            else if (item.Key.CompareTo("cf_nativeicfull") == 0)
                            {
                                obads.adCfPlacementNtIcFull = scf;
                            }
                            else if (item.Key.CompareTo("cf_nativepgfull") == 0)
                            {
                                obads.adCfPlacementNtPgFull = scf;
                            }
                            else if (item.Key.CompareTo("cf_full_img") == 0)
                            {
                                PlayerPrefs.SetString($"mem_ad_placement_low", slist);
                                if (AdsHelper.Instance != null && AdsHelper.Instance.adsAdmobMyLower != null)
                                {
                                    ((AdsAdmobMyLow)AdsHelper.Instance.adsAdmobMyLower).initIdLow(scf);
                                }
                            }
                            else if (item.Key.CompareTo("cf_full") == 0)
                            {
                                obads.adCfPlacementFull = scf;
                            }
                            else if (item.Key.CompareTo("cf_fullRwInter") == 0)
                            {
                                obads.adCfPlacementFullRwInter = scf;
                            }
                            else if (item.Key.CompareTo("cf_fullRwRw") == 0)
                            {
                                obads.adCfPlacementFullRwRw = scf;
                            }
                            else if (item.Key.CompareTo("cf_gift") == 0)
                            {
                                obads.adCfPlacementGift = scf;
                            }
                            else if (item.Key.CompareTo("cf_nativegift") == 0)
                            {
                                obads.adCfPlacementNtGift = scf;
                            }
                            else if (item.Key.CompareTo("cf_immersive") == 0)
                            {
                                PlayerPrefs.SetString($"mem_list_ids_low", slist);
                                if (GoogleHelper.Instance != null)
                                {
                                    GoogleHelper.Instance.initListIds(slist);
                                }
                            }
                        }
                    }
                }
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("mytarget_ad_placement");
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    var listfloorecpm = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    if (listfloorecpm != null || listfloorecpm.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> item in listfloorecpm)
                        {
                            ObjectAdsCf obads = null;
                            if (item.Key.CompareTo("default") == 0)
                            {
                                obads = AdsHelper.Instance.currConfig;
                                isConfigAds = true;
                            }
                            else if (item.Key.Contains(GameHelper.Instance.countryCode))
                            {
                                obads = AdsHelper.Instance.currConfig;
                                obads.countrycode = GameHelper.Instance.countryCode;
                                isConfigAds = true;
                            }
                            if (obads != null)
                            {
                                Debug.Log($"mysdk: fir parserConfig mytarget ecpm floor country=" + item.Key);
                                IDictionary<string, object> oitm = (IDictionary<string, object>)item.Value;
                                if (oitm.ContainsKey("cf_banner"))
                                {
                                    obads.mytargetStepFloorECPMBanner = (string)oitm["cf_banner"];
                                }
                                if (oitm.ContainsKey("cf_full"))
                                {
                                    obads.mytargetStepFloorECPMFull = (string)oitm["cf_full"];
                                }
                                if (oitm.ContainsKey("cf_gift"))
                                {
                                    obads.mytargetStepFloorECPMGift = (string)oitm["cf_gift"];
                                }
                            }
                        }
                    }
                }
                v = FirebaseRemoteConfig.DefaultInstance.GetValue("yandex_ad_placement");
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    var listfloorecpm = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    if (listfloorecpm != null || listfloorecpm.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> item in listfloorecpm)
                        {
                            ObjectAdsCf obads = null;
                            if (item.Key.CompareTo("default") == 0)
                            {
                                obads = AdsHelper.Instance.currConfig;
                                isConfigAds = true;
                            }
                            else if (item.Key.Contains(GameHelper.Instance.countryCode))
                            {
                                obads = AdsHelper.Instance.currConfig;
                                obads.countrycode = GameHelper.Instance.countryCode;
                                isConfigAds = true;
                            }
                            if (obads != null)
                            {
                                Debug.Log($"mysdk: fir parserConfig yandex ecpm floor country=" + item.Key);
                                IDictionary<string, object> oitm = (IDictionary<string, object>)item.Value;
                                if (oitm.ContainsKey("cf_banner"))
                                {
                                    obads.yandexStepFloorECPMBanner = (string)oitm["cf_banner"];
                                }
                                if (oitm.ContainsKey("cf_full"))
                                {
                                    obads.yandexStepFloorECPMFull = (string)oitm["cf_full"];
                                }
                                if (oitm.ContainsKey("cf_gift"))
                                {
                                    obads.yandexStepFloorECPMGift = (string)oitm["cf_gift"];
                                }
                            }
                        }
                    }
                }

                v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_log_ab_test_key");
                if (v.StringValue is { Length: > 0 })
                {
                    LogEventManager.SetABTestKey(v.StringValue);
                }

                if (LogEventManager.ABTestKeys != null)
                {
                    foreach (var key in LogEventManager.ABTestKeys)
                    {
                        v = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                        if (v.StringValue is { Length: > 0 })
                        {
                            PlayerPrefs.SetString(key, v.StringValue);
                        }
                    }
                }

                if (isConfigAds)
                {
                    AdsHelper.Instance.configWithRegion(true);
                }
                parserPromo();
                parserPuzzleGame();
                parserUpdateGame();
                parserMoregame();
                parserCfGame();
                FIRParserOtherConfig.parserInGameConfig();
#endif
                Debug.Log($"mysdk: fir parserConfig1");
            }
            catch (Exception ex)
            {
                Debug.Log($"mysdk: fir ex=" + ex);
            }
        }
        public static string FirConfigGet(string key, string vdef)
        {
            if (isFetchConfig != 1)
            {
                return vdef;
            }
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            if (FirebaseRemoteConfig.DefaultInstance == null)
            {
                return vdef;
            }
            //Debug.LogError("mysdk: FirConfigGet string=" + key);
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                return v.StringValue;
            }
            else
            {
                return vdef;
            }
#else
            return vdef;
#endif
        }
        public static long FirConfigGet(string key, long vdef)
        {
            if (isFetchConfig != 1)
            {
                return vdef;
            }
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            if (FirebaseRemoteConfig.DefaultInstance == null)
            {
                return vdef;
            }
            //Debug.LogError("mysdk: FirConfigGet long=" + key);
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                return v.LongValue;
            }
            else
            {
                return vdef;
            }
#else
            return vdef;
#endif
        }
        public static double FirConfigGet(string key, double vdef)
        {
            if (isFetchConfig != 1)
            {
                return vdef;
            }
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            if (FirebaseRemoteConfig.DefaultInstance == null)
            {
                return vdef;
            }
            //Debug.LogError("mysdk: FirConfigGet double=" + key);
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                return v.DoubleValue;
            }
            else
            {
                return vdef;
            }
#else
            return vdef;
#endif
        }
        private void parserPuzzleGame()
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            //my game promo
#if UNITY_ANDROID
            string key_cf_puzzle = "cf_puzzle_android";
#else
            string key_cf_puzzle = "cf_puzzle_ios";
#endif
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key_cf_puzzle);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                Debug.Log($"mysdk: fir parserPuzzleGame");
                var obPuzzleGame = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                if (obPuzzleGame != null || obPuzzleGame.Count > 0)
                {
                    if (obPuzzleGame.ContainsKey("condition"))
                    {
                        var obCondition = (IDictionary<string, object>)obPuzzleGame["condition"];
                        if (obCondition != null || obCondition.Count > 0)
                        {
                            if (obCondition.ContainsKey("level"))
                            {
                                PuzzleControl.getInstance().levelShow = Convert.ToInt32(obCondition["level"]);
                            }
                            if (obCondition.ContainsKey("day"))
                            {
                                PuzzleControl.getInstance().dayShow = Convert.ToInt32(obCondition["day"]);
                            }
                            if (obCondition.ContainsKey("session"))
                            {
                                PuzzleControl.getInstance().sessionShow = Convert.ToInt32(obCondition["session"]);
                            }
                            if (obCondition.ContainsKey("delta_time"))
                            {
                                PuzzleControl.getInstance().deltaTimeShow = Convert.ToInt32(obCondition["delta_time"]);
                            }
                            if (obCondition.ContainsKey("game_show_num"))
                            {
                                PuzzleControl.getInstance().cfNextImp = Convert.ToInt32(obCondition["game_show_num"]);
                            }
                            PuzzleControl.getInstance().saveCondition();
                        }
                    }
                    int verPuzzleGame = 0;
                    if (obPuzzleGame.ContainsKey("ver"))
                    {
                        verPuzzleGame = Convert.ToInt32(obPuzzleGame["ver"]);
                    }
                    int memverpuzzle = PlayerPrefs.GetInt("mem_ver_puzzlegame", 0);
                    if (verPuzzleGame > memverpuzzle)
                    {
                        PlayerPrefs.SetInt("mem_ver_puzzlegame", verPuzzleGame);
                        PuzzleControl.getInstance().resetShow();
                    }

                    if (obPuzzleGame.ContainsKey("games"))
                    {
                        bool isnewGame = false;
                        var listGames = (List<object>)obPuzzleGame["games"];
                        var listnewgame = new List<PromoGameOb>();
                        foreach (var itemgame in listGames)
                        {
                            var gamepuzzle = (IDictionary<string, object>)itemgame;
                            PromoGameOb g = PromoGameOb.newFromData(gamepuzzle);
                            listnewgame.Add(g);
                            if (PuzzleControl.getInstance().checkNewGameAndRemoveExist(g))
                            {
                                isnewGame = true;
                            }
                        }
                        PuzzleControl.getInstance().updateListGames(listnewgame);
                        listnewgame.Clear();
                        if (isnewGame)
                        {
                            PuzzleControl.getInstance().saveListGame();
                            PuzzleControl.getInstance().load4Show();
                        }
                    }
                }
            }
#endif
        }
        private void parserPromo()
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            //my game promo
#if UNITY_ANDROID
            string key_cf_game_promo = "cf_game_promo_android";
#else
            string key_cf_game_promo = "cf_game_promo_ios";
#endif
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key_cf_game_promo);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                Debug.Log($"mysdk: fir cf_game_promo");
                var obPromo = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                if (obPromo != null || obPromo.Count > 0)
                {
                    int verPromo = 0;
                    if (obPromo.ContainsKey("ver"))
                    {
                        verPromo = Convert.ToInt32(obPromo["ver"]);
                    }
                    int memverpromo = PlayerPrefs.GetInt("mem_ver_gamepromo", 0);
                    if (verPromo > memverpromo)
                    {
                        gamePromoCurr = null;
                        PlayerPrefs.SetInt("mem_ver_gamepromo", verPromo);
                        PlayerPrefs.SetString("cf_game_promo", "");
                        PlayerPrefs.SetString("mem_game_will_promo", "");
                        PlayerPrefs.SetString("mem_game_will_ntpromo", "");
                    }
                    string memlistgame = PlayerPrefs.GetString("cf_game_promo", "");
                    var obmemgames = (IDictionary<string, object>)JsonDecoder.DecodeText(memlistgame);
                    List<object> listmemgames = null;
                    if (obmemgames != null && obmemgames.ContainsKey("games"))
                    {
                        listmemgames = (List<object>)obmemgames["games"];
                    }

                    List<PromoGameOb> listnewgame = new List<PromoGameOb>();
                    bool isnewGame = false;
                    if (obPromo.ContainsKey("games"))
                    {
                        var listGames = (List<object>)obPromo["games"];
                        foreach (var itemgame in listGames)
                        {
                            var gamepromo = (IDictionary<string, object>)itemgame;
                            PromoGameOb g = PromoGameOb.newFromData(gamepromo);
                            listnewgame.Add(g);
                            if (!isnewGame)
                            {
                                if (listmemgames != null)
                                {
                                    bool hasinlist = false;
                                    for (int kk = 0; kk < listmemgames.Count; kk++)
                                    {
                                        gamepromo = (IDictionary<string, object>)listmemgames[kk];
                                        string pkgmm = (string)gamepromo["pkg"];
                                        if (pkgmm.Equals(g.pkg))
                                        {
                                            hasinlist = true;
                                            break;
                                        }
                                    }
                                    if (!hasinlist)
                                    {
                                        isnewGame = true;
                                    }
                                }
                                else
                                {
                                    isnewGame = true;
                                }
                            }
                        }
                    }
                    if (isnewGame)
                    {
                        memlistgame = "{\"games\":[";
                        for (int kk = 0; kk < listnewgame.Count; kk++)
                        {
                            string itemgameData = listnewgame[kk].toJsonData();
                            if (kk != 0)
                            {
                                memlistgame += $",{itemgameData}";
                            }
                            else
                            {
                                memlistgame += itemgameData;
                            }
                        }
                        memlistgame += "]}";
                        PlayerPrefs.SetString("cf_game_promo", memlistgame);
                        gamePromoCurr = null;
                        getGamePromo();
                    }
                }
            }
#endif
        }
        private void parserUpdateGame()
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            //my game promo
#if UNITY_ANDROID
            string key_cf_game_update = "cf_game_update_android";
#else
            string key_cf_game_update = "cf_game_update_ios";
#endif
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key_cf_game_update);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                var obupdategame = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                if (obupdategame != null || obupdategame.Count > 0)
                {
                    int statusUpdate = Convert.ToInt32(obupdategame["status"]);
                    int verUpdate = Convert.ToInt32(obupdategame["version"]);
                    int daycf = 0;
                    if (obupdategame.ContainsKey("day"))
                    {
                        daycf = Convert.ToInt32(obupdategame["day"]);
                    }
                    string gameid = AppConfig.appid;
                    string titleup = "";
                    string desup = "";
                    string link = "";
                    if (obupdategame.ContainsKey("gameid"))
                    {
                        gameid = (string)obupdategame["gameid"];
                    }
                    if (obupdategame.ContainsKey("title"))
                    {
                        titleup = (string)obupdategame["title"];
                    }
                    if (obupdategame.ContainsKey("des"))
                    {
                        desup = (string)obupdategame["des"];
                    }
                    if (obupdategame.ContainsKey("link"))
                    {
                        link = (string)obupdategame["link"];
                    }
                    PlayerPrefs.SetInt("update_ver", verUpdate);
                    PlayerPrefs.SetInt("update_status", statusUpdate);
                    PlayerPrefs.GetInt("update_day_cf", daycf);
                    PlayerPrefs.SetString("update_gameid", gameid);
                    PlayerPrefs.SetString("update_link", link);
                    PlayerPrefs.SetString("update_title", titleup);
                    PlayerPrefs.SetString("update_des", desup);
                    checkUpdate();
                }
            }
#endif
        }
        private void parserMoregame()
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
#if UNITY_ANDROID
            string key_cf_game_update = "cf_more_game_android";
#else
            string key_cf_game_update = "cf_more_game_ios";
#endif
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(key_cf_game_update);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                var obupdategame = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                if (obupdategame != null || obupdategame.Count > 0)
                {
                    int idob = Convert.ToInt32(obupdategame["id"]);
                    int memid = PlayerPrefs.GetInt("my_more_game_id_mem", 0);
                    Debug.Log($"mysdk: fir parserMoregame idcf=" + idob + ", idmem=" + memid);
                    if (idob > memid)
                    {
                        SDKManager.Instance.listMoreGame.Clear();
                        if (obupdategame.ContainsKey("btMore"))
                        {
                            PlayerPrefs.SetString("my_more_game_btmore_mem", (string)obupdategame["btMore"]);
                        }
                        if (obupdategame.ContainsKey("games"))
                        {
                            var games = (List<object>)obupdategame["games"];
                            foreach (var itemgame in games)
                            {
                                var gamemore = (IDictionary<string, object>)itemgame;
                                MoreGameOb g = new MoreGameOb();
                                SDKManager.Instance.listMoreGame.Add(g);
                                g.gameName = (string)gamemore["name"];
                                g.icon = (string)gamemore["icon"];
                                g.gameId = (string)gamemore["gameId"];
                                g.playAbleAds = (string)gamemore["playAbleAds"];
                                g.isStatic = Convert.ToInt32(gamemore["isStatic"]);
                                g.linkStore = (string)gamemore["linkStore"];
                            }
                            SDKManager.Instance.saveMoreGame();
                            SDKManager.Instance.downMoreGame();
                        }
                        PlayerPrefs.SetInt("my_more_game_id_mem", idob);
                    }
                }
            }
#endif
        }
        private void parserCfGame()
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue("level_show_request_IDFA_iOS");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int lvshow = PlayerPrefs.GetInt("lv_show_request_idfa", 2);
                if (lvshow > 0)
                {
                    int per = (int)v.LongValue;
                    PlayerPrefs.SetInt("lv_show_request_idfa", per);
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lv_ktro");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int lvban = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_lv_ktro", lvban);
            }
#if ENABLE_LOGDATA_BUCKET
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_log_databucket");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int cflog = (int)v.LongValue;
                LogDataBucket.CF_EnableLogDataBucket = (cflog == 1);
            }
#endif
#if ENABLE_LOGDATA_MYSERVER
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_log_server");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int cflog = (int)v.LongValue;
                LogServer.CF_EnableLogServer = (cflog == 1);
            }
#endif
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ads_placement");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_ads_placement", v.StringValue);
                if (AdsHelper.Instance != null)
                {
                    AdsHelper.Instance.parserConfigAdsPlacement(v.StringValue);
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ver_os_show_idfa");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_ver_os_show_idfa", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ver_game_show_idfa");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_ver_game_show_idfa", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("LvShowRate");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("mem_lv_showrate", v.StringValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_count_open_checkcon");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_count_open_checkcon", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_lv_checkcon");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_lv_checkcon", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_trathai_xadu");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_trathai_xadu", per);
                Debug.Log($"mysdk: fir cf_trathai_xadu={per}");
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adcanvas_CMP");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adcanvas_CMP", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adcanvas_enable");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adcanvas_enable", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adcanvas_alshow");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adcanvas_alshow", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adcanvas_click");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adcanvas_click", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adaudio_enable");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adaudio_enable", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adaudio_CMP");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adaudio_CMP", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adaudio_deltatime");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adaudio_deltatime", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adaudio_cofull");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_adaudio_cofull", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_flag_mylog");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("mem_flag_log", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_url_mylog");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("mem_url_log", v.StringValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("is_unload_when_lowmem");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("is_unload_when_lowmem", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_game_kitr");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsBase.Instance().setInt("mem_procinva_gema", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_game_flag_bira");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsBase.Instance().setInt("mem_flag_check_bira", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("is_vali_appsf");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefsBase.Instance().setInt("is_vali_appsf", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_per_post_adva");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                perPostAdVa = (int)v.LongValue;
                PlayerPrefs.SetInt("mem_va_of_ad_postfir", perPostAdVa);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("time_splash_scr");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                SDKManager.Instance.updateTimeSplash((int)v.LongValue);
                PlayerPrefs.SetInt("time_splash_scr", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_first_open");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_show_first_open", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_count_show_aodiomob");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int va = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_count_show_aodiomob", va);
                if (AdAudioHelper.Instance != null)
                {
                    AdAudioHelper.Instance.CfCountShowAudioMob = va;
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_time_close_ntfull");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_time_close_ntfull", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_time_close_ntfull2");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_time_close_ntfull2", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_langage");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_show_langage", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_per_post_iap");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_per_post_iap", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_per_postfir_iap");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_per_postfir_iap", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_use_native_cl");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                AdsHelper.Instance.useNativeCollapse = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_use_native_cl", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_bnnt_refresh");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_bnnt_refresh", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ntcl_closeclick");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                AdsHelper.Instance.isNtclCloseWhenClick = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_ntcl_clos_w_click", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ss_show2_first");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_ss_show2_first", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ntfull_fb_excluse");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_ntfull_fb_excluse", v.StringValue);
                if (AdsHelper.Instance != null)
                {
                    if (AdsHelper.Instance.adsAdmobMy != null)
                    {
                        ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).setCfNtFullFbExcluse(v.StringValue);
                    }
                    if (AdsHelper.Instance.adsFb != null)
                    {
                        ((AdsAFbMy)AdsHelper.Instance.adsFb).setCfNtFullFbExcluse(v.StringValue);
                    }
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ntcl_fb_excluse");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_ntcl_fb_excluse", v.StringValue);
                if (AdsHelper.Instance != null && AdsHelper.Instance.adsAdmobMy != null)
                {
                    ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).setCfNtClFbExcluse(v.StringValue);
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_bnnt_showmedia");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_bnnt_showmedia", (int)v.LongValue);
                if (AdsHelper.Instance != null && AdsHelper.Instance.adsAdmobMy != null)
                {
                    ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).setTypeBnnt((int)v.LongValue);
                }
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_check_ad_err");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int va = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_check_ad_err", va);
                GameHelper.cfCheckAdErr(va == 1);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("flag_log_4check_err");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                FIRhelper.flagLog4CheckErr = (int)v.LongValue;
                PlayerPrefs.SetInt("flag_log_4check_err", FIRhelper.flagLog4CheckErr);
            }

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_threshold_troas");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                float va = (int)v.LongValue;
                FIRhelper.ValueDailyAdRevenew = va / 10000.0f;
                PlayerPrefs.SetInt("cf_threshold_troas", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_threshold_troasgr");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_threshold_troasgr", v.StringValue);
                FIRhelper.parThresholdTroas(v.StringValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_threshold_top");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_threshold_top", v.StringValue);
                FIRhelper.parThresholdTop(v.StringValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_splash_vn");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                var per = v.LongValue;
                SplashLoadingCtr.CFShowSplashVN = (int)per;
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_post_ad_ntsplash");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                var per = v.DoubleValue;
                FIRhelper.perPostAdsNtSplash = (float)per / 100.0f;
                if (FIRhelper.perPostAdsNtSplash > 100)
                {
                    FIRhelper.perPostAdsNtSplash = 100;
                }
                else if (FIRhelper.perPostAdsNtSplash < 0)
                {
                    FIRhelper.perPostAdsNtSplash = 0;
                }

                PlayerPrefs.SetFloat("cf_post_ad_ntsplash", FIRhelper.perPostAdsNtSplash);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_post_ad_ntfull2");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                var per = v.DoubleValue;
                FIRhelper.perPostAdsNtFull2 = (float)per / 100.0f;
                if (FIRhelper.perPostAdsNtFull2 > 1)
                {
                    FIRhelper.perPostAdsNtFull2 = 1;
                }
                else if (FIRhelper.perPostAdsNtFull2 < 0)
                {
                    FIRhelper.perPostAdsNtFull2 = 0;
                }
                PlayerPrefs.SetFloat("cf_post_ad_ntfull2", FIRhelper.perPostAdsNtFull2);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_ntcl_flic");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("mem_cf_ntcl_flic", v.StringValue);
                if (AdsHelper.Instance != null)
                {
                    ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).setCfNtCl(v.StringValue);
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_targeting");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_admob_targeting", v.StringValue);
                if (AdsHelper.Instance != null)
                {
                    ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).targetTing(v.StringValue);
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_fb_per_post_ads");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                AdsAFbMy.FbPerPost = v.LongValue;
                PlayerPrefs.SetInt("fb_mem_per_post", (int)AdsAFbMy.FbPerPost);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_logic_skip");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                AdsHelper.Instance.isApplyLogicSkip = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_logic_skip", AdsHelper.Instance.isApplyLogicSkip);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_deltatime_nextloadid");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                AdsAdmobMy.timeDeltaLoad = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_deltime_load_admob2id", AdsAdmobMy.timeDeltaLoad);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_load_all");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_admob_load_all", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_check_lowdevie_4ad");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                if (AdsHelper.Instance != null)
                {
                    AutoPerformanceOptimizer.flagCheckDevice4Ads = (int)v.LongValue;
                    PlayerPrefs.SetInt("flag_cde_4_ad", (int)v.LongValue);
                    AutoPerformanceOptimizer.checkChangeLogicAdsLowRam();
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_layout_ntfull");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_layout_ntfull", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_time_ntfullpg");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_time_ntfullpg", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_time_ntgiftpg");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetInt("cf_time_ntgiftpg", (int)v.LongValue);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_banner_refresh");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_admob_bn_refresh", v.StringValue);
                if (AdsHelper.Instance != null && AdsHelper.Instance.adsAdmobMy != null)
                {
                    ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).configBannerRefresh();
                }
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_admob_banner_areatouchex");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_admob_bn_areatouchex", v.StringValue);
                if (AdsHelper.Instance != null && AdsHelper.Instance.adsAdmobMy != null)
                {
                    ((AdsAdmobMy)AdsHelper.Instance.adsAdmobMy).configTouchexcludeBn();
                }
            }

#if ENABLE_ADCANVAS
#if UNITY_IOS || UNITY_IPHONE
            string keyadcanvas = "adcanvas_df_v1_ios";
#else
            string keyadcanvas = "adcanvas_df_v1_android";
#endif
            if (AdCanvasHelper.Instance != null)
            {
                v = FirebaseRemoteConfig.DefaultInstance.GetValue(keyadcanvas);
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    var adcanvasdefault = (IDictionary<string, object>)JsonDecoder.DecodeText(v.StringValue);
                    if (adcanvasdefault != null || adcanvasdefault.Count > 0)
                    {
                        if (adcanvasdefault.ContainsKey("6x5"))
                        {
                            List<object> listgameAdcanvas = (List<object>)adcanvasdefault["6x5"];
                            string dataadcanvasdf = "";
                            for (int kk = 0; kk < listgameAdcanvas.Count; kk++)
                            {
                                var adcanvasgame = (IDictionary<string, object>)listgameAdcanvas[kk];
                                string img = (string)adcanvasgame["img"];
                                string link = (string)adcanvasgame["link"];
                                if (dataadcanvasdf.Length > 0)
                                {
                                    dataadcanvasdf += "#";
                                }
                                dataadcanvasdf += img;
                                if (link.Length > 5)
                                {
                                    dataadcanvasdf += ";" + link;
                                }
                            }
                            Debug.Log($"mysdk: fir adcanvas get cf5=" + dataadcanvasdf);
                            PlayerPrefs.SetString("adcanvas_v1_6x5", dataadcanvasdf);
                        }
                        AdCanvasHelper.Instance.configDefault();
                    }
                }
#if UNITY_IOS || UNITY_IPHONE
                keyadcanvas = "adcanvas_cf_list_show_ios";
#else
                keyadcanvas = "adcanvas_cf_list_show_and";
#endif
                v = FirebaseRemoteConfig.DefaultInstance.GetValue(keyadcanvas);
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    PlayerPrefs.SetString("cf_list_adcanvas", v.StringValue);
                    AdCanvasHelper.Instance.initListAdcanvas(v.StringValue);
                }
#if UNITY_IOS || UNITY_IPHONE
                keyadcanvas = "adcanvas_cf_listvideo_ios";
#else
                keyadcanvas = "adcanvas_cf_listvideo_and";
#endif
                v = FirebaseRemoteConfig.DefaultInstance.GetValue(keyadcanvas);
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    PlayerPrefs.SetString("cf_adcanvas_video", v.StringValue);
                    //AdCanvasHelper.Instance.initListVideo(v.StringValue);
                }

                v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_adcanvas_loadotherfgg");
                if (v.StringValue != null && v.StringValue.Length > 0)
                {
                    PlayerPrefs.SetInt("cf_adcanvas_loadotherfgg", (int)v.LongValue);
                }
            }
#endif//ENABLE_ADCANVAS

#if ENABLE_ADAUDIO

            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_list_adaudio");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                PlayerPrefs.SetString("cf_list_adaudio", v.StringValue);
                AdAudioHelper.Instance.initListAdAu(v.StringValue);
            }
#endif//ENABLE_ADAUDIO

#endif
        }
    }
}