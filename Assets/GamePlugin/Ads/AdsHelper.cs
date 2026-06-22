using System;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_ADJUST
#endif
using UnityEngine;
using UnityEngine.UI;

namespace mygame.sdk
{
    public delegate void AdCallBack(AD_State state);
    public delegate void AdBnCallBack(AD_State state, int w, int h);

    public enum AD_State
    {
        AD_NONE,
        AD_LOAD_FAIL,
        AD_LOAD_OK,
        AD_LOAD_OK_WAIT,
        AD_SHOW_FAIL,
        AD_SHOW,
        AD_SHOW_MISS_CB,
        AD_REWARD_FAIL,
        AD_REWARD_OK,
        AD_CLOSE,
        AD_LOAD_FROM_EXIST,
        AD_SHOW_FAIL2,
        AD_SHOW2,
        AD_CLOSE2,
    }

    public enum AD_TYPE
    {
        TYPE_ADMODE,
        TYPE_FB,
        TYPE_IRON,
        TYPE_UNITY,
        TYPE_START
    }

    public enum AD_BANNER_POS
    {
        TOP = 0,
        BOTTOM,
        CENTER,
        TOPDOWN
    }

    public enum ADJUST_ADS_event
    {
        Event_ShowFUll = 0,
        Event_ShowGift
    }

    public class AdsHelper : MonoBehaviour
    {
        public static event Action<int, Rect> CBGetHighBanner = null;
        public static event Action<int> CBShowFullGift = null;
        public static event Action<string, AD_State> CB4BnCl = null;
        //-------------AppOpenAd-------------
        [Header("App Open Ad")]
        public string AdmobAppID4Android;
        public string AdmobAppID4iOS;
        public string OpenAdIdAndroid;
        public string OpenAdIdiOS;
        //----------------------------
        private int LowPoint4LogVipAds = 15;
        private int StepPoint4LogVipAds = 5;
        public static AdsHelper Instance;
#if UNITY_EDITOR
        private AdCallBack _cbFullEditor;
        private AdCallBack _cbGiftEditor;
#endif
        [Header("  ")]
        public AdsBase adsAdmob;
        public AdsBase adsAdmobMy;
        public AdsBase adsAdmobLower;
        public AdsBase adsAdmobMyLower;
        public AdsBase adsFb;
        public AdsBase adsIron;
        public AdsBase adsApplovinMax;
        public AdsBase adsApplovinMaxMy;
        public AdsBase adsApplovinMaxLow;
        public AdsBase adsMyTarget;
        public AdsBase adsMyYandex;
        public AdsBase adsToponMe;

        private List<AdsBase> listStepShowNative;

        private Dictionary<int, AdsBase> listAds = new Dictionary<int, AdsBase>();
        public ObjectAdsCf currConfig;

        private Dictionary<string, AdCfPlacement> listAdCf = new Dictionary<string, AdCfPlacement>();

        [SerializeField] Image ImgBgBanner;
        [SerializeField] GameObject bgFullGift;
        public GameObject obWaitAd;
        [HideInInspector] public int useNativeCollapse = 1;
        public bool isShowBanner { get; private set; }
        [HideInInspector] public int statusShowBannerAfterCloseNtCl = 0;
        [HideInInspector] public bool isPreBannerWhenCloseNtCl = false;
        [HideInInspector] public int ntclCountShowing = 0;
        [HideInInspector] public int isNtclCloseWhenClick = 0;
        private bool bnIsCl = false;
        [HideInInspector] public int bnCurrShow = -1;
        private int bnNmFlagCl = 0;
        [HideInInspector] public int bnMaxH = -2;
        [HideInInspector] public int rectMaxH = -2;
        [HideInInspector] public AD_BANNER_POS bnPos;
        [HideInInspector] private AD_BANNER_POS bnCollapsePos;
        [HideInInspector] private AD_BANNER_POS bnRectPos;
        private int idxBNLoad;
        private int stepBNLoad;
        private int countBNLoad;
        private int idxBNShowCircle;
        [HideInInspector] public int typeBn;
        [HideInInspector] public int typeBnShowing;
        private bool isBannerLoading = false;
        public App_Open_ad_Orien bnOrien { get; private set; }
        private bool isCallReloadBanner = false;
        private int statusLoadBanner = 0;
        private bool isLoadBannerok = false;
        private long tbnLoadFail = 0;
        private long tbnLoadOk = 0;
        private int bnWidth;
        private int bnClWidth;
        private float bnRectWidth;
        private float bnDxCenter;
        private float bnClDxCenter;
        private float bnRectDxCenter;
        private float bnRectDyVertical;
        private AdCallBack _cbBNLoad;
        private AdCallBack _cbBNShow;
        private List<AdsBase> listStepShowBNCircle;
        private List<AdsBase> listStepShowBNRe;
        private string imgBanner = "";
        private string actionBanner = "";
        [HideInInspector] public string memPlacementBn = "";
        [HideInInspector] public string memPlacementCl = "";
        [HideInInspector] public string memPlacementRect = "";
        [HideInInspector] public bool isBannerClExpand = false;
        private float bnIsWaitSessionAndStart = -1;
        private int deltaloverCountCl = 100;
        [HideInInspector] public int stateMybanner = 0;
        private int stepShowMyAdOpen = 10000;

        private bool isShowNatie = false;
        public bool isShowBNAdsMob = false;
        //-------------full------------------
        private long tFullShow = 0;
        private long tFullImgShow = 0;
        private int deltaloverCountFull = 100;
        private int countFullShowOfDay;
        private int idxFullLoad;
        private int stepFullLoad;
        private int countFullLoad;
        private int idxFullShowCircle;
        private int idxFullShowCircle4Skip = -1;
        public bool isFullLoadStart = false;
        public bool isFullLoadWhenClose = false;
        private bool isFullCallMissCB = false;
        private int fullMemLv4CountLose = -1;
        private int fullStoreCountLose = 0;
        public int level4ApplovinFull { get; private set; }
        private AdCallBack _cbFullLoad;
        private AdCallBack _cbFullShow;

        private AdCallBack _cbFull2Show;

        private long tFull2Show = 0;
        private int deltaloverCountFull2 = 100;
        private int idxFull2Load;
        private int countFull2Load;
        private int stepFull2Load;
        private int idxFull2ShowCircle;
        private int lvFullStepStart = -1;
        private int lvFullStepEnd = -1;
        public bool isShowFulled { get; private set; }
        private int statusAds2 = 0;
        private AD_State ad2MemStatusShow;

        [HideInInspector] public int FbntFullECPMCurr = 0;
        [HideInInspector] private float FbntFullECPMAverage = 0;
        [HideInInspector] private int FbntFullECPMCountAverage = 0;
        [HideInInspector] public float FbntFullECPMdefault = 0.8f;

        [HideInInspector] public int fullRwNumTotal = 0;
        [HideInInspector] public int fullRwNumSession = 0;
        [HideInInspector] public long fullRwTimeShow = 0;
        private bool fullRwFlagCount = false;

        public Dictionary<string, ObjectAdsCf> dicRegionWait4Campain = null;

        private Dictionary<string, long> dicforTimeLoad = new Dictionary<string, long>();
        private Dictionary<string, long> dicforTimeShow = new Dictionary<string, long>();

        //-------------gift----------------------------
        private bool giftIsloadWhenErr = false;
        private long tGiftShow = 0;
        private int countGiftShowOfDay;
        private int idxGiftLoad;
        private int stepGiftLoad;
        private int countGiftLoad;
        private int idxGiftShowCircle;
        public bool isGiftLoadStart = false;
        public bool isGiftLoadWhenClose = false;
        private bool isGiftCallMissCB = false;
        private AdCallBack _cbGiftLoad;
        private AdCallBack _cbGiftShow;

        public int count4AdShowing { get; private set; }
        private bool isWaitSetAdShowing = false;
        [HideInInspector] public long tShowAdsCheckContinue = 0;
        private bool isPauseAudio = false;
        [HideInInspector] public int statuschekAdsFullOpenErr = 0;
        [HideInInspector] public int statuschekAdsFullCloseErr = 0;
        [HideInInspector] public int statuschekAdsFull2OpenErr = 0;
        [HideInInspector] public int statuschekAdsFull2CloseErr = 0;
        [HideInInspector] public int statuschekAdsGiftErr = 0;
        public int level4ApplovinGift { get; private set; }
        public int statusLogicIron { get; set; }

        public int levelCurr4Full { get; private set; }
        public int levelCurr4Gift { get; private set; }
        public int cfStatusRemoveAdsInterval { get; set; }
        private bool isinitCall = false;

        private static int countTotalShowAds = 0;
        private int typeFullGift = 0;
        private bool isInitSuc = false;
        private bool isWaitShowBanner = false;
        private bool isWaitShowCollapseBanner = false;
        private bool isWaitShowRectBanner = false;

        private int countTryCheckRemoveInterval = 0;

        private int specialTypeCurr = 0;
        private int specialStartCurr = 0;
        private int specialEndCurr = 0;
        private int fullDeltalTimeCurr = 0;
        private bool isApplySpec = false;

        [HideInInspector] public int isNewlogFirtOpen = AppConfig.open_newlogic;
        [HideInInspector] public int isApplyLogicSkip = 3;
        [HideInInspector] public int islogttttttt = 0;

        string nameShowFullGift = "";
        int StatusShowiso = 0;
        int typeNtFull2 = 0;
        int flagTimeRmAd = 0;
        int timeRmAd = 0;
        float time4CheckTimeRm = 0;
        public int isNtfullNewId = 0;

        private short currTypeFullGiftShow = -10000;

        private bool isCheckLoadFullStart = false;
        private bool isFirstShowGift = true;

        public bool isQcThu = false;
        private byte flagInitSuc = 0;

#if UNITY_EDITOR
        [Header("Ad Editor")]
        public GameObject adsEditorPrefab;
        public AdsEditorCtr adsEditorCtr;
#endif

