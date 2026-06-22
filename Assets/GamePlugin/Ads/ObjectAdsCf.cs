using System;
using System.Collections.Generic;
using UnityEngine;
using mygame.sdk;
using MyJson;

public class ObjectAdsCf
{
    public string countrycode = mygame.sdk.GameHelper.CountryDefault;
    public string mediaCampain = "unknow";

    public int OpenAdShowtype = 4;
    public int OpenAdShowat;
    public int OpenAdIsShowFirstOpen;
    public int OpenAdDelTimeOpen;
    public int OpenAdTimeShowStart;
    public int OpenadLvshow;
    public int OpenAdTimeWaitShowFirst;
    public int typeMyopenAd;
    public int OpenAdTimeBg;
    public int OpenAdFlagOpenFormat;
    public string adCfPlacementOpen;

    public int verShowBanner;
    public int timeReloadBanner;
    public int timeChangeCl2Banner;
    public int timeAutoReloadBanner;
    public int timeAutoReloadBannerCl;
    public int typeAutoReloadBannerCl;
    public string stepShowBanner;
    public int bnSessionShow = 1;
    public int bnTimeStartShow = 0;
    public string clIntervalnumover;
    public string adCfPlacementBanner;
    public string adCfPlacementCollapse;
    public string adCfPlacementRect;
    public string adCfPlacementBnNt;
    public string adCfPlacementNattiveCl;
    public string adCfPlacementRectNt;
    public List<int> bnStepShowCircle;
    public List<int> bnStepShowRe;

    public List<int> nativeStepShow;
    public string adCfPlacementNative;
    
    public string activeRewardLevel;
    public string goldCompleteLevel;
    public int fullTotalOfday;
    public int fullLevelStart;
    public int fullSessionStart;
    public int full2LevelStart;
    public int fullTimeStart;
    public int fullDeltatime;
    public int full2Deltatime;
    public int full2TypeShow;
    public int fullShowPlaying;
    public int fullDefaultNumover = 2;
    public int full2Numover = 2;
    public int fullImgDeltatime;
    public int full2Starttime = 0;
    public int full2Sessiontime = 0;
    public int full2CountFull = 0;
    public int full2Flag4VN = 0;
    public int fullFlagFor2vscl = 7;//bit1 for full, bit2 for gift bit3 for collapse
    public int fullDeltaLose = 1;
    public int fullDeltaTime4Gift = 0;
    public int fullTimeAfterPurchase = 0; //second
    public int cfDayActiveIdx = -1;
    public List<DayActiveCondition> cfDayActiveCon = new List<DayActiveCondition>();

#if UNITY_IOS || UNITY_IPHONE
    public int fullNtIsIc = 0;
#else
    public int fullNtIsIc = 1;
#endif
    public string full2StepShow;
    public string stepShowFull;
    public string fullStepLevel;
    public string fullIntervalnumover;
    public string adCfPlacementNtFull;
    public string adCfPlacementNtIcFull;
    public string adCfPlacementFull;
    public string adCfPlacementNtPgFull;
    public string adCfPlacementFullRwInter;
    public string adCfPlacementFullRwRw;
    public string adCfPlacementGift;
    public string adCfPlacementNtGift;
    public string adCfPlacementGiftFull;
    public string excluseFullrunning;
    public List<IntervalLevelShowfull> fullListIntervalShow = new List<IntervalLevelShowfull>();
    public List<StepLevelShowfull> fullListStepLevelShow = new List<StepLevelShowfull>();
    public List<int> fullStepShowCircle;
    public List<int> fullStepShowRe;
    public List<int> fullExcluseShowRunning;
    public List<int> full2StepShowCircle;
    public List<int> full2StepShowRe;
    public List<IntervalLevelShowfull> clListIntervalShow = new List<IntervalLevelShowfull>();
    public int isLoadMaxLow;

    public string cfFullRw = "-1,2,5,90,90,240";
    public int fullRwType = 2;
    public int fullRwNumSession = 2;
    public int fullRwNumTotal = 5;
    public int fullRwTimeStart = 90;
    public int fullRwTimeSession = 90;
    public int fullRwDeltatime = 180;

    public int maskAdsStatus = 0xf;

    public int giftTotalOfday;
    public int giftDeltatime;
    public string stepShowGift;
    public List<int> giftStepShowCircle;
    public List<int> giftStepShowRe;

    //mytarget
    public string mytargetStepFloorECPMBanner;
    public string mytargetStepFloorECPMFull;
    public string mytargetStepFloorECPMGift;

    //Yandex
    public string yandexStepFloorECPMBanner;
    public string yandexStepFloorECPMFull;
    public string yandexStepFloorECPMGift;

    public int specialType;// =0-session, 1-timeplay
    public string specialData = "";
    public List<SpecialConditionShow> listSpecialShow = new List<SpecialConditionShow>();

    public int typeLoadStart = 1;//0-auto, 1-video-auto, 2-video only

    public ObjectAdsCf(string code = mygame.sdk.GameHelper.CountryDefault, string media = "default")
    {
        bnStepShowCircle = new List<int>();
        bnStepShowRe = new List<int>();

        nativeStepShow = new List<int>();

        fullStepShowCircle = new List<int>();
        fullStepShowRe = new List<int>();
        fullExcluseShowRunning = new List<int>();
        full2StepShowCircle = new List<int>();
        full2StepShowRe = new List<int>();
        fullListIntervalShow.Clear();
        clListIntervalShow.Clear();

        giftStepShowCircle = new List<int>();
        giftStepShowRe = new List<int>();

        listSpecialShow.Clear();

        countrycode = code;
        mediaCampain = media;
        if (mediaCampain == null || mediaCampain.Length < 1)
        {
            mediaCampain = "default";
        }
        mediaCampain = mediaCampain.ToLower();
        //loadFromPlayerPrefs();
    }