        public static bool isRemoveAds(int flagCheckGift)
        {
            int flag = PlayerPrefs.GetInt("key_mem_rm_ads", 0);
            if (Instance != null)
            {
                if ((flag & 8) > 0)
                {
                    if (Instance.flagTimeRmAd <= 0)
                    {
                        Instance.flagTimeRmAd = PlayerPrefs.GetInt("key_mem_rm_timebuy_ads", 0);
                        Instance.timeRmAd = PlayerPrefs.GetInt("key_mem_rm_time_ads", 0) * 3600;
                    }
                }
                else
                {
                    Instance.flagTimeRmAd = 0;
                    Instance.timeRmAd = 0;
                }
            }
            if (flagCheckGift == 1)
            {
                int r = (flag & 6);
                return (r > 0);
            }
            else
            {
                return (flag > 0);
            }
        }
        public static long timePurchaseRemoveAdWithTime()
        {
            int flag = PlayerPrefs.GetInt("key_mem_rm_ads", 0);
            if (flag > 0)
            {
                int tp = PlayerPrefs.GetInt("key_mem_rm_timebuy_ads", 0);
                return (tp * 1000);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// flag - bit0-rm banner+full,  bit1-all, bit2-is sub?, bit4-remove bn+full with time
        /// </summary>
        /// <param name="flag"></param>
        public static void setRemoveAds(int flag, int timeRemoveHours = 0, int timeBuy = 0)
        {
            SdkUtil.logd($"ads helper rmads setRemoveAds flag={flag}-time={timeRemoveHours}-timeBuy={timeBuy}");
            if (flag > 0)
            {
                int curflag = PlayerPrefs.GetInt("key_mem_rm_ads", 0);
                if (timeRemoveHours > 0)
                {
                    flag = 8;
                    int tpu = timeBuy;
                    if (tpu < 10000)
                    {
                        tpu = (int)(GameHelper.CurrentTimeMilisReal() / 1000);
                    }
                    //tpu -= 86290;//test
                    if (Instance != null)
                    {
                        Instance.flagTimeRmAd = tpu;
                        Instance.timeRmAd = timeRemoveHours * 3600;
                    }

                    PlayerPrefs.SetInt("key_mem_rm_time_ads", timeRemoveHours);
                    PlayerPrefs.SetInt("key_mem_rm_timebuy_ads", tpu);
                }
                int newflag = (curflag | flag);
                PlayerPrefs.SetInt("key_mem_rm_ads", newflag);
                PlayerPrefs.Save();
                if (Instance != null)
                {
                    Instance.hideBanner(0);
                    Instance.hideBanner(1);
                    Instance.hideBannerCollapse();
                    Instance.hideBannerRect();
                }
            }
        }

        void ResetAdsTime()
        {
            SdkUtil.logd($"ads helper rmads ResetAdsTime");
            int curflag = PlayerPrefs.GetInt("key_mem_rm_ads", 0);
            int newflag = (curflag & 7);
            flagTimeRmAd = 0;
            timeRmAd = 0;
            PlayerPrefs.SetInt("key_mem_rm_timebuy_ads", 0);
            PlayerPrefs.SetInt("key_mem_rm_time_ads", 0);
            PlayerPrefs.SetInt("key_mem_rm_ads", newflag);
            PlayerPrefs.Save();
        }

        public static void resetAdsSub()
        {
            SdkUtil.logd($"ads helper rmads resetAdsSub");
            int curflag = PlayerPrefs.GetInt("key_mem_rm_ads", 0);
            int newflag = (curflag & 11);
            PlayerPrefs.SetInt("key_mem_rm_ads", newflag);
            PlayerPrefs.Save();
        }

        public void parserConfigAdsPlacement(string data)
        {
            try
            {
                var dictmp = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(data);
                foreach (KeyValuePair<string, object> item in dictmp)
                {
                    var itemCf = (IDictionary<string, object>)item.Value;
                    string[] arrkeys = item.Key.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    AdCfPlacement memitem = null;
                    foreach (string ikey in arrkeys)
                    {
                        AdCfPlacement clcf;
                        if (listAdCf.ContainsKey(ikey))
                        {
                            clcf = listAdCf[ikey];
                        }
                        else
                        {
                            if (memitem == null)
                            {
                                clcf = new AdCfPlacement();
                            }
                            else
                            {
                                clcf = new AdCfPlacement(memitem);
                            }
                            listAdCf.Add(ikey, clcf);
                        }
                        if (memitem == null)
                        {
                            foreach (KeyValuePair<string, object> plcf in itemCf)
                            {
                                if (plcf.Key.CompareTo("flag") == 0)
                                {
                                    clcf.flagShow = Convert.ToInt32(plcf.Value);
                                }
                                else if (plcf.Key.CompareTo("lv") == 0)
                                {
                                    clcf.lvStart = Convert.ToInt32(plcf.Value);
                                }
                                else if (plcf.Key.CompareTo("num") == 0)
                                {
                                    clcf.maxShow = Convert.ToInt32(plcf.Value);
                                }
                                else if (plcf.Key.CompareTo("del_time") == 0)
                                {
                                    clcf.delTime = Convert.ToInt32(plcf.Value);
                                }
                                else if (plcf.Key.CompareTo("over") == 0)
                                {
                                    clcf.numOver = Convert.ToInt32(plcf.Value);
                                }
                                else if (plcf.Key.CompareTo("apply_interval") == 0)
                                {
                                    clcf.apply_interval = Convert.ToInt32(plcf.Value);
                                }
                                else if (plcf.Key.CompareTo("type_ad") == 0)
                                {
                                    clcf.typeAd = Convert.ToInt32(plcf.Value);
                                }
                            }
                            memitem = clcf;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("mysdk: ads helper parserConfigAdsPlacement ex=" + ex.ToString());
            }
        }

        public void resetCfAdsPlacement()
        {
            foreach (var cf in listAdCf)
            {
                cf.Value.countshow = 0;
            }
        }

        public AdCfPlacement getCfAdsPlacement(string pl, int dfValue)
        {
            if (listAdCf.ContainsKey(pl))
            {
                return listAdCf[pl];
            }
            else
            {
                if (dfValue >= 0)
                {
                    var clcf = new AdCfPlacement();
                    clcf.flagShow = dfValue;
                    listAdCf.Add(pl, clcf);
                    return clcf;
                }
                else
                {
                    return null;
                }
            }
        }

        public void checkCampain(bool isCheck)
        {
            if (!isCheck)
            {
                Debug.Log($"mysdk: ads full helper checkCampain not check");
                if (dicRegionWait4Campain != null)
                {
                    dicRegionWait4Campain.Clear();
                    dicRegionWait4Campain = null;
                }
            }
            checkWaitCampain(dicRegionWait4Campain);
            dicRegionWait4Campain = null;
        }

        private void checkWaitCampain(Dictionary<string, ObjectAdsCf> dicWait4Campain)
        {
            if (dicWait4Campain != null && dicWait4Campain.Count > 0)
            {
                string txtlog = "region";
                SdkUtil.logd($"ads {txtlog} helper checkCampain");
                if (SDKManager.Instance.mediaCampain != null && SDKManager.Instance.mediaCampain.Length > 3)
                {
                    string lowmcam = SDKManager.Instance.mediaCampain.ToLower();
                    SdkUtil.logd($"ads {txtlog} helper checkCampain={lowmcam}");
                    foreach (KeyValuePair<string, ObjectAdsCf> item in dicWait4Campain)
                    {
                        string[] arrik = item.Key.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int it = 0; it < arrik.Length; it++)
                        {
                            string[] arrPks = arrik[it].ToLower().Split('_');

                            SdkUtil.logd($"ads {txtlog} helper checkCampain key={arrik[it]} cap={arrPks[0]}");
                            if (arrPks != null && arrPks.Length > 0)
                            {
                                string camCf = arrPks[0];
                                if (arrPks.Length > 1 && arrPks[1] != null && arrPks[1].CompareTo("any") != 0)
                                {
                                    camCf += $"_{arrPks[1]}";
                                }
                                if (lowmcam.StartsWith(camCf + "_") || camCf.CompareTo("cany") == 0)
                                {
                                    if (arrPks.Length > 2 && arrPks[2] != null && arrPks[2].Length > 1)
                                    {
                                        Debug.Log($"mysdk: ads {txtlog} helper checkCampain key={arrik[it]} source={arrPks[2]}");
                                        if (AppsFlyerHelperScript.Instance.checkSameSource(arrPks[2]))
                                        {
                                            Debug.Log($"mysdk: ads {txtlog} helper checkCampain2={SDKManager.Instance.mediaCampain}");
                                            currConfig.mediaCampain = SDKManager.Instance.mediaCampain.ToLower();
                                            currConfig.coppyFromOther(item.Value);
                                            currConfig.saveAllConfig();
                                            initListBN();
                                            initListFull();
                                            initListGift();
                                            break;
                                        }
                                        else
                                        {
                                            string[] camarr = lowmcam.Split('_');
                                            if (camarr != null && camarr.Length > 2 && camarr[2] != null && camarr[2].Length > 1)
                                            {
                                                if (arrPks[2].CompareTo(camarr[2]) == 0)
                                                {
                                                    Debug.Log($"mysdk: ads {txtlog} helper checkCampain3={SDKManager.Instance.mediaCampain}");
                                                    currConfig.mediaCampain = SDKManager.Instance.mediaCampain.ToLower();
                                                    currConfig.coppyFromOther(item.Value);
                                                    currConfig.saveAllConfig();
                                                    initListBN();
                                                    initListFull();
                                                    initListGift();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log($"mysdk: ads {txtlog} helper checkCampain1={SDKManager.Instance.mediaCampain}");
                                        currConfig.mediaCampain = SDKManager.Instance.mediaCampain.ToLower();
                                        currConfig.coppyFromOther(item.Value);
                                        currConfig.saveAllConfig();
                                        initListBN();
                                        initListFull();
                                        initListGift();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                dicWait4Campain.Clear();
            }
        }

        public void setEcpmNtFull4Fb(long microValue)
        {
            FbntFullECPMCurr = (int)microValue;
            PlayerPrefs.SetInt("fbntfull_ecpm_curr", FbntFullECPMCurr);

            FbntFullECPMAverage += ((float)microValue / 1000000.0f);
            FbntFullECPMCountAverage++;
            PlayerPrefs.GetFloat("fbntfull_ecpm_average", FbntFullECPMAverage);
            PlayerPrefs.GetInt("fbntfull_ecpm_countaverage", FbntFullECPMCountAverage);
        }

        public float getFbNtFullEcpmAverage()
        {
            if (FbntFullECPMAverage > 0 && FbntFullECPMCountAverage > 0)
            {
                return FbntFullECPMAverage / FbntFullECPMCountAverage;
            }
            else
            {
                return FbntFullECPMdefault / 1000.0f;
            }
        }

        public static void onAdLoad(string placement, string adFormat, string adUnitId, string mediation)
        {
            Debug.Log($"mysdk: ads onAdLoad adformat={adFormat} pl={placement} adplatform={mediation} id={adUnitId}");
            string key = $"{adFormat}_{mediation}_{adUnitId}";
            if (!Instance.dicforTimeLoad.ContainsKey(key))
            {
                Instance.dicforTimeLoad.Add(key, SdkUtil.CurrentTimeMilis());
            }
            else
            {
                Debug.Log($"mysdk: ads onAdLoad key={key} errrrrrrrrrrrr!!!!!");
            }
        }

        public static void onAdLoadResult(string placement, string adFormat, string adUnitId, string mediation, string adNetwork, bool loadResult)
        {
            string key = $"{adFormat}_{mediation}_{adUnitId}";
            long tload = 0;
            if (Instance.dicforTimeLoad.ContainsKey(key))
            {
                tload = SdkUtil.CurrentTimeMilis() - Instance.dicforTimeLoad[key];
                Instance.dicforTimeLoad.Remove(key);
            }
            else
            {
                Debug.Log($"mysdk: ads onAdLoadResult key={key} errrrrrrrrrrrr!!!!!");
            }
            if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
            {
                int ntfullclient = 0;
                int iscallsplash = 0;
                string loadrs = "t";
                if (!loadResult)
                {
                    loadrs = "f";
                }
                if (AdIdsConfig.AdmobPlNtFull.Contains(adUnitId) || AdIdsConfig.AdmobPlNtIcFull.Contains(adUnitId))
                {
                    ntfullclient = 1;
                }
                if (SDKManager.Instance.isCallShowSplash)
                {
                    iscallsplash = 1;
                }
                FIRhelper.logEvent($"splash_load_{adFormat}_{ntfullclient}_{Instance.isNtfullNewId}_{iscallsplash}_{loadrs}");
                Debug.Log($"mysdk: ads splash load: {adFormat}_{ntfullclient}_{Instance.isNtfullNewId}_{iscallsplash}_{loadrs}");
            }
            LogEventManager.Instance.LogAdLoad(adFormat, mediation, adNetwork, placement, loadResult, tload / 1000f);
            Debug.Log($"mysdk: ads onAdLoadResult adformat={adFormat} pl={placement} adplatform={mediation} net={adNetwork} id={adUnitId} loadResult={loadResult} t={tload}");
        }

        public static void onAdImpression(string placement, string adUnitId, string adFormat, string mediation, string adNetwork, float revalue, long originMicro = -1)
        {
            LogEventManager.Instance.LogAdsRevenue(adFormat, placement, adNetwork, mediation, revalue);
            if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
            {
                int ntfullclient = 0;
                if (AdIdsConfig.AdmobPlNtFull.Contains(adUnitId) || AdIdsConfig.AdmobPlNtIcFull.Contains(adUnitId))
                {
                    ntfullclient = 1;
                }
                FIRhelper.logEvent($"splash_imp_{adFormat}_{ntfullclient}");
                Debug.Log($"mysdk: ads splash impl: {adFormat}_{ntfullclient}");
            }
            Debug.Log($"mysdk: ads onAdImpression adformat={adFormat} pl={placement} adplatform={mediation} net={adNetwork} id={adUnitId} va={revalue} originva={originMicro}");
        }

        public static void onAdClick(string placement, string adFormat, string mediation, string adNetwork, string adUnitId)
        {
            LogEventManager.Instance.LogAdClick(adFormat, mediation, adNetwork, placement);
            Debug.Log($"mysdk: ads onAdClickAd adformat={adFormat} pl={placement} adplatform={mediation} net={adNetwork} id={adUnitId}");
        }

        public static void onAdShowStart(string placement, string adFormat, string mediation, string adUnitId)
        {
            Debug.Log($"mysdk: ads onAdShowStart adformat={adFormat} pl={placement} adplatform={mediation} id={adUnitId}");
            string key = $"{placement}_{adFormat}_{mediation}";
            if (!Instance.dicforTimeShow.ContainsKey(key))
            {
                Instance.dicforTimeShow.Add(key, SdkUtil.CurrentTimeMilis());
            }
            else
            {
                Debug.Log($"mysdk: ads onAdShowStart key={key} errrrrrrrrrrrr!!!!!");
            }
        }

        public static void onAdShowEnd(string placement, string adFormat, string mediation, string adNetwork, string adUnitId, bool isShow, string resion)
        {
            string key = $"{placement}_{adFormat}_{mediation}";
            long tshow = 0;
            if (Instance.dicforTimeShow.ContainsKey(key))
            {
                tshow = SdkUtil.CurrentTimeMilis() - Instance.dicforTimeShow[key];
                Instance.dicforTimeShow.Remove(key);
            }
            else
            {
                Debug.Log($"mysdk: ads onAdShowEnd key={key} errrrrrrrrrrrr!!!!!");
            }
            LogEventManager.Instance.LogAdsShow(placement, adFormat, mediation, adNetwork, isShow, resion, tshow / 1000f);
            Debug.Log($"mysdk: ads onAdShowEnd adformat={adFormat} pl={placement} adplatform={mediation} net={adNetwork} id={adUnitId}-is={isShow} r={resion}, tshow={tshow}");
            if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
            {
                int ntfullclient = 0;
                if (AdIdsConfig.AdmobPlNtFull.Contains(adUnitId) || AdIdsConfig.AdmobPlNtIcFull.Contains(adUnitId))
                {
                    ntfullclient = 1;
                }
                FIRhelper.logEvent($"splash_showend_{adFormat}_{ntfullclient}_{Instance.isNtfullNewId}");
                Debug.Log($"mysdk: ads splash showend: {adFormat}_{ntfullclient}_{Instance.isNtfullNewId}");
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

#if USE_ADSMOB_MY
                if (adsAdmob != null)
                {
                    listAds.Add(0, adsAdmobMy);
                }
#else
                if (adsAdmob != null)
                {
                    listAds.Add(0, adsAdmob);
                }
#endif
                listAds.Add(1, adsFb);

#if USE_ADSMOB_MY
                if (adsAdmobMyLower != null)
                {
                    listAds.Add(10, adsAdmobMyLower);
                }
#else
                if (adsAdmobLower != null)
                {
                    listAds.Add(10, adsAdmobLower);
                }
#endif
                if (adsIron != null)
                {
                    listAds.Add(3, adsIron);
                }
                if (adsApplovinMax != null)
                {
                    listAds.Add(6, adsApplovinMax);
                }
                if (adsApplovinMaxMy != null)
                {
                    listAds.Add(60, adsApplovinMaxMy);
                }
                if (adsApplovinMaxLow != null)
                {
                    listAds.Add(11, adsApplovinMaxLow);
                }
                if (adsMyTarget != null)
                {
                    listAds.Add(8, adsMyTarget);
                }
                if (adsMyYandex != null)
                {
                    listAds.Add(9, adsMyYandex);
                }
#if USE_TOPON_MY
                if (adsToponMy != null)
                {
                    listAds.Add(12, adsToponMy);
                }
#else
                if (adsToponMe != null)
                {
                    listAds.Add(12, adsToponMe);
                }
#endif

                cfStatusRemoveAdsInterval = 1;

                int nop = PlayerPrefs.GetInt("key_count_open_game", 0);
                PlayerPrefs.SetInt("key_count_open_game", nop++);
                countTotalShowAds = PlayerPrefs.GetInt("mem_count_to_show", 0);
                useNativeCollapse = PlayerPrefs.GetInt("cf_use_native_cl", 1);

                isNtclCloseWhenClick = PlayerPrefs.GetInt("cf_ntcl_clos_w_click", 0);
                fullRwNumTotal = PlayerPrefs.GetInt("mem_num_fullrw", 0);
                fullRwTimeShow = PlayerPrefs.GetInt("mem_fullrw_tshow", 0) * 1000;

                FbntFullECPMCurr = PlayerPrefs.GetInt("fbntfull_ecpm_curr", 0);
                FbntFullECPMAverage = PlayerPrefs.GetInt("fbntfull_ecpm_average", 0);
                FbntFullECPMCountAverage = PlayerPrefs.GetInt("fbntfull_ecpm_countaverage", 0);
                isApplyLogicSkip = PlayerPrefs.GetInt("cf_logic_skip", 3);
                islogttttttt = PlayerPrefs.GetInt("mem_is_log_ttt", 0);

                currConfig = new ObjectAdsCf();
                currConfig.loadFromPlayerPrefs();
                checkUseAds();

                typeBnShowing = -1;
                count4AdShowing = 0;
                isShowBNAdsMob = false;
                setIsAdsShowing(false);
                isShowFulled = false;
                isInitSuc = false;
                isWaitShowBanner = false;
                ntclCountShowing = 0;
                isFirstShowGift = true;

                listStepShowBNCircle = new List<AdsBase>();
                listStepShowBNRe = new List<AdsBase>();
                listStepShowNative = new List<AdsBase>();

                countFullShowOfDay = currConfig.fullTotalOfday;
                countGiftShowOfDay = currConfig.giftTotalOfday;
            }
            else
            {

            }
        }

        private void Start()
        {
#if UNITY_EDITOR
            var ob = Instantiate(adsEditorPrefab, Vector3.zero, Quaternion.identity, SDKManager.Instance.transform);
            adsEditorCtr = ob.GetComponent<AdsEditorCtr>();
#endif
            idxBNLoad = 0;
            stepBNLoad = 0;
            countBNLoad = 0;
            idxBNShowCircle = 0;
            typeBnShowing = -1;
            typeBn = -1;

            idxFullLoad = 0;
            countFullLoad = 0;
            stepFullLoad = 0;
            idxFullShowCircle = 0;

            idxFull2Load = 0;
            idxFull2ShowCircle = 0;

            idxGiftLoad = 0;
            countGiftLoad = 0;
            stepGiftLoad = 0;
            idxGiftShowCircle = 0;
            currConfig.checkDayActive();

            checkTotalShowGiftFull();

            checkSpecialCon(true);
            initListBN();
            initListNative();
            initListFull();
            initListFull2();
            initListGift();


#if ENABLE_ADS_IRON
            statusLogicIron = PlayerPrefs.GetInt("mem_cf_login_iron", 0);
            if (!AppsFlyerHelperScript.Instance.checkSameSource("ir"))
            {
                statusLogicIron = 0;
            }
#else
            statusLogicIron = 0;
#endif

            checkShowCMP();
            StartCoroutine(waitInitAds());
            countTryCheckRemoveInterval = 0;
            checkRemoveAdsInterval();
            int cfcaer = PlayerPrefs.GetInt("cf_check_ad_err", 0);
            GameHelper.cfCheckAdErr(cfcaer == 1);
            FIRhelper.logEvent("splash_session");

            isCheckLoadFullStart = false;
            if (currConfig.fullTimeStart >= 30 && SDKManager.Instance.timeEnterGame < (currConfig.fullTimeStart - 30))
            {
                isCheckLoadFullStart = true;
            }
            GameHelper.Instance.configAppOpenAd(currConfig.OpenAdTimeBg);
        }

        private void Update()
        {
            if (flagTimeRmAd > 0 && timeRmAd > 0)
            {
                time4CheckTimeRm += Time.deltaTime * Time.timeScale;
                if (time4CheckTimeRm >= 5)
                {
                    time4CheckTimeRm = 0;
                    long tcurr = GameHelper.CurrentTimeMilisReal() / 1000;
                    if ((tcurr - flagTimeRmAd) >= timeRmAd)
                    {
                        ResetAdsTime();
                    }
                }
            }
            if (isCheckLoadFullStart)
            {
                if (SDKManager.Instance.timeEnterGame >= (currConfig.fullTimeStart - 30))
                {
                    Debug.Log($"mysdk: ads helper load full when fullTimeStart={currConfig.fullTimeStart}");
                    isCheckLoadFullStart = false;
                    loadFull4ThisTurn(AdsBase.PLFullDefault, false, GameRes.GetLevel(Level_type.Common), 0, false);
                }
            }
            if (bnIsWaitSessionAndStart >= 0)
            {
                bnIsWaitSessionAndStart += Time.deltaTime * Time.timeScale;
                if (bnIsWaitSessionAndStart >= 5)
                {
                    if (currConfig.bnSessionShow <= SDKManager.Instance.counSessionGame && currConfig.bnTimeStartShow <= SDKManager.Instance.timeEnterGame)
                    {
                        bnIsWaitSessionAndStart = -1;
                        showBanner(memPlacementBn, bnPos, bnOrien, bnNmFlagCl, bnWidth, bnMaxH, bnDxCenter);
                    }
                    else
                    {
                        bnIsWaitSessionAndStart = 0;
                    }
                }
            }
        }

        void checkRemoveAdsInterval()
        {
            int tinval = PlayerPrefs.GetInt("ads_remove_inval", 0);
            if (tinval > 0)
            {
                countTryCheckRemoveInterval++;
                Myapi.ApiManager.Instance.getTimeOnline((status, time) =>
                {
                    if (status)
                    {
                        SDKManager.Instance.timeOnline = (int)(time / 60000);
                        SDKManager.Instance.timeWhenGetOnline = (int)(GameHelper.CurrentTimeMilisReal() / 60000);
                    }
                    else
                    {
                        if (countTryCheckRemoveInterval <= 3)
                        {
                            Invoke("checkRemoveAdsInterval", 60);
                        }
                    }
                });
            }
        }

        public bool isRemoveAdsInterval()
        {
            int tinval = PlayerPrefs.GetInt("ads_remove_inval", 0);
            if (tinval > 0 && SDKManager.Instance.timeOnline > 0 && cfStatusRemoveAdsInterval == 1)
            {
                long t = GameHelper.CurrentTimeMilisReal() / 60000;
                if (t > SDKManager.Instance.timeWhenGetOnline)
                {
                    long tbg = PlayerPrefs.GetInt("ads_remove_inval_tbegin", 0);
                    long dt = tinval + tbg - SDKManager.Instance.timeOnline;
                    if (dt > 0)
                    {
                        if ((t - SDKManager.Instance.timeWhenGetOnline) < dt)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void checkShowCMP()
        {
            Debug.Log($"mysdk: sdkmanager ShowCMP");
#if !UNITY_EDITOR
            mygame.sdk.GameHelper.showCMP();
#endif//UNITY_EDITOR
        }

        public void onFinishCmpIDFA()
        {
            Debug.Log($"mysdk: sdkmanager onFinishCmpIDFA");
            initAds();
        }

#if UNITY_IOS || UNITY_IPHONE
        public void initAds()
#else
        private void initAds()
#endif
        {
            if (!isinitCall)
            {
                isinitCall = true;
                flagInitSuc = 0;
                Debug.Log("mysdk: ads helper initAds");
                foreach (var item in listAds)
                {
                    item.Value.InitAds();
                }
                StartCoroutine(waitFlagInitSuc());
            }
            else
            {
                SdkUtil.logd($"ads helper initAds re call");
            }
        }

        IEnumerator waitInitAds()
        {
            yield return new WaitForSeconds(10);
            Debug.Log("mysdk: ads helper waitInitAds");
            initAds();
        }

        public void onAdsInitSuc(int adType)
        {
            SdkUtil.logd($"ads helper onAdsInitSuc wbn={isWaitShowBanner}, wbncl={isWaitShowCollapseBanner}, wrect={isWaitShowRectBanner} adType={adType}");
            if (adType == 0 || adType == 3 || adType == 6)
            {
                int toaduse = 1;
#if ENABLE_ADS_MAX
                toaduse++;
#endif
#if ENABLE_ADS_IRON
                toaduse++;
#endif
                flagInitSuc++;
                if (flagInitSuc >= toaduse)
                {
                    adType = -100;
                }
            }

            if (adType == -100)
            {
                isInitSuc = true;
                if (isWaitShowBanner)
                {
                    isWaitShowBanner = false;
                    showBanner(memPlacementBn, bnPos, bnOrien, bnNmFlagCl, bnWidth, bnMaxH, bnDxCenter);
                }
                if (isWaitShowCollapseBanner)
                {
                    isWaitShowCollapseBanner = false;
                    showBannerCollapse(memPlacementCl, bnCollapsePos, bnOrien, bnClWidth, bnMaxH, bnDxCenter);
                }
                if (isWaitShowRectBanner)
                {
                    isWaitShowRectBanner = false;
                    showBannerRect(memPlacementRect, bnRectPos, bnRectWidth, rectMaxH, bnRectDxCenter, bnRectDyVertical, 1);
                }

                StartCoroutine(loadStart());
            }
        }

        IEnumerator waitFlagInitSuc()
        {
            Debug.Log("mysdk: ads helper waitFlagInitSuc");
            yield return new WaitForSeconds(7);
            onAdsInitSuc(-100);
        }

        IEnumerator loadStart()
        {
            Debug.Log("mysdk: ads helper loadStart");
            yield return new WaitForSeconds(0.2f);
            if (isShowOpenAds(true) > 0)
            {
                loadFull4ThisTurn(AdsBase.PLFullSplash, true, GameRes.GetLevel(Level_type.Common), 0, false);
                Debug.Log($"mysdk: ads helper loadStart openshowtype={currConfig.OpenAdShowtype} openflag={currConfig.OpenAdFlagOpenFormat}");
                if (currConfig.OpenAdShowtype == 0 || currConfig.OpenAdShowtype == 2 || currConfig.OpenAdShowtype == 3)
                {
                    if (currConfig.OpenAdFlagOpenFormat == 0)
                    {
                        GameHelper.Instance.loadAppOpenAd();
                    }
                }
            }
            yield return new WaitForSeconds(2.1f);
            if (isFullLoadStart)
            {
                loadFull4ThisTurn(AdsBase.PLFullDefault, false, GameRes.GetLevel(Level_type.Common), 0, false);
            }
            if (isGiftLoadStart)
            {
                yield return new WaitForSeconds(6.0f);
                loadGift4ThisTurn(AdsBase.PLGiftDefault, GameRes.GetLevel(Level_type.Common), null);
            }
        }

        void showTransFullGift()
        {
            bgFullGift.SetActive(false);
            bgFullGift.transform.GetChild(0).gameObject.SetActive(false);
            AdsProcessCB.Instance().Enqueue(() =>
            {
                if (bgFullGift.activeInHierarchy)
                {
                    bgFullGift.transform.GetChild(0).gameObject.SetActive(true);
                }
            }, 1.0f);
        }

        private void checkResumeAudio()
        {
#if UNITY_IOS || UNITY_IPHONE
            if (isPauseAudio)
            {
                SdkUtil.logd($"ads helper checkResumeAudio");
                isPauseAudio = false;
                AudioListener.pause = false;
            }
#endif
        }

        public void reinit()
        {
            Debug.Log("mysdk: ads helper reinit");
            try
            {
                if (listAds != null)
                {
                    checkSpecialCon(false);
                    initListBN();
                    initListNative();
                    initListFull();
                    initListFull2();
                    initListGift();
                }
            }
            catch (Exception)
            {

            }
        }

        public bool checkSpecialCon(bool isForceNew)
        {
            if (isForceNew)
            {
                specialStartCurr = 0;
                specialEndCurr = 0;
            }
            bool re = false;
            specialTypeCurr = currConfig.specialType;
            fullDeltalTimeCurr = currConfig.fullDeltatime;
            bool memisapply = isApplySpec;
            bool newapply = false;
            isApplySpec = false;
            if (currConfig.listSpecialShow.Count > 0 && SDKManager.Instance != null)
            {
                for (int i = 0; i < currConfig.listSpecialShow.Count; i++)
                {
                    SpecialConditionShow con = currConfig.listSpecialShow[i];
                    int concomp;
                    if (currConfig.specialType == 0)
                    {
                        concomp = SDKManager.Instance.counSessionGame;
                    }
                    else
                    {
                        concomp = SDKManager.Instance.totalTimePlayGame;
                    }
                    if (concomp >= con.startCon && concomp < con.endCon)
                    {
                        if (specialStartCurr != con.startCon || specialEndCurr != con.endCon)
                        {
                            newapply = true;
                        }
                        specialStartCurr = con.startCon;
                        specialEndCurr = con.endCon;
                        if (con.deltal4Show > 1)
                        {
                            fullDeltalTimeCurr = con.deltal4Show;
                        }
                        if (con.stepFull != null && con.stepFull.Length > 3)
                        {
                            if (newapply)
                            {
                                re = true;
                                currConfig.parSerStepFull(con.stepFull);
                            }
                            isApplySpec = true;
                        }
                        if (con.stepGift != null && con.stepGift.Length > 3)
                        {
                            if (newapply)
                            {
                                re = true;
                                currConfig.parSerStepGift(con.stepGift);
                            }
                            isApplySpec = true;
                        }
                        break;
                    }
                }
            }
            if (memisapply && !isApplySpec)
            {
                SdkUtil.logd($"ads helper change to common cf from spec type=" + specialTypeCurr);
                currConfig.parSerStepFull(currConfig.stepShowFull);
                currConfig.parSerStepGift(currConfig.stepShowGift);
                re = true;
            }
            else if (!memisapply && isApplySpec)
            {
                SdkUtil.logd($"ads helper change to spec cf from common type=" + specialTypeCurr);
            }
            else
            {
                SdkUtil.logd($"ads helper change spec logic isApplySpec=" + isApplySpec + ", type=" + specialTypeCurr);
            }
            return re;
        }

        public void checkChangeSpecialCon()
        {
            if (checkSpecialCon(false))
            {
                initListBN();
                initListNative();
                initListFull();
                initListFull2();
                initListGift();
            }
        }

        public void removeAdsWithTimeInterval(int timeIntervalInHour, Action<bool> result)
        {
            Myapi.ApiManager.Instance.getTimeOnline((status, time) =>
            {
                result?.Invoke(status);
                if (status)
                {
                    SDKManager.Instance.timeOnline = (int)(time / 60000);
                    SDKManager.Instance.timeWhenGetOnline = (int)(GameHelper.CurrentTimeMilisReal() / 60000);
                    PlayerPrefs.SetInt("ads_remove_inval", timeIntervalInHour * 60);
                    PlayerPrefs.SetInt("ads_remove_inval_tbegin", SDKManager.Instance.timeOnline);
                }
            });
        }

        public void setStartTimeNoAds()
        {
            long ts = GameHelper.CurrentTimeMilisReal() / 60000;
            PlayerPrefs.SetInt("time_start_noads_fi", (int)ts);
        }

        public bool isNoAdsTime()
        {
            long tcurr = GameHelper.CurrentTimeMilisReal() / 60000;
            int ts = PlayerPrefs.GetInt("time_start_noads_fi", 0);
            if ((tcurr - ts) <= 240)
            {
                SdkUtil.logd($"ads helper isNoAdsTime true");
                return true;
            }

            return false;
        }
        //flag = 0-banner, 1-full, 2-gift 3-rectnt
        public bool isDisableAds(int flag)
        {
            int[] ch = { 1, 2, 4, 8, 16, 32, 64 };
            int ns = (currConfig.maskAdsStatus & ch[flag]);
            return (ns == 0);
        }

        private void setIsAdsShowing(bool isshow)
        {
            bool isAllow = true;
            if (isshow)
            {
                isWaitSetAdShowing = false;
                count4AdShowing++;
                isAllow = false;
                if (GameHelper.Instance != null)
                {
                    GameHelper.Instance.isAlowShowAppOpenAd = isAllow;
                }
                SdkUtil.logd($"ads helper setIsAdsShowing isshow={isshow} isAlowShowAppOpenAd={isAllow}");
            }
            else
            {
                if (AdsProcessCB.Exists() && count4AdShowing > 0)
                {
                    SdkUtil.logd($"ads helper setIsAdsShowing isshow={isshow} and wait for set");
                    isWaitSetAdShowing = true;
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        isWaitSetAdShowing = false;
                        count4AdShowing--;
                        if (count4AdShowing < 0)
                        {
                            count4AdShowing = 0;
                            isAllow = true;
                        }
                        if (GameHelper.Instance != null)
                        {
                            GameHelper.Instance.isAlowShowAppOpenAd = isAllow;
                        }
                        SdkUtil.logd($"ads helper setIsAdsShowing delay isshow={isshow} isAlowShowAppOpenAd={isAllow}");
                    }, 0.2f);
                }
                else
                {
                    isWaitSetAdShowing = false;
                    count4AdShowing--;
                    if (count4AdShowing < 0)
                    {
                        count4AdShowing = 0;
                        isAllow = true;
                    }
                    if (GameHelper.Instance != null)
                    {
                        GameHelper.Instance.isAlowShowAppOpenAd = isAllow;
                    }
                    SdkUtil.logd($"ads helper setIsAdsShowing isshow={isshow} isAlowShowAppOpenAd={isAllow}");
                }
            }
        }

        public void clearCurrAds(string placement)
        {
            foreach (var item in listAds)
            {
                item.Value.clearCurrFull(placement);
                item.Value.clearCurrGift(placement);
            }
        }

        private void checkUseAds()
        {

        }

        private void checkLogVipAds()
        {
            int countfull4point = PlayerPrefs.GetInt("count_full_4_point", 0);
            int countGift4point = PlayerPrefs.GetInt("count_gift_4_point", 0);
            int point = countfull4point * 2 + countGift4point * 3;
            if (point > LowPoint4LogVipAds)
            {
                int lvvip = point / StepPoint4LogVipAds;
                point = lvvip * StepPoint4LogVipAds;
                int pointlastlog = PlayerPrefs.GetInt("point_last_log_vipads", 0);
                if (pointlastlog != point)
                {
                    PlayerPrefs.SetInt("point_last_log_vipads", point);
                    string eventname = $"SR_{point:000}";
                    FIRhelper.logEvent(eventname);
                }
            }
        }

        private void initListBN()
        {
            bool hasAdsEnable = false;
            foreach (var item in listAds)
            {
                if (item.Value.isEnable)
                {
                    hasAdsEnable = true;
                    break;
                }
            }
            List<int> listShowCircle = currConfig.bnStepShowCircle;
            List<int> listShowRe = currConfig.bnStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.fullStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads bn helper initListBN step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            listStepShowBNCircle.Clear();
            string steplog = "";
            for (int i = 0; i < listShowCircle.Count; i++)
            {
                if (listAds.ContainsKey(listShowCircle[i]))
                {
                    if (!hasAdsEnable || listAds[listShowCircle[i]].isEnable)
                    {
                        listStepShowBNCircle.Add(listAds[listShowCircle[i]]);
                        steplog += $"{listShowCircle[i]},";
                    }
                }
                else if (listShowCircle[i] == 20)
                {
                    listStepShowBNCircle.Add(listAds[10]);
                    steplog += $"{listShowCircle[i]},";
                }

            }
            if (listStepShowBNCircle.Count == 0)
            {
                foreach (var item in listAds)
                {
                    if (item.Value.isEnable && item.Value.adsType != 10 && item.Value.adsType != 11 && (item.Value.adsType == 3 || item.Value.adsType == 6))
                    {
                        listStepShowBNCircle.Add(item.Value);
                        steplog += $"{item.Value.adsType},";
                    }
                }
            }
            if (listStepShowBNCircle.Count == 0)
            {
                listStepShowBNCircle.Add(adsAdmob);
                steplog += $"{adsAdmob.adsType},";
            }

            listStepShowBNRe.Clear();
            for (int i = 0; i < listShowRe.Count; i++)
            {
                if (listAds.ContainsKey(listShowRe[i]))
                {
                    if (!hasAdsEnable || listAds[listShowRe[i]].isEnable)
                    {
                        listStepShowBNRe.Add(listAds[listShowRe[i]]);
                        steplog += $"_{listShowRe[i]}";
                    }
                }
                else if (listShowRe[i] == 20)
                {
                    listStepShowBNRe.Add(listAds[10]);
                    steplog += $"_{listShowRe[i]},";
                }
            }
            if (idxBNShowCircle >= listStepShowBNCircle.Count)
            {
                idxBNShowCircle = 0;
            }
            Debug.Log($"mysdk: ads bn listbn=" + steplog);

            initMyBanner();
            string scf = PlayerPrefs.GetString("cf_ads_placement", "");
            if (scf.Length <= 0)
            {
                TextAsset txt = (TextAsset)Resources.Load("cfadsplacement", typeof(TextAsset));
                if (txt != null && txt.text != null)
                {
                    scf = txt.text;
                }
            }
            if (scf.Length > 3)
            {
                parserConfigAdsPlacement(scf);
            }
        }

        private void initECPMFloorAdMobMy(bool iSFromFir)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if (adsAdmobMy != null)
            {
                if (iSFromFir)
                {
                    ((AdsAdmobMy)adsAdmobMy).checkFloorInitRemote();
                }
            }
#endif
#if ENABLE_ADS_ADMOB && !USE_ADSMOB_MY
            if (adsAdmob != null)
            {
                if (iSFromFir)
                {
                    ((AdsAdmob)adsAdmobMy).checkFloorInit();
                }
                ((AdsAdmob)adsAdmob).initBanner();
                ((AdsAdmob)adsAdmob).initNative();
                ((AdsAdmob)adsAdmob).initFull();
                ((AdsAdmob)adsAdmob).initGift();
            }
#endif
        }

        private void initECPMFloorMytarget()
        {
#if ENABLE_ADS_MYTARGET
            if (adsMyTarget != null)
            {
                ((AdsMyTarget)adsMyTarget).initBanner();
                ((AdsMyTarget)adsMyTarget).initFull();
                ((AdsMyTarget)adsMyTarget).initGift();
            }
#endif
#if ENABLE_ADS_YANDEX
            if (adsMyYandex != null)
            {
                ((AdsMyYandex)adsMyYandex).initBanner();
                ((AdsMyYandex)adsMyYandex).initFull();
                ((AdsMyYandex)adsMyYandex).initGift();
            }
#endif
        }

        private void initMyBanner()
        {
            string cfmybn = PlayerPrefs.GetString("key_my_banner", "");
            if (cfmybn.Length > 5)
            {
                string[] arrcfmbn = cfmybn.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrcfmbn != null && arrcfmbn.Length >= 2)
                {
                    imgBanner = arrcfmbn[0];
                    actionBanner = arrcfmbn[1];
                }
            }
        }

        private void initListNative()
        {
            listStepShowNative.Clear();
            for (int i = 0; i < currConfig.nativeStepShow.Count; i++)
            {
                if (listAds.ContainsKey(currConfig.nativeStepShow[i]))
                {
                    listStepShowNative.Add(listAds[currConfig.nativeStepShow[i]]);
                }
            }
            //vvvvv
            listStepShowNative.Clear();
            listStepShowNative.Add(listAds[0]);
        }
        private void initListFull()
        {
            string steplog = "";
            for (int i = 0; i < currConfig.fullStepShowCircle.Count; i++)
            {
                bool isHas = false;
                if (listAds.ContainsKey(currConfig.fullStepShowCircle[i]))
                {
                    if (listAds[currConfig.fullStepShowCircle[i]].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.fullStepShowCircle[i]},";
                    }
                }
                else if (listAds.ContainsKey(0) && (currConfig.fullStepShowCircle[i] == 20
                    || currConfig.fullStepShowCircle[i] == 24
                    || currConfig.fullStepShowCircle[i] == 25
                    || currConfig.fullStepShowCircle[i] == 26))
                {
                    if (listAds[0].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.fullStepShowCircle[i]},";
                    }
                }
                else if (listAds.ContainsKey(1) && (currConfig.fullStepShowCircle[i] == 70 || currConfig.fullStepShowCircle[i] == 71))
                {
                    if (listAds[1].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.fullStepShowCircle[i]},";
                    }
                }
                if (!isHas)
                {
                    currConfig.fullStepShowCircle.RemoveAt(i);
                    i--;
                }
            }
            if (currConfig.fullStepShowCircle.Count == 0)
            {
                foreach (var item in listAds)
                {
                    if (item.Value.isEnable && (item.Value.adsType == 3 || item.Value.adsType == 6))
                    {
                        currConfig.fullStepShowCircle.Add(item.Value.adsType);
                        steplog += $"{item.Value.adsType},";
                    }
                }
            }
            if (currConfig.fullStepShowCircle.Count == 0)
            {
                currConfig.fullStepShowCircle.Add(0);
                steplog += $"0,";
            }

            for (int i = 0; i < currConfig.fullStepShowRe.Count; i++)
            {
                bool isHas = false;
                if (listAds.ContainsKey(currConfig.fullStepShowRe[i]))
                {
                    if (listAds[currConfig.fullStepShowRe[i]].isEnable)
                    {
                        isHas = true;
                        steplog += $"_{currConfig.fullStepShowRe[i]}";
                    }
                }
                else if (listAds.ContainsKey(0) && (currConfig.fullStepShowRe[i] == 20
                    || currConfig.fullStepShowRe[i] == 24
                    || currConfig.fullStepShowRe[i] == 25
                    || currConfig.fullStepShowRe[i] == 26))
                {
                    if (listAds[0].isEnable)
                    {
                        isHas = true;
                        steplog += $"_{currConfig.fullStepShowRe[i]}";
                    }
                }
                else if (listAds.ContainsKey(1) && (currConfig.fullStepShowRe[i] == 70 || currConfig.fullStepShowRe[i] == 71))
                {
                    if (listAds[1].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.fullStepShowRe[i]},";
                    }
                }
                if (!isHas)
                {
                    currConfig.fullStepShowRe.RemoveAt(i);
                    i--;
                }
            }
            Debug.Log($"mysdk: ads full helper listfull=" + steplog);
            if (idxFullShowCircle >= currConfig.fullStepShowCircle.Count)
            {
                idxFullShowCircle = 0;
            }
        }
        private void initListFull2()
        {
            string steplog = "";
            for (int i = 0; i < currConfig.full2StepShowCircle.Count; i++)
            {
                bool isHas = false;
                if (listAds.ContainsKey(currConfig.full2StepShowCircle[i]))
                {
                    if (listAds[currConfig.full2StepShowCircle[i]].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowCircle[i]},";
                    }
                }
                else if (listAds.ContainsKey(0) && (currConfig.full2StepShowCircle[i] == 20 || currConfig.full2StepShowCircle[i] == 21))
                {
                    if (listAds[0].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowCircle[i]},";
                    }
                }
                else if (listAds.ContainsKey(1) && (currConfig.full2StepShowCircle[i] == 70 || currConfig.full2StepShowCircle[i] == 71))
                {
                    if (listAds[1].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowCircle[i]},";
                    }
                }
                else if (listAds.ContainsKey(60) && currConfig.full2StepShowCircle[i] == 61)
                {
                    if (listAds[60].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowCircle[i]},";
                    }
                }
                if (!isHas)
                {
                    currConfig.full2StepShowCircle.RemoveAt(i);
                    i--;
                }
            }
            steplog += "_";
            for (int i = 0; i < currConfig.full2StepShowRe.Count; i++)
            {
                bool isHas = false;
                if (listAds.ContainsKey(currConfig.full2StepShowRe[i]))
                {
                    if (listAds[currConfig.full2StepShowRe[i]].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowRe[i]},";
                    }
                }
                else if (listAds.ContainsKey(0) && (currConfig.full2StepShowRe[i] == 20 || currConfig.full2StepShowRe[i] == 21))
                {
                    if (listAds[0].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowRe[i]},";
                    }
                }
                else if (listAds.ContainsKey(1) && (currConfig.full2StepShowRe[i] == 70 || currConfig.full2StepShowRe[i] == 71))
                {
                    if (listAds[1].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowRe[i]},";
                    }
                }
                else if (listAds.ContainsKey(60) && currConfig.full2StepShowRe[i] == 61)
                {
                    if (listAds[60].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.full2StepShowRe[i]},";
                    }
                }
                if (!isHas)
                {
                    currConfig.full2StepShowRe.RemoveAt(i);
                    i--;
                }
            }

            Debug.Log($"mysdk: ads full helper init ListFull323=" + steplog);
            if (idxFull2ShowCircle >= currConfig.full2StepShowCircle.Count)
            {
                idxFull2ShowCircle = 0;
            }
        }
        private void initListGift()
        {
            string steplog = "";
            for (int i = 0; i < currConfig.giftStepShowCircle.Count; i++)
            {
                bool isHas = false;
                if (listAds.ContainsKey(currConfig.giftStepShowCircle[i]))
                {
                    if (listAds[currConfig.giftStepShowCircle[i]].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.giftStepShowCircle[i]},";
                    }
                }
                else if (listAds.ContainsKey(0) && (currConfig.giftStepShowCircle[i] == 20))
                {
                    if (listAds[0].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.giftStepShowCircle[i]},";
                    }
                }
                if (!isHas)
                {
                    currConfig.giftStepShowCircle.RemoveAt(i);
                    i--;
                }
            }
            if (currConfig.giftStepShowCircle.Count == 0)
            {
                foreach (var item in listAds)
                {
                    if (item.Value.isEnable && (item.Value.adsType == 3 || item.Value.adsType == 6))
                    {
                        currConfig.giftStepShowCircle.Add(item.Value.adsType);
                        steplog += $"{item.Value.adsType},";
                    }
                }
            }
            if (currConfig.giftStepShowCircle.Count == 0)
            {
                currConfig.giftStepShowCircle.Add(0);
                steplog += $"0,";
            }

            for (int i = 0; i < currConfig.giftStepShowRe.Count; i++)
            {
                bool isHas = false;
                if (listAds.ContainsKey(currConfig.giftStepShowRe[i]))
                {
                    if (listAds[currConfig.giftStepShowRe[i]].isEnable)
                    {
                        isHas = true;
                        steplog += $"_{currConfig.giftStepShowRe[i]}";
                    }
                }
                else if (listAds.ContainsKey(0) && (currConfig.giftStepShowRe[i] == 20))
                {
                    if (listAds[0].isEnable)
                    {
                        isHas = true;
                        steplog += $"{currConfig.giftStepShowRe[i]},";
                    }
                }
                if (!isHas)
                {
                    currConfig.giftStepShowRe.RemoveAt(i);
                    i--;
                }
            }
            Debug.Log($"mysdk: ads gift helper listgift=" + steplog);
            if (idxGiftShowCircle >= currConfig.giftStepShowCircle.Count)
            {
                idxGiftShowCircle = 0;
            }
        }
        public void configWithRegion(bool iSFromFir)
        {
            SdkUtil.logd($"ads helper cf ads=" + GameHelper.Instance.countryCode);
            currConfig.saveAllConfig();
            checkSpecialCon(true);
            initListBN();
            initListNative();
            initListFull();
            initListFull2();
            initListGift();
            checkUseAds();
            initECPMFloorAdMobMy(iSFromFir);
            initECPMFloorMytarget();
            if (isDisableAds(0))
            {
                hideBanner(0);
            }
            if (currConfig.fullTimeStart >= 30 && SDKManager.Instance.timeEnterGame < (currConfig.fullTimeStart - 30))
            {
                isCheckLoadFullStart = true;
            }
            if (iSFromFir)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    GameHelper.Instance.configAppOpenAd(currConfig.OpenAdTimeBg);
                }, 1);
            }
        }
        private void checkTotalShowGiftFull()
        {
            countFullShowOfDay = PlayerPrefs.GetInt("mem_ads_count_showFull", currConfig.fullTotalOfday);
            countGiftShowOfDay = PlayerPrefs.GetInt("mem_ads_count_showGift", currConfig.giftTotalOfday);
            isNewlogFirtOpen = PlayerPrefs.GetInt("cf_open_newlogic", AppConfig.open_newlogic);

            if (SDKManager.Instance.flagNewDay)
            {
                SdkUtil.logd($"ads helper reset TotalShowGift");
                countGiftShowOfDay = currConfig.giftTotalOfday;
                PlayerPrefs.SetInt("mem_ads_count_showGift", countGiftShowOfDay);

                //for full
                countFullShowOfDay = currConfig.fullTotalOfday;
                PlayerPrefs.SetInt("mem_ads_count_showFull", countFullShowOfDay);

                //calculate day return game
                int dayreturn = PlayerPrefs.GetInt("analy_pr_dayReturn", -1);
                dayreturn++;
                PlayerPrefs.GetInt("analy_pr_dayReturn", dayreturn);
            }
        }
        private void subCountShowFull()
        {
            if (countFullShowOfDay > 0)
            {
                countFullShowOfDay--;
                PlayerPrefs.SetInt("mem_ads_count_showFull", countFullShowOfDay);
            }
        }
        private void subCountShowGift()
        {
            if (countGiftShowOfDay > 0)
            {
                countGiftShowOfDay--;
                PlayerPrefs.SetInt("mem_ads_count_showGift", countGiftShowOfDay);
            }
        }

        //==========================================================================
        public AdsBase getAdsFromId(int type)
        {
            if (listAds.ContainsKey(type) && listAds[type].isEnable)
            {
                return listAds[type];
            }
            else if (type == 20 || type == 21 || type == 24 || type == 25 || type == 26)
            {
                if (listAds.ContainsKey(0) && listAds[0].isEnable)
                {
                    return listAds[0];
                }
            }
            else if (type == 70 || type == 71)
            {
                if (listAds.ContainsKey(1) && listAds[1].isEnable)
                {
                    return listAds[1];
                }
            }
            else if (type == 30)
            {
                if (listAds.ContainsKey(3) && listAds[3].isEnable)
                {
                    return listAds[3];
                }
            }
            else if (type == 62)
            {
                if (listAds.ContainsKey(6) && listAds[6].isEnable)
                {
                    return listAds[6];
                }
            }
            else if (type == 60 || type == 61)
            {
                if (listAds.ContainsKey(60) && listAds[60].isEnable)
                {
                    return listAds[60];
                }
            }
            return null;
        }
        public int isShowOpenAds(bool isShowFirst)
        {
            if (isRemoveAds(0))
            {
                return 0;
            }
            int cfcountopenshow = currConfig.OpenAdShowat;
            int countopenshow = 1;
            float timeStartGame = 0;
            if (SDKManager.Instance != null)
            {
                countopenshow = SDKManager.Instance.counSessionGame;
                timeStartGame = SDKManager.Instance.timeEnterGame;
            }
            int cflvshow = currConfig.OpenadLvshow;
            int cfFirst = currConfig.OpenAdIsShowFirstOpen;
            SdkUtil.logd($"ads open helper isShowOpenAds cfcountopenshow={cfcountopenshow}, countopenshow={countopenshow}, cflv={cflvshow}, lv={GameRes.LevelCommon()} cfstart={currConfig.OpenAdTimeShowStart}-timestart={timeStartGame}, cfshowfirst={cfFirst}, isfirst={isShowFirst}");
            if (cfcountopenshow > 0 && timeStartGame >= currConfig.OpenAdTimeShowStart && countopenshow >= cfcountopenshow && GameRes.LevelCommon() >= cflvshow)
            {
                if (isShowFirst)
                {
                    if (cfFirst > 0)
                    {
                        SdkUtil.logd($"ads open helper isShowOpenAds first true");
                        return cfFirst;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads open helper isShowOpenAds true");
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }

        //==========================================================================
        public void onGetBanner(int adType, Rect r)
        {
            Debug.Log("aaaaa=" + r);
            if (CBGetHighBanner != null)
            {
                CBGetHighBanner(adType, r);
            }
        }
        public float getBannerSize(int bannersize)
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            float iosw = GameHelper.getScreenWidth();
            if (iosw > 0)
            {
                float rw = (Screen.width * (float)bannersize) / iosw;
                return rw;
            }
            else
            {
                return Screen.dpi * (float)bannersize / 160.0f;
            }
#else
            return Screen.dpi * (float)bannersize / 160.0f;
#endif
        }
        public void onBannerLoadFail(int adsType)
        {
            SdkUtil.logd($"ads bn helper onBannerLoadFail 0");
            if (statusLoadBanner != -1)
            {
                tbnLoadFail = GameHelper.CurrentTimeMilisReal();
                statusLoadBanner = -1;
            }
        }
        public void onBannerLoadOk(int adsType)
        {
            SdkUtil.logd($"ads bn onBannerLoadOk 0");
            tbnLoadOk = GameHelper.CurrentTimeMilisReal();
            tbnLoadFail = tbnLoadOk;
            statusLoadBanner = 1;
            isLoadBannerok = true;
        }

        private IEnumerator reloadBanner()
        {
            yield return new WaitForSeconds(20);
            //SdkUtil.logd($"ads bn reloadBanner 0");
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                //SdkUtil.logd($"ads bn reloadBanner 01 statusLoadBanner=" + statusLoadBanner);
                if (statusLoadBanner == -1)
                {
                    long tcu = GameHelper.CurrentTimeMilisReal();
                    if ((tcu - tbnLoadFail) >= 20000 && !isLoadBannerok)
                    {
                        //SdkUtil.logd($"ads bn reloadBanner 1");
                        statusLoadBanner = 0;
                        tbnLoadFail = tcu;
                        if (isShowBanner)
                        {
                            SdkUtil.logd($"ads bn destroy banner and reshow");
                            destroyBanner();
                            showBanner(memPlacementBn, bnPos, bnOrien, 0, bnWidth, bnMaxH, bnDxCenter);
                        }
                    }
                }
                else if (statusLoadBanner == 1)
                {
                    long tcu = GameHelper.CurrentTimeMilisReal();
                    if ((tcu - tbnLoadOk) > 60000)
                    {
                        //SdkUtil.logd($"ads bn reloadBanner 2");
                        //destroyBanner(0);
                        statusLoadBanner = 0;
                        tbnLoadFail = tcu;
                        tbnLoadOk = tcu;
                        //showBanner(typeBn, bnLocation);
                    }
                }
            }
            if (!isRemoveAds(0))
            {
                StartCoroutine(reloadBanner());
            }
        }

        public void onBnClCb(string placement, AD_State sate)
        {
            if (CB4BnCl != null)
            {
                CB4BnCl(placement, sate);
            }
        }

        public void prebannerwhenCloseNtCl()
        {
            if (isPreBannerWhenCloseNtCl)
            {
                SdkUtil.logd($"ads bn adshelper prebannerwhenCloseNtCl idx={idxBNShowCircle}");
                isPreBannerWhenCloseNtCl = false;
                idxBNShowCircle--;
                if (idxBNShowCircle < 0)
                {
                    idxBNShowCircle = listStepShowBNCircle.Count - 1;
                }
            }
        }

        public void loadNativeCl4NextShow(string placement)
        {
            string ntclpl = placement.Replace("bn_", "cl_");
            if (!ntclpl.StartsWith("cl_"))
            {
                ntclpl = "cl_" + ntclpl;
            }
            adsAdmobMy.loadNtCl(ntclpl, null);
        }

        public void loadBanner(string placement, App_Open_ad_Orien orien, bool isCollapse, int width, float _dxCenter = 0, int dfFlagShow = 1, int lvShow = -1)
        {
            if (currConfig.bnSessionShow > SDKManager.Instance.counSessionGame)
            {
                SdkUtil.logd($"ads bn {placement} loadBanner is not miss session show cf={currConfig.bnSessionShow} curr={SDKManager.Instance.counSessionGame}");
                return;
            }
            if (currConfig.bnTimeStartShow > SDKManager.Instance.timeEnterGame)
            {
                SdkUtil.logd($"ads bn {placement} loadBanner is not miss time start show cf={currConfig.bnTimeStartShow} curr={SDKManager.Instance.timeEnterGame}");
                return;
            }
            if (isCollapse && useNativeCollapse == 0)
            {
                loadBannerCollapse(placement, orien, width, _dxCenter, dfFlagShow, lvShow);
                return;
            }
            if (placement.StartsWith("cl_"))
            {
                placement = placement.Replace("cl_", "bn_");
            }
            if (!placement.StartsWith("bn_"))
            {
                placement = "bn_" + placement;
            }
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(0)) //vvv
            {
                SdkUtil.logd($"ads bn {placement} loadBanner is removee ads or disable");
                ImgBgBanner.gameObject.SetActive(false);
                return;
            }
            AdCfPlacement cf = getCfAdsPlacement(placement, dfFlagShow);
            if (isCollapse && isInitSuc && cf != null && cf.flagShow > 0)
            {
                int lvcurr = GameRes.GetLevel();
                if (lvShow == -1)
                {
                    lvShow = lvcurr;
                }
                SdkUtil.logd($"ads bn {placement} loadBanner cl lv current {lvcurr} cf.lvStart={cf.lvStart}");
                if (lvcurr < cf.lvStart)
                {
                    SdkUtil.logd($"ads bn {placement} loadBanner cl lv not show");
                    isCollapse = false;
                }
                else
                {
                    if (lvShow == cf.lv4countShow)
                    {
                        SdkUtil.logd($"ads bn {placement} loadBanner cl cf.maxShow={cf.maxShow}, cf.countshow={cf.countshow}");
                        if (cf.countshow > cf.maxShow)
                        {
                            SdkUtil.logd($"ads bn {placement} loadBanner cl over num show");
                            isCollapse = false;
                        }
                    }
                }
            }
            SdkUtil.logd($"ads bn {placement} loadBanner isCollapse={isCollapse}");
            if (listStepShowBNCircle.Count == 0 && listStepShowBNRe.Count == 0)
            {
                initListBN();
            }
            if (idxBNShowCircle >= listStepShowBNCircle.Count)
            {
                idxBNShowCircle = 0;
            }
            idxBNLoad = 0;
            if (listStepShowBNCircle.Count > 0)
            {
                stepBNLoad = 0;
                countBNLoad = listStepShowBNCircle.Count;
                idxBNLoad = idxBNShowCircle;
            }
            else
            {
                stepBNLoad = 1;
                countBNLoad = listStepShowBNRe.Count;
                idxBNLoad = 0;
            }
            loadBnCircle(placement);
        }
        private void loadBnCircle(string placement)
        {
            SdkUtil.logd($"ads bn {placement} loadBnCircle stepBNLoad=" + stepBNLoad);
            AdsBase tmads = null;
            int idxforerr1 = -3;
            int idxforerr2 = -3;
            if (countBNLoad > 0)
            {
                idxforerr1 = -2;
                idxforerr2 = -2;
                if (stepBNLoad == 0)
                {
                    idxforerr1 = -1;
                    if (idxBNLoad >= listStepShowBNCircle.Count)
                    {
                        idxBNLoad = 0;
                    }
                    int tmpidx = idxBNLoad;
                    idxBNLoad++;
                    if (idxBNLoad >= listStepShowBNCircle.Count)
                    {
                        idxBNLoad = 0;
                    }
                    countBNLoad--;
                    if (countBNLoad <= 0)
                    {
                        stepBNLoad = 1;
                        countBNLoad = listStepShowBNRe.Count;
                        idxBNLoad = 0;
                    }
                    SdkUtil.logd($"ads bn {placement} loadBnCircle cir tmpidx=" + tmpidx);
                    tmads = listStepShowBNCircle[tmpidx];
                    idxforerr1 = tmpidx;
                }
                else
                {
                    idxforerr2 = -1;
                    if (listStepShowBNRe.Count > 0)
                    {
                        idxforerr2 = 10;
                        if (idxBNLoad >= listStepShowBNRe.Count)
                        {
                            idxBNLoad = 0;
                        }
                        int tmpidx = idxBNLoad;
                        idxBNLoad++;
                        if (idxBNLoad >= listStepShowBNRe.Count)
                        {
                            idxBNLoad = 0;
                        }
                        SdkUtil.logd($"ads bn {placement} loadBnCircle re tmpidx=" + tmpidx);
                        tmads = listStepShowBNRe[tmpidx];
                        idxforerr2 = tmpidx;
                    }
                    countBNLoad--;
                }
            }
            if (tmads != null)
            {
                SdkUtil.logd($"ads bn {placement} loadBnCircle tmads=" + tmads.adsType);
                if (isShowBNAdsMob && tmads.adsType != 0)
                {
                    SdkUtil.logd("ads bn helper loadBnCircle next for isShowBNAdsMob");
                    tmads.hideBanner();
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        loadBnCircle(placement);
                    }, 0.001f);
                }
                else
                {
                    if (tmads.adsType == 20)
                    {
                        if (adsAdmobMy != null)
                        {
                            adsAdmobMy.loadBnNt(placement, cblll);
                        }
                        else
                        {
                            loadBnCircle(placement);
                        }
                    }
                    else
                    {
                        tmads.loadBanner(placement, cblll);
                    }
                    void cblll(AD_State state)
                    {
                        SdkUtil.logd($"ads bn {placement} loadBnCircle tmads show state=" + state);
                        if (state == AD_State.AD_LOAD_FAIL)
                        {
                            loadBnCircle(placement);
                        }
                    }
                }
            }
            else
            {
                isBannerLoading = false;
                SdkUtil.logd($"ads bn {placement} err loadBnCircle listStepShowBNCircle.Count=" + listStepShowBNCircle.Count + ", re=" + listStepShowBNRe.Count);
                SdkUtil.logd($"ads bn {placement} err loadBnCircle idxforerr1=" + idxforerr1 + ", idxforerr2=" + idxforerr2);
                string stepbn = "cir:";
                for (int i = 0; i < listStepShowBNCircle.Count; i++)
                {
                    stepbn += ("" + listStepShowBNCircle[i].adsType + ",");
                }
                stepbn += "#re:";
                for (int i = 0; i < listStepShowBNRe.Count; i++)
                {
                    stepbn += ("" + listStepShowBNRe[i].adsType + ",");
                }
                SdkUtil.logd($"ads bn {placement} err showbanner stepbn=" + stepbn);
                FIRhelper.logEvent("ErrShowBanner");
            }
        }
        public void showBanner(string placement, AD_BANNER_POS location, App_Open_ad_Orien orien, int flagCollapse, int width, int maxH, float _dxCenter = 0, int dfFlagShow = 1, int lvShow = -1)
        {
            if (flagCollapse > 0 && useNativeCollapse == 0)
            {
                showBannerCollapse(placement, location, orien, width, maxH, _dxCenter, dfFlagShow, lvShow);
                return;
            }
            if (placement.StartsWith("cl_"))
            {
                placement = placement.Replace("cl_", "bn_");
            }
            if (!placement.StartsWith("bn_"))
            {
                placement = "bn_" + placement;
            }
            SdkUtil.logd($"ads bn {placement} showBanner location={location}, isCollapse={flagCollapse}, width={width}, dxCenter={_dxCenter} isShowBNAdsMob={isShowBNAdsMob} dfFlagShow={dfFlagShow}");
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(0)) //vvv
            {
                SdkUtil.logd($"ads bn {placement} showBanner is removee ads or disable");
                ImgBgBanner.gameObject.SetActive(false);
                return;
            }
            memPlacementBn = placement;
            bnOrien = orien;
            isShowBanner = true;
            bnPos = location;
            bnNmFlagCl = flagCollapse;
            bnWidth = width;
            bnDxCenter = _dxCenter;
            bnMaxH = maxH;
            bnIsWaitSessionAndStart = -1;
            if (currConfig.bnSessionShow > SDKManager.Instance.counSessionGame)
            {
                SdkUtil.logd($"ads bn {placement} showBanner is not miss session show cf={currConfig.bnSessionShow} curr={SDKManager.Instance.counSessionGame}");
                bnIsWaitSessionAndStart = 0;
                return;
            }
            if (currConfig.bnTimeStartShow > SDKManager.Instance.timeEnterGame)
            {
                SdkUtil.logd($"ads bn {placement} showBanner is not miss time start show cf={currConfig.bnTimeStartShow} curr={SDKManager.Instance.timeEnterGame}");
                bnIsWaitSessionAndStart = 0;
                return;
            }
            var cfplbn = getCfAdsPlacement(placement, dfFlagShow);
            if (cfplbn != null)
            {
                if (cfplbn.flagShow <= 0)
                {
                    SdkUtil.logd($"ads bn {placement} showBanner is cf not show");
                    return;
                }
                if (lvShow <= -1)
                {
                    lvShow = GameRes.GetLevel();
                }
                if (lvShow < cfplbn.lvStart)
                {
                    SdkUtil.logd($"ads bn {placement} showBanner is cf not miss condition lv show cf={cfplbn.lvStart} lv={lvShow}");
                    return;
                }
            }
            AdCfPlacement cfplCl = null;
            if (flagCollapse > 0)
            {
                string clpl = placement;
                if (clpl.StartsWith("bn_"))
                {
                    clpl = clpl.Replace("bn_", "cl_");
                }
                if (!clpl.StartsWith("cl_"))
                {
                    clpl = "cl_" + clpl;
                }
                cfplCl = getCfAdsPlacement(clpl, dfFlagShow);
                if (cfplCl != null)
                {
                    if (cfplCl.flagShow <= 0)
                    {
                        flagCollapse = 0;
                    }
                    else
                    {
                        if (lvShow <= -1)
                        {
                            lvShow = GameRes.GetLevel();
                        }
                        if (lvShow < cfplCl.lvStart)
                        {
                            SdkUtil.logd($"ads bnclnt {placement} showbanner change flagCollapse=0 lv={lvShow} cflv={cfplCl.lvStart}");
                            flagCollapse = 0;
                        }
                        if (flagCollapse > 0)
                        {
                            long tcurr = SdkUtil.CurrentTimeMilis() / 1000;
                            if ((tcurr - cfplCl.timeShow) <= cfplCl.delTime)
                            {
                                SdkUtil.logd($"ads bnclnt {placement} showbanner change flagCollapse=0 del={tcurr - cfplCl.timeShow} cf={cfplCl.delTime}");
                                flagCollapse = 0;
                            }
                        }
                    }
                }
                if (flagCollapse > 0 && (currConfig.fullFlagFor2vscl & 4) <= 0)
                {
                    SdkUtil.logd($"ads bnclnt {placement} showbanner not show cl fullFlagFor2vscl={currConfig.fullFlagFor2vscl}");
                    flagCollapse = 0;
                }
            }

#if UNITY_ANDROID
            int buildversionallowShowBanner = PlayerPrefs.GetInt("android_build_ver_show_bn", 0);
            if (SdkUtil.getAndroidBuildVersion() < buildversionallowShowBanner)
            {
                ImgBgBanner.gameObject.SetActive(false);
                return;
            }