    public void coppyFromOther(ObjectAdsCf other)
    {
        if (other != null)
        {
            Debug.Log($"mysdk: object ads coppy {other.countrycode} to {countrycode}");
            maskAdsStatus = other.maskAdsStatus;
            clIntervalnumover = other.clIntervalnumover;
            parserIntervalnumoverShow(clIntervalnumover, false);
            stepShowBanner = other.stepShowBanner;
            bnSessionShow = other.bnSessionShow;
            bnTimeStartShow = other.bnTimeStartShow;
            timeReloadBanner = other.timeReloadBanner;
            timeAutoReloadBanner = other.timeAutoReloadBanner;
            timeAutoReloadBannerCl = other.timeAutoReloadBannerCl;
            typeAutoReloadBannerCl = other.typeAutoReloadBannerCl;
            timeChangeCl2Banner = other.timeChangeCl2Banner;
            parSerStepBN(stepShowBanner);
            if (bnStepShowCircle.Count == 0 && bnStepShowRe.Count == 0)
            {
                parSerStepBN(AppConfig.defaultStepBanner);
            }
            string sstepnative = PlayerPrefs.GetString("cf_step_native", "0,10");
            parSerStepNative(sstepnative);

            copyCfFullFromOther(other);

            full2LevelStart = other.full2LevelStart;
            full2Numover = other.full2Numover;
            full2Deltatime = other.full2Deltatime;
            full2TypeShow = other.full2TypeShow;
            full2Starttime = other.full2Starttime;
            full2Sessiontime = other.full2Sessiontime;
            full2CountFull = other.full2CountFull;
            full2StepShow = other.full2StepShow;
            full2Flag4VN = other.full2Flag4VN;
            parSerStepFull2(full2StepShow);
            checkFlagVn();

            giftTotalOfday = other.giftTotalOfday;
            giftDeltatime = other.giftDeltatime;
            stepShowGift = other.stepShowGift;
            parSerStepGift(stepShowGift);
            if (giftStepShowCircle.Count == 0 && giftStepShowRe.Count == 0)
            {
                parSerStepGift(AppConfig.defaultStepGift);
            }

            if (AdsHelper.Instance != null)
            {
                OpenAdShowtype = 4;
            }
            OpenAdShowtype = other.OpenAdShowtype;
            OpenAdShowat = other.OpenAdShowat;
            OpenAdIsShowFirstOpen = other.OpenAdIsShowFirstOpen;
            OpenAdDelTimeOpen = other.OpenAdDelTimeOpen;
            OpenAdTimeShowStart = other.OpenAdTimeShowStart;
            OpenadLvshow = other.OpenadLvshow;
            OpenAdTimeWaitShowFirst = other.OpenAdTimeWaitShowFirst;
            OpenAdTimeBg = other.OpenAdTimeBg;
            OpenAdFlagOpenFormat = other.OpenAdFlagOpenFormat;
            adCfPlacementOpen = other.adCfPlacementOpen;

            typeMyopenAd = other.typeMyopenAd;

            adCfPlacementNative = other.adCfPlacementNative;

            adCfPlacementBanner = other.adCfPlacementBanner;
            adCfPlacementBnNt = other.adCfPlacementBnNt;
            adCfPlacementRectNt = other.adCfPlacementRectNt;
            adCfPlacementNattiveCl = other.adCfPlacementNattiveCl;
            adCfPlacementCollapse = other.adCfPlacementCollapse;
            adCfPlacementRect = other.adCfPlacementRect;
            adCfPlacementNtFull = other.adCfPlacementNtFull;
            adCfPlacementNtIcFull = other.adCfPlacementNtIcFull;
            adCfPlacementNtPgFull = other.adCfPlacementNtPgFull;
            adCfPlacementFull = other.adCfPlacementFull;
            adCfPlacementFullRwInter = other.adCfPlacementFullRwInter;
            adCfPlacementFullRwRw = other.adCfPlacementFullRwRw;
            adCfPlacementGift = other.adCfPlacementGift;
            adCfPlacementNtGift = other.adCfPlacementNtGift;
            adCfPlacementGiftFull = other.adCfPlacementGiftFull;

            mytargetStepFloorECPMBanner = other.mytargetStepFloorECPMBanner;
            mytargetStepFloorECPMFull = other.mytargetStepFloorECPMFull;
            mytargetStepFloorECPMGift = other.mytargetStepFloorECPMGift;

            yandexStepFloorECPMBanner = other.yandexStepFloorECPMBanner;
            yandexStepFloorECPMFull = other.yandexStepFloorECPMFull;
            yandexStepFloorECPMGift = other.yandexStepFloorECPMGift;

            specialType = other.specialType;
            specialData = other.specialData;
            parSerSpecialStep(specialData);
        }
    }

    public void copyCfFullFromOther(ObjectAdsCf other)
    {
        if (other != null)
        {
            Debug.Log($"mysdk: ads full coppy cf from  {other.countrycode} to {countrycode}");
            fullTotalOfday = other.fullTotalOfday;
            fullLevelStart = other.fullLevelStart;
            fullSessionStart = other.fullSessionStart;
            fullFlagFor2vscl = other.fullFlagFor2vscl;
            fullNtIsIc = other.fullNtIsIc;
            fullTimeStart = other.fullTimeStart;
            fullDefaultNumover = other.fullDefaultNumover;
            fullShowPlaying = other.fullShowPlaying;
            fullIntervalnumover = other.fullIntervalnumover;
            parserIntervalnumoverShow(fullIntervalnumover, true);
            fullStepLevel = other.fullStepLevel;
            parserFullStepLevelShow(fullStepLevel);
            fullDeltatime = other.fullDeltatime;
            fullImgDeltatime = other.fullImgDeltatime;
            typeLoadStart = other.typeLoadStart;
            stepShowFull = other.stepShowFull;
            isLoadMaxLow = other.isLoadMaxLow;
            parSerStepFull(stepShowFull);
            if (fullStepShowCircle.Count == 0 && fullStepShowRe.Count == 0)
            {
                parSerStepFull(AppConfig.defaultStepFull);
            }
            excluseFullrunning = other.excluseFullrunning;
            parSerExcluseFull(excluseFullrunning);

            fullDeltaLose = other.fullDeltaLose;
            fullDeltaTime4Gift = other.fullDeltaTime4Gift;
            cfDayActiveIdx = other.cfDayActiveIdx;
            cfDayActiveCon.Clear();
            cfDayActiveCon.AddRange(other.cfDayActiveCon);

            cfFullRw = other.cfFullRw;
            fullRwType = other.fullRwType;
            fullRwNumSession = other.fullRwNumSession;
            fullRwNumTotal = other.fullRwNumTotal;
            fullRwTimeStart = other.fullRwTimeStart;
            fullRwTimeSession = other.fullRwTimeSession;
            fullRwDeltatime = other.fullRwDeltatime;
        }
    }

    public void loadFromPlayerPrefs()
    {
        maskAdsStatus = PlayerPrefs.GetInt("cf_mask_ads_status", 0xf);
        clIntervalnumover = PlayerPrefs.GetString("cf_clNumover", "");
        parserIntervalnumoverShow(clIntervalnumover, false);
        stepShowBanner = PlayerPrefs.GetString("cf_step_banner", AppConfig.defaultStepBanner);
        bnSessionShow = PlayerPrefs.GetInt("cf_bn_sesion_show", 1);
        bnTimeStartShow = PlayerPrefs.GetInt("cf_bn_tstart_show", 0);
        timeReloadBanner = PlayerPrefs.GetInt("cf_time_reload_banner", 36000);
        timeChangeCl2Banner = PlayerPrefs.GetInt("cf_time_change_banner", 15);
        timeAutoReloadBanner = PlayerPrefs.GetInt("cf_time_autoreload_banner", -1);
        timeAutoReloadBannerCl = PlayerPrefs.GetInt("cf_time_autoreload_bannercl", -1);
        typeAutoReloadBannerCl = PlayerPrefs.GetInt("cf_type_autoreload_bannercl", 0);
        parSerStepBN(stepShowBanner);
        if (bnStepShowCircle.Count == 0 && bnStepShowRe.Count == 0)
        {
            parSerStepBN(AppConfig.defaultStepBanner);
        }
        string sstepnative = PlayerPrefs.GetString("cf_step_native", "0,10");
        parSerStepNative(sstepnative);

        fullTotalOfday = PlayerPrefs.GetInt("cf_fullTotalOfday", 200);
        fullLevelStart = PlayerPrefs.GetInt("cf_fullLevelStart", AppConfig.full_lv_start);
        fullSessionStart = PlayerPrefs.GetInt("cf_fullSessionStart", 1);
        fullFlagFor2vscl = PlayerPrefs.GetInt("cf_fullFlagFor2vscl", 7);
#if UNITY_IOS || UNITY_IPHONE
        fullNtIsIc = 0;
#else
        fullNtIsIc = PlayerPrefs.GetInt("cf_fullNtIsIc", 1);
#endif

        fullTimeStart = PlayerPrefs.GetInt("cf_fullTimeStart", 0);
        fullDefaultNumover = PlayerPrefs.GetInt("cf_fulldefaultnumover", 1);
        fullIntervalnumover = PlayerPrefs.GetString("cf_fullNumover", "");
        parserIntervalnumoverShow(fullIntervalnumover, true);
        fullStepLevel = PlayerPrefs.GetString("cf_fullsteplevel", "");
        parserFullStepLevelShow(fullStepLevel);
        fullDeltatime = PlayerPrefs.GetInt("cf_fullDeltatime", AppConfig.full_deltime * 1000);
        fullImgDeltatime = PlayerPrefs.GetInt("cf_fullImgDeltatime", 90000);
        typeLoadStart = PlayerPrefs.GetInt("cf_type_full_start", 1);
        fullShowPlaying = PlayerPrefs.GetInt("cf_fullShowPlaying", 0);
        isLoadMaxLow = PlayerPrefs.GetInt("cf_load_maxlow", 0);
        stepShowFull = PlayerPrefs.GetString("cf_step_full", AppConfig.defaultStepFull);
        parSerStepFull(stepShowFull);
        if (fullStepShowCircle.Count == 0 && fullStepShowRe.Count == 0)
        {
            parSerStepFull(AppConfig.defaultStepFull);
        }
        excluseFullrunning = PlayerPrefs.GetString("cf_full_excluse_run", "");
        parSerExcluseFull(excluseFullrunning);

        cfFullRw = PlayerPrefs.GetString("cf_fullrw", "-1,2,5,90,90,240");
        parserFullRw(cfFullRw);

        fullDeltaLose = PlayerPrefs.GetInt("cf_full_deltalose", 1);
        fullDeltaTime4Gift = PlayerPrefs.GetInt("cf_full_deltatfugift", 0);
        string configdayactive = PlayerPrefs.GetString("cf_full_dayactive", "");
        parserDayActive(configdayactive);

        full2LevelStart = PlayerPrefs.GetInt("cf_full2LevelStart", AppConfig.full2_lv_start);
        full2Numover = PlayerPrefs.GetInt("cf_full2numover", 1);
        full2Deltatime = PlayerPrefs.GetInt("cf_full2Deltatime", AppConfig.full2_deltime * 1000);
        full2TypeShow = PlayerPrefs.GetInt("cf_full2TypeShow", AppConfig.full2_type);
        full2Starttime = PlayerPrefs.GetInt("cf_full2Starttime", 0);
        full2Sessiontime = PlayerPrefs.GetInt("cf_full2Sessiontime", 3600);
        full2CountFull = PlayerPrefs.GetInt("cf_full2CountFull", 0);
        full2Flag4VN = PlayerPrefs.GetInt("cf_full2Flag4VN", 0);
        full2StepShow = PlayerPrefs.GetString("cf_full2StepShow", "cir:20#re:60,61,21,10");
        parSerStepFull2(full2StepShow);
        checkFlagVn();
        fullTimeAfterPurchase = PlayerPrefs.GetInt("cf_fullTimeAfterPurchase", 0);
        giftTotalOfday = PlayerPrefs.GetInt("cf_giftTotalOfday", 200);
        giftDeltatime = PlayerPrefs.GetInt("cf_giftDeltatime", 5000);
        stepShowGift = PlayerPrefs.GetString("cf_step_gift", AppConfig.defaultStepGift);
        parSerStepGift(stepShowGift);
        if (giftStepShowCircle.Count == 0 && giftStepShowRe.Count == 0)
        {
            parSerStepGift(AppConfig.defaultStepGift);
        }

        OpenAdShowtype = PlayerPrefs.GetInt("cf_open_ad_type", AppConfig.open_type);
        OpenAdShowat = PlayerPrefs.GetInt("cf_open_ad_showat", 1);
        OpenAdIsShowFirstOpen = PlayerPrefs.GetInt("cf_open_ad_show_firstopen", 1);
        OpenAdDelTimeOpen = PlayerPrefs.GetInt("cf_open_ad_deltime", 30);
        OpenAdTimeShowStart = PlayerPrefs.GetInt("cf_open_ad_timestart", 0);
        OpenadLvshow = PlayerPrefs.GetInt("cf_open_ad_level_show", AppConfig.open_lv);
        OpenAdTimeWaitShowFirst = PlayerPrefs.GetInt("cf_open_ad_wait_first", 60);
        OpenAdTimeBg = PlayerPrefs.GetInt("cf_open_ad_tbg", 20);
        OpenAdFlagOpenFormat = PlayerPrefs.GetInt("cf_open_ad_formatopen", 1);
        adCfPlacementOpen = PlayerPrefs.GetString("cf_placement_openad", AdIdsConfig.AdmobPlOpenAd);

        typeMyopenAd = PlayerPrefs.GetInt("cf_type_myopenad", 0);

        adCfPlacementNative = PlayerPrefs.GetString("cf_placement_native", AdIdsConfig.AdmobPlNative);

        adCfPlacementBanner = PlayerPrefs.GetString("cf_placement_banner", AdIdsConfig.AdmobPlBanner);
        adCfPlacementBnNt = PlayerPrefs.GetString("cf_placement_bnnt", AdIdsConfig.AdmobPlBnNt);
        adCfPlacementRectNt = PlayerPrefs.GetString("cf_placement_rectnt", AdIdsConfig.AdmobPlRectNt);
        adCfPlacementNattiveCl = PlayerPrefs.GetString("cf_placement_ntcl", AdIdsConfig.AdmobPlBnNativeCl);
        adCfPlacementCollapse = PlayerPrefs.GetString("cf_placement_collapse", AdIdsConfig.AdmobPlCl);
        adCfPlacementRect = PlayerPrefs.GetString("cf_placement_rect", AdIdsConfig.AdmobPlRect);
        adCfPlacementNtFull = PlayerPrefs.GetString("cf_placement_ntfull", AdIdsConfig.AdmobPlNtFull);
        adCfPlacementNtIcFull = PlayerPrefs.GetString("cf_placement_nticfull", AdIdsConfig.AdmobPlNtIcFull);
        adCfPlacementNtPgFull = PlayerPrefs.GetString("cf_placement_ntpgfull", AdIdsConfig.AdmobPlNtPgFull);
        adCfPlacementFull = PlayerPrefs.GetString("cf_placement_full", AdIdsConfig.AdmobPlFullAll);
        adCfPlacementFullRwInter = PlayerPrefs.GetString("cf_placement_fullrwinter", AdIdsConfig.AdmobPlFullRwInter);
        adCfPlacementFullRwRw = PlayerPrefs.GetString("cf_placement_fullrwrw", AdIdsConfig.AdmobPlFullRwRw);
        adCfPlacementGift = PlayerPrefs.GetString("cf_placement_gift", AdIdsConfig.AdmobPlGift);
        adCfPlacementNtGift = PlayerPrefs.GetString("cf_placement_ntgift", AdIdsConfig.AdmobPlNtGift);
        adCfPlacementGiftFull = PlayerPrefs.GetString("cf_placement_giftfull", AdIdsConfig.AdmobPlGiftFull);

        mytargetStepFloorECPMBanner = PlayerPrefs.GetString("mytarget_banner_floor_ecpm", AdIdsConfig.TargetBnFloor);
        mytargetStepFloorECPMFull = PlayerPrefs.GetString("mytarget_full_floor_ecpm", AdIdsConfig.TargetFullFloor);
        mytargetStepFloorECPMGift = PlayerPrefs.GetString("mytarget_gift_floor_ecpm", AdIdsConfig.TargetGiftFloor);

        yandexStepFloorECPMBanner = PlayerPrefs.GetString("yandex_banner_floor_ecpm", AdIdsConfig.YandexBnFloor);
        yandexStepFloorECPMFull = PlayerPrefs.GetString("yandex_full_floor_ecpm", AdIdsConfig.YandexFullFloor);
        yandexStepFloorECPMGift = PlayerPrefs.GetString("yandex_gift_floor_ecpm", AdIdsConfig.YandexGiftFloor);

        specialType = PlayerPrefs.GetInt("cf_special_type", 0);
        specialData = PlayerPrefs.GetString("cf_special_data", "");
        parSerSpecialStep(specialData);
    }