#endif
            if (width <= 0)
            {
                ImgBgBanner.gameObject.SetActive(false);
            }
            else
            {
                SdkUtil.logd($"ads bn {placement} showbanner w={width}, dpi={Screen.dpi}, screen width={Screen.width}");
                ImgBgBanner.gameObject.SetActive(true);
                float wbn = getBannerSize(width) + 4.0f;
                float hbn = getBannerSize(50);
                float pw = wbn / (Screen.width * 2.0f);
                float ph = (hbn + 2.0f) / Screen.height;
                RectTransform rc = ImgBgBanner.GetComponent<RectTransform>();
                if (location == AD_BANNER_POS.TOP)
                {
                    rc.anchorMin = new Vector2(0.5f - pw, 1.0f - ph);
                    rc.anchorMax = new Vector2(0.5f + pw, 1);
                }
                else
                {
                    rc.anchorMin = new Vector2(0.5f - pw, 0);
                    rc.anchorMax = new Vector2(0.5f + pw, ph);
                }
                rc.sizeDelta = new Vector2(0, 0);
                rc.anchoredPosition = Vector2.zero;
                rc.anchoredPosition3D = Vector3.zero;
                ImgBgBanner.gameObject.SetActive(false);//vvv
            }
            bnIsCl = false;
            if (listStepShowBNCircle.Count == 0 && listStepShowBNRe.Count == 0)
            {
                initListBN();
            }
            if (idxBNShowCircle >= listStepShowBNCircle.Count)
            {
                idxBNShowCircle = 0;
            }
            idxBNLoad = 0;
            if (listStepShowBNCircle.Count > 0)
            {
                stepBNLoad = 0;
                countBNLoad = listStepShowBNCircle.Count;
            }
            else
            {
                stepBNLoad = 1;
                countBNLoad = listStepShowBNRe.Count;
            }
            if (!isInitSuc)
            {
                isWaitShowBanner = true;
                SdkUtil.logd($"ads bn helper wait show banner");
                return;
            }
            isWaitShowBanner = false;
            isBannerLoading = true;
            bool isHasCl = false;
            statusShowBannerAfterCloseNtCl = 0;
            if (flagCollapse > 0)
            {
                SdkUtil.logd($"ads bnclnt helper showbanner show cl");
                string ntclpl = placement.Replace("bn_", "cl_");
                isHasCl = adsAdmobMy.showNtCl(ntclpl, (int)bnPos, width, _dxCenter, flagCollapse != 2, null);
                if (isHasCl && cfplCl != null)
                {
                    cfplCl.timeShow = SdkUtil.CurrentTimeMilis() / 1000;
                }
            }
            if (AutoPerformanceOptimizer.Instance != null && AutoPerformanceOptimizer.Instance.devicesIsLowEnd)
            {
                SdkUtil.logd($"ads bn helper showbanner not show when low devies low=");
                hideBanner(0);
                return;
            }
            //return;//vvvvvv
            if (!isHasCl)
            {
                SdkUtil.logd($"ads bn helper showbanner not cl and show bn");
                isPreBannerWhenCloseNtCl = true;
                showBNCircle(placement, maxH);
            }
            else
            {
                SdkUtil.logd($"ads bn helper showbanner has cl and load bn");
                statusShowBannerAfterCloseNtCl = 1;
                isPreBannerWhenCloseNtCl = false;
                loadBanner(placement, orien, flagCollapse > 0, width, _dxCenter, dfFlagShow, lvShow);
            }
        }
        private void showBNCircle(string placement, int maxH)
        {
            SdkUtil.logd($"ads bn {placement} showBNCircle stepBNLoad=" + stepBNLoad);
            AdsBase tmads = null;
            int idxforerr1 = -3;
            int idxforerr2 = -3;
            if (countBNLoad > 0)
            {
                idxforerr1 = -2;
                idxforerr2 = -2;
                if (stepBNLoad == 0)
                {
                    idxforerr1 = -1;
                    if (idxBNShowCircle >= listStepShowBNCircle.Count)
                    {
                        idxBNShowCircle = 0;
                    }
                    int tmpidx = idxBNShowCircle;
                    idxBNShowCircle++;
                    if (idxBNShowCircle >= listStepShowBNCircle.Count)
                    {
                        idxBNShowCircle = 0;
                    }
                    countBNLoad--;
                    if (countBNLoad <= 0)
                    {
                        stepBNLoad = 1;
                        countBNLoad = listStepShowBNRe.Count;
                        idxBNLoad = 0;
                    }
                    SdkUtil.logd($"ads bn {placement} showBNCircle cir tmpidx=" + tmpidx);
                    tmads = listStepShowBNCircle[tmpidx];
                    idxforerr1 = tmpidx;
                }
                else
                {
                    idxforerr2 = -1;
                    if (listStepShowBNRe.Count > 0)
                    {
                        idxforerr2 = 10;
                        if (idxBNLoad >= listStepShowBNRe.Count)
                        {
                            idxBNLoad = 0;
                        }
                        int tmpidx = idxBNLoad;
                        idxBNLoad++;
                        if (idxBNLoad >= listStepShowBNRe.Count)
                        {
                            idxBNLoad = 0;
                        }
                        SdkUtil.logd($"ads bn {placement} showBNCircle re tmpidx=" + tmpidx);
                        tmads = listStepShowBNRe[tmpidx];
                        idxforerr2 = tmpidx;
                    }
                    countBNLoad--;
                }
            }
            if (tmads != null)
            {
                SdkUtil.logd($"ads bn {placement} showBNCircle tmads=" + tmads.adsType);
                if (isShowBNAdsMob && tmads.adsType != 0)
                {
                    SdkUtil.logd("ads helper showBNCircle next for isShowBNAdsMob");
                    tmads.hideBanner();
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        if (isShowBanner)
                        {
                            showBNCircle(placement, maxH);
                        }
                    }, 0.001f);
                }
                else
                {
                    if (tmads.adsType == 10)
                    {
                        if (adsAdmobMy != null)
                        {
                            bnCurrShow = 20;
                            adsAdmobMy.showBnNt(placement, (int)bnPos, bnWidth, maxH, cbbnshow, bnDxCenter);
                        }
                        else
                        {
                            showBNCircle(placement, maxH);
                        }
                    }
                    else
                    {
                        bnCurrShow = tmads.adsType;
                        tmads.showBanner(placement, (int)bnPos, bnWidth, maxH, cbbnshow, bnDxCenter);
                    }

                    void cbbnshow(AD_State state)
                    {
                        SdkUtil.logd($"ads bn {placement} showBNCircle tmads-{tmads.adsType} show state=" + state);
                        if (state == AD_State.AD_LOAD_FAIL)
                        {
                            if (isShowBanner)
                            {
                                showBNCircle(placement, maxH);
                            }
                            else
                            {
                                isBannerLoading = false;
                            }
                        }
                        else if (state == AD_State.AD_LOAD_OK)
                        {
                            isBannerLoading = false;
                            stateMybanner = -1;
                            if (bnIsCl)
                            {
                                hideBanner(0);
                            }
                        }
                        else if (state == AD_State.AD_SHOW)
                        {
                            stateMybanner = -1;
                        }
                    }
                }
                if (!isCallReloadBanner)
                {
                    isCallReloadBanner = true;
                    tbnLoadOk = GameHelper.CurrentTimeMilisReal();
                    tbnLoadFail = tbnLoadOk;
                    StartCoroutine(reloadBanner());
                }
            }
            else
            {
                isBannerLoading = false;
                SdkUtil.logd($"ads bn {placement} err showBNCircle listStepShowBNCircle.Count=" + listStepShowBNCircle.Count + ", re=" + listStepShowBNRe.Count);
                SdkUtil.logd($"ads bn {placement} err showBNCircle idxforerr1=" + idxforerr1 + ", idxforerr2=" + idxforerr2);
                string stepbn = "cir:";
                for (int i = 0; i < listStepShowBNCircle.Count; i++)
                {
                    stepbn += ("" + listStepShowBNCircle[i].adsType + ",");
                }
                stepbn += "#re:";
                for (int i = 0; i < listStepShowBNRe.Count; i++)
                {
                    stepbn += ("" + listStepShowBNRe[i].adsType + ",");
                }
                SdkUtil.logd($"ads bn {placement} err showbanner stepbn=" + stepbn);
                FIRhelper.logEvent("ErrShowBanner");
            }
            //#endif
        }
        public void hideBanner(int type, bool isOnlyBn = false)
        {
            SdkUtil.logd($"ads bn hideBanner 1");
            ImgBgBanner.gameObject.SetActive(false);
            isShowBanner = false;
            isWaitShowBanner = false;
            foreach (var ads in listAds)
            {
                if (ads.Value != null)
                {
                    ads.Value.hideBanner();
                }
            }
            if (adsAdmobMy != null)
            {
                adsAdmobMy.hideBnNt();
            }
            if (!isOnlyBn)
            {
                adsAdmobMy.hideNtCl();
            }
        }
        public void destroyBanner()
        {
            SdkUtil.logd($"ads bn hideBanner 1");
            ImgBgBanner.gameObject.SetActive(false);
            isShowBanner = false;
            isWaitShowBanner = false;
            foreach (var ads in listAds)
            {
                if (ads.Value != null)
                {
                    ads.Value.hideBanner();
                    ads.Value.destroyBanner();
                }
            }
            if (adsAdmobMy != null)
            {
                adsAdmobMy.destroyBnNt();
            }
        }
        public void hideOtherBanner(int typeAdsShow)
        {
            SdkUtil.logd($"ads bn ads helper hide other={typeAdsShow}");
            foreach (var ads in listAds)
            {
                if (ads.Value != null && ads.Value.adsType != typeAdsShow)
                {
                    ads.Value.hideBanner();
                }
            }
            if (adsAdmobMy != null && typeAdsShow != 20)
            {
                adsAdmobMy.hideBnNt();
            }
        }
        public void hideNtCl()
        {
            SdkUtil.logd($"ads bnntcl hideNtCl 1");
            if (adsAdmobMy != null)
            {
                adsAdmobMy.hideNtCl();
            }
        }
        //
        public void loadBannerCollapse(string placement, App_Open_ad_Orien orien, int width, float dxCenter = 0, int dfFlagShow = 1, int lvShow = -1)
        {
            if (!placement.StartsWith("cl_"))
            {
                placement = "cl_" + placement;
            }
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(0)) //vvv
            {
                SdkUtil.logd($"ads bncl {placement} loadBannerCollapse is removee ads or disable");
                return;
            }
            AdCfPlacement cf = getCfAdsPlacement(placement, dfFlagShow);
            SdkUtil.logd($"ads bncl {placement} loadBannerCollapse");
            if (isInitSuc && cf != null && cf.flagShow > 0)
            {
                int lvcurr = GameRes.GetLevel();
                if (lvShow == -1)
                {
                    lvShow = lvcurr;
                }
                SdkUtil.logd($"ads bncl {placement} loadBannerCollapse lv current {lvcurr} cf.lvStart={cf.lvStart}");
                if (lvcurr < cf.lvStart)
                {
                    SdkUtil.logd($"ads bncl {placement} loadBannerCollapse lv not show");
                    return;
                }
                else
                {
                    if (lvShow == cf.lv4countShow)
                    {
                        SdkUtil.logd($"ads bncl {placement} loadBannerCollapse cf.maxShow={cf.maxShow}, cf.countshow={cf.countshow}");
                        if (cf.countshow > cf.maxShow)
                        {
                            SdkUtil.logd($"ads bncl {placement} loadBannerCollapse over num show");
                            return;
                        }
                    }
                }
                if (cf.typeAd == 0 || typeBn == 2)
                {
                    if (adsAdmobMy != null)
                    {
                        adsAdmobMy.loadCollapseBanner(placement, null);
                    }
                }
            }
        }
        public bool showBannerCollapse(string placement, AD_BANNER_POS location, App_Open_ad_Orien orien, int width, int maxH, float dxCenter = 0, int dfFlagShow = 1, int lvShow = -1)
        {
            if (placement.StartsWith("bn_"))
            {
                placement = placement.Replace("bn_", "cl_");
            }
            if (!placement.StartsWith("cl_"))
            {
                placement = "cl_" + placement;
            }
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(0)) //vvv
            {
                SdkUtil.logd($"ads bncl {placement} showBannerCollapse is removee ads or disable");
                ImgBgBanner.gameObject.SetActive(false);
                return false;
            }
            memPlacementCl = placement;
            memPlacementBn = placement;
            bnCollapsePos = location;
            bnOrien = orien;
            bnClWidth = width;
            bnClDxCenter = dxCenter;
            bnMaxH = maxH;
            AdCfPlacement cf = getCfAdsPlacement(placement, dfFlagShow);
            SdkUtil.logd($"ads bncl {placement} showBannerCollapse location={location} pl={placement}");
            if (cf == null || cf.flagShow == 0 || (currConfig.fullFlagFor2vscl & 4) <= 0)
            {
                SdkUtil.logd($"ads bncl {placement} showBannerCollapse location={location} flagshow = 0 || fullFlagFor2vscl={currConfig.fullFlagFor2vscl}");
                hideBannerCollapse();
                showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                return false;
            }

            if (cf != null)
            {
                if (lvShow == -1)
                {
                    lvShow = GameRes.GetLevel();
                }
                SdkUtil.logd($"ads bncl {placement} showBannerCollapse lv current={lvShow} cf.lvStart={cf.lvStart}");
                if (lvShow < cf.lvStart)
                {
                    SdkUtil.logd($"ads bncl {placement} showBannerCollapse location={location} lv not show");
                    showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                    return false;
                }
                else
                {
                    if (lvShow == cf.lv4countShow)
                    {
                        SdkUtil.logd($"ads bncl {placement} showBannerCollapse cf.maxShow={cf.maxShow}, cf.countshow={cf.countshow}");
                        if (cf.countshow > cf.maxShow)
                        {
                            SdkUtil.logd($"ads bncl {placement} showBannerCollapse location={location} over num show");
                            showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                            return false;
                        }
                    }
                    else
                    {
                        SdkUtil.logd($"ads bncl {placement} showBannerCollapse setlv 4 count");
                        cf.lv4countShow = lvShow;
                        cf.countshow = 0;
                    }
                    long tcc = SdkUtil.CurrentTimeMilis() / 1000;
                    SdkUtil.logd($"ads bncl {placement} showBannerCollapse cf.timeShow={cf.timeShow}");
                    if ((tcc - cf.timeShow) < cf.delTime)
                    {
                        SdkUtil.logd($"ads bncl {placement} showBannerCollapse location={location} deltime not show");
                        showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                        return false;
                    }
                }

                if (cf.apply_interval == 1)
                {
                    SdkUtil.logd($"ads bncl {placement} showBannerCollapse Check interval");
                    deltaloverCountCl++;
                    int overconfig = 1;
                    for (int i = 0; i < currConfig.fullListIntervalShow.Count; i++)
                    {
                        if (currConfig.clListIntervalShow[i].startlevel <= lvShow && currConfig.clListIntervalShow[i].endLevel > lvShow)
                        {
                            overconfig = currConfig.clListIntervalShow[i].deltal4Show;
                            SdkUtil.logd($"ads bncl {placement} showBannerCollapse cf interval start= " + currConfig.clListIntervalShow[i].startlevel + "; end = " +
                              currConfig.clListIntervalShow[i].endLevel + ", num=" + overconfig);
                            break;
                        }
                    }
                    if (deltaloverCountCl < overconfig)
                    {
                        SdkUtil.logd($"ads bncl {placement} showBannerCollapse location={location} interval not show {deltaloverCountCl}, {overconfig}");
                        showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                        return false;
                    }
                }
            }

            if (!isInitSuc)
            {
                isWaitShowCollapseBanner = true;
                SdkUtil.logd($"ads bncl {placement} wait showBannerCollapse");
                return true;
            }
            isWaitShowCollapseBanner = false;
            bnIsCl = true;
            if (adsAdmob != null || adsAdmobMy != null)
            {
                bool iscallre = true;
                AdsBase adshow = adsAdmob;
#if USE_ADSMOB_MY
                adshow = adsAdmobMy;
#endif
                bool isShow = adshow.showCollapseBanner(placement, (int)bnCollapsePos, width, maxH, dxCenter, (AD_State state) =>
                {
                    SdkUtil.logd($"ads bncl {placement} adshelper showCollapseBanner cb={state} bnIsCl={bnIsCl}");
                    if (state == AD_State.AD_LOAD_FAIL)
                    {
                        if (iscallre)
                        {
                            iscallre = false;
                            showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                            bnIsCl = true;
                        }
                    }
                    else if (state == AD_State.AD_LOAD_OK)
                    {
                        stateMybanner = -1;
                        if (bnIsCl)
                        {
                            SdkUtil.logd($"ads bncl {placement} hide bn when cl load ok and still show cl");
                            hideBanner(0);
                        }
                        else
                        {
                            SdkUtil.logd($"ads bncl {placement} hide because is showing bn");
                            hideBannerCollapse();
                        }
                    }
                    else if (state == AD_State.AD_SHOW)
                    {
                        stateMybanner = -1;
                    }
                });
                deltaloverCountCl = 0;
                if (cf != null)
                {
                    cf.timeShow = SdkUtil.CurrentTimeMilis() / 1000;
                    cf.countshow++;
                }
                if (!isShow)
                {
                    SdkUtil.logd($"ads bncl {placement} showBannerCollapse show bn when wait load cl");
                    showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                    bnIsCl = true;
                }
                else
                {
                    hideBanner(0);
                }
                return true;
            }
            else
            {
                showBanner(placement, location, orien, 0, width, maxH, dxCenter);
                return false;
            }
        }
        public void hideBannerCollapse(bool isDestroy = false)
        {
            SdkUtil.logd($"ads bncl hideBannerCollapse");
#if UNITY_IOS || UNITY_IPHONE
            isDestroy = false;
#endif
            if (adsAdmob != null || adsAdmobMy != null)
            {
                AdsBase adshow = adsAdmob;
#if USE_ADSMOB_MY
                adshow = adsAdmobMy;
#endif
                if (!isDestroy)
                {
                    adshow.hideCollapseBanner();
                }
                else
                {
                    adshow.destroyCollapseBanner();
                }
            }
        }
        //
        public void showBannerRect(string placement, AD_BANNER_POS location, float width, int maxH, float dxCenter, float dyVertical, int dfFlagShow)
        {
            if (!placement.StartsWith("rect_"))
            {
                placement = "rect_" + placement;
            }
            SdkUtil.logd($"ads rect {placement} helper showBannerRect location={location} pl={placement}");
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(0)) //vvv
            {
                SdkUtil.logd($"ads rect {placement} helper is removee ads or disable");
                return;
            }
            var cf = getCfAdsPlacement(placement, dfFlagShow);
            if (cf == null || cf.flagShow == 0)
            {
                SdkUtil.logd($"ads rect {placement} helper not cf show");
                return;
            }
            memPlacementRect = placement;
            bnRectPos = location;
            bnRectWidth = width;
            bnRectDxCenter = dxCenter;
            bnRectDyVertical = dyVertical;
            rectMaxH = maxH;
            //if (!isInitSuc)
            //{
            //    isWaitShowRectBanner = true;
            //    SdkUtil.logd($"ads rect {placement} helper wait showBannerRect");
            //    return;
            //}
            isWaitShowRectBanner = false;
#if UNITY_EDITOR
            showRectEditor(location, width, maxH, dxCenter, dyVertical);
#else
            if (adsAdmob != null || adsAdmobMy != null)
            {
                AdsBase adshow = adsAdmob;
#if USE_ADSMOB_MY
                adshow = adsAdmobMy;
#endif
                adshow.showRectBanner(placement, (int)bnRectPos, width, maxH, dxCenter, dyVertical, (AD_State state) =>
                {
                    if (state == AD_State.AD_LOAD_FAIL)
                    {
                    }
                    else if (state == AD_State.AD_LOAD_OK)
                    {
                        stateMybanner = -1;
                    }
                    else if (state == AD_State.AD_SHOW)
                    {
                        stateMybanner = -1;
                    }
                });
            }