    public void saveAllConfig()
    {
        SdkUtil.logd($"ads object adcf cam={mediaCampain} country={countrycode}");
        checkFlagVn();
        PlayerPrefs.SetInt("cf_mask_ads_status", maskAdsStatus);
        PlayerPrefs.SetInt("cf_open_ad_type", OpenAdShowtype);
        PlayerPrefs.SetInt("cf_open_ad_showat", OpenAdShowat);
        PlayerPrefs.SetInt("cf_open_ad_tbg", OpenAdTimeBg);
        PlayerPrefs.SetInt("cf_open_ad_formatopen", OpenAdFlagOpenFormat);
        PlayerPrefs.SetInt("cf_open_ad_show_firstopen", OpenAdIsShowFirstOpen);
        PlayerPrefs.SetInt("cf_open_ad_deltime", OpenAdDelTimeOpen);
        PlayerPrefs.SetInt("cf_open_ad_timestart", OpenAdTimeShowStart);
        PlayerPrefs.SetInt("cf_open_ad_level_show", OpenadLvshow);
        PlayerPrefs.SetInt("cf_open_ad_wait_first", OpenAdTimeWaitShowFirst);
        PlayerPrefs.SetString("cf_placement_openad", adCfPlacementOpen);

        PlayerPrefs.SetInt("cf_type_myopenad", typeMyopenAd);

        PlayerPrefs.SetInt("android_build_ver_show_bn", verShowBanner);

        PlayerPrefs.SetString("cf_placement_native", adCfPlacementNative);

        PlayerPrefs.SetString("cf_placement_banner", adCfPlacementBanner);
        PlayerPrefs.SetString("cf_placement_bnnt", adCfPlacementBnNt);
        PlayerPrefs.SetString("cf_placement_rectnt", adCfPlacementRectNt);
        PlayerPrefs.SetString("cf_placement_ntcl", adCfPlacementNattiveCl);
        PlayerPrefs.SetString("cf_placement_collapse", adCfPlacementCollapse);
        PlayerPrefs.SetString("cf_placement_rect", adCfPlacementRect);
        PlayerPrefs.SetString("cf_placement_ntfull", adCfPlacementNtFull);
        PlayerPrefs.SetString("cf_placement_nticfull", adCfPlacementNtIcFull);
        PlayerPrefs.SetString("cf_placement_ntpgfull", adCfPlacementNtPgFull);
        PlayerPrefs.SetString("cf_placement_full", adCfPlacementFull);
        PlayerPrefs.SetString("cf_placement_fullrwinter", adCfPlacementFullRwInter);
        PlayerPrefs.SetString("cf_placement_fullrwrw", adCfPlacementFullRwRw);
        PlayerPrefs.SetString("cf_placement_gift", adCfPlacementGift);
        PlayerPrefs.SetString("cf_placement_ntgift", adCfPlacementNtGift);
        PlayerPrefs.SetString("cf_placement_giftfull", adCfPlacementGiftFull);

        PlayerPrefs.SetString("mytarget_banner_floor_ecpm", mytargetStepFloorECPMBanner);
        PlayerPrefs.SetString("mytarget_full_floor_ecpm", mytargetStepFloorECPMFull);
        PlayerPrefs.SetString("mytarget_gift_floor_ecpm", mytargetStepFloorECPMGift);

        PlayerPrefs.SetString("yandex_banner_floor_ecpm", yandexStepFloorECPMBanner);
        PlayerPrefs.SetString("yandex_full_floor_ecpm", yandexStepFloorECPMFull);
        PlayerPrefs.SetString("yandex_gift_floor_ecpm", yandexStepFloorECPMGift);

        PlayerPrefs.SetInt("cf_special_type", specialType);
        PlayerPrefs.SetString("cf_special_data", specialData);

        saveBNConfig();
        saveFullConfig();
        savefull2Cf();
        saveGiftConfig();
    }

    public void saveBNConfig()
    {
        //banner
        PlayerPrefs.SetString("cf_step_banner", stepShowBanner);
        PlayerPrefs.SetInt("cf_bn_sesion_show", bnSessionShow);
        PlayerPrefs.GetInt("cf_bn_tstart_show", bnTimeStartShow);
        PlayerPrefs.SetInt("cf_time_reload_banner", timeReloadBanner);
        PlayerPrefs.SetInt("cf_time_change_banner", timeChangeCl2Banner);
        PlayerPrefs.SetInt("cf_time_autoreload_banner", timeAutoReloadBanner);
        PlayerPrefs.SetInt("cf_time_autoreload_bannercl", timeAutoReloadBannerCl);
        PlayerPrefs.SetInt("cf_type_autoreload_bannercl", typeAutoReloadBannerCl);
        PlayerPrefs.SetString("cf_clNumover", listIntervaltoString(clListIntervalShow));
    }

    public void saveFullConfig()
    {
        //full
        PlayerPrefs.SetInt("cf_fullTotalOfday", fullTotalOfday);
        PlayerPrefs.SetInt("cf_fullLevelStart", fullLevelStart);
        PlayerPrefs.SetInt("cf_fullSessionStart", fullSessionStart);
        PlayerPrefs.SetInt("cf_fullFlagFor2vscl", fullFlagFor2vscl);
        PlayerPrefs.SetInt("cf_fullNtIsIc", fullNtIsIc);
        PlayerPrefs.SetInt("cf_fullTimeStart", fullTimeStart);
        PlayerPrefs.SetInt("cf_fulldefaultnumover", fullDefaultNumover);
        PlayerPrefs.SetInt("cf_fullShowPlaying", fullShowPlaying);
        PlayerPrefs.SetString("cf_fullNumover", fullIntervalnumover);
        PlayerPrefs.SetString("cf_fullsteplevel", fullStepLevel);
        PlayerPrefs.SetInt("cf_fullDeltatime", fullDeltatime);
        PlayerPrefs.SetInt("cf_fullImgDeltatime", fullImgDeltatime);
        PlayerPrefs.SetInt("cf_type_full_start", typeLoadStart);
        PlayerPrefs.SetInt("cf_load_maxlow", isLoadMaxLow);
        PlayerPrefs.SetString("cf_step_full", stepShowFull);
        PlayerPrefs.SetString("cf_full_excluse_run", excluseFullrunning);
        PlayerPrefs.SetString("cf_fullrw", cfFullRw);

        PlayerPrefs.SetInt("cf_full_deltalose", fullDeltaLose);
        PlayerPrefs.SetInt("cf_full_deltatfugift", fullDeltaTime4Gift);
        PlayerPrefs.SetInt("cf_fullTimeAfterPurchase", fullTimeAfterPurchase);
        PlayerPrefs.SetString("cf_full_dayactive", dayActiveToString());
    }

    public void savefull2Cf()
    {
        PlayerPrefs.SetInt("cf_full2LevelStart", full2LevelStart);
        PlayerPrefs.SetInt("cf_full2numover", full2Numover);
        PlayerPrefs.SetInt("cf_full2Deltatime", full2Deltatime);
        PlayerPrefs.SetInt("cf_full2TypeShow", full2TypeShow);
        PlayerPrefs.SetInt("cf_full2Starttime", full2Starttime);
        PlayerPrefs.SetInt("cf_full2Sessiontime", full2Sessiontime);
        PlayerPrefs.SetInt("cf_full2CountFull", full2CountFull);
        PlayerPrefs.SetString("cf_full2StepShow", full2StepShow);
        PlayerPrefs.SetInt("cf_full2Flag4VN", full2Flag4VN);
    }

    public void saveGiftConfig()
    {
        //gift
        PlayerPrefs.SetInt("cf_giftTotalOfday", giftTotalOfday);
        PlayerPrefs.SetInt("cf_giftDeltatime", giftDeltatime);
        PlayerPrefs.SetString("cf_step_gift", stepShowGift);
    }