#endif
        }
        public void hideBannerRect()
        {
            SdkUtil.logd($"ads rect helper hideBannerRect");
            if (adsAdmob != null || adsAdmobMy != null)
            {
                AdsBase adshow = adsAdmob;
#if USE_ADSMOB_MY
                adshow = adsAdmobMy;
#endif
                adshow.hideRectBanner();
            }
#if UNITY_EDITOR
            hideRectEditor();
#endif
        }

        //-----------rect nt--------
        public void loadRectNt(string placement)
        {
            if (!placement.StartsWith("rectnt_"))
            {
                placement = "rectnt_" + placement;
            }
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(3)) //vvv
            {
                SdkUtil.logd($"ads rect nt {placement} helper is removee ads or disable");
                return;
            }
            var cf = getCfAdsPlacement(placement, 1);
            if (cf == null || cf.flagShow == 0)
            {
                SdkUtil.logd($"ads rect nt {placement} helper not cf show");
                return;
            }

            SdkUtil.logd($"ads rect nt {placement} helper loadRectNt");
            if (adsAdmobMy != null)
            {
                adsAdmobMy.loadRectNt(placement, null);
            }
        }
        public void showRectNt(string placement, int pos, int orien, float width, float height, float dx, float dy)
        {
            if (!placement.StartsWith("rectnt_"))
            {
                placement = "rectnt_" + placement;
            }
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(3)) //vvv
            {
                SdkUtil.logd($"ads rect nt {placement} helper is removee ads or disable");
                return;
            }
            var cf = getCfAdsPlacement(placement, 1);
            if (cf == null || cf.flagShow == 0)
            {
                SdkUtil.logd($"ads rect nt {placement} helper not cf show");
                return;
            }
            SdkUtil.logd($"ads rect nt {placement} helper showRectNt");
            if (adsAdmobMy != null)
            {
                adsAdmobMy.showRectNt(placement, pos, orien, width, height, dx, dy, null);
            }
        }
        public void hideRectNt()
        {
            if (adsAdmobMy != null)
            {
                adsAdmobMy.hideRectNt();
            }
        }
        //----------------------
        public void showNative(string placement, AdsNativeObject obAds, bool isRefresh, bool stillShowWhenRmAd, AdCallBack cb, int dfShow = 1)
        {
            if (!placement.StartsWith("nt_"))
            {
                placement = "nt_" + placement;
            }
            if (!stillShowWhenRmAd && isRemoveAds(0)) //vvv
            {
                if (obAds != null)
                {
                    obAds.gameObject.SetActive(false);
                }
                return;
            }
            var cf = getCfAdsPlacement(placement, dfShow);
            if (cf != null && cf.flagShow == 0)
            {
                SdkUtil.logd($"ads native {placement} helper showNative cf not show");
                if (obAds != null)
                {
                    obAds.gameObject.SetActive(false);
                }
                return;
            }

            SdkUtil.logd($"ads native {placement} helper showNative");
            if (listStepShowNative.Count == 0)
            {
                initListNative();
            }
            isShowNatie = true;
#if UNITY_EDITOR
            if (obAds != null)
            {
                obAds.gameObject.SetActive(true);
            }
#else
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            if (adsAdmobMy != null)
            {
                adsAdmobMy.showNative(placement, obAds, isRefresh, cb);
            }
#elif ENABLE_ADS_ADMOB && !USE_ADSMOB_MY && ENABLE_ADS_ADMOB_NATIVE
            if (adsAdmob != null)
            {
                adsAdmob.showNative(placement, obAds, isRefresh, cb);
            }
#endif
#endif
        }
        public void loadNative(string placement, bool stillShowWhenRmAd)
        {
            if (!placement.StartsWith("nt_"))
            {
                placement = "nt_" + placement;
            }
            if (!stillShowWhenRmAd && isRemoveAds(0)) //vvv
            {
                return;
            }

            SdkUtil.logd($"ads native {placement} helper loadNative ");
            if (listStepShowNative.Count == 0)
            {
                initListNative();
            }
            isShowNatie = true;
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            if (adsAdmobMy != null)
            {
                adsAdmobMy.loadNative(placement, null, null);
            }
#elif ENABLE_ADS_ADMOB && !USE_ADSMOB_MY && ENABLE_ADS_ADMOB_NATIVE
            if (adsAdmob != null)
            {
                adsAdmob.loadNative(placement, null, null);
            }
#endif
        }
        public void loadNative4NextShow(string placement, bool stillShowWhenRmAd)
        {
            loadNative(placement + "_more", stillShowWhenRmAd);
        }
        public void freeNative(AdsNativeObject ad)
        {
            SdkUtil.logd($"ads native helper freeNative");
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
            if (adsAdmobMy != null)
            {
                adsAdmobMy.freeNative(ad);
            }
#elif ENABLE_ADS_ADMOB && !USE_ADSMOB_MY && ENABLE_ADS_ADMOB_NATIVE
            if (adsAdmob != null)
            {
                adsAdmob.freeNative(ad);
            }
#endif
        }
        //===============================
        public void setTimeShowFull()
        {
            checkResumeAudio();
            tFullImgShow = GameHelper.CurrentTimeMilisReal();
            tFullShow = tFullImgShow;
        }
        private void checkLoadLogicFullRw(string placement)
        {
            if (currConfig.fullRwType >= 0 && currConfig.fullRwNumTotal > fullRwNumTotal && currConfig.fullRwNumSession > fullRwNumSession)
            {
                if (SDKManager.Instance.timeEnterGame >= (currConfig.fullRwTimeStart - 30))
                {
                    if (SDKManager.Instance.timeEnterSession >= (currConfig.fullRwTimeSession - 30))
                    {
                        long tcurr = SdkUtil.CurrentTimeMilis();
                        long dtshow = (tcurr - fullRwTimeShow) / 1000;
                        if (dtshow >= (currConfig.fullRwDeltatime - 30))
                        {
                            if (adsAdmobMy != null)
                            {
                                loadFullRwCircle(placement, currConfig.fullRwType);
                            }
                        }
                    }
                }
            }
        }
        private bool checkShowLogicFullRw(string placement)
        {
            //SdkUtil.logd($"ads full {placement} helper checkShowLogicFullRw type={currConfig.fullRwType} rwt={currConfig.fullRwNumTotal}-{fullRwNumTotal} rwsst={currConfig.fullRwNumSession}-{fullRwNumSession}");
            if (currConfig.fullRwType >= 0 && currConfig.fullRwNumTotal > fullRwNumTotal && currConfig.fullRwNumSession > fullRwNumSession)
            {
                //SdkUtil.logd($"ads full {placement} helper checkShowLogicFullRw dtstart={dtstart}-{currConfig.fullRwTimeStart}");
                if (SDKManager.Instance.timeEnterGame >= currConfig.fullRwTimeStart)
                {
                    //SdkUtil.logd($"ads full {placement} helper checkShowLogicFullRw dtSS={dtSS}-{currConfig.fullRwTimeSession}");
                    if (SDKManager.Instance.timeEnterSession >= currConfig.fullRwTimeSession)
                    {
                        long tcurr = GameHelper.CurrentTimeMilisReal();
                        long dtshow = (tcurr - fullRwTimeShow) / 1000;
                        //SdkUtil.logd($"ads full {placement} helper checkShowLogicFullRw dtshow={dtshow}-{currConfig.fullRwDeltatime}");
                        if (dtshow >= currConfig.fullRwDeltatime)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void loadFullRw4ThisTurn(string placement, int level, AdCallBack cb = null)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            if (!isinitCall)
            {
                SdkUtil.logd($"ads full {placement} helper loadFullRw4ThisTurn ad not init");
                return;
            }
            checkResumeAudio();
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(1)) //vvv
            {
                SdkUtil.logd($"ads full {placement} helper loadFullRw4ThisTurn is removee ads");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if ((SDKManager.Instance.counSessionGame < currConfig.fullSessionStart || level < currConfig.fullLevelStart))
            {
                SdkUtil.logd($"ads full {placement} helper loadFullRw4ThisTurn lvstart: curr=" + level + ", lvs=" + currConfig.fullLevelStart + "ss=" + SDKManager.Instance.counSessionGame + "cfss=" + currConfig.fullSessionStart);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (countFullShowOfDay <= 0)
            {
                SdkUtil.logd($"ads full {placement} helper loadFullRw4ThisTurn over num show of day, to=" + currConfig.fullTotalOfday);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (adsAdmobMy != null)
            {
                loadFullRwCircle(placement, currConfig.fullRwType);
            }
        }
        private void loadFullRwCircle(string placement, int typeAds)
        {
            SdkUtil.logd($"ads full {placement} helper loadFullRwCircle typeAds={typeAds}");
            int tmptype = typeAds;
            if (typeAds == 0 || typeAds == 2)
            {
                adsAdmobMy.loadFullRwInter(placement, cbload);
            }
            else
            {
                adsAdmobMy.loadFullRwRw(placement, cbload);
            }
            void cbload(AD_State statusLoad)
            {
                if (statusLoad == AD_State.AD_LOAD_FAIL)
                {
                    if (tmptype == 2)
                    {
                        loadFullRwCircle(placement, 1);
                    }
                    else if (tmptype == 3 || tmptype == 4)
                    {
                        loadFullRwCircle(placement, 0);
                    }
                }
            }
        }
        public void loadFull4ThisTurn(string placement, bool isSplash, int level, int typeImg, bool ischecknumover = true, AdCallBack cb = null)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            if (!isinitCall)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn ad not init");
                return;
            }
            checkResumeAudio();
            _cbFullLoad = null;
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(1)) //vvv
            {
                SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn is removee ads");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (ischecknumover && !isSplash)
            {
                int count = deltaloverCountFull + 1;
                int overconfig = currConfig.fullDefaultNumover;
                for (int i = 0; i < currConfig.fullListIntervalShow.Count; i++)
                {
                    if (currConfig.fullListIntervalShow[i].startlevel <= level && currConfig.fullListIntervalShow[i].endLevel >= level)
                    {
                        overconfig = currConfig.fullListIntervalShow[i].deltal4Show;
                        break;
                    }
                }

                if (count < overconfig)
                {
                    SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn 2 check delta count full count={count} cf={overconfig}");
                    if (cb != null)
                    {
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                    return;
                }
            }
            if ((SDKManager.Instance.counSessionGame < currConfig.fullSessionStart || (level + 1) < currConfig.fullLevelStart || (SDKManager.Instance.timeEnterGame + 30) < currConfig.fullTimeStart || (currConfig.fullTimeAfterPurchase - 30) * 1000 + InappHelper.LastTimePurchase > time.MGTime.GetUtcTime()) && !isSplash)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn lvstart: curr={level} lvs={currConfig.fullLevelStart} dtstart={SDKManager.Instance.timeEnterGame + 30} dtcf={currConfig.fullTimeStart} ss={SDKManager.Instance.counSessionGame} cfss={currConfig.fullSessionStart} ltp={InappHelper.LastTimePurchase} cftap={currConfig.fullTimeAfterPurchase-30}");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (countFullShowOfDay <= 0)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn over num show of day, to=" + currConfig.fullTotalOfday);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }

            SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn load level={level}");
            _cbFullLoad = cb;
            if (currConfig.fullStepShowCircle.Count == 0 && currConfig.fullStepShowRe.Count == 0)
            {
                initListFull();
            }
            List<int> listShowCircle = currConfig.fullStepShowCircle;
            List<int> listShowRe = currConfig.fullStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.fullStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            if (stepCurrShow == null)
            {
                StepLevelShowfull slvf = currConfig.GetStepLevelShowfull(level);
                if (slvf != null && slvf.fullStep.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads full {placement} helper loadFull4ThisTurn step with level");
                    listShowCircle = slvf.fullStep.stepShowCircle;
                    listShowRe = slvf.fullStep.stepShowRe;
                    if (lvFullStepStart != slvf.startlevel && lvFullStepEnd != slvf.endLevel)
                    {
                        idxFullShowCircle = 0;
                        lvFullStepStart = slvf.startlevel;
                        lvFullStepEnd = slvf.endLevel;
                    }
                }
            }
            if (idxFullShowCircle >= listShowCircle.Count)
            {
                idxFullShowCircle = 0;
            }

            if (listShowCircle.Count == 0)
            {
                stepFullLoad = 1;
                idxFullLoad = 0;
                countFullLoad = listShowRe.Count;
            }
            else
            {
                stepFullLoad = 0;
                idxFullLoad = idxFullShowCircle;
                countFullLoad = listShowCircle.Count;
            }
            AdsBase admobLoad = adsAdmob;
            AdsBase admobLowLoad = adsAdmobLower;
#if USE_ADSMOB_MY
            admobLoad = adsAdmobMy;
            admobLowLoad = adsAdmobMyLower;
#endif
            bool isLoadimg = true;
            if (admobLoad != null && (typeImg & 1) > 0)
            {
                isLoadimg = false;
                if (currConfig.fullNtIsIc == 1)
                {
                    admobLoad.loadNativeIcFull(placement, null);
                }
                else
                {
                    admobLoad.loadNativeFull(placement, null);
                }
            }
            if ((isSplash || (typeImg & 2) > 0) && admobLowLoad != null)
            {
                isLoadimg = false;
                admobLowLoad.loadFull(placement, null);
            }

            if (typeImg > 0)
            {
                return;
            }


            if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
            {
                int tmpnew = isNewlogFirtOpen % 20;
                if (tmpnew == 1 || tmpnew == 2 || tmpnew > 7)
                {
                    loadFullCircle(placement, isSplash, level);
                    checkLoadLogicFullRw(placement);
                }
            }
            else
            {
                loadFullCircle(placement, isSplash, level);
                checkLoadLogicFullRw(placement);
            }

            if (currConfig.full2LevelStart <= level && (currConfig.full2TypeShow == 1 || currConfig.full2TypeShow == 3))
            {
                if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
                {
                    int tmpnew = isNewlogFirtOpen % 20;
                    if ((tmpnew == 1 || tmpnew == 2 || tmpnew > 7) && isNewlogFirtOpen < 20)
                    {
                        isLoadimg = false;
                        loadFull2(placement, level, null);
                    }
                }
                else
                {
                    isLoadimg = false;
                    loadFull2(placement, level, null);
                }
            }
            if (isLoadimg || (AdsBase.PLFullSplash.CompareTo(placement) == 0 && (isNewlogFirtOpen == 4 || isNewlogFirtOpen == 24)))
            {
                if (admobLoad != null)
                {
                    if (currConfig.fullNtIsIc == 1)
                    {
                        admobLoad.loadNativeIcFull(placement, null);
                    }
                    else
                    {
                        admobLoad.loadNativeFull(placement, null);
                    }
                }
            }
        }
        private void loadFullCircle(string placement, bool isSplash, int lv)
        {
#if UNITY_EDITOR
            if (!adsEditorCtr.isFullEditorLoading && !adsEditorCtr.isFullEditorLoaded)
            {
                loadFullEditor();
            }
            return;
#endif
            SdkUtil.logd($"ads full {placement} helper loadFullCircle idx=" + idxFullLoad);
            List<int> listShowCircle = currConfig.fullStepShowCircle;
            List<int> listShowRe = currConfig.fullStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.fullStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads full {placement} helper loadFullCircle step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            if (stepCurrShow == null)
            {
                StepLevelShowfull slvf = currConfig.GetStepLevelShowfull(lv);
                if (slvf != null && slvf.fullStep.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads full {placement} helper loadFullCircle step with level");
                    listShowCircle = slvf.fullStep.stepShowCircle;
                    listShowRe = slvf.fullStep.stepShowRe;
                    if (lvFullStepStart != slvf.startlevel && lvFullStepEnd != slvf.endLevel)
                    {
                        idxFullShowCircle = 0;
                        lvFullStepStart = slvf.startlevel;
                        lvFullStepEnd = slvf.endLevel;
                    }
                }
            }

            if (countFullLoad > 0)
            {
                level4ApplovinFull = lv;
                if (idxFullLoad >= listShowCircle.Count)
                {
                    idxFullLoad = 0;
                }
                int idxcurr = idxFullLoad;
                idxFullLoad++;
                List<int> listcurr = null;
                if (stepFullLoad == 0)
                {
                    listcurr = listShowCircle;
                    if (idxFullLoad >= listShowCircle.Count)
                    {
                        idxFullLoad = 0;
                    }
                }
                else
                {
                    listcurr = listShowRe;
                    if (idxFullLoad >= listShowRe.Count)
                    {
                        idxFullLoad = 0;
                    }
                }
                countFullLoad--;
                if (countFullLoad <= 0 && stepFullLoad == 0)
                {
                    stepFullLoad = 1;
                    idxFullLoad = 0;
                    countFullLoad = listShowRe.Count;
                }

                if (listcurr != null && listcurr.Count > idxcurr && idxcurr >= 0)
                {
                    AdsBase adscurr = getAdsFromId(listcurr[idxcurr]);
                    if (adscurr != null)
                    {
                        if (listcurr[idxcurr] == 20 || listcurr[idxcurr] == 60 || listcurr[idxcurr] == 70 || listcurr[idxcurr] == 71)
                        {
                            if (currConfig.fullNtIsIc != 1 || listcurr[idxcurr] == 60 || listcurr[idxcurr] == 70 || listcurr[idxcurr] == 71)
                            {
                                adscurr.loadNativeFull(placement, (AD_State state) =>
                                {
                                    if (state == AD_State.AD_LOAD_OK)
                                    {
                                        if (_cbFullLoad != null)
                                        {
                                            _cbFullLoad(AD_State.AD_LOAD_OK);
                                            _cbFullLoad = null;
                                        }
                                    }
                                    else
                                    {
                                        loadFullCircle(placement, isSplash, lv);
                                    }
                                });
                            }
                            else
                            {
                                adscurr.loadNativeIcFull(placement, (AD_State state) =>
                                {
                                    if (state == AD_State.AD_LOAD_OK)
                                    {
                                        if (_cbFullLoad != null)
                                        {
                                            _cbFullLoad(AD_State.AD_LOAD_OK);
                                            _cbFullLoad = null;
                                        }
                                    }
                                    else
                                    {
                                        loadFullCircle(placement, isSplash, lv);
                                    }
                                });
                            }
                        }
                        else if (listcurr[idxcurr] == 24)
                        {
                            adscurr.loadFullRwInter(placement, (AD_State state) =>
                            {
                                if (state == AD_State.AD_LOAD_OK)
                                {
                                    if (_cbFullLoad != null)
                                    {
                                        _cbFullLoad(AD_State.AD_LOAD_OK);
                                        _cbFullLoad = null;
                                    }
                                }
                                else
                                {
                                    loadFullCircle(placement, isSplash, lv);
                                }
                            });
                        }
                        else if (listcurr[idxcurr] == 25)
                        {
                            adscurr.loadFullRwRw(placement, (AD_State state) =>
                            {
                                if (state == AD_State.AD_LOAD_OK)
                                {
                                    if (_cbFullLoad != null)
                                    {
                                        _cbFullLoad(AD_State.AD_LOAD_OK);
                                        _cbFullLoad = null;
                                    }
                                }
                                else
                                {
                                    loadFullCircle(placement, isSplash, lv);
                                }
                            });
                        }
                        else if (listcurr[idxcurr] == 26)
                        {
                            adscurr.loadNtPgFull(placement, (AD_State state) =>
                            {
                                if (state == AD_State.AD_LOAD_OK)
                                {
                                    if (_cbFullLoad != null)
                                    {
                                        _cbFullLoad(AD_State.AD_LOAD_OK);
                                        _cbFullLoad = null;
                                    }
                                }
                                else
                                {
                                    loadFullCircle(placement, isSplash, lv);
                                }
                            });
                        }
                        else
                        {
                            adscurr.loadFull(placement, (AD_State state) =>
                            {
                                if (state == AD_State.AD_LOAD_OK)
                                {
                                    if (_cbFullLoad != null)
                                    {
                                        _cbFullLoad(AD_State.AD_LOAD_OK);
                                        _cbFullLoad = null;
                                    }
                                }
                                else
                                {
                                    loadFullCircle(placement, isSplash, lv);
                                }
                            });
                        }
                    }
                    else
                    {
                        loadFullCircle(placement, isSplash, lv);
                    }
                }
                else
                {
                    if (_cbFullLoad != null)
                    {
                        _cbFullLoad(AD_State.AD_LOAD_FAIL);
                        _cbFullLoad = null;
                    }
                }
            }
            else
            {
                if (_cbFullLoad != null)
                {
                    _cbFullLoad(AD_State.AD_LOAD_FAIL);
                    _cbFullLoad = null;
                }
            }
        }
        public bool isFull4Show(string placement, bool isOpenAds, bool isAll)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            checkResumeAudio();

#if UNITY_EDITOR
            return adsEditorCtr.isFullEditorLoaded;
#endif
            for (int i = 0; i < currConfig.fullStepShowCircle.Count; i++)
            {
                AdsBase adCurr = getAdsFromId(currConfig.fullStepShowCircle[i]);
                if (adCurr != null)
                {
                    if (currConfig.fullStepShowCircle[i] == 20
                        || currConfig.fullStepShowCircle[i] == 60
                        || currConfig.fullStepShowCircle[i] == 70
                        || currConfig.fullStepShowCircle[i] == 71)
                    {
                        if (currConfig.fullNtIsIc != 1
                            || currConfig.fullStepShowCircle[i] == 60
                            || currConfig.fullStepShowCircle[i] == 70
                            || currConfig.fullStepShowCircle[i] == 71)
                        {
                            if (adCurr.getNativeFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull4Show nt main true ad=" + currConfig.fullStepShowCircle[i]);
                                return true;
                            }
                        }
                        else
                        {
                            if (adCurr.getNativeIcFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull4Show ntic main true = " + currConfig.fullStepShowCircle[i]);
                                return true;
                            }
                        }
                    }
                    else if (currConfig.fullStepShowCircle[i] == 26)
                    {
                        if (adCurr.getNtPgFullLoaded(placement) > 0)
                        {
                            SdkUtil.logd($"ads full {placement} helper isFull4Show ntpg main true ad=" + currConfig.fullStepShowCircle[i]);
                            return true;
                        }
                    }
                    else
                    {
                        if (adCurr.getFullLoaded(placement) > 0)
                        {
                            SdkUtil.logd($"ads full {placement} helper isFull4Show main true ad=" + currConfig.fullStepShowCircle[i]);
                            return true;
                        }
                    }
                }
            }
            if (isAll || currConfig.fullStepShowCircle.Count == 0)
            {
                for (int i = 0; i < currConfig.fullStepShowRe.Count; i++)
                {
                    AdsBase adCurr = getAdsFromId(currConfig.fullStepShowRe[i]);
                    if (adCurr != null)
                    {
                        if (currConfig.fullStepShowRe[i] == 20
                        || currConfig.fullStepShowRe[i] == 60
                        || currConfig.fullStepShowRe[i] == 70
                        || currConfig.fullStepShowRe[i] == 71)
                        {
                            if (currConfig.fullNtIsIc != 1
                                || currConfig.fullStepShowRe[i] == 60
                                || currConfig.fullStepShowRe[i] == 70
                                || currConfig.fullStepShowRe[i] == 71)
                            {
                                if (adCurr.getNativeFullLoaded(placement) > 0)
                                {
                                    SdkUtil.logd($"ads full {placement} helper isFull4Show nt re true ad=" + currConfig.fullStepShowRe[i]);
                                    return true;
                                }
                            }
                            else
                            {
                                if (adCurr.getNativeIcFullLoaded(placement) > 0)
                                {
                                    SdkUtil.logd($"ads full {placement} helper isFull4Show ntic re true ad=" + currConfig.fullStepShowRe[i]);
                                    return true;
                                }
                            }
                        }
                        else if (currConfig.fullStepShowRe[i] == 26)
                        {
                            if (adCurr.getNtPgFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull4Show ntpg re true ad=" + currConfig.fullStepShowRe[i]);
                                return true;
                            }
                        }
                        else
                        {
                            if (adCurr.getFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull4Show re true ad=" + currConfig.fullStepShowRe[i]);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool hasFullLoaded(string placement, bool incluseImg)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            for (int i = 0; i < currConfig.fullStepShowCircle.Count; i++)
            {
                AdsBase adCurr = getAdsFromId(currConfig.fullStepShowCircle[i]);
                if (adCurr != null)
                {
                    bool ishas = false;
                    if (currConfig.fullStepShowCircle[i] != 20
                        && currConfig.fullStepShowCircle[i] != 26
                        && currConfig.fullStepShowCircle[i] != 60
                        && currConfig.fullStepShowCircle[i] != 70
                        && currConfig.fullStepShowCircle[i] != 71)
                    {
                        if (adCurr.getFullLoaded(placement) > 0)
                        {
                            ishas = true;
                        }
                    }
                    else if (currConfig.fullStepShowCircle[i] == 26)
                    {
                        if (adCurr.getNtPgFullLoaded(placement) > 0)
                        {
                            ishas = true;
                        }
                    }
                    else
                    {
                        if (currConfig.fullNtIsIc == 1 && currConfig.fullStepShowCircle[i] == 20)
                        {
                            if (adCurr.getNativeIcFullLoaded(placement) > 0)
                            {
                                ishas = true;
                            }
                        }
                        else
                        {
                            if (adCurr.getNativeFullLoaded(placement) > 0)
                            {
                                ishas = true;
                            }
                        }
                    }
                    if (ishas)
                    {
                        if (incluseImg)
                        {
                            return true;
                        }
                        else
                        {
                            if (currConfig.fullStepShowCircle[i] != 10)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < currConfig.fullStepShowRe.Count; i++)
            {
                AdsBase adCurr = getAdsFromId(currConfig.fullStepShowRe[i]);
                if (adCurr != null)
                {
                    bool ishas = false;
                    if (currConfig.fullStepShowRe[i] != 20
                        && currConfig.fullStepShowRe[i] != 26
                        && currConfig.fullStepShowRe[i] != 60
                        && currConfig.fullStepShowRe[i] != 70
                        && currConfig.fullStepShowRe[i] != 71)
                    {
                        if (adCurr.getFullLoaded(placement) > 0)
                        {
                            ishas = true;
                        }
                    }
                    else if (currConfig.fullStepShowRe[i] == 26)
                    {
                        if (adCurr.getNtPgFullLoaded(placement) > 0)
                        {
                            ishas = true;
                        }
                    }
                    else
                    {
                        if (currConfig.fullNtIsIc == 1 && currConfig.fullStepShowRe[i] == 20)
                        {
                            if (adCurr.getNativeIcFullLoaded(placement) > 0)
                            {
                                ishas = true;
                            }
                        }
                        else
                        {
                            if (adCurr.getNativeFullLoaded(placement) > 0)
                            {
                                ishas = true;
                            }
                        }
                    }
                    if (ishas)
                    {
                        if (incluseImg)
                        {
                            return true;
                        }
                        else
                        {
                            if (currConfig.fullStepShowRe[i] != 10)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private int checkShowFull(string placement, bool isSplash, int level, bool isCheckDeltaTime, bool isLog = false)
        {
            if (isSplash)
            {
                long t1 = GameHelper.CurrentTimeMilisReal();
                if (t1 - tFullShow <= 5000)
                {
                    if (isLog)
                    {
                        FIRhelper.logEvent("ads_full_return_splashShort");
                    }
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            if ((SDKManager.Instance.counSessionGame < currConfig.fullSessionStart || level < currConfig.fullLevelStart || SDKManager.Instance.timeEnterGame < currConfig.fullTimeStart || currConfig.fullTimeAfterPurchase * 1000 + InappHelper.LastTimePurchase > time.MGTime.GetUtcTime()) && !isSplash)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull lvstart: curr={level} lvs={currConfig.fullLevelStart} dtstart={SDKManager.Instance.timeEnterGame} dtcf={currConfig.fullTimeStart} ss={SDKManager.Instance.counSessionGame} cfss={currConfig.fullSessionStart} ltp={InappHelper.LastTimePurchase} cftap={currConfig.fullTimeAfterPurchase}");

                if (isLog)
                {
                    if (SDKManager.Instance.counSessionGame < currConfig.fullSessionStart)
                    {
                        FIRhelper.logEvent("ads_full_return_sessionStart");
                    }
                    else if (level < currConfig.fullLevelStart)
                    {
                        FIRhelper.logEvent("ads_full_return_lvStart");
                    }
                    else if (SDKManager.Instance.timeEnterGame < currConfig.fullTimeStart)
                    {
                        FIRhelper.logEvent("ads_full_return_TimeStart");
                    }
                    else
                    {
                        FIRhelper.logEvent("ads_full_return_after_purchase");
                    }
                }
                return 0;
            }
            if (countFullShowOfDay <= 0)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull over num show of day, to=" + currConfig.fullTotalOfday);
                if (isLog)
                {
                    FIRhelper.logEvent("ads_full_return_overOfDay");
                }
                return -1;
            }

            if (isCheckDeltaTime)
            {
                long t = GameHelper.CurrentTimeMilisReal();
                long cfDt = fullDeltalTimeCurr;
                var daycf = currConfig.getDayImpact();
                if (daycf != null && daycf.deltaTime > 1000)
                {
                    SdkUtil.logd($"ads full {placement} helper checkShowFull apply day active cf dtshow={daycf.deltaTime}");
                    cfDt = daycf.deltaTime;
                    if (cfDt >= 100000000)
                    {
                        if (isLog)
                        {
                            FIRhelper.logEvent("ads_full_return_dtCfDayLong1");
                        }
                        return -1;
                    }
                }
                else
                {
                    var cffull = getCfAdsPlacement(placement, -1);
                    if (cffull != null && cffull.delTime > 10)
                    {
                        SdkUtil.logd($"ads full {placement} helper checkShowFull apply delta time by placement cf={cffull.delTime}");
                        cfDt = cffull.delTime * 1000;
                    }
                }

                if (t - tFullShow < cfDt)
                {
                    SdkUtil.logd($"ads full {placement} helper checkShowFull + d:" + cfDt + " dt:" + (t - tFullShow));
                    if (t - tFullImgShow < currConfig.fullImgDeltatime)
                    {
                        if (isLog)
                        {
                            FIRhelper.logEvent("ads_full_return_dtimg");
                        }
                        return 2;
                    }
                    else
                    {
                        return 3;
                    }
                }
            }

            return 1;
        }
        public bool checkFullWillShow(string placement, bool isSplash, int level, bool isShowOnPlaying, bool ischecknumover, bool isIncreaseDeltaCall = false)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            checkResumeAudio();
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(1)) //vvv
            {
                SdkUtil.logd($"ads full {placement} helper checkFullWillShow is removed ads");
                return false;
            }

            if (isShowOnPlaying)
            {
                if (currConfig.fullShowPlaying != 1)
                {
                    SdkUtil.logd($"ads full {placement} helper checkFullWillShow return when not allow show on playing");
                    return false;
                }
            }

            if (count4AdShowing > 0)
            {
                long tc = GameHelper.CurrentTimeMilisReal();
                if ((tc - tShowAdsCheckContinue) >= 30 * 1000)
                {
                    SdkUtil.logd($"ads full {placement} helper checkFullWillShow isAdsShowing and overtime -> reset isAdsShowing=false");
                    setIsAdsShowing(false);
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} helper checkFullWillShow is ads showing");
                    return false;
                }
            }

            if (ischecknumover && !isSplash)
            {
                int countover;
                if (isIncreaseDeltaCall)
                {
                    deltaloverCountFull++;
                    countover = deltaloverCountFull;
                }
                else
                {
                    countover = deltaloverCountFull + 1;
                }
                int overconfig = currConfig.fullDefaultNumover;
                for (int i = 0; i < currConfig.fullListIntervalShow.Count; i++)
                {
                    if (currConfig.fullListIntervalShow[i].startlevel <= level && currConfig.fullListIntervalShow[i].endLevel >= level)
                    {
                        overconfig = currConfig.fullListIntervalShow[i].deltal4Show;
                        SdkUtil.logd($"ads full {placement} helper checkFullWillShow cf interval start= " + currConfig.fullListIntervalShow[i].startlevel + "; end = " +
                        currConfig.fullListIntervalShow[i].endLevel + ", num=" + overconfig);
                        break;
                    }
                }
                SdkUtil.logd($"ads full {placement} helper checkFullWillShow count4ShowFull={countover}; numOverShowFull={overconfig}");
                if (countover < overconfig) return false;
            }

            int scheck = checkShowFull(placement, isSplash, level, true);
            if (scheck != 1 && scheck != 3)
            {
                SdkUtil.logd($"ads full {placement} helper checkFullWillShow checkShowFull={scheck}");
                return false;
            }

            return isFull4Show(placement, isSplash, true);
        }
        /*
         * countLose - number of lose
         */
        public bool showFull(string placement, int level, int countLose, int typeShowImg, int typeShowOnPlaying, bool isWaitAd, bool isHideBtClose, bool ischecknumover = true, AdCallBack cb = null, bool isCheckDeltaTime = true, bool isAllowShow2 = true)
        {
            FIRhelper.logEvent("ads_total_call");
            FIRhelper.logEvent("ads_full_call");
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            checkResumeAudio();
            if (isRemoveAds(0) || isNoAdsTime() || isRemoveAdsInterval() || isDisableAds(1)) //vvv
            {
                SdkUtil.logd($"ads full {placement} helper showFull is removed ads");
                if (isRemoveAds(0))
                {
                    FIRhelper.logEvent("ads_full_return_remove");
                }
                else if (isNoAdsTime())
                {
                    FIRhelper.logEvent("ads_full_return_noadTime");
                }
                else if (isRemoveAdsInterval())
                {
                    FIRhelper.logEvent("ads_full_return_removeInterval");
                }
                else
                {
                    FIRhelper.logEvent("ads_full_return_disable");
                }
                return false;
            }
            if (typeShowOnPlaying > 0)
            {
                if (currConfig.fullShowPlaying != 1)
                {
                    SdkUtil.logd($"ads full {placement} helper showFull return when not allow show on playing");
                    FIRhelper.logEvent("ads_full_return_notShowPlaying");
                    return false;
                }
            }

            if (count4AdShowing > 0)
            {
                long tc = GameHelper.CurrentTimeMilisReal();
                if ((tc - tShowAdsCheckContinue) >= 30 * 1000)
                {
                    SdkUtil.logd($"ads full {placement} helper showFull isAdsShowing and overtime -> reset isAdsShowing=false");
                    setIsAdsShowing(false);
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} helper showFull is ads showing");
                    FIRhelper.logEvent("ads_full_return_showing");
                    return false;
                }
            }

            bool isSplash = false;
            if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
            {
                isSplash = true;
            }

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.fullStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads full {placement} helper showFull step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                }
            }
            if (ischecknumover && !isSplash)
            {
                deltaloverCountFull++;
                int overconfig = currConfig.fullDefaultNumover;
                if (daycf != null && daycf.deltaCall > 0)
                {
                    SdkUtil.logd($"ads full {placement} helper showfull apply day active cf dtcall={daycf.deltaCall}");
                    overconfig = daycf.deltaCall;
                    if (daycf.deltaCall >= 1000000)
                    {
                        FIRhelper.logEvent("ads_full_return_dtCfDayLong");
                        return false;
                    }
                }
                else
                {
                    for (int i = 0; i < currConfig.fullListIntervalShow.Count; i++)
                    {
                        if (currConfig.fullListIntervalShow[i].startlevel <= level && currConfig.fullListIntervalShow[i].endLevel >= level)
                        {
                            overconfig = currConfig.fullListIntervalShow[i].deltal4Show;
                            break;
                        }
                    }
                }

                if (deltaloverCountFull < overconfig)
                {
                    SdkUtil.logd($"ads full {placement} helper showfull count4ShowFull = " + deltaloverCountFull + "; numOverShowFull = " + overconfig);
                    FIRhelper.logEvent("ads_full_return_dshow");
                    return false;
                }
            }


            if (countLose > 0 && currConfig.fullDeltaLose > 1)
            {
                if (fullMemLv4CountLose != level)
                {
                    fullMemLv4CountLose = level;
                    fullStoreCountLose = 0;
                }
                int nc = countLose % currConfig.fullDeltaLose;
                if (nc == 0)
                {
                    nc = currConfig.fullDeltaLose;
                }
                nc += fullStoreCountLose;
                if (nc < currConfig.fullDeltaLose)
                {
                    SdkUtil.logd($"ads full {placement} helper showfull don't match condition lose cf={currConfig.fullDeltaLose} lose={countLose}");
                    FIRhelper.logEvent("ads_full_return_dlose");
                    return false;
                }
                else
                {
                    fullStoreCountLose = currConfig.fullDeltaLose;
                }
            }
            else
            {
                fullMemLv4CountLose = level;
                fullStoreCountLose = 0;
            }

            var cffull = getCfAdsPlacement(placement, -1);
            if (cffull != null)
            {
                if (cffull.flagShow <= 0)
                {
                    SdkUtil.logd($"ads full {placement} helper showFull is disable ads with placement");
                    FIRhelper.logEvent("ads_full_return_disByPl");
                    return false;
                }
                typeShowImg = cffull.flagShow - 1;
            }

            fullRwFlagCount = false;
            int scheck = checkShowFull(placement, isSplash, level, isCheckDeltaTime, true);
            if (scheck != 1 && scheck != 3)
            {
                SdkUtil.logd($"ads full {placement} helper showFull checkShowFull={scheck}");
                if (scheck == 2)
                {
                    loadFull4ThisTurn(placement, isSplash, level, 0, ischecknumover, null);
                }
                return false;
            }
            else
            {
                if (scheck == 3)
                {
                    SdkUtil.logd($"ads full {placement} helper showFull show img replace");
                    FIRhelper.logEvent("ads_full_changeImgByDt");
                    isAllowShow2 = false;
                    if (typeShowImg <= 0)
                    {
                        typeShowImg = 4;
                    }
                }
                else if (checkShowLogicFullRw(placement))
                {
                    SdkUtil.logd($"ads full {placement} helper showFull show rw logic");
                    fullRwFlagCount = true;
                    typeShowImg = currConfig.fullRwType + 10;
                }
            }
            FIRhelper.logEvent("ads_total_doShow");
            FIRhelper.logEvent("ads_full_doShow");
            FIRhelper.logEvent("show_ads_full_call");
            if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
            {
                FIRhelper.logEvent("ads_full_doShow_resume");
                FIRhelper.logEvent("show_ads_full_resume_call");
            }
            SdkUtil.logd($"ads full {placement} helper showFull call level={level} isHideBtClose={isHideBtClose}");
            _cbFullShow = cb;
            levelCurr4Full = level;
            StatusShowiso = 0;

#if UNITY_EDITOR
            if (isFull4Show(placement, isSplash, false))
            {
                _cbFullEditor = cb;
                tFullImgShow = GameHelper.CurrentTimeMilisReal();
                if (scheck != 3)
                {
                    tFullShow = tFullImgShow;
                }

                deltaloverCountFull = 0;
                fullStoreCountLose = 0;
                subCountShowFull();
                showFullEditor();
                int countfull4pointe = PlayerPrefs.GetInt("count_full_4_point", 0);
                countfull4pointe++;
                AnalyticCommonParam.Instance().countShowAdsFull = countfull4pointe;
                PlayerPrefs.SetInt("count_full_4_point", countfull4pointe);
                checkLogVipAds();
                return true;
            }

            loadFull4ThisTurn(placement, isSplash, level, 0, ischecknumover, null);
            return false;
#endif
            if (stepCurrShow == null || stepCurrShow.stepShowCircle.Count == 0)
            {
                StepLevelShowfull cfsteplv = currConfig.GetStepLevelShowfull(level);
                if (cfsteplv != null)
                {
                    SdkUtil.logd($"ads full {placement} helper showFull step with lv:{cfsteplv.startlevel}-{cfsteplv.endLevel}");
                    stepCurrShow = cfsteplv.fullStep;
                    if (lvFullStepStart != cfsteplv.startlevel && lvFullStepEnd != cfsteplv.endLevel)
                    {
                        idxFullShowCircle = 0;
                        lvFullStepStart = cfsteplv.startlevel;
                        lvFullStepEnd = cfsteplv.endLevel;
                    }
                }
            }
            if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
            {
                if (idxFullShowCircle >= stepCurrShow.stepShowCircle.Count)
                {
                    idxFullShowCircle = 0;
                }
            }
            else
            {
                if (idxFullShowCircle >= currConfig.fullStepShowCircle.Count)
                {
                    idxFullShowCircle = 0;
                }
            }
            nameShowFullGift = fullWillShow(placement, stepCurrShow);
            string full2WS = full2WillShow(placement, level);
            statusAds2 = 0;
            if (!showFullCircle(placement, typeShowImg, stepCurrShow, typeShowOnPlaying, isWaitAd, ischecknumover, isAllowShow2, scheck == 3, full2WS.Length > 0, isHideBtClose))
            {
                FIRhelper.logEvent("ads_total_show_fail");
                FIRhelper.logEvent("ads_full_show_fail");
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    FIRhelper.logEvent("ads_full_show_fail_notCon");
                }
                _cbFullShow = null;
                if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
                {
                    loadFull4ThisTurn(AdsBase.PLFullDefault, isSplash, level, 0, ischecknumover, null);
                }
                else
                {
                    loadFull4ThisTurn(placement, isSplash, level, 0, ischecknumover, null);
                }
                return false;
            }
            else
            {
                isFullCallMissCB = false;
                deltaloverCountFull = 0;
                fullStoreCountLose = 0;
                AdjustHelper.LogEvent(AdjustEventName.AdsFull);
                AdjustHelper.LogEvent(AdjustEventName.AdsTotal);
                Myapi.LogEventApi.Instance().LogAds(Myapi.AdsTypeLog.Interstitial, placement);//vvv

                float timeNtdl = 2;
#if UNITY_IOS || UNITY_IPHONE
                statuschekAdsGiftErr = 0;
                //Invoke("waitStatusAdsShowErr", 3);
                isPauseAudio = true;
                AudioListener.pause = true;
                timeNtdl = 0.5f;
#elif UNITY_ANDROID
                statuschekAdsFullOpenErr = 1;
#endif
                FIRhelper.logEvent("ads_full2_call");
                FIRhelper.logEvent("ads_full2_call_full");
                statusAds2++;
                if (isQcThu && FIRhelper.Instance.isAdSkip != 97 && (isApplyLogicSkip == 1 || isApplyLogicSkip == 3))
                {
                    SdkUtil.logd($"ads full {placement} helper showFull2 chang ko sho do ad thu niem !!!!!!!!!!!!");
                    isAllowShow2 = false;
                }
                if (isAllowShow2 && (currConfig.full2TypeShow == 1 || currConfig.full2TypeShow == 3) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                {
                    if (currTypeFullGiftShow == 26)
                    {
                        timeNtdl = 0;
                    }
                    bool isShow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, timeNtdl, nameShowFullGift, -1, true, cb);
                    if (isShow2)
                    {
                        statusAds2++;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} helper showFull2 isAllowShow2={isAllowShow2} type2={currConfig.full2TypeShow}");
                    if (!isAllowShow2)
                    {
                        FIRhelper.logEvent("ads_full2_f_return_notAlow");
                    }
                    else if ((currConfig.fullFlagFor2vscl & 1) <= 0)
                    {
                        FIRhelper.logEvent("ads_full2_f_return_flag2vscl");
                    }
                    else
                    {
                        FIRhelper.logEvent("ads_full2_f_return_type");
                    }
                }

                int countfull4point = PlayerPrefs.GetInt("count_full_4_point", 0);
                countfull4point++;
                AnalyticCommonParam.Instance().countShowAdsFull = countfull4point;
                PlayerPrefs.SetInt("count_full_4_point", countfull4point);
                checkLogVipAds();

                if (CBShowFullGift != null)
                {
                    CBShowFullGift(1);
                }
                return true;
            }
        }
        private bool showFullCircle(string placement, int typeShowImg, StepShow stepShow, int typeShowOnPlaying, bool isWaitAd, bool ischecknumover, bool isAllowShow2, bool isReplaceimgtime, bool isDelayShow, bool isHideBtClose)
        {
            SdkUtil.logd($"ads full {placement} helper showFullCircle idxFullShowCircle={idxFullShowCircle}, typeShowImg={typeShowImg} isAllowShow2={isAllowShow2} isReplaceimgtime={isReplaceimgtime}, isDelayShow={isDelayShow} isHideBtClose={isHideBtClose}");
            bool re = false;
            setIsAdsShowing(false);
            List<int> list4Show = new List<int>();
            int count4Circle = 0;
            if (typeShowImg > 0)
            {
                if (typeShowImg == 1)
                {
                    list4Show.Add(20);
                }
                else if (typeShowImg == 2)
                {
                    list4Show.Add(10);
                }
                else if (typeShowImg == 3)
                {
                    list4Show.Add(60);
                }
                else if (typeShowImg == 4)
                {
                    list4Show.Add(20);
                    list4Show.Add(70);
                    list4Show.Add(10);
                    list4Show.Add(60);
                }
                else if (typeShowImg == 5)
                {
                    list4Show.Add(0);
                    list4Show.Add(20);
                    list4Show.Add(70);
                    list4Show.Add(10);
                    list4Show.Add(60);
                }
                else if (typeShowImg == 6)
                {
                    list4Show.Add(70);
                }
                else if (typeShowImg == 7)
                {
                    list4Show.Add(70);
                    list4Show.Add(20);
                }
                count4Circle = -1;
            }
            if (list4Show.Count <= 0)
            {
                if (isQcThu && FIRhelper.Instance.isAdSkip != 97 && (isApplyLogicSkip == 1 || isApplyLogicSkip == 3))
                {
                    SdkUtil.logd($"ads full adshelper isChangeLogicShowfullwhenSkip idxFullShowCircle={idxFullShowCircle} idxFullShowCircle4Skip={idxFullShowCircle4Skip}");
#if ENABLE_ADS_IRON
                    list4Show.Add(3);
#elif ENABLE_ADS_MAX
                    list4Show.Add(6);
#else
                    list4Show.Add(0);
#endif
                    if (idxFullShowCircle4Skip < 0)
                    {
                        idxFullShowCircle4Skip = 0;
                    }
                    idxFullShowCircle = idxFullShowCircle4Skip;
                    if (idxFullShowCircle > 0)
                    {
                        list4Show.Insert(0, 20);
                    }
                    else
                    {
                        list4Show.Add(20);
                    }
                    List<int> listShowRe = currConfig.fullStepShowRe;
                    list4Show.AddRange(listShowRe);
                    count4Circle = 2;
                }
                else
                {
                    List<int> listShowCircle = currConfig.fullStepShowCircle;
                    List<int> listShowRe = currConfig.fullStepShowRe;
                    if (stepShow != null && stepShow.stepShowCircle.Count > 0)
                    {
                        listShowCircle = stepShow.stepShowCircle;
                        listShowRe = stepShow.stepShowRe;
                    }
                    if (idxFullShowCircle >= listShowCircle.Count)
                    {
                        idxFullShowCircle = 0;
                    }
                    int idxtmp = idxFullShowCircle;
                    for (int i = 0; i < listShowCircle.Count; i++)
                    {
                        list4Show.Add(listShowCircle[idxtmp]);
                        idxtmp++;
                        if (idxtmp >= listShowCircle.Count)
                        {
                            idxtmp = 0;
                        }
                    }
                    list4Show.AddRange(listShowRe);
                    count4Circle = listShowCircle.Count;
                }
            }
            float tsdl = 0;
            if (isDelayShow)
            {
#if UNITY_IOS || UNITY_IPHONE
                tsdl = 1.1f;
#else
                tsdl = 0.1f;
#endif
            }
            float tWaitAniShow = 0;
            if (isWaitAd)
            {
                tWaitAniShow = AppConfig.TimeAnimShowWaitAd;
            }
            if (SdkUtil.isLog() && stepShow != null)
            {
                string sstel = "";
                for (int ii = 0; ii < list4Show.Count; ii++)
                {
                    sstel += "" + list4Show[ii] + ",";
                }
                SdkUtil.logd($"ads full {placement} helper showFullCircle step={sstel}");
            }
            if (typeShowImg == 10)
            {
                list4Show.Insert(0, 24);
            }
            else if (typeShowImg == 11)
            {
                list4Show.Insert(0, 25);
            }
            else if (typeShowImg == 12)
            {
                list4Show.Insert(0, 24);
                list4Show.Insert(1, 25);
            }
            else if (typeShowImg == 13)
            {
                list4Show.Insert(0, 25);
                list4Show.Insert(1, 24);
            }
            else if (typeShowImg == 14)
            {
                int addidx = 0;
                if (adsIron != null && adsIron.isEnable && currConfig.fullStepShowCircle.Contains(3))
                {
                    list4Show.Insert(addidx++, 30);//ir rewarded
                }
                if (adsApplovinMax != null && adsApplovinMax.isEnable && currConfig.fullStepShowCircle.Contains(6))
                {
                    list4Show.Insert(addidx++, 62);//max rewarded
                }
                list4Show.Insert(addidx++, 25);
                list4Show.Insert(addidx++, 24);
            }
            for (int ii = 0; ii < list4Show.Count; ii++)
            {
                if (ii < count4Circle)
                {
                    idxFullShowCircle++;
                    if (idxFullShowCircle >= count4Circle)
                    {
                        idxFullShowCircle = 0;
                    }
                    if (idxFullShowCircle4Skip >= 0)
                    {
                        idxFullShowCircle4Skip++;
                        if (idxFullShowCircle4Skip >= count4Circle)
                        {
                            idxFullShowCircle4Skip = 0;
                        }
                    }
                }
                SdkUtil.logd($"ads full {placement} helper showFullCircle adshow={list4Show[ii]} ii={ii} idxFullShowCircle={idxFullShowCircle} count4Circle={count4Circle}");
                AdsBase adShow = getAdsFromId(list4Show[ii]);
                if (adShow != null
                    && list4Show[ii] != 20
                    && list4Show[ii] != 24
                    && list4Show[ii] != 25
                    && list4Show[ii] != 26
                    && list4Show[ii] != 30
                    && list4Show[ii] != 60
                    && list4Show[ii] != 62
                    && list4Show[ii] != 70
                    && list4Show[ii] != 71
                    && adShow.getFullLoaded(placement) > 0)
                {
                    nameShowFullGift = adShow.getFullNetName();
                    bool iss = adShow.showFull(placement, tWaitAniShow + tsdl, false, (stateshow) =>
                    {
                        SdkUtil.logd($"ads full {placement} helper showFullCircle callback ads=" + adShow.adsType + ", state=" + stateshow.ToString());
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFullOpenErr = 0;
                            statuschekAdsFullCloseErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, true, false, typeShowOnPlaying != 2, isReplaceimgtime);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 4 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                            {
                                bool isshow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, 0, "", 0, true, _cbFullShow);
                                if (isshow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                SdkUtil.logd($"ads full {placement} helper showFullCircle callback call close = " + _cbFullShow);
                                if (_cbFullShow != null)
                                {
                                    _cbFullShow(stateshow);
                                }
                            }
                        }
                        else
                        {
                            if (stateshow == AD_State.AD_SHOW)
                            {
                                bgFullGift.SetActive(false);
                                SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                                //if (statuschekAdsFullErr == 1)
                                //{
                                //    statuschekAdsFullErr = 2;
                                //    Invoke("waitStatusAdsCloseErr", 5);
                                //}
#endif
                            }
                            if (_cbFullShow != null)
                            {
                                _cbFullShow(stateshow);
                            }
                        }
                    });
                    if (iss)
                    {
                        re = true;
                        statuschekAdsFullCloseErr = 1;
                        subCountShowFull();
                        whenShowFullOk(isWaitAd, false, adShow.adsType, typeShowOnPlaying != 2, isReplaceimgtime);
                        if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
                        {
                            FIRhelper.logEvent("show_ads_full_resume");
                        }
                        break;
                    }
                }
                else if (adShow != null
                    && ((list4Show[ii] == 20 && currConfig.fullNtIsIc != 1) || list4Show[ii] == 60 || list4Show[ii] == 70 || list4Show[ii] == 71)
                    && adShow.getNativeFullLoaded(placement) > 0)
                {
                    int timeShowBtClose = PlayerPrefs.GetInt("cf_time_close_ntfull", 5);
                    if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
                    {
                        timeShowBtClose = 5;
                    }
                    nameShowFullGift = adShow.getFullNetName();
                    bool iss = adShow.showNativeFull(placement, tWaitAniShow, (int)(tsdl * 1000), isHideBtClose, false, timeShowBtClose, true, (stateshow) =>
                    {
                        SdkUtil.logd($"ads fullnt {placement} helper showFullCircle callback state=" + stateshow.ToString());
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFullOpenErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, true, false, typeShowOnPlaying != 2, isReplaceimgtime);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 4 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                            {
                                bool isshow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, 0, "", 0, true, _cbFullShow);
                                if (isshow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                SdkUtil.logd($"ads fullnt {placement} helper showFullCircle callback call close = " + _cbFullShow);
                                if (_cbFullShow != null)
                                {
                                    _cbFullShow(stateshow);
                                }
                            }
                        }
                        else
                        {
                            if (stateshow == AD_State.AD_SHOW)
                            {
                                bgFullGift.SetActive(false);
                                SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                                //if (statuschekAdsFullErr == 1)
                                //{
                                //    statuschekAdsFullErr = 2;
                                //    Invoke("waitStatusAdsCloseErr", 5);
                                //}
#endif
                            }
                            if (_cbFullShow != null)
                            {
                                _cbFullShow(stateshow);
                            }
                        }
                    });
                    if (iss)
                    {
                        re = true;
                        subCountShowFull();
                        whenShowFullOk(isWaitAd, false, -list4Show[ii], typeShowOnPlaying != 2, isReplaceimgtime);
                        if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
                        {
                            FIRhelper.logEvent("show_ads_full_resume");
                        }
                        break;
                    }
                }
                else if (adShow != null && list4Show[ii] == 20 && currConfig.fullNtIsIc == 1 && adShow.getNativeIcFullLoaded(placement) > 0)
                {
                    int timeShowBtClose = PlayerPrefs.GetInt("cf_time_close_ntfull", 5);
                    if (AdsBase.PLFullSplash.CompareTo(placement) == 0)
                    {
                        timeShowBtClose = 5;
                    }
                    nameShowFullGift = adShow.getFullNetName();
                    bool iss = adShow.showNativeIcFull(placement, tWaitAniShow, (int)(tsdl * 1000), isHideBtClose, false, timeShowBtClose, true, (stateshow) =>
                    {
                        SdkUtil.logd($"ads fullnt {placement} helper showFullCircle callback state=" + stateshow.ToString());
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFullOpenErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, true, false, typeShowOnPlaying != 2, isReplaceimgtime);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 4 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                            {
                                bool isshow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, 0, "", 0, true, _cbFullShow);
                                if (isshow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                SdkUtil.logd($"ads fullnt {placement} helper showFullCircle callback call close = " + _cbFullShow);
                                if (_cbFullShow != null)
                                {
                                    _cbFullShow(stateshow);
                                }
                            }
                        }
                        else
                        {
                            if (stateshow == AD_State.AD_SHOW)
                            {
                                bgFullGift.SetActive(false);
                                SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                                //if (statuschekAdsFullErr == 1)
                                //{
                                //    statuschekAdsFullErr = 2;
                                //    Invoke("waitStatusAdsCloseErr", 5);
                                //}
#endif
                            }
                            if (_cbFullShow != null)
                            {
                                _cbFullShow(stateshow);
                            }
                        }
                    });
                    if (iss)
                    {
                        re = true;
                        subCountShowFull();
                        whenShowFullOk(isWaitAd, false, -list4Show[ii], typeShowOnPlaying != 2, isReplaceimgtime);
                        if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
                        {
                            FIRhelper.logEvent("show_ads_full_resume");
                        }
                        break;
                    }
                }
                else if (adShow != null && list4Show[ii] == 26 && adShow.getNtPgFullLoaded(placement) > 0)
                {
                    nameShowFullGift = adShow.getFullNetName();
                    bool iss = adShow.showNtPgFull(placement, tWaitAniShow + tsdl + 0.2f, isHideBtClose, (stateshow) =>
                    {
                        SdkUtil.logd($"ads fullntpg {placement} helper showFullCircle callback state=" + stateshow.ToString());
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFullOpenErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, true, false, typeShowOnPlaying != 2, isReplaceimgtime);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 4 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                            {
                                bool isshow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, 0, "", 0, true, _cbFullShow);
                                if (isshow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                SdkUtil.logd($"ads full npg {placement} helper showFullCircle callback call close = " + _cbFullShow);
                                if (_cbFullShow != null)
                                {
                                    _cbFullShow(stateshow);
                                }
                            }
                        }
                        else
                        {
                            if (stateshow == AD_State.AD_SHOW)
                            {
                                bgFullGift.SetActive(false);
                                SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                                //if (statuschekAdsFullErr == 1)
                                //{
                                //    statuschekAdsFullErr = 2;
                                //    Invoke("waitStatusAdsCloseErr", 5);
                                //}
#endif
                            }
                            if (_cbFullShow != null)
                            {
                                _cbFullShow(stateshow);
                            }
                        }
                    });
                    if (iss)
                    {
                        re = true;
                        subCountShowFull();
                        whenShowFullOk(isWaitAd, false, -list4Show[ii], typeShowOnPlaying != 2, isReplaceimgtime);
                        if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
                        {
                            FIRhelper.logEvent("show_ads_full_resume");
                        }
                        break;
                    }
                }
                else if (adShow != null && ((list4Show[ii] == 24 && adShow.getFullRwInterLoaded(placement) > 0) || (list4Show[ii] == 25 && adShow.getFullRwRwLoaded(placement) > 0)))
                {
                    bool iss = false;
                    if (list4Show[ii] == 24)
                    {
                        nameShowFullGift = adShow.getFullRwInterNetName();
                        iss = adShow.showFullRwInter(placement, tWaitAniShow + tsdl, false, cbfullrw);
                    }
                    else
                    {
                        nameShowFullGift = adShow.getFullRwRwNetName();
                        iss = adShow.showFullRwRw(placement, tWaitAniShow + tsdl, false, cbfullrw);
                    }
                    void cbfullrw(AD_State stateshow)
                    {
                        SdkUtil.logd($"ads full {placement} helper showFullCircle callback ads=" + adShow.adsType + ", state=" + stateshow.ToString());
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFullOpenErr = 0;
                            statuschekAdsFullCloseErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, true, false, typeShowOnPlaying != 2, isReplaceimgtime);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 4 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                            {
                                bool isshow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, 0, "", 0, true, _cbFullShow);
                                if (isshow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                SdkUtil.logd($"ads full {placement} helper showFullCircle callback call close = " + _cbFullShow);
                                if (_cbFullShow != null)
                                {
                                    _cbFullShow(stateshow);
                                }
                            }
                        }
                        else
                        {
                            if (stateshow == AD_State.AD_SHOW)
                            {
                                bgFullGift.SetActive(false);
                                SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                                //if (statuschekAdsFullErr == 1)
                                //{
                                //    statuschekAdsFullErr = 2;
                                //    Invoke("waitStatusAdsCloseErr", 5);
                                //}
#endif
                            }
                            if (_cbFullShow != null)
                            {
                                _cbFullShow(stateshow);
                            }
                        }
                    }
                    if (iss)
                    {
                        re = true;
                        statuschekAdsFullCloseErr = 1;
                        fullRwTimeShow = SdkUtil.CurrentTimeMilis();
                        fullRwNumTotal++;
                        fullRwNumSession++;
                        PlayerPrefs.SetInt("mem_num_fullrw", fullRwNumTotal);
                        PlayerPrefs.SetInt("mem_fullrw_tshow", (int)(fullRwTimeShow / 1000));
                        subCountShowFull();
                        whenShowFullOk(isWaitAd, false, -list4Show[ii], typeShowOnPlaying != 2, isReplaceimgtime);
                        if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
                        {
                            FIRhelper.logEvent("show_ads_full_resume");
                        }
                        break;
                    }
                }
                else if (adShow != null && (list4Show[ii] == 30 || list4Show[ii] == 62) && adShow.getGiftLoaded(placement) > 0)
                {
                    nameShowFullGift = adShow.getGiftNetName();
                    bool iss = adShow.showGift(placement, tWaitAniShow + tsdl, cbfullrw);
                    void cbfullrw(AD_State stateshow)
                    {
                        SdkUtil.logd($"ads full {placement} helper showFullCircle callback ads=" + adShow.adsType + ", state=" + stateshow.ToString());
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFullOpenErr = 0;
                            statuschekAdsFullCloseErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, true, false, typeShowOnPlaying != 2, isReplaceimgtime);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 4 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 1) > 0))
                            {
                                bool isshow2 = showFull2(placement, levelCurr4Full, isHideBtClose, true, typeShowOnPlaying, isWaitAd, ischecknumover, 0, "", 0, true, _cbFullShow);
                                if (isshow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                SdkUtil.logd($"ads full {placement} helper showFullCircle callback call close = " + _cbFullShow);
                                if (_cbFullShow != null)
                                {
                                    _cbFullShow(stateshow);
                                }
                            }
                        }
                        else
                        {
                            if (stateshow == AD_State.AD_SHOW)
                            {
                                bgFullGift.SetActive(false);
                                SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                                //if (statuschekAdsFullErr == 1)
                                //{
                                //    statuschekAdsFullErr = 2;
                                //    Invoke("waitStatusAdsCloseErr", 5);
                                //}
#endif
                            }
                            if (_cbFullShow != null)
                            {
                                _cbFullShow(stateshow);
                            }
                        }
                    }
                    if (iss)
                    {
                        re = true;
                        statuschekAdsFullCloseErr = 1;
                        fullRwTimeShow = SdkUtil.CurrentTimeMilis();
                        fullRwNumTotal++;
                        fullRwNumSession++;
                        PlayerPrefs.SetInt("mem_num_fullrw", fullRwNumTotal);
                        PlayerPrefs.SetInt("mem_fullrw_tshow", (int)(fullRwTimeShow / 1000));
                        subCountShowFull();
                        whenShowFullOk(isWaitAd, false, -list4Show[ii], typeShowOnPlaying != 2, isReplaceimgtime);
                        if (placement != null && (placement.CompareTo(AdsBase.PLFullSplash) == 0 || placement.CompareTo(AdsBase.PLFullResume) == 0))
                        {
                            FIRhelper.logEvent("show_ads_full_resume");
                        }
                        break;
                    }
                }
            }
            return re;
        }
        private string fullWillShow(string placement, StepShow stepShow)
        {
            string re = "";
            List<int> list4Show = new List<int>();
            List<int> listShowCircle = currConfig.fullStepShowCircle;
            List<int> listShowRe = currConfig.fullStepShowRe;
            if (stepShow != null && stepShow.stepShowCircle.Count > 0)
            {
                listShowCircle = stepShow.stepShowCircle;
                listShowRe = stepShow.stepShowRe;
            }
            if (idxFullShowCircle >= listShowCircle.Count)
            {
                idxFullShowCircle = 0;
            }
            int idxtmp = idxFullShowCircle;
            for (int i = 0; i < listShowCircle.Count; i++)
            {
                list4Show.Add(listShowCircle[idxtmp]);
                idxtmp++;
                if (idxtmp >= listShowCircle.Count)
                {
                    idxtmp = 0;
                }
            }
            list4Show.AddRange(listShowRe);

            for (int ii = 0; ii < list4Show.Count; ii++)
            {
                AdsBase adShow = getAdsFromId(list4Show[ii]);
                if (adShow != null
                    && list4Show[ii] != 20 && list4Show[ii] != 60 && list4Show[ii] != 70 && list4Show[ii] != 71
                    && adShow.getFullLoaded(placement) > 0)
                {
                    re = adShow.getFullNetName();
                    break;
                }
                else if (adShow != null && list4Show[ii] == 20)
                {
                    if (currConfig.fullNtIsIc != 1 && adShow.getNativeFullLoaded(placement) > 0)
                    {
                        re = "";
                        break;
                    }
                    else if (currConfig.fullNtIsIc == 1 && adShow.getNativeIcFullLoaded(placement) > 0)
                    {
                        re = "";
                        break;
                    }
                }
                else if (adShow != null && list4Show[ii] == 60 && adShow.getNativeFullLoaded(placement) > 0)
                {
                    re = "max";
                    break;
                }
                else if (adShow != null && (list4Show[ii] == 70 || list4Show[ii] == 71) && adShow.getNativeFullLoaded(placement) > 0)
                {
                    re = "fb";
                    break;
                }
            }
            return re;
        }

        public bool isFull24Show(string placement)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            checkResumeAudio();