    public void parSerStepBN(string step)
    {
        if (step == null || step.Length == 0)
        {
            return;
        }
        if (AppConfig.isOnlyDefault)
        {
            step = AppConfig.defaultStepBanner;
        }
        if (AdsHelper.Instance != null && AdsHelper.Instance.statusLogicIron == 1)
        {
            if (countrycode.CompareTo("ru") == 0)
            {
                if (step.StartsWith("cir:8"))
                {
                    step = step.Replace("cir:8", "cir:3");
                    if (!step.Contains("8"))
                    {
                        step += ",8";
                    }
                }
            }
        }
        Debug.Log($"mysdk: parSerStepBN={step}");
        string[] steptype = step.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
        for (int it = 0; it < steptype.Length; it++)
        {
            if (steptype[it].StartsWith("cir:"))
            {
                string reSrep = steptype[it].Substring(4);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                bnStepShowCircle.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        bnStepShowCircle.Add(value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (steptype[it].StartsWith("re:"))
            {
                string reSrep = steptype[it].Substring(3);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                bnStepShowRe.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        bnStepShowRe.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }

    public void parSerStepNative(string step)
    {
        if (step == null || step.Length == 0)
        {
            return;
        }
        // Debug.Log("mysdk: parSerStepNative=" + step);
        string[] sl = step.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        nativeStepShow.Clear();
        for (int i = 0; i < sl.Length; i++)
        {
            int value;
            if (int.TryParse(sl[i], out value))
            {
                nativeStepShow.Add(value);
            }
        }
    }

    public string listIntervaltoString(List<IntervalLevelShowfull> list)
    {
        string re = "";
        for (int i = 0; i < list.Count; i++)
        {
            string sd = string.Format("{0},{1},{2}", list[i].startlevel, list[i].endLevel,
                list[i].deltal4Show);
            if (i == 0)
            {
                re += sd;
            }
            else
            {
                re += (";" + sd);
            }
        }

        return re;
    }

    public void parserFullRw(string cfdata)
    {
        try
        {
            string[] arr = cfdata.Split(new char[] { ',' });
            if (arr != null && arr.Length >= 6)
            {
                fullRwType = int.Parse(arr[0]);
                fullRwNumSession = int.Parse(arr[1]);
                fullRwNumTotal = int.Parse(arr[2]);
                fullRwTimeStart = int.Parse(arr[3]);
                fullRwTimeSession = int.Parse(arr[4]);
                fullRwDeltatime = int.Parse(arr[5]);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"mysdk: ObjectAdsCf parserFullRw ex=" + ex.ToString());
        }
    }

    public void parserFullStepLevelShow(string data)
    {
        fullListStepLevelShow.Clear();
        try
        {
            string[] arr = data.Split(new char[] { '&' });
            for (int i = 0; i < arr.Length; i++)
            {
                StepLevelShowfull inver = new StepLevelShowfull(arr[i]);
                if (inver.fullStep.stepShowRe.Count >= 0 || inver.fullStep.stepShowCircle.Count > 0)
                {
                    fullListStepLevelShow.Add(inver);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"mysdk: ObjectAdsCf parserFullStepLevelShow ex=" + ex.ToString());
        }
    }

    public void parserIntervalnumoverShow(string data, bool isFull)
    {
        var list = fullListIntervalShow;
        if (!isFull)
        {
            list = clListIntervalShow;
        }
        list.Clear();
        try
        {
            string[] arr = data.Split(new char[] { ';' });
            for (int i = 0; i < arr.Length; i++)
            {
                IntervalLevelShowfull inver = new IntervalLevelShowfull(arr[i]);
                if (inver.deltal4Show >= 0)
                {
                    list.Add(inver);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"mysdk: ObjectAdsCf parserIntervalnumover isfull-{isFull} ex=" + ex.ToString());
        }
    }

    public void parSerExcluseFull(string excluse)
    {
        if (excluse == null || excluse.Length == 0)
        {
            return;
        }
        Debug.Log("mysdk: parSerExcluseFull=" + excluse);
        string[] steptype = excluse.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        fullExcluseShowRunning.Clear();
        for (int it = 0; it < steptype.Length; it++)
        {
            int adste;
            if (int.TryParse(steptype[it], out adste))
            {
                fullExcluseShowRunning.Add(adste);
            }
        }
    }

    public void parSerStepFull(string step)
    {
        if (step == null || step.Length == 0)
        {
            return;
        }
        if (AutoPerformanceOptimizer.isChangeLogicAdsLowRam == 101)
        {
            SdkUtil.logd($"ads full parSerStepFull change to image when device very low");
            step = "cir:10";
        }
        if (AppConfig.isOnlyDefault)
        {
            step = AppConfig.defaultStepFull;
        }
        if (AdsHelper.Instance != null && AdsHelper.Instance.statusLogicIron == 1)
        {
            if (countrycode.CompareTo("ru") == 0)
            {
                if (step.StartsWith("cir:8"))
                {
                    step = step.Replace("cir:8", "cir:3");
                    if (!step.Contains("8"))
                    {
                        step += ",8";
                    }
                }
            }
        }
        Debug.Log("mysdk: parSerStepFull=" + step);
        string[] steptype = step.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
        for (int it = 0; it < steptype.Length; it++)
        {
            if (steptype[it].StartsWith("cir:"))
            {
                string reSrep = steptype[it].Substring(4);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                fullStepShowCircle.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        fullStepShowCircle.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else if (steptype[it].StartsWith("re:"))
            {
                string reSrep = steptype[it].Substring(3);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                fullStepShowRe.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        fullStepShowRe.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }

    private void checkFlagVn()
    {
        int flagvn = GameHelper.getFlagVn();//is vn
        if ((flagvn & full2Flag4VN) > 0)
        {
            full2TypeShow = 0;
            full2Sessiontime = 1800;
            Debug.Log($"mysdk: ads full in nc ta={flagvn}, cf={full2Flag4VN}");
        }
    }

    public void parSerStepFull2(string step)
    {
        if (step == null || step.Length == 0)
        {
            return;
        }
        Debug.Log("mysdk: parSerStepFull2=" + step);
        string[] steptype = step.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
        for (int it = 0; it < steptype.Length; it++)
        {
            if (steptype[it].StartsWith("cir:"))
            {
                string reSrep = steptype[it].Substring(4);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                full2StepShowCircle.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        full2StepShowCircle.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else if (steptype[it].StartsWith("re:"))
            {
                string reSrep = steptype[it].Substring(3);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                full2StepShowRe.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        full2StepShowRe.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }

    public void parSerStepGift(string step)
    {
        if (step == null || step.Length == 0)
        {
            return;
        }
        if (AppConfig.isOnlyDefault)
        {
            step = AppConfig.defaultStepGift;
        }
        if (AdsHelper.Instance != null && AdsHelper.Instance.statusLogicIron == 1)
        {
            if (countrycode.CompareTo("ru") == 0)
            {
                if (step.StartsWith("cir:8"))
                {
                    step = step.Replace("cir:8", "cir:3");
                    if (!step.Contains("8"))
                    {
                        step += ",8";
                    }
                }
            }
        }
        Debug.Log("mysdk: parSerStepGift=" + step);
        string[] steptype = step.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
        for (int it = 0; it < steptype.Length; it++)
        {
            if (steptype[it].StartsWith("cir:"))
            {
                string reSrep = steptype[it].Substring(4);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                giftStepShowCircle.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        giftStepShowCircle.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else if (steptype[it].StartsWith("re:"))
            {
                string reSrep = steptype[it].Substring(3);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                giftStepShowRe.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        giftStepShowRe.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }

    public void parSerSpecialStep(string data)
    {
        if (data != null && data.Length > 3)
        {
            listSpecialShow.Clear();
            var liststep = (List<object>)JsonDecoder.DecodeText(specialData);
            if (liststep != null && liststep.Count > 0)
            {
                foreach (object ob in liststep)
                {
                    IDictionary<string, object> dicob = (IDictionary<string, object>)ob;
                    SpecialConditionShow spec = new SpecialConditionShow(dicob);
                    listSpecialShow.Add(spec);
                }
            }
        }
    }

    public void parSerSpecial(IDictionary<string, object> dicdata)
    {
        listSpecialShow.Clear();
        if (dicdata == null || dicdata.Count == 0)
        {
            return;
        }
        specialType = 0;
        if (dicdata.ContainsKey("type"))
        {
            specialType = Convert.ToInt32(dicdata["type"]);
        }
        if (dicdata.ContainsKey("data"))
        {
            List<object> liststep = (List<object>)dicdata["data"];
            if (liststep != null && liststep.Count > 0)
            {
                specialData = "[";
                bool isStart = true;
                foreach (object ob in liststep)
                {
                    IDictionary<string, object> dicob = (IDictionary<string, object>)ob;
                    SpecialConditionShow spec = new SpecialConditionShow(dicob);
                    listSpecialShow.Add(spec);
                    if (!isStart)
                    {
                        specialData += ",";
                    }
                    specialData += spec.toJsonOb();

                    isStart = false;
                }
                specialData += "]";
            }
        }
    }

    public StepLevelShowfull GetStepLevelShowfull(int lv)
    {
        if (AppConfig.isOnlyDefault)
        {
            return null;
        }
        else
        {
            for (int i = 0; i < fullListStepLevelShow.Count; i++)
            {
                if (fullListStepLevelShow[i].startlevel <= lv && fullListStepLevelShow[i].endLevel >= lv)
                {
                    return fullListStepLevelShow[i];
                }
            }
            return null;
        }
    }

    public DayActiveCondition getDayImpact()
    {
        if (cfDayActiveIdx >= 0 && cfDayActiveCon.Count > cfDayActiveIdx)
        {
            return cfDayActiveCon[cfDayActiveIdx];
        }
        else
        {
            return null;
        }
    }

    public void checkDayActive()
    {
        if (SDKManager.Instance != null)
        {
            for (int i = 0; i < cfDayActiveCon.Count; i++)
            {
                if (SDKManager.Instance.activeDay >= cfDayActiveCon[i].dayStart && SDKManager.Instance.activeDay <= cfDayActiveCon[i].dayEnd)
                {
                    cfDayActiveIdx = i;
                    break;
                }
            }
        }
    }

    public void parserDayActive(string data)
    {
        if (data != null)
        {
            cfDayActiveCon.Clear();
            string[] steptype = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < steptype.Length; i++)
            {
                if (steptype[i].Length > 3)
                {
                    DayActiveCondition day = new DayActiveCondition();
                    day.fromStringData(steptype[i]);
                    cfDayActiveCon.Add(day);
                    if (SDKManager.Instance != null && SDKManager.Instance.activeDay >= day.dayStart && SDKManager.Instance.activeDay <= day.dayEnd)
                    {
                        cfDayActiveIdx = cfDayActiveCon.Count - 1;
                    }
                }
            }
        }
    }

    public void parserDayActive(IDictionary<string, object> dicdayac)
    {
        if (dicdayac != null && dicdayac.Count > 0)
        {
            cfDayActiveCon.Clear();
            foreach (KeyValuePair<string, object> item in dicdayac)
            {
                string[] arrkey = item.Key.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrkey.Length > 0)
                {
                    for (int ii = 0; ii < arrkey.Length; ii++)
                    {
                        string[] arrday = arrkey[ii].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrday.Length == 2)
                        {
                            int ds = 0;
                            int de = 0;
                            if (int.TryParse(arrday[0], out ds) && int.TryParse(arrday[1], out de))
                            {
                                if (ds <= de && de > 0)
                                {
                                    IDictionary<string, object> dayitem = (IDictionary<string, object>)item.Value;
                                    DayActiveCondition dac = new DayActiveCondition();
                                    dac.dayStart = ds;
                                    dac.dayEnd = de;
                                    cfDayActiveCon.Add(dac);
                                    if (SDKManager.Instance != null && SDKManager.Instance.activeDay >= ds && SDKManager.Instance.activeDay <= de)
                                    {
                                        cfDayActiveIdx = cfDayActiveCon.Count - 1;
                                    }
                                    if (dayitem.ContainsKey("dtShow"))
                                    {
                                        dac.setDeltaTime(Convert.ToInt32(dayitem["dtShow"]));
                                    }
                                    if (dayitem.ContainsKey("dCall"))
                                    {
                                        dac.deltaCall = Convert.ToInt32(dayitem["dCall"]);
                                    }
                                    if (dayitem.ContainsKey("banner"))
                                    {
                                        dac.bnStep = new StepShow();
                                        dac.bnStep.parSerStep((string)dayitem["banner"]);
                                    }
                                    if (dayitem.ContainsKey("full"))
                                    {
                                        dac.fullStep = new StepShow();
                                        dac.fullStep.parSerStep((string)dayitem["full"]);
                                    }
                                    if (dayitem.ContainsKey("gift"))
                                    {
                                        dac.giftStep = new StepShow();
                                        dac.giftStep.parSerStep((string)dayitem["gift"]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public string dayActiveToString()
    {
        string re = "";
        bool isS = true;
        for (int i = 0; i < cfDayActiveCon.Count; i++)
        {
            if (isS)
            {
                isS = false;
                re = cfDayActiveCon[i].toString();
            }
            else
            {
                re += ";" + cfDayActiveCon[i].toString();
            }
        }

        return re;
    }
}
//-------------------------------------------------
public class StepShow
{
    public List<int> stepShowCircle = new List<int>();
    public List<int> stepShowRe = new List<int>();

    public void coppyFromOther(StepShow other)
    {
        if (other != null)
        {
            clearStep();
            stepShowCircle.AddRange(other.stepShowCircle);
            stepShowRe.AddRange(other.stepShowRe);
        }
    }

    public void clearStep()
    {
        stepShowCircle.Clear();
        stepShowRe.Clear();
    }

    public string toDataString()
    {
        string re = "";
        if (stepShowCircle.Count > 0)
        {
            re = "cir:";
            for (int i = 0; i < stepShowCircle.Count; i++)
            {
                re += $"{stepShowCircle[i]}";
                if (i != 0)
                {
                    re += ",";
                }
            }
        }
        if (stepShowRe.Count > 0)
        {
            if (re.Length > 2)
            {
                re += "#re:";
            }
            else
            {
                re = "re:";
            }
            for (int i = 0; i < stepShowRe.Count; i++)
            {
                re += $"{stepShowRe[i]}";
                if (i != 0)
                {
                    re += ",";
                }
            }
        }
        return re;
    }

    public void parSerStep(string step)
    {
        if (step == null || step.Length == 0)
        {
            return;
        }
        Debug.Log("mysdk: StepShow fromStringData data=" + step);
        string[] steptype = step.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
        for (int it = 0; it < steptype.Length; it++)
        {
            if (steptype[it].StartsWith("cir:"))
            {
                string reSrep = steptype[it].Substring(4);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                stepShowCircle.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        stepShowCircle.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else if (steptype[it].StartsWith("re:"))
            {
                string reSrep = steptype[it].Substring(3);
                string[] sl = reSrep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                stepShowRe.Clear();
                for (int i = 0; i < sl.Length; i++)
                {
                    try
                    {
                        int value = int.Parse(sl[i]);
                        stepShowRe.Add(value);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }
}
//-------------------------------------------------
public class StepLevelShowfull
{
    public int startlevel;
    public int endLevel;

    public StepShow fullStep = new StepShow();

    public StepLevelShowfull(string data)
    {
        fromStringData(data);
    }

    public void fromStringData(string data)
    {
        fullStep.clearStep();
        try
        {
            string[] sarr = data.Split(new char[] { ';' });
            if (sarr.Length >= 3)
            {
                startlevel = int.Parse(sarr[0]);
                endLevel = int.Parse(sarr[1]);
                fullStep.parSerStep(sarr[2]);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: StepLevelShowfull fromStringData ex=" + ex.ToString());
        }
    }

}

//-------------------------------------------------
public class IntervalLevelShowfull
{
    public int startlevel;
    public int endLevel;
    public int deltal4Show;

    public IntervalLevelShowfull(string data)
    {
        fromStringData(data);
    }

    public void fromStringData(string data)
    {
        deltal4Show = -1;
        try
        {
            string[] sarr = data.Split(new char[] { ',' });
            if (sarr.Length == 3)
            {
                startlevel = int.Parse(sarr[0]);
                endLevel = int.Parse(sarr[1]);
                deltal4Show = int.Parse(sarr[2]);
                if (startlevel >= endLevel)
                {
                    deltal4Show = -1;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: fromStringData ex=" + ex.ToString());
        }
    }
}
//------------------------------------------------
public class SpecialConditionShow
{
    public int startCon;//con session, timeplay
    public int endCon;//con session, timeplay

    public int deltal4Show = 0;
    public string stepFull = "";
    public string stepGift = "";

    public SpecialConditionShow(IDictionary<string, object> dicdata)
    {
        fromStringData(dicdata);
    }

    public void fromStringData(IDictionary<string, object> dicdata)
    {
        deltal4Show = 0;
        stepFull = "";
        stepGift = "";
        try
        {
            if (dicdata.ContainsKey("start"))
            {
                startCon = Convert.ToInt32(dicdata["start"]);
            }
            if (dicdata.ContainsKey("end"))
            {
                endCon = Convert.ToInt32(dicdata["end"]);
            }
            if (dicdata.ContainsKey("delta"))
            {
                deltal4Show = Convert.ToInt32(dicdata["delta"]);
            }
            if (dicdata.ContainsKey("full"))
            {
                stepFull = (string)dicdata["full"];
            }
            if (dicdata.ContainsKey("gift"))
            {
                stepGift = (string)dicdata["gift"];
            }
        }
        catch (Exception ex)
        {
            Debug.Log("mysdk: fromStringData ex=" + ex.ToString());
        }
    }

    public string toJsonOb()
    {
        string sjson = "{";
        sjson += $"\"start\":{startCon}";
        sjson += $",\"end\":{endCon}";
        sjson += $",\"delta\":{deltal4Show}";
        if (stepFull != null && stepFull.Length > 3)
        {
            sjson += $",\"full\":\"{stepFull}\"";
        }
        if (stepFull != null && stepFull.Length > 3)
        {
            sjson += $",\"gift\":\"{stepGift}\"";
        }
        sjson += "}";

        return sjson;
    }
}

public class DayActiveCondition
{
    public int dayStart = 0;
    public int dayEnd = 0;
    public int deltaTime { get; private set; }
    public int deltaCall = 0;

    public StepShow bnStep = null;
    public StepShow fullStep = null;
    public StepShow giftStep = null;

    public DayActiveCondition()
    {
        deltaTime = 0;
    }
    public void setDeltaTime(int dt)
    {
        deltaTime = dt * 1000;
    }

    public string toString()
    {
        string re = "";
        if (dayEnd > 0)
        {
            if (deltaTime < 1000)
            {
                deltaTime = 0;
            }
            re = $"{dayStart};{dayEnd};{deltaTime / 1000};{deltaCall}";
            if (bnStep != null && (bnStep.stepShowCircle.Count > 0 || bnStep.stepShowRe.Count > 0))
            {
                re += $"bn:{bnStep.toDataString()}";
            }
            if (fullStep != null && (fullStep.stepShowCircle.Count > 0 || fullStep.stepShowRe.Count > 0))
            {
                re += $"full:{fullStep.toDataString()}";
            }
            if (giftStep != null && (giftStep.stepShowCircle.Count > 0 || giftStep.stepShowRe.Count > 0))
            {
                re += $"gift:{giftStep.toDataString()}";
            }
        }
        return re;
    }

    public void fromStringData(string data)
    {
        if (data.Length > 3)
        {
            string[] steptype = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (steptype.Length >= 4)
            {
                int dt = 0;
                int.TryParse(steptype[0], out dayStart);
                int.TryParse(steptype[1], out dayEnd);
                int.TryParse(steptype[2], out dt);
                int.TryParse(steptype[3], out deltaCall);
                setDeltaTime(dt);
                if (steptype.Length > 4)
                {
                    for (int i = 4; i < steptype.Length; i++)
                    {
                        if (steptype[i].StartsWith("bn:"))
                        {
                            string datastep = steptype[i].Substring(3);
                            bnStep = new StepShow();
                            bnStep.parSerStep(datastep);
                        }
                        else if (steptype[i].StartsWith("full:"))
                        {
                            string datastep = steptype[i].Substring(5);
                            fullStep = new StepShow();
                            fullStep.parSerStep(datastep);
                        }
                        else if (steptype[i].StartsWith("gift:"))
                        {
                            string datastep = steptype[i].Substring(5);
                            giftStep = new StepShow();
                            giftStep.parSerStep(datastep);
                        }
                    }
                }
            }
        }
    }

    public void fromOther(DayActiveCondition other)
    {
        if (other != null)
        {
            this.dayStart = other.dayStart;
            this.dayEnd = other.dayEnd;
            this.deltaTime = other.deltaTime;
            this.deltaCall = other.deltaCall;
            if (other.bnStep != null)
            {
                bnStep = new StepShow();
                bnStep.coppyFromOther(other.bnStep);
            }
            if (other.fullStep != null)
            {
                fullStep = new StepShow();
                fullStep.coppyFromOther(other.fullStep);
            }
            if (other.giftStep != null)
            {
                giftStep = new StepShow();
                giftStep.coppyFromOther(other.giftStep);
            }
        }
    }
}