#if UNITY_EDITOR
            return adsEditorCtr.isFullEditorLoaded;
#endif
            for (int i = 0; i < currConfig.full2StepShowCircle.Count; i++)
            {
                AdsBase adCurr = getAdsFromId(currConfig.full2StepShowCircle[i]);
                if (adCurr != null)
                {
                    if (currConfig.full2StepShowCircle[i] == 20
                        || currConfig.full2StepShowCircle[i] == 60
                        || currConfig.full2StepShowCircle[i] == 70
                        || currConfig.full2StepShowCircle[i] == 71)
                    {
                        if (currConfig.full2StepShowCircle[i] == 20 && currConfig.fullNtIsIc != 1)
                        {
                            if (adCurr.getNativeFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull24Show m true = " + currConfig.full2StepShowCircle);
                                return true;
                            }
                        }
                        else
                        {
                            if (adCurr.getNativeIcFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull24Show m true = " + currConfig.full2StepShowCircle);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (adCurr.getFullLoaded(placement) > 0)
                        {
                            SdkUtil.logd($"ads full {placement} helper isFull24Show m true = " + currConfig.full2StepShowCircle);
                            return true;
                        }
                    }
                }
            }
            for (int i = 0; i < currConfig.full2StepShowRe.Count; i++)
            {
                AdsBase adCurr = getAdsFromId(currConfig.full2StepShowRe[i]);
                if (adCurr != null > 0)
                {
                    if (currConfig.full2StepShowRe[i] == 20
                        || currConfig.full2StepShowRe[i] == 60
                        || currConfig.full2StepShowRe[i] == 70
                        || currConfig.full2StepShowRe[i] == 71)
                    {
                        if (currConfig.full2StepShowRe[i] == 20 && currConfig.fullNtIsIc != 1)
                        {
                            if (adCurr.getNativeFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull24Show r true = " + currConfig.full2StepShowRe);
                                return true;
                            }
                        }
                        else
                        {
                            if (adCurr.getNativeIcFullLoaded(placement) > 0)
                            {
                                SdkUtil.logd($"ads full {placement} helper isFull24Show r true = " + currConfig.full2StepShowRe);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (adCurr.getFullLoaded(placement) > 0)
                        {
                            SdkUtil.logd($"ads full {placement} helper isFull24Show f true = " + currConfig.full2StepShowRe);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private int checkShowFull2(string placement, int level, bool is4Load = false, byte isLog = 0)
        {
            if (is4Load)
            {
                level++;
            }
            if (((currConfig.fullFlagFor2vscl & 3) <= 0))
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull2 not meet fullFlagFor2vscl={currConfig.fullFlagFor2vscl}");
                if (isLog == 1)
                {
                    FIRhelper.logEvent("ads_full2_return_flag2vscl");
                }
                return -2;
            }
            if (level < currConfig.full2LevelStart)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull2 not meet lv={level} cflv={currConfig.full2LevelStart}");
                if (isLog == 1)
                {
                    FIRhelper.logEvent("ads_full2_return_lvStart");
                }
                return 0;
            }
            if (countFullShowOfDay <= 0)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull2 over num show of day, to=" + currConfig.fullTotalOfday);
                if (isLog == 1)
                {
                    FIRhelper.logEvent("ads_full2_return_overOfDay");
                }
                return -1;
            }
            int countfull = PlayerPrefs.GetInt("count_full_4_point", 0);
            if (countfull < currConfig.full2CountFull)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull2 not meet count full: cfcount={currConfig.full2CountFull}, count={countfull}");
                if (isLog == 1)
                {
                    FIRhelper.logEvent("ads_full2_return_countFull");
                }
                return 0;
            }
            long t = GameHelper.CurrentTimeMilisReal();
            if (SDKManager.Instance.timeEnterGame < currConfig.full2Starttime || SDKManager.Instance.timeEnterSession < currConfig.full2Sessiontime)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull2 not meet start time or session time: cfssart={currConfig.full2Starttime}, sst={currConfig.full2Sessiontime}");
                if (SDKManager.Instance.timeEnterGame < 20 && SDKManager.Instance.timeEnterSession < 20)
                {
                    if (isLog == 1)
                    {
                        FIRhelper.logEvent("ads_full2_return_tEnterGame");
                    }
                    return 2;
                }
                else
                {
                    if (isLog == 1)
                    {
                        if (SDKManager.Instance.timeEnterGame < currConfig.full2Starttime)
                        {
                            FIRhelper.logEvent("ads_full2_return_tStart");
                        }
                        else
                        {
                            FIRhelper.logEvent("ads_full2_return_sessTime");
                        }
                    }
                    return 0;
                }
            }
            if (t - tFull2Show < currConfig.full2Deltatime)
            {
                SdkUtil.logd($"ads full {placement} helper checkShowFull2 + d:" + currConfig.full2Deltatime + " dt:" + (t - tFull2Show));
                if (isLog == 1)
                {
                    FIRhelper.logEvent("ads_full2_return_dt");
                }
                return 2;
            }
            return 1;
        }
        private void loadFull2(string placement, int level, AdCallBack cb)
        {
            if (placement.StartsWith("gift_"))
            {
                placement = placement.Replace("gift_", "full_2_");
            }
            else if (!placement.StartsWith("full_2_"))
            {
                if (placement.StartsWith("full_"))
                {
                    placement = placement.Replace("full_", "full_2_");
                }
                else
                {
                    placement = "full_2_" + placement;
                }
            }
            checkResumeAudio();
            if (isRemoveAds(0) || isNoAdsTime() || isDisableAds(1)) //vvv
            {
                SdkUtil.logd($"ads full {placement} helper loadFull2 is removee ads");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (AutoPerformanceOptimizer.isChangeLogicAdsLowRam == 101)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull2 not load because device very low");
                return;
            }
            if (currConfig.full2LevelStart > level)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull2 is not pass level lvcf={currConfig.full2LevelStart}");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            if (countFullShowOfDay <= 0)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull2 over num show of day, to=" + currConfig.fullTotalOfday);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            int s2check = checkShowFull2(placement, level, true);
            if (s2check <= 0)
            {
                SdkUtil.logd($"ads full {placement} helper loadFull2 checkShowFull2={s2check}");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }

            SdkUtil.logd($"ads full {placement} helper loadFull2 load");
            if (currConfig.full2StepShowCircle.Count == 0)
            {
                initListFull2();
            }
            if (idxFull2ShowCircle >= currConfig.full2StepShowCircle.Count)
            {
                idxFull2ShowCircle = 0;
            }
            if (currConfig.full2StepShowCircle.Count <= 0)
            {
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            idxFull2Load = idxFull2ShowCircle;
            stepFull2Load = 0;
            countFull2Load = currConfig.full2StepShowCircle.Count;
            loadFull2Circle(placement, cb);
        }
        private void loadFull2Circle(string placement, AdCallBack cb)
        {
#if UNITY_EDITOR
            if (!adsEditorCtr.isFullEditorLoading && !adsEditorCtr.isFullEditorLoaded)
            {
                loadFullEditor();
            }
            return;
#endif
            SdkUtil.logd($"ads full {placement} helper loadFull2Circle step2load={stepFull2Load} idx={idxFull2Load}, countLoad={countFull2Load}");
            if (countFull2Load > 0)
            {
                int idxcurr = idxFull2Load;
                idxFull2Load++;
                List<int> list2curr = null;
                if (stepFull2Load == 0)
                {
                    list2curr = currConfig.full2StepShowCircle;
                    if (idxFull2Load >= currConfig.full2StepShowCircle.Count)
                    {
                        idxFull2Load = 0;
                    }
                }
                else
                {
                    list2curr = currConfig.full2StepShowRe;
                    if (idxFullLoad >= currConfig.full2StepShowRe.Count)
                    {
                        idxFullLoad = 0;
                    }
                }
                countFull2Load--;
                if (countFull2Load <= 0 && stepFull2Load == 0)
                {
                    stepFull2Load = 1;
                    idxFull2Load = 0;
                    countFull2Load = currConfig.full2StepShowRe.Count;
                }
                if (list2curr != null && list2curr.Count > idxcurr && idxcurr >= 0)
                {
                    AdsBase adscurr = getAdsFromId(list2curr[idxcurr]);
                    if (adscurr != null)
                    {
                        int typecurrload = list2curr[idxcurr];
                        if (list2curr[idxcurr] == 20 && adscurr.adsType == 0)
                        {
                            if (currConfig.fullNtIsIc != 1)
                            {
                                adscurr.loadNativeFull(placement, cbload);
                            }
                            else
                            {
                                adscurr.loadNativeIcFull(placement, cbload);
                            }
                        }
                        else if (list2curr[idxcurr] == 21 && adscurr.adsType == 0)
                        {
#if UNITY_ANDROID
                            adscurr.loadOpenAd(placement, cbload);
#else
                            cbload(AD_State.AD_LOAD_FAIL);
#endif
                        }
                        else if (list2curr[idxcurr] == 10)
                        {
#if UNITY_ANDROID
                            adscurr.loadFull(placement, cbload);
#else
                            cbload(AD_State.AD_LOAD_FAIL);
#endif
                        }
                        else if (list2curr[idxcurr] == 60)
                        {
                            adscurr.loadNativeFull(placement, cbload);
                        }
                        else if (list2curr[idxcurr] == 61)
                        {
#if UNITY_ANDROID
                            adscurr.loadOpenAd(placement, cbload);
#else
                            cbload(AD_State.AD_LOAD_FAIL);
#endif
                        }
                        else if (list2curr[idxcurr] == 70 || list2curr[idxcurr] == 71)
                        {
                            adscurr.loadNativeFull(placement, cbload);
                        }
                        else
                        {
                            loadFull2Circle(placement, cb);
                        }
                        void cbload(AD_State stateLoad)
                        {
                            SdkUtil.logd($"ads full {placement} helper loadFull2Circle cbload stateLoad={stateLoad}");
                            if (stateLoad == AD_State.AD_LOAD_OK)
                            {
                                if (cb != null)
                                {
                                    cb(AD_State.AD_LOAD_OK);
                                }
                                //if (typecurrload != 10 && (currConfig.full2StepShowCircle.Contains(60) || currConfig.full2StepShowRe.Contains(60)))
                                //{
                                //    if (adsAdmobMy != null)
                                //    {
                                //        adsAdmobMy.loadNativeFull(placement, null);
                                //    }
                                //}
                            }
                            else
                            {
                                loadFull2Circle(placement, cb);
                            }
                        }
                    }
                    else
                    {
                        loadFull2Circle(placement, cb);
                    }
                }
                else
                {
                    if (cb != null)
                    {
                        cb(AD_State.AD_LOAD_FAIL);
                    }
                }
            }
            else
            {
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
        }
        private bool showFull2(string placement, int level, bool isHideBtClose, bool isFull, int typeShowOnPlaying, bool isWaitAd, bool ischecknumover, float timeNtdl, string netFG, int stateShowAfter, bool isautoClosentfullwhenClick, AdCallBack __cb = null)
        {
            checkResumeAudio();
            if (placement.StartsWith("gift_"))
            {
                placement = placement.Replace("gift_", "full_2_");
            }
            else if (!placement.StartsWith("full_2_"))
            {
                if (placement.StartsWith("full_"))
                {
                    placement = placement.Replace("full_", "full_2_");
                }
                else
                {
                    placement = "full_2_" + placement;
                }
            }
            if (isRemoveAds(0) || isNoAdsTime() || isRemoveAdsInterval() || isDisableAds(1)) //vvv
            {
                SdkUtil.logd($"ads full {placement} helper showFull2 is removed ads");
                FIRhelper.logEvent("ads_full2_return_remove");
                return false;
            }
            if (typeShowOnPlaying > 0)
            {
                SdkUtil.logd($"ads full {placement} helper showFull2 typeShowOnPlaying>0");
                FIRhelper.logEvent("ads_full2_return_playing");
                return false;
            }
            if (AutoPerformanceOptimizer.isChangeLogicAdsLowRam == 101)
            {
                SdkUtil.logd($"ads full {placement} helper showFull2 not show because device very low");
                FIRhelper.logEvent("ads_full2_return_lowRam");
                return false;
            }
            levelCurr4Full = level;
            int scheck = checkShowFull2(placement, level, false, 1);
            if (scheck != 1)
            {
                SdkUtil.logd($"ads full {placement} helper showFull2 checkShowFull2={scheck}");
                if (scheck == 2)
                {
                    loadFull2(placement, level, null);
                }
                return false;
            }
            if (ischecknumover)
            {
                deltaloverCountFull2++;
                int overconfig = currConfig.full2Numover;
                SdkUtil.logd($"ads full {placement} helper showFull2 count4ShowFull2 = " + deltaloverCountFull2 + "; numOverShowFull2 = " + overconfig);
                if (deltaloverCountFull2 < overconfig)
                {
                    FIRhelper.logEvent4CheckErr("check_full_2_f4_" + overconfig);
                    FIRhelper.logEvent("ads_full2_return_countfull");
                    return false;
                }
            }

            if (idxFull2ShowCircle >= currConfig.full2StepShowCircle.Count)
            {
                idxFull2ShowCircle = 0;
            }
            FIRhelper.logEvent("ads_full2_doShow");
            SdkUtil.logd($"ads full helper showFull2 call isHideBtClose={isHideBtClose}");

#if UNITY_EDITOR
            if (isFull24Show(placement))
            {
                tFullImgShow = GameHelper.CurrentTimeMilisReal();
                deltaloverCountFull = 0;
                tFullShow = tFullImgShow;
                tFull2Show = tFullImgShow;
                deltaloverCountFull2 = 0;
                showFullEditor();
                return true;
            }

            loadFull2(placement, levelCurr4Full, null);
            return false;
#endif
            // if (adsAdmobMy != null)
            // {
            //     bool istt = false;
            //     if (GameHelper.Instance.AdsIdentify.Length > 5 && !GameHelper.Instance.AdsIdentify.Contains("0000000")) {
            //         istt = GameHelper.Instance.isContainDeviceTest(GameHelper.Instance.AdsIdentify);
            //     }
            //     SdkUtil.logd($"ads full {placement} helper showFull2 not show viia={GameHelper.Instance.AdsIdentify} re={istt}");
            //     if (istt)
            //     {
            //         SdkUtil.logd($"ads full {placement} helper showFull2 not show viia");
            //         return true;
            //     } 
            // }
            SdkUtil.logd($"ads full {placement} helper showFull2 show");
            _cbFull2Show = __cb;
            if (!showFull2Circle(placement, isFull, isWaitAd, timeNtdl, netFG, stateShowAfter, isHideBtClose, isautoClosentfullwhenClick))
            {
                FIRhelper.logEvent("ads_full2_show_fail");
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    FIRhelper.logEvent("ads_full2_show_fail_notCon");
                }
                loadFull2(placement, levelCurr4Full, null);
                return false;
            }
            else
            {
                isFullCallMissCB = false;
                deltaloverCountFull = 0;
                deltaloverCountFull2 = 0;
#if UNITY_IOS || UNITY_IPHONE
                statuschekAdsGiftErr = 0;
                //Invoke("waitStatusAdsShowErr", 3);
                isPauseAudio = true;
                AudioListener.pause = true;
#elif UNITY_ANDROID
                statuschekAdsFull2OpenErr = 1;
                statuschekAdsFull2CloseErr = 1;
#endif
                if (CBShowFullGift != null)
                {
                    CBShowFullGift(1);
                }
                return true;
            }
        }
        private bool showFull2Circle(string placement, bool isFull, bool isShowWait, float timeNtdl, string nameFullGift, int stateShowAfter, bool isHideBtClose, bool isautoClosentfullwhenClick)
        {
            SdkUtil.logd($"ads full {placement} helper showFull2Circle idxFull2ShowCircle={idxFull2ShowCircle}-type={currConfig.full2TypeShow} isadmob={nameFullGift}-autoclose={isautoClosentfullwhenClick} isShowWait={isShowWait} timeNtdl={timeNtdl} isHideBtClose={isHideBtClose}");
            bool re = false;
            setIsAdsShowing(false);
            int typeshow4log = 0;
            if (adsAdmobMy != null)
            {
                List<int> list4Show = new List<int>();
                int idxtmp = idxFull2ShowCircle;
                bool hasntadmob = false;
                bool hasntMax = false;
                for (int i = 0; i < currConfig.full2StepShowCircle.Count; i++)
                {
                    list4Show.Add(currConfig.full2StepShowCircle[idxtmp]);
                    if (currConfig.full2StepShowCircle[idxtmp] == 20)
                    {
                        hasntadmob = true;
                    }
                    else if (currConfig.full2StepShowCircle[idxtmp] == 60)
                    {
                        hasntMax = true;
                    }
                    idxtmp++;
                    if (idxtmp >= currConfig.full2StepShowCircle.Count)
                    {
                        idxtmp = 0;
                    }
                }
                for (int i = 0; i < currConfig.full2StepShowRe.Count; i++)
                {
                    list4Show.Add(currConfig.full2StepShowRe[i]);
                    if (currConfig.full2StepShowRe[i] == 20)
                    {
                        hasntadmob = true;
                    }
                    else if (currConfig.full2StepShowRe[i] == 60)
                    {
                        hasntMax = true;
                    }
                }
                if (hasntadmob)
                {
                    list4Show.Add(20);
                }
                if (hasntMax)
                {
                    list4Show.Add(60);
                }
                int count = currConfig.full2StepShowCircle.Count;
                float tWaitAniShow = 0;
                if (isShowWait)
                {
                    tWaitAniShow = AppConfig.TimeAnimShowWaitAd;
                }
                while (!re && currConfig.full2TypeShow > 0 && list4Show.Count > 0)
                {
                    if (count > 0)
                    {
                        count--;
                        idxFull2ShowCircle++;
                        if (idxFull2ShowCircle >= currConfig.full2StepShowCircle.Count)
                        {
                            idxFull2ShowCircle = 0;
                        }
                    }

                    int typeAdShow = list4Show[0];
                    list4Show.RemoveAt(0);
                    if (typeAdShow == 20)
                    {
                        typeNtFull2 = 0;
                        typeshow4log = 20;
                        int timeShowBtClose = PlayerPrefs.GetInt("cf_time_close_ntfull2", 3);
                        if (tWaitAniShow >= 0.1f && (currTypeFullGiftShow == 20 || currTypeFullGiftShow == 60 || currTypeFullGiftShow == 70 || currTypeFullGiftShow == 71))
                        {
                            tWaitAniShow = AppConfig.TimeAnimShowWaitAd - 0.1f;
                        }
                        if (currConfig.fullNtIsIc != 1)
                        {
                            re = adsAdmobMy.showNativeFull(placement, tWaitAniShow, (int)(timeNtdl * 1000), isHideBtClose, true, timeShowBtClose, isautoClosentfullwhenClick, cbshow2);
                        }
                        else
                        {
                            re = adsAdmobMy.showNativeIcFull(placement, tWaitAniShow, (int)(timeNtdl * 1000), isHideBtClose, true, timeShowBtClose, isautoClosentfullwhenClick, cbshow2);
                        }
                    }
                    else if (typeAdShow == 21)
                    {
                        typeshow4log = 21;
#if UNITY_ANDROID
                        re = adsAdmobMy.showOpenAd(placement, tWaitAniShow, true, true, cbshow2);
#endif
                    }
                    else if (typeAdShow == 10 && (!checkAdShowIsSame("admob", nameFullGift) || currConfig.full2TypeShow >= 4))
                    {
                        typeshow4log = 10;
                        AdsBase admobLowShow = adsAdmobLower;
#if USE_ADSMOB_MY
                        admobLowShow = adsAdmobMyLower;
#endif

#if UNITY_ANDROID
                        float lowdl = timeNtdl;
                        if (stateShowAfter > 0)
                        {
                            lowdl = 5;
                        }
                        else if (stateShowAfter < 0)
                        {
                            lowdl = 0;
                        }
                        lowdl = 0;
                        re = admobLowShow.showFull(placement, tWaitAniShow + lowdl, true, cbshow2);
#endif
                    }
                    else if (typeAdShow == 60)
                    {
                        typeNtFull2 = 1;
                        typeshow4log = 60;
                        int timeShowBtClose = PlayerPrefs.GetInt("cf_time_close_ntfull2", 3);
                        if (tWaitAniShow >= 0.1f && (currTypeFullGiftShow == 20 || currTypeFullGiftShow == 60 || currTypeFullGiftShow == 70 || currTypeFullGiftShow == 71))
                        {
                            tWaitAniShow = AppConfig.TimeAnimShowWaitAd - 0.1f;
                        }
                        re = adsApplovinMaxMy.showNativeFull(placement, tWaitAniShow, (int)(timeNtdl * 1000), isHideBtClose, true, timeShowBtClose, isautoClosentfullwhenClick, cbshow2);
                    }
                    else if (typeAdShow == 61)
                    {
                        typeshow4log = 61;
#if UNITY_ANDROID
                        re = adsApplovinMaxMy.showOpenAd(placement, tWaitAniShow, true, true, cbshow2);
#endif
                    }
                    else if (typeAdShow == 70 || typeAdShow == 71)
                    {
                        typeNtFull2 = 2;
                        typeshow4log = 70;
                        int timeShowBtClose = PlayerPrefs.GetInt("cf_time_close_ntfull2", 3);
                        if (tWaitAniShow >= 0.1f && (currTypeFullGiftShow == 20 || currTypeFullGiftShow == 60 || currTypeFullGiftShow == 70 || currTypeFullGiftShow == 71))
                        {
                            tWaitAniShow = AppConfig.TimeAnimShowWaitAd - 0.1f;
                        }
                        re = adsFb.showNativeFull(placement, tWaitAniShow, (int)(timeNtdl * 1000), isHideBtClose, true, timeShowBtClose, isautoClosentfullwhenClick, cbshow2);
                    }

                    void cbshow2(AD_State stateshow)
                    {
                        SdkUtil.logd($"ads full {placement} helper showFull2Circle ads=openad state={stateshow.ToString()}");
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isFullCallMissCB = true;
                            statuschekAdsFull2OpenErr = 0;
                            statuschekAdsFull2CloseErr = 0;
                            SDKManager.Instance.closeWaitShowFull();
                            onclickCloseFullGift(false, isFull, true, true, false);
                            SdkUtil.logd($"ads full {placement} helper showFull2Circle callback call close = " + _cbFullShow);
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                if (_cbFull2Show != null)
                                {
                                    AD_State ss2 = AD_State.AD_CLOSE2;
                                    if (stateshow == AD_State.AD_SHOW_FAIL)
                                    {
                                        ss2 = AD_State.AD_SHOW_FAIL2;
                                    }
                                    _cbFull2Show(ss2);
                                }
                            }
#if UNITY_IOS || UNITY_IPHONE
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                        }
                        else if (stateshow == AD_State.AD_SHOW)
                        {
                            if (_cbFull2Show != null)
                            {
                                _cbFull2Show(AD_State.AD_SHOW2);
                            }
                        }
                    }
                }
                if (re)
                {
                    statuschekAdsFull2CloseErr = 1;
#if UNITY_IOS || UNITY_IPHONE
                    StatusShowiso = 2;
#endif
                    whenShowFullOk(isShowWait, true, typeshow4log, true, false);
                }
                else
                {
#if UNITY_IOS || UNITY_IPHONE
                    //StatusShowiso = 1;
#endif
                }
            }
            return re;
        }
        private string full2WillShow(string placement, int lv)
        {
            if (placement.StartsWith("gift_"))
            {
                placement = placement.Replace("gift_", "full_2_");
            }
            else if (!placement.StartsWith("full_2_"))
            {
                if (placement.StartsWith("full_"))
                {
                    placement = placement.Replace("full_", "full_2_");
                }
                else
                {
                    placement = "full_2_" + placement;
                }
            }
            string re = "";
            int s2check = checkShowFull2(placement, lv);
            if (s2check != 1)
            {
                return "";
            }
            if (adsAdmobMy != null)
            {
                List<int> list4Show = new List<int>();
                int idxtmp = idxFull2ShowCircle;
                for (int i = 0; i < currConfig.full2StepShowCircle.Count; i++)
                {
                    list4Show.Add(currConfig.full2StepShowCircle[idxtmp]);
                    idxtmp++;
                    if (idxtmp >= currConfig.full2StepShowCircle.Count)
                    {
                        idxtmp = 0;
                    }
                }
                for (int i = 0; i < currConfig.full2StepShowRe.Count; i++)
                {
                    list4Show.Add(currConfig.full2StepShowRe[i]);
                }

                while (currConfig.full2TypeShow > 0 && list4Show.Count > 0)
                {
                    int typeAdShow = list4Show[0];
                    list4Show.RemoveAt(0);
                    if (typeAdShow == 20)
                    {
                        if (currConfig.fullNtIsIc != 1)
                        {
                            if (adsAdmobMy.getNativeFullLoaded(placement) > 0)
                            {
                                re = "";
                                break;
                            }
                        }
                        else
                        {
                            if (adsAdmobMy.getNativeIcFullLoaded(placement) > 0)
                            {
                                re = "";
                                break;
                            }
                        }
                    }
                    else if (typeAdShow == 21)
                    {
#if UNITY_ANDROID
                        if (adsAdmobMy.getOpenAdLoaded(placement) > 0)
                        {
                            re = "admob";
                            break;
                        }
#endif
                    }
                    else if (typeAdShow == 10)
                    {
                        AdsBase admobLowShow = adsAdmobLower;
#if USE_ADSMOB_MY
                        admobLowShow = adsAdmobMyLower;
#endif
#if UNITY_ANDROID
                        if (admobLowShow.getFullLoaded(placement) > 0)
                        {
                            re = "admob";
                            break;
                        }
#endif
                    }
                    else if (typeAdShow == 60)
                    {
                        if (adsApplovinMaxMy.getNativeFullLoaded(placement) > 0)
                        {
                            re = "";
                            break;
                        }
                    }
                    else if (typeAdShow == 61)
                    {
                        if (adsApplovinMaxMy.getOpenAdLoaded(placement) > 0)
                        {
                            re = adsApplovinMaxMy.getFullNetName();
                            break;
                        }
                    }
                    else if (typeAdShow == 70 || typeAdShow == 71)
                    {
                        if (adsFb.getNativeFullLoaded(placement) > 0)
                        {
                            re = "fb";
                            break;
                        }
                    }
                }
            }
            return re;
        }
        private void whenShowFullOk(bool isWaitAd, bool isShoww2, int typeShow, bool isSetTimeFull, bool isFullImgReTime)
        {
            isShowFulled = true;
            setIsAdsShowing(true);
            typeFullGift = 0;
            showTransFullGift();
            long tCurr = GameHelper.CurrentTimeMilisReal();
            if (isSetTimeFull)
            {
                tFullImgShow = tCurr;
                if (!isFullImgReTime)
                {
                    tFullShow = tCurr;
                }
                tShowAdsCheckContinue = tCurr;
            }
            if (isWaitAd)
            {
                SDKManager.Instance.showWait4ShowFull();
            }
            if (isShoww2)
            {
                tFullImgShow = tCurr;
                tFull2Show = tCurr;
                FIRhelper.logEvent("ads_full2_show_ok");
                FIRhelper.logEvent("show_ads_full2");
                if (typeShow == 20)
                {
                    FIRhelper.logEvent("ads_full2_show_ok_nt");
                }
                else if (typeShow == 21)
                {
                    FIRhelper.logEvent("ads_full2_show_ok_open");
                }
                else if (typeShow == 10)
                {
                    FIRhelper.logEvent("ads_full2_show_ok_10");
                }
                else if (typeShow == 60)
                {
                    FIRhelper.logEvent("ads_full2_show_ok_nt6");
                }
                else if (typeShow == 61)
                {
                    FIRhelper.logEvent("ads_full2_show_ok_open6");
                }
                else if (typeShow == 70 || typeShow == 71)
                {
                    FIRhelper.logEvent("ads_full2_show_ok_nt1");
                }
            }
            else
            {
                FIRhelper.logEvent("ads_total_show_ok");
                FIRhelper.logEvent("ads_full_show_ok");
                FIRhelper.logEvent("show_ads_total");
                FIRhelper.logEvent("show_ads_full");
                currTypeFullGiftShow = (short)typeShow;
                if (currTypeFullGiftShow < 0)
                {
                    currTypeFullGiftShow = (short)(-currTypeFullGiftShow);
                }
                if (typeShow > 0)
                {
                    FIRhelper.logEvent("ads_full_show_ok_" + typeShow);
                }
                else if (typeShow == -20)
                {
                    FIRhelper.logEvent("ads_full_show_ok_nt");
                }
                else if (typeShow == -21)
                {
                    FIRhelper.logEvent("ads_full_show_ok_open");
                }
                else if (typeShow == -24)
                {
                    FIRhelper.logEvent("ads_full_show_ok_rwinter0");
                }
                else if (typeShow == -25)
                {
                    FIRhelper.logEvent("ads_full_show_ok_rwrw0");
                }
                else if (typeShow == -26)
                {
                    FIRhelper.logEvent("ads_full_show_ok_ntpg");
                }
                else if (typeShow == -30)
                {
                    FIRhelper.logEvent("ads_full_show_ok_rwrw3");
                }
                else if (typeShow == -60)
                {
                    FIRhelper.logEvent("ads_full_show_ok_nt6");
                }
                else if (typeShow == -61)
                {
                    FIRhelper.logEvent("ads_full_show_ok_open6");
                }
                else if (typeShow == -62)
                {
                    FIRhelper.logEvent("ads_full_show_ok_rwrw6");
                }
                else if (typeShow == -70 || typeShow == -71)
                {
                    FIRhelper.logEvent("ads_full_show_ok_nt1");
                }
                logCountTotal();
            }
        }
        private void waitCheckCbFullErr()
        {
            SdkUtil.logd($"ads full helper waitCheckCbFullErr 0");
            if (!isFullCallMissCB)
            {
                SdkUtil.logd($"ads full helper waitCheckCbFullErr: AD_SHOW_MISS_CB");
                isFullCallMissCB = true;
                if (_cbFullShow != null)
                {
                    _cbFullShow(AD_State.AD_SHOW_MISS_CB);
                }
            }
        }
        public void onclickCloseFullGift(bool isCallCb, bool isFull, bool isFull2, bool isSetTimeFull, bool isfullImgrebytime)
        {
            bgFullGift.SetActive(false);
            if (isFull)
            {
                if (isSetTimeFull)
                {
                    tFullImgShow = GameHelper.CurrentTimeMilisReal();
                    if (!isfullImgrebytime)
                    {
                        tFullShow = tFullImgShow;
                    }
                    if (isFull2)
                    {
                        tFullShow = tFullImgShow;
                        tFull2Show = tFullImgShow;
                    }
                }
            }
            else
            {
                tGiftShow = GameHelper.CurrentTimeMilisReal();
                if (isFull2)
                {
                    tFull2Show = tGiftShow;
                }
            }
            if (isCallCb)
            {
                SdkUtil.logd($"ads full helper onclickCloseFullGift typeFullGift={typeFullGift}");
                if (typeFullGift == 0)
                {
                    setIsAdsShowing(false);
                    isFullCallMissCB = true;
                    statuschekAdsFull2OpenErr = 0;
                    statuschekAdsFull2CloseErr = 0;
                    SDKManager.Instance.closeWaitShowFull();
#if UNITY_IOS || UNITY_IPHONE
                    isPauseAudio = false;
                    AudioListener.pause = false;
#endif
                    if (_cbFullShow != null)
                    {
                        _cbFullShow(AD_State.AD_SHOW_FAIL);
                    }
                }
                else
                {
                    setIsAdsShowing(false);
                    isGiftCallMissCB = true;
                    statuschekAdsGiftErr = 0;
#if UNITY_IOS || UNITY_IPHONE
                    isPauseAudio = false;
                    AudioListener.pause = false;
#endif
                    if (_cbGiftShow != null)
                    {
                        _cbGiftShow(AD_State.AD_SHOW_FAIL);
                    }
                }
            }
        }
        //-----------------------
        public bool isGift4Show(string placement, bool isAll)
        {
            if (!placement.StartsWith("gift_"))
            {
                placement = "gift_" + placement;
            }
            if (isDisableAds(2)) //vvv
            {
                SdkUtil.logd($"ads gift {placement} helper isGift4Show is dis");
                return false;
            }
            checkResumeAudio();
#if UNITY_EDITOR
            return adsEditorCtr.isGiftEditorLoaded;
#endif
            List<int> listShowCircle = currConfig.giftStepShowCircle;
            List<int> listShowRe = currConfig.giftStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.giftStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads gift {placement} helper isGift4Show step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            if (idxGiftShowCircle >= listShowCircle.Count)
            {
                idxGiftShowCircle = 0;
            }
            for (int i = 0; i < listShowCircle.Count; i++)
            {
                AdsBase adCurr = getAdsFromId(listShowCircle[i]);
                if (adCurr != null)
                {
                    if (listShowCircle[i] == 20)
                    {
                        if (adCurr.getNtGiftLoaded(placement) > 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (adCurr.getGiftLoaded(placement) > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            if (isAll || listShowCircle.Count == 0)
            {
                for (int i = 0; i < listShowRe.Count; i++)
                {
                    AdsBase adCurr = getAdsFromId(listShowRe[i]);
                    if (adCurr != null)
                    {
                        if (listShowRe[i] == 20)
                        {
                            if (adCurr.getNtGiftLoaded(placement) > 0)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (adCurr.getGiftLoaded(placement) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private int checkShowGift(bool isLog = false)
        {
            if (countGiftShowOfDay <= 0)
            {
                SdkUtil.logd($"ads gift helper checkShowgift over num show of day, to=" + currConfig.giftTotalOfday);
                if (isLog)
                {
                    FIRhelper.logEvent("ads_gift_return_overOfDay");
                }
                return -1;
            }

            long t = GameHelper.CurrentTimeMilisReal();
            if (t - tGiftShow < currConfig.giftDeltatime)
            {
                SdkUtil.logd($"ads gift helper checkShowGift + d:" + currConfig.giftDeltatime + " dt:" + (t - tGiftShow));
                if (isLog)
                {
                    FIRhelper.logEvent("ads_gift_return_dt");
                }
                return 2;
            }
            return 1;
        }
        public void loadGift4ThisTurn(string placement, int lv, AdCallBack cb)
        {
            if (!placement.StartsWith("gift_"))
            {
                placement = "gift_" + placement;
            }
            if (!isinitCall)
            {
                SdkUtil.logd($"ads gift {placement} helper loadGift4ThisTurn ad not init");
                return;
            }
            if (isDisableAds(2)) //vvv
            {
                SdkUtil.logd($"ads gift {placement} helper loadGift4ThisTurn is dis");
                return;
            }
            checkResumeAudio();
            _cbGiftLoad = null;

            if (countGiftShowOfDay <= 0)
            {
                SdkUtil.logd($"ads gift {placement} helper loadGift4ThisTurn over num show of day, to=" + currConfig.giftTotalOfday);
                return;
            }

            // long t = GameHelper.CurrentTimeMilisReal();
            // SdkUtil.logd($"ads full helper loadGift4ThisTurn + d:" + currConfig.giftDeltatime + " del_t:" + (t - tGiftShow));
            // if ((t - tGiftShow) < (currConfig.giftDeltatime - 30000)) return;
            // SdkUtil.logd($"ads full helper loadGift4ThisTurn 2");
            //if (isGift4Show(false)) return;
            SdkUtil.logd($"ads gift {placement} helper loadGift4ThisTurn load level={lv}");
            _cbGiftLoad = cb;
            if (currConfig.giftStepShowCircle.Count == 0 && currConfig.giftStepShowRe.Count == 0)
            {
                initListGift();
            }
            List<int> listShowCircle = currConfig.giftStepShowCircle;
            List<int> listShowRe = currConfig.giftStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.giftStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads gift {placement} helper loadGift4ThisTurn step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            if (idxGiftShowCircle >= listShowCircle.Count)
            {
                idxGiftShowCircle = 0;
            }

            if (listShowCircle.Count == 0)
            {
                stepGiftLoad = 1;
                idxGiftLoad = 0;
                countGiftLoad = listShowRe.Count;
            }
            else
            {
                stepGiftLoad = 0;
                idxGiftLoad = idxGiftShowCircle;
                countGiftLoad = listShowCircle.Count;
                if (listShowCircle[idxGiftLoad] == 0)
                {
                    isFirstShowGift = false;
                }
            }
            if (isFirstShowGift)
            {
                isFirstShowGift = false;
                if (adsAdmobMy != null)
                {
                    adsAdmobMy.loadGift(placement, null);
                }
            }
            loadGiftCircle(placement, lv);
            if (currConfig.full2LevelStart <= lv && (currConfig.full2TypeShow == 2 || currConfig.full2TypeShow == 3))
            {
                loadFull2(placement, lv, null);
            }
        }
        private void loadGiftCircle(string placement, int lv)
        {
#if UNITY_EDITOR
            if (!adsEditorCtr.isGiftEditorLoading && !adsEditorCtr.isGiftEditorLoaded)
            {
                loadGiftEditor();
            }
            return;
#endif
            SdkUtil.logd($"ads gift {placement} helper loadGiftCircle idx=" + idxGiftLoad);
            if (countGiftLoad > 0)
            {
                level4ApplovinGift = lv;
                int idxcurr = idxGiftLoad;
                idxGiftLoad++;
                List<int> listShowCircle = currConfig.giftStepShowCircle;
                List<int> listShowRe = currConfig.giftStepShowRe;

                StepShow stepCurrShow = null;
                var daycf = currConfig.getDayImpact();
                if (daycf != null)
                {
                    stepCurrShow = daycf.giftStep;
                    if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                    {
                        SdkUtil.logd($"ads gift {placement} helper loadGift4ThisTurn step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                        listShowCircle = stepCurrShow.stepShowCircle;
                        listShowRe = stepCurrShow.stepShowRe;
                    }
                }
                if (idxGiftLoad >= listShowCircle.Count)
                {
                    idxGiftLoad = 0;
                }
                List<int> listcurr = null;
                if (stepGiftLoad == 0)
                {
                    listcurr = listShowCircle;
                    if (idxGiftLoad >= listShowCircle.Count)
                    {
                        idxGiftLoad = 0;
                    }
                }
                else
                {
                    listcurr = listShowRe;
                    if (idxGiftLoad >= listShowRe.Count)
                    {
                        idxGiftLoad = 0;
                    }
                }
                countGiftLoad--;
                if (countGiftLoad <= 0 && stepGiftLoad == 0)
                {
                    stepGiftLoad = 1;
                    idxGiftLoad = 0;
                    countGiftLoad = listShowRe.Count;
                }

                if (listcurr != null && listcurr.Count > idxcurr && idxcurr >= 0)
                {
                    AdsBase adscurr = getAdsFromId(listcurr[idxcurr]);
                    if (adscurr != null)
                    {
                        if (listcurr[idxcurr] == 20)
                        {
                            adscurr.loadNtGift(placement, (AD_State state) =>
                            {
                                if (state == AD_State.AD_LOAD_OK)
                                {
                                    if (_cbFullLoad != null)
                                    {
                                        _cbFullLoad(AD_State.AD_LOAD_OK);
                                        _cbFullLoad = null;
                                    }
                                }
                                else
                                {
                                    loadGiftCircle(placement, lv);
                                }
                            });
                        }
                        else
                        {
                            adscurr.loadGift(placement, (AD_State state) =>
                            {
                                if (state == AD_State.AD_LOAD_FAIL || state == AD_State.AD_LOAD_OK_WAIT)
                                {
                                    loadGiftCircle(placement, lv);
                                }
                                else
                                {
                                    if (_cbGiftLoad != null)
                                    {
                                        _cbGiftLoad(AD_State.AD_LOAD_OK);
                                        _cbGiftLoad = null;
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        loadGiftCircle(placement, lv);
                    }
                }
                else
                {
                    if (_cbGiftLoad != null)
                    {
                        _cbGiftLoad(AD_State.AD_LOAD_FAIL);
                        _cbGiftLoad = null;
                    }
                }
            }
            else
            {
                if (_cbGiftLoad != null)
                {
                    _cbGiftLoad(AD_State.AD_LOAD_FAIL);
                    _cbGiftLoad = null;
                }
            }
        }
        public int showGift(string placement, int lv, bool isShowWait, AdCallBack cb, bool isHideBtClose = false, bool isAllowShow2 = true, bool isautoClosentfullwhenClick = true)
        {
            if (!placement.StartsWith("gift_"))
            {
                placement = "gift_" + placement;
            }
            FIRhelper.logEvent("ads_total_call");
            FIRhelper.logEvent("ads_gift_call");
            if (isRemoveAds(1))
            {
                if (cb != null)
                {
                    cb(AD_State.AD_REWARD_OK);
                }
                FIRhelper.logEvent("ads_gift_return_remove");
                return 0;
            }
            if (isDisableAds(2)) //vvv
            {
                SdkUtil.logd($"ads gift {placement} helper showGift is dis");
                FIRhelper.logEvent("ads_gift_return_disable");
                return -4;
            }
            checkResumeAudio();
            if (count4AdShowing > 0)
            {
                long tc = GameHelper.CurrentTimeMilisReal();
                if ((tc - tShowAdsCheckContinue) >= 30 * 1000)
                {
                    SdkUtil.logd($"ads gift {placement} helper showGift isAdsShowing and overtime -> reset isAdsShowing=false");
                    setIsAdsShowing(false);
                }
                else
                {
                    SdkUtil.logd($"ads gift {placement} helper showGift is ads showing");
                    FIRhelper.logEvent("ads_gift_return_showing");
                    return -3;
                }
            }

            levelCurr4Gift = lv;
            int scheck = checkShowGift(true);
            if (scheck != 1)
            {
                SdkUtil.logd($"ads gift {placement} helper showGift checkShowGift={scheck}");
                if (scheck == 2)
                {
                    loadGift4ThisTurn(placement, lv, null);
                }
                return -1;
            }

            SdkUtil.logd($"ads gift {placement} helper showGift lv={lv}");
            FIRhelper.logEvent("ads_total_doShow");
            FIRhelper.logEvent("ads_gift_doShow");
            FIRhelper.logEvent("show_ads_reward_call");
            giftIsloadWhenErr = true;
            _cbGiftShow = cb;
            StatusShowiso = 0;
#if UNITY_EDITOR
            if (adsEditorCtr.isGiftEditorLoaded)
            {
                _cbGiftEditor = cb;
                tGiftShow = GameHelper.CurrentTimeMilisReal();
                tShowAdsCheckContinue = tGiftShow;
                subCountShowGift();
                showGiftEditor();
                int countGift4pointe = PlayerPrefs.GetInt("count_gift_4_point", 0);
                countGift4pointe++;
                AnalyticCommonParam.Instance().countShowAdsGift = countGift4pointe;
                PlayerPrefs.SetInt("count_gift_4_point", countGift4pointe);
                checkLogVipAds();
                return 0;
            }

            loadGift4ThisTurn(placement, lv, null);
            return -2;
#endif
            List<int> listShowCircle = currConfig.giftStepShowCircle;
            List<int> listShowRe = currConfig.giftStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.giftStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads gift {placement} helper showGift step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            if (idxGiftShowCircle >= listShowCircle.Count)
            {
                idxGiftShowCircle = 0;
            }
            nameShowFullGift = giftWillShow(placement);
            string full2WS = full2WillShow(placement, lv);
            bool issames = checkAdShowIsSame(full2WS, nameShowFullGift);
            float tsdl = 0;
            if (issames)
            {
#if UNITY_IOS || UNITY_IPHONE
                tsdl = 1.1f;
#else
                tsdl = 0.1f;
#endif
            }
            statusAds2 = 0;
            if (!showGiftCircle(placement, isShowWait, isAllowShow2, tsdl, isHideBtClose, isautoClosentfullwhenClick))
            {
                FIRhelper.logEvent("ads_total_show_fail");
                FIRhelper.logEvent("ads_gift_show_fail");
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    FIRhelper.logEvent("ads_gift_show_fail_notCon");
                }
                else if (isFirstShowGift)
                {
                    isFirstShowGift = false;
                    FIRhelper.logEvent("ads_gift_show_fail_first");
                }
                _cbGiftShow = null;
                loadGift4ThisTurn(placement, lv, null);
                return -2;
            }
            isGiftCallMissCB = false;
            int countGift4point = PlayerPrefs.GetInt("count_gift_4_point", 0);
            countGift4point++;
            AnalyticCommonParam.Instance().countShowAdsGift = countGift4point;
            PlayerPrefs.SetInt("count_gift_4_point", countGift4point);
            checkLogVipAds();
            AdjustHelper.LogEvent(AdjustEventName.AdsGift);
            AdjustHelper.LogEvent(AdjustEventName.AdsTotal);
            Myapi.LogEventApi.Instance().LogAds(Myapi.AdsTypeLog.Reward, placement);//vvv

            float timeNtdl = 2;
#if UNITY_IOS || UNITY_IPHONE
            statuschekAdsGiftErr = 1;
            //Invoke("waitStatusAdsShowErr", 3);
            isPauseAudio = true;
            AudioListener.pause = true;
            timeNtdl = 0.5f;
#elif UNITY_ANDROID
            Invoke("waitCheckCbGiftErr", 3);
#endif
            FIRhelper.logEvent("ads_full2_call");
            FIRhelper.logEvent("ads_full2_call_gift");
            statusAds2++;
            if (isAllowShow2 && (currConfig.full2TypeShow == 2 || currConfig.full2TypeShow == 3) && ((currConfig.fullFlagFor2vscl & 2) > 0))
            {
                if (currTypeFullGiftShow == -20)
                {
                    timeNtdl = 0;
                }
                bool isShow2 = showFull2(placement, levelCurr4Gift, isHideBtClose, false, 0, isShowWait, false, timeNtdl, nameShowFullGift, 1, isautoClosentfullwhenClick, cb);
                if (isShow2)
                {
                    statusAds2++;
                }
            }
            else
            {
                SdkUtil.logd($"ads full {placement} helper showGift not showFull2 isAllowShow2={isAllowShow2} type={currConfig.full2TypeShow} flag={currConfig.fullFlagFor2vscl}");
                if (!isAllowShow2)
                {
                    FIRhelper.logEvent("ads_full2_g_return_notAlow");
                }
                else if ((currConfig.fullFlagFor2vscl & 2) <= 0)
                {
                    FIRhelper.logEvent("ads_full2_g_return_flag2vscl");
                }
                else
                {
                    FIRhelper.logEvent("ads_full2_g_return_type");
                }
            }
            if (CBShowFullGift != null)
            {
                CBShowFullGift(2);
            }
            return 0;
        }
        private bool showGiftCircle(string placement, bool isShowWait, bool isAllowShow2, float timeDelay, bool isHideBtClose, bool isautoClosentfullwhenClick)
        {
            SdkUtil.logd($"ads gift {placement} helper showGiftCircle idxgiftShowCircle={idxGiftShowCircle} isShowWait={isShowWait} timeDelay={timeDelay}");
            bool re = false;
            setIsAdsShowing(false);
            float tWaitAniShow = 0;
            if (isShowWait)
            {
                tWaitAniShow = AppConfig.TimeAnimShowWaitAd;
            }
#if USE_ADSMOB_MY
            if (adsAdmobMy != null)
            {
                if (adsAdmobMy.getGiftLoaded(placement) == 2 && statusLogicIron > 0)
                {
                    SdkUtil.logd($"ads gift {placement} helper showGiftCircle show admob high priority");
                    nameShowFullGift = adsAdmobMy.getGiftNetName();
                    bool iss = adsAdmobMy.showGift(placement, tWaitAniShow + timeDelay, (stateshow) =>
                    {
                        if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                        {
                            isGiftCallMissCB = true;
                            statuschekAdsGiftErr = 0;
                            onclickCloseFullGift(false, false, false, false, false);
                            bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                            isCon4Showios = (StatusShowiso == 1);
                            isPauseAudio = false;
                            AudioListener.pause = false;
#endif
                            if (isAllowShow2 && (currConfig.full2TypeShow == 5 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 2) > 0))
                            {
                                bool isShow2 = showFull2(placement, levelCurr4Full, isHideBtClose, false, 0, true, false, 0, "", 0, isautoClosentfullwhenClick, _cbGiftShow);
                                if (isShow2)
                                {
                                    statusAds2++;
                                }
                            }
                            statusAds2--;
                            if (statusAds2 <= 0)
                            {
                                setIsAdsShowing(false);
                                if (_cbGiftShow != null)
                                {
                                    _cbGiftShow(stateshow);
                                }
                            }
                            //apply logic after show gift t(s) -> show full
                            if (currConfig.fullDeltaTime4Gift > 2000)
                            {
                                long tcurr = GameHelper.CurrentTimeMilisReal();
                                long tallow = tcurr + currConfig.fullDeltaTime4Gift;
                                long cfDt = fullDeltalTimeCurr;
                                var daycf = currConfig.getDayImpact();
                                if (daycf != null && daycf.deltaTime > 1000)
                                {
                                    cfDt = daycf.deltaTime;
                                }
                                if ((tallow - tFullShow) > cfDt)
                                {

                                    tFullShow = tallow - cfDt;
                                }
                            }
                        }
                        else if (stateshow == AD_State.AD_SHOW)
                        {
                            bgFullGift.SetActive(false);
                            statusAds2++;
#if UNITY_IOS || UNITY_IPHONE
                            if (statuschekAdsGiftErr == 1)
                            {
                                statuschekAdsGiftErr = 2;
                                //Invoke("waitStatusAdsCloseErr", 5);
                            }
#endif
                            if (_cbGiftShow != null)
                            {
                                _cbGiftShow(stateshow);
                            }
                        }
                        else
                        {

                            if (_cbGiftShow != null)
                            {
                                _cbGiftShow(stateshow);
                            }
                        }
                    });
                    if (iss)
                    {
                        subCountShowGift();
                        setIsAdsShowing(true);
                        typeFullGift = 1;
                        showTransFullGift();
                        tGiftShow = GameHelper.CurrentTimeMilisReal();
                        tShowAdsCheckContinue = tGiftShow;
                        currTypeFullGiftShow = 0;

                        FIRhelper.logEvent("ads_total_show_ok");
                        FIRhelper.logEvent("ads_gift_show_ok");
                        FIRhelper.logEvent("ads_gift_show_ok_" + adsAdmobMy.adsType);

                        FIRhelper.logEvent("show_ads_total");
                        FIRhelper.logEvent("show_ads_reward");
                        logCountTotal();
                        if (isShowWait)
                        {
                            SDKManager.Instance.showWait4ShowFull();
                        }
                        re = true;
                    }
                }
            }
#endif
            if (!re)
            {
                List<int> listShowCircle = currConfig.giftStepShowCircle;
                List<int> listShowRe = currConfig.giftStepShowRe;

                StepShow stepCurrShow = null;
                var daycf = currConfig.getDayImpact();
                if (daycf != null)
                {
                    stepCurrShow = daycf.giftStep;
                    if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                    {
                        SdkUtil.logd($"ads gift {placement} helper showGiftCircle step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                        listShowCircle = stepCurrShow.stepShowCircle;
                        listShowRe = stepCurrShow.stepShowRe;
                    }
                }
                if (idxGiftShowCircle >= listShowCircle.Count)
                {
                    idxGiftShowCircle = 0;
                }
                List<int> list4Show = new List<int>();
                int idxtmp = idxGiftShowCircle;
                for (int i = 0; i < listShowCircle.Count; i++)
                {
                    list4Show.Add(listShowCircle[idxtmp]);
                    idxtmp++;
                    if (idxtmp >= listShowCircle.Count)
                    {
                        idxtmp = 0;
                    }
                }
                list4Show.AddRange(listShowRe);
                if (SdkUtil.isLog())
                {
                    string sstel = "";
                    for (int ii = 0; ii < list4Show.Count; ii++)
                    {
                        sstel += "" + list4Show[ii] + ",";
                    }
                    SdkUtil.logd($"ads gift {placement} helper showGiftCircle step={sstel}");
                }
                for (int ii = 0; ii < list4Show.Count; ii++)
                {
                    if (ii < listShowCircle.Count)
                    {
                        idxGiftShowCircle++;
                        if (idxGiftShowCircle >= listShowCircle.Count)
                        {
                            idxGiftShowCircle = 0;
                        }
                    }
                    AdsBase adShow = getAdsFromId(list4Show[ii]);
                    if (adShow != null)
                    {
                        nameShowFullGift = adShow.getGiftNetName();
                        if (list4Show[ii] == 20 && adShow.getNtGiftLoaded(placement) > 0)
                        {
                            bool iss = adShow.showNtGift(placement, tWaitAniShow + timeDelay + 0.2f, false, (stateShow) =>
                            {
                                SdkUtil.logd($"ads gift {placement} helper showGiftCircle callback ads=" + adShow.adsType + ", state=" + stateShow.ToString());
                                if (stateShow == AD_State.AD_CLOSE || stateShow == AD_State.AD_SHOW_FAIL)
                                {
                                    isGiftCallMissCB = true;
                                    statuschekAdsGiftErr = 0;
                                    onclickCloseFullGift(false, false, false, false, false);
                                    bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                                    isCon4Showios = (StatusShowiso == 1);
                                    isPauseAudio = false;
                                    AudioListener.pause = false;
#endif
                                    if (isAllowShow2 && (currConfig.full2TypeShow == 5 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 2) > 0))
                                    {
                                        bool isShow2 = showFull2(placement, levelCurr4Full, isHideBtClose, false, 0, true, false, 0, "", 0, isautoClosentfullwhenClick, _cbGiftShow);
                                        if (isShow2)
                                        {
                                            statusAds2++;
                                        }
                                    }
                                    statusAds2--;
                                    if (statusAds2 <= 0)
                                    {
                                        setIsAdsShowing(false);
                                        if (_cbGiftShow != null)
                                        {
                                            _cbGiftShow(stateShow);
                                        }
                                    }
                                }
                                else if (stateShow == AD_State.AD_SHOW)
                                {
                                    bgFullGift.SetActive(false);
#if UNITY_IOS || UNITY_IPHONE
                                if (statuschekAdsGiftErr == 1)
                                {
                                    statuschekAdsGiftErr = 2;
                                    //Invoke("waitStatusAdsCloseErr", 5);
                                }
#endif
                                    if (_cbGiftShow != null)
                                    {
                                        _cbGiftShow(stateShow);
                                    }
                                }
                                else
                                {
                                    if (_cbGiftShow != null)
                                    {
                                        _cbGiftShow(stateShow);
                                    }
                                }
                            });
                            if (iss)
                            {
                                subCountShowGift();
                                setIsAdsShowing(true);
                                typeFullGift = 1;
                                showTransFullGift();
                                tGiftShow = GameHelper.CurrentTimeMilisReal();
                                tShowAdsCheckContinue = tGiftShow;
                                currTypeFullGiftShow = -20;

                                FIRhelper.logEvent("ads_total_show_ok");
                                FIRhelper.logEvent("ads_gift_show_ok");
                                FIRhelper.logEvent("ads_gift_show_ok_0_nt");

                                FIRhelper.logEvent("show_ads_total");
                                FIRhelper.logEvent("show_ads_reward");

                                logCountTotal();
                                if (isShowWait)
                                {
                                    SDKManager.Instance.showWait4ShowFull();
                                }
                                re = true;
                                break;
                            }
                        }
                        else if (adShow.getGiftLoaded(placement) > 0)
                        {
                            bool iss = adShow.showGift(placement, tWaitAniShow + timeDelay, (stateshow) =>
                            {
                                SdkUtil.logd($"ads gift {placement} helper showGiftCircle callback ads=" + adShow.adsType + ", state=" + stateshow.ToString());
                                if (stateshow == AD_State.AD_CLOSE || stateshow == AD_State.AD_SHOW_FAIL)
                                {
                                    isGiftCallMissCB = true;
                                    statuschekAdsGiftErr = 0;
                                    onclickCloseFullGift(false, false, false, false, false);
                                    bool isCon4Showios = false;
#if UNITY_IOS || UNITY_IPHONE
                                    isCon4Showios = (StatusShowiso == 1);
                                    isPauseAudio = false;
                                    AudioListener.pause = false;
#endif
                                    if (isAllowShow2 && (currConfig.full2TypeShow == 5 || currConfig.full2TypeShow == 6 || isCon4Showios) && ((currConfig.fullFlagFor2vscl & 2) > 0))
                                    {
                                        bool isShow2 = showFull2(placement, levelCurr4Full, isHideBtClose, false, 0, true, false, 0, "", 0, isautoClosentfullwhenClick, _cbGiftShow);
                                        if (isShow2)
                                        {
                                            statusAds2++;
                                        }
                                    }
                                    statusAds2--;
                                    if (statusAds2 <= 0)
                                    {
                                        setIsAdsShowing(false);
                                        if (_cbGiftShow != null)
                                        {
                                            _cbGiftShow(stateshow);
                                        }
                                    }
                                }
                                else if (stateshow == AD_State.AD_SHOW)
                                {
                                    bgFullGift.SetActive(false);
#if UNITY_IOS || UNITY_IPHONE
                                if (statuschekAdsGiftErr == 1)
                                {
                                    statuschekAdsGiftErr = 2;
                                    //Invoke("waitStatusAdsCloseErr", 5);
                                }
#endif
                                    if (_cbGiftShow != null)
                                    {
                                        _cbGiftShow(stateshow);
                                    }
                                }
                                else
                                {
                                    if (_cbGiftShow != null)
                                    {
                                        _cbGiftShow(stateshow);
                                    }
                                }
                            });
                            if (iss)
                            {
                                subCountShowGift();
                                setIsAdsShowing(true);
                                typeFullGift = 1;
                                showTransFullGift();
                                tGiftShow = GameHelper.CurrentTimeMilisReal();
                                tShowAdsCheckContinue = tGiftShow;
                                currTypeFullGiftShow = (short)adShow.adsType;

                                FIRhelper.logEvent("ads_total_show_ok");
                                FIRhelper.logEvent("ads_gift_show_ok");
                                FIRhelper.logEvent("ads_gift_show_ok_" + adShow.adsType);

                                FIRhelper.logEvent("show_ads_total");
                                FIRhelper.logEvent("show_ads_reward");
                                logCountTotal();
                                if (isShowWait)
                                {
                                    SDKManager.Instance.showWait4ShowFull();
                                }
                                re = true;
                                break;
                            }
                        }
                    }
                }
            }
            return re;
        }
        private string giftWillShow(string placement)
        {
            string re = "";
#if USE_ADSMOB_MY
            if (adsAdmobMy != null)
            {
                if (adsAdmobMy.getGiftLoaded(placement) == 2 && statusLogicIron > 0)
                {
                    return re = "admob";
                }
            }
#endif
            List<int> listShowCircle = currConfig.giftStepShowCircle;
            List<int> listShowRe = currConfig.giftStepShowRe;

            StepShow stepCurrShow = null;
            var daycf = currConfig.getDayImpact();
            if (daycf != null)
            {
                stepCurrShow = daycf.giftStep;
                if (stepCurrShow != null && stepCurrShow.stepShowCircle.Count > 0)
                {
                    SdkUtil.logd($"ads gift {placement} helper giftWillShow step with dayactive:{daycf.dayStart}-{daycf.dayEnd}");
                    listShowCircle = stepCurrShow.stepShowCircle;
                    listShowRe = stepCurrShow.stepShowRe;
                }
            }
            if (idxGiftShowCircle >= listShowCircle.Count)
            {
                idxGiftShowCircle = 0;
            }
            List<int> list4Show = new List<int>();
            int idxtmp = idxGiftShowCircle;
            for (int i = 0; i < listShowCircle.Count; i++)
            {
                list4Show.Add(listShowCircle[idxtmp]);
                idxtmp++;
                if (idxtmp >= listShowCircle.Count)
                {
                    idxtmp = 0;
                }
            }
            list4Show.AddRange(listShowRe);
            for (int ii = 0; ii < list4Show.Count; ii++)
            {
                AdsBase adShow = getAdsFromId(list4Show[ii]);
                if (adShow != null)
                {
                    if (list4Show[ii] == 20)
                    {
                        if (adShow.getNtGiftLoaded(placement) > 0)
                        {
                            re = "admob";
                            break;
                        }
                    }
                    else
                    {
                        if (adShow.getGiftLoaded(placement) > 0)
                        {
                            re = adShow.getGiftNetName();
                            break;
                        }
                    }
                }
            }
            return re;
        }

        public void onCloseFullGift(bool isFull)
        {
#if UNITY_IOS || UNITY_IPHONE
            if (StatusShowiso == 2)
            {
                StatusShowiso = 0;
                if (typeNtFull2 == 0)
                {
                    AdsAdmobMy.reCountNtFull();
                }
                else if (typeNtFull2 == 2)
                {
                    AdsAFbMy.reCountNtFull();
                }
                else
                {
                    AdsMaxMy.reCountNtFull();
                }
            }
#endif
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (statuschekAdsFullCloseErr == 1)
                {
                    SdkUtil.logd($"ads full helper app pause statuschekAdsFullCloseErr={statuschekAdsFullCloseErr}");
                    statuschekAdsFullCloseErr = 2;
                }
            }
            else
            {
                if (statuschekAdsFullCloseErr == 2)
                {
                    SdkUtil.logd($"ads full helper resume statuschekAdsFullCloseErr={statuschekAdsFullCloseErr}");
                    statuschekAdsFullCloseErr = 0;
                }
            }
        }

        private void waitStatusAdsShowErr()
        {
            SdkUtil.logd($"ads helper waitStatusAdsShowErr");
            if (statuschekAdsFullOpenErr == 1)
            {
                SdkUtil.logd($"ads helper waitStatusAdsShowErr full");
                statuschekAdsFullOpenErr = 0;
                if (!isFullCallMissCB)
                {
                    SdkUtil.logd($"ads helper waitStatusAdsShowErr full: AD_SHOW_MISS_CB");
                    isFullCallMissCB = true;
                    if (_cbFullShow != null)
                    {
                        _cbFullShow(AD_State.AD_SHOW_MISS_CB);
                    }
                }
            }
            if (statuschekAdsGiftErr == 1)
            {
                SdkUtil.logd($"ads helper waitStatusAdsShowErr gift");
                statuschekAdsGiftErr = 0;
                if (!isGiftCallMissCB)
                {
                    SdkUtil.logd($"ads helper waitStatusAdsShowErr gift: AD_SHOW_MISS_CB");
                    isGiftCallMissCB = true;
                    if (_cbGiftShow != null)
                    {
                        _cbGiftShow(AD_State.AD_SHOW_MISS_CB);
                    }
                }

            }
        }
        public void waitStatusAdsCloseErr()
        {
            if (statuschekAdsFullCloseErr == 2)
            {
                SdkUtil.logd($"ads full helper waitStatusAdsCloseErr close errrrrrr");
                statuschekAdsFullCloseErr = 0;
                if (_cbFullShow != null)
                {
                    _cbFullShow(AD_State.AD_CLOSE);
                }
            }
            else
            {
                SdkUtil.logd($"ads full helper waitStatusAdsCloseErr statuschekAdsFullCloseErr={statuschekAdsFullCloseErr}");
            }
            if (statuschekAdsGiftErr == 2)
            {
                SdkUtil.logd($"ads full helper waitStatusAdsCloseErr gift");
                statuschekAdsGiftErr = 0;
                if (!isGiftCallMissCB)
                {
                    SdkUtil.logd($"ads full helper waitStatusAdsCloseErr gift: AD_SHOW_MISS_CB");
                    isGiftCallMissCB = true;
                    if (_cbGiftShow != null)
                    {
                        _cbGiftShow(AD_State.AD_SHOW_MISS_CB);
                    }
                }
            }
        }
        private void waitCheckCbGiftErr()
        {
            SdkUtil.logd($"ads helper waitCheckCbGiftErr 0");
            if (!isGiftCallMissCB)
            {
                SdkUtil.logd($"ads helper waitCheckCbGiftErr gift: AD_SHOW_MISS_CB");
                isGiftCallMissCB = true;
                if (_cbGiftShow != null)
                {
                    _cbGiftShow(AD_State.AD_SHOW_MISS_CB);
                }
            }
        }

        private bool checkAdShowIsSame(string namefg1, string fullgiftadnet)
        {
            if (namefg1 != null && fullgiftadnet != null && namefg1.Length > 1 && fullgiftadnet.Length > 1)
            {
                namefg1 = namefg1.ToLower();
                string adNet = fullgiftadnet.ToLower();
                if (adNet.Contains("google") || adNet.Contains("admob") || adNet.Contains("admanager"))
                {
                    if (namefg1.Contains("google") || namefg1.Contains("admob") || namefg1.Contains("admanager"))
                    {
                        SdkUtil.logd($"ads fullgift checkAdShowIsSame is same {namefg1} and {fullgiftadnet}");
                        return true;
                    }
                }
                else if (adNet.Contains("applovin") || adNet.Contains("max"))
                {
                    if (namefg1.Contains("applovin") || namefg1.Contains("max"))
                    {
                        SdkUtil.logd($"ads fullgift checkAdShowIsSame is same {namefg1} and {fullgiftadnet}");
                        return true;
                    }
                }
                else if (adNet.Contains("fb") || adNet.Contains("facebook") || adNet.Contains("meta"))
                {
                    if (namefg1.Contains("fb") || namefg1.Contains("facebook") || namefg1.Contains("meta"))
                    {
                        SdkUtil.logd($"ads fullgift checkAdShowIsSame is same {namefg1} and {fullgiftadnet}");
                        return true;
                    }
                }
                else if (adNet.Contains("mintegral"))
                {
                    if (namefg1.Contains("mintegral"))
                    {
                        SdkUtil.logd($"ads fullgift checkAdShowIsSame is same {namefg1} and {fullgiftadnet}");
                        return true;
                    }
                }
                else if (adNet.Contains("pangle") || adNet.Contains("bytedance"))
                {
                    if (namefg1.Contains("pangle") || namefg1.Contains("bytedance"))
                    {
                        SdkUtil.logd($"ads fullgift checkAdShowIsSame is same {namefg1} and {fullgiftadnet}");
                        return true;
                    }
                }
                else if (adNet.Contains("vungle") || adNet.Contains("liftoff"))
                {
                    if (namefg1.Contains("vungle") || namefg1.Contains("liftoff"))
                    {
                        SdkUtil.logd($"ads fullgift checkAdShowIsSame is same {namefg1} and {fullgiftadnet}");
                        return true;
                    }
                }
            }
            return false;
        }
        public static void logCountTotal()
        {
            countTotalShowAds++;
            PlayerPrefs.SetInt("mem_count_to_show", countTotalShowAds);
            string slog = $"show_ads_total_{countTotalShowAds:000}";
            FIRhelper.logEvent(slog);
            if (countTotalShowAds == 3
                || countTotalShowAds == 5
                || countTotalShowAds == 10
                || countTotalShowAds == 15
            )
            {
#if ENABLE_AppsFlyer
                Dictionary<string, string> additionalParameters = new Dictionary<string, string>();
                AppsFlyerSDK.AppsFlyer.sendEvent(slog, additionalParameters);
#endif
            }
            else if (countTotalShowAds % 10 == 0)
            {
#if ENABLE_AppsFlyer
                Dictionary<string, string> additionalParameters = new Dictionary<string, string>();
                AppsFlyerSDK.AppsFlyer.sendEvent(slog, additionalParameters);
#endif
            }
        }
        //===========================================================================================
        private void loadFullEditor()
        {
#if UNITY_EDITOR
            adsEditorCtr.fullEditor.loadAds();
#endif
        }
        private void showFullEditor()
        {
#if UNITY_EDITOR
            adsEditorCtr.showFullEditor();
#endif
        }
        private void loadGiftEditor()
        {
#if UNITY_EDITOR
            SdkUtil.logd($"ads gift helper RequestRewardBasedVideo editor");
            adsEditorCtr.giftEditor.loadAds();
#endif
        }
        private void showGiftEditor()
        {
#if UNITY_EDITOR
            adsEditorCtr.showGiftEditor();
#endif
        }

        private void showRectEditor(AD_BANNER_POS location, float width, int maxH, float dxCenter, float dyVertical)
        {
#if UNITY_EDITOR
            adsEditorCtr.showRect(location, width, maxH, dxCenter, dyVertical);
#endif
        }
        private void hideRectEditor()
        {
#if UNITY_EDITOR
            adsEditorCtr.hideRect();
#endif
        }

#if UNITY_EDITOR
        public void EditorOnFullClose()
        {
            if (_cbFullEditor != null)
            {
                SdkUtil.logd($"ads full helper onFullClose1");
                _cbFullEditor(AD_State.AD_CLOSE);
            }
            if (isFullLoadWhenClose)
            {
                loadFull4ThisTurn(AdsBase.PLFullDefault, false, GameRes.GetLevel(Level_type.Common), 0, false);
            }

        }
        public void EditorOnGiftClose(bool isrw)
        {
            if (isrw && _cbGiftEditor != null)
            {
                SdkUtil.logd($"ads gift helper rw onGifClose1");
                _cbGiftEditor(AD_State.AD_REWARD_OK);
            }


            if (_cbGiftEditor != null)
            {
                SdkUtil.logd($"ads gift helper rw onGifClose2");
                _cbGiftEditor(AD_State.AD_CLOSE);
            }

            if (isGiftLoadWhenClose)
            {
                loadGift4ThisTurn(AdsBase.PLGiftDefault, GameRes.LevelCommon(), null);
            }

        }
#endif
    }

    public class BannerPlacementCf
    {
        public int status = 1;
        public int timeAutoReload = 0;
        public int timeChangeCl = 20;
    }
}