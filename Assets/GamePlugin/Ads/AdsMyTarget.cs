//#define ENABLE_ADS_MYTARGET

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if ENABLE_ADS_MYTARGET

#endif

namespace mygame.sdk
{
    public class AdsMyTarget : AdsBase
    {
#if ENABLE_ADS_MYTARGET
#endif

        long timeShowBanner = 0;
        float _membnDxCenter;
        bool __isInit = false;

        string bnidLoad = "";
        string fullidLoad = "";
        string giftidLoad = "";

        bool adsIsClick = false;

        public override void InitAds()
        {
#if ENABLE_ADS_MYTARGET
            AdsMyTargetBridge.Instance.Initialize();
            isEnable = true;
#endif
        }

        public override void AdsAwake()
        {
#if ENABLE_ADS_MYTARGET
            isEnable = true;
#endif
        }

        private void Start()
        {
#if ENABLE_ADS_MYTARGET
            if (advhelper == null || advhelper.currConfig == null)
            {
                return;
            }
            initBanner();
            initFull();
            initGift();
            __isInit = true;
            fullAdNetwork = "mytarget";
            giftAdNetwork = "mytarget";

            AdsMyTargetBridge.onBNLoaded += OnBannerAdLoadedEvent;
            AdsMyTargetBridge.onBNLoadFail += OnBannerAdLoadFailedEvent;
            AdsMyTargetBridge.onBNPaid += OnBannerAdPaidEvent;
            AdsMyTargetBridge.onBNClick += OnBannerAdClickEvent;

            AdsMyTargetBridge.onInterstitialLoaded += OnInterstitialLoadedEvent;
            AdsMyTargetBridge.onInterstitialLoadFail += OnInterstitialFailedEvent;
            AdsMyTargetBridge.onInterstitialShowed += OnInterstitialDisplayedEvent;
            AdsMyTargetBridge.onInterstitialFailedToShow += onInterstitialFailedToShow;
            AdsMyTargetBridge.onInterstitialClick += onInterstitialClickEvent;
            AdsMyTargetBridge.onInterstitialDismissed += OnInterstitialDismissedEvent;
            AdsMyTargetBridge.onInterstitialPaid += OnInterstitialAdPaidEvent;

            AdsMyTargetBridge.onRewardLoaded += OnRewardedAdLoadedEvent;
            AdsMyTargetBridge.onRewardLoadFail += OnRewardedAdFailedEvent;
            AdsMyTargetBridge.onRewardFailedToShow += OnRewardedAdFailedToDisplayEvent;
            AdsMyTargetBridge.onRewardShowed += OnRewardedAdDisplayedEvent;
            AdsMyTargetBridge.onRewardClick += OnRewardedAdClickEvent;
            AdsMyTargetBridge.onRewardDismissed += OnRewardedAdDismissedEvent;
            AdsMyTargetBridge.onRewardReward += OnRewardedAdReceivedRewardEvent;
            AdsMyTargetBridge.onRewardPaid += OnRewardedAdPaidEvent;
#if TEST_AD_RU
            AdsProcessCB.Instance().Enqueue(() =>
            {
                AdsMyTargetBridge.Instance.addTestDevice("08898EE6AF9BF89D54AB6F4842B36BDD");
            }, 5);
#endif
#endif
        }

        private void OnDestroy()
        {
#if ENABLE_ADS_MYTARGET
            if (__isInit)
            {
                __isInit = false;
                AdsMyTargetBridge.onBNLoaded -= OnBannerAdLoadedEvent;
                AdsMyTargetBridge.onBNLoadFail -= OnBannerAdLoadFailedEvent;
                AdsMyTargetBridge.onBNPaid -= OnBannerAdPaidEvent;

                AdsMyTargetBridge.onInterstitialLoaded -= OnInterstitialLoadedEvent;
                AdsMyTargetBridge.onInterstitialLoadFail -= OnInterstitialFailedEvent;
                AdsMyTargetBridge.onInterstitialShowed -= OnInterstitialDisplayedEvent;
                AdsMyTargetBridge.onInterstitialFailedToShow -= onInterstitialFailedToShow;
                AdsMyTargetBridge.onInterstitialDismissed -= OnInterstitialDismissedEvent;
                AdsMyTargetBridge.onInterstitialPaid -= OnInterstitialAdPaidEvent;

                AdsMyTargetBridge.onRewardLoaded -= OnRewardedAdLoadedEvent;
                AdsMyTargetBridge.onRewardLoadFail -= OnRewardedAdFailedEvent;
                AdsMyTargetBridge.onRewardFailedToShow -= OnRewardedAdFailedToDisplayEvent;
                AdsMyTargetBridge.onRewardShowed -= OnRewardedAdDisplayedEvent;
                AdsMyTargetBridge.onRewardDismissed -= OnRewardedAdDismissedEvent;
                AdsMyTargetBridge.onRewardReward -= OnRewardedAdReceivedRewardEvent;
                AdsMyTargetBridge.onRewardPaid -= OnRewardedAdPaidEvent;
            }
#endif
        }

        public void initBanner()
        {
            try
            {
                SdkUtil.logd($"ads bn mytarget stepFloorECPMBanner=" + advhelper.currConfig.mytargetStepFloorECPMBanner);
                if (advhelper.currConfig.mytargetStepFloorECPMBanner.Length <= 2)
                {
                    advhelper.currConfig.mytargetStepFloorECPMBanner = AdIdsConfig.TargetBnFloor;
                }
                if (advhelper.currConfig.mytargetStepFloorECPMBanner.Length > 0)
                {
                    dicPLBanner.Clear();
                    AdPlacementBanner plbn = new AdPlacementBanner();
                    dicPLBanner.Add(PLBnDefault, plbn);
                    plbn.placement = PLBnDefault;
                    plbn.adECPM.idxHighPriority = -1;
                    plbn.adECPM.listFromDstring(advhelper.currConfig.mytargetStepFloorECPMBanner);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: bn mytarget initBanner ex=" + ex.ToString());
            }
        }

        public void initFull()
        {
            try
            {
                SdkUtil.logd($"ads full mytarget stepFloorECPMFull=" + advhelper.currConfig.mytargetStepFloorECPMFull);
                if (advhelper.currConfig.mytargetStepFloorECPMFull.Length <= 2)
                {
                    advhelper.currConfig.mytargetStepFloorECPMFull = AdIdsConfig.TargetFullFloor;
                }
                if (advhelper.currConfig.mytargetStepFloorECPMFull.Length > 0)
                {
                    dicPLFull.Clear();
                    AdPlacementFull plfull = new AdPlacementFull();
                    dicPLFull.Add(PLFullDefault, plfull);
                    plfull.placement = PLFullDefault;
                    plfull.adECPM.idxHighPriority = -1;
                    plfull.adECPM.listFromDstring(advhelper.currConfig.mytargetStepFloorECPMFull);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: mytarget initFull ex=" + ex.ToString());
            }
        }

        public void initGift()
        {
            try
            {
                SdkUtil.logd($"ads gift mytarget stepFloorECPMGift=" + advhelper.currConfig.mytargetStepFloorECPMGift);
                if (advhelper.currConfig.mytargetStepFloorECPMGift.Length <= 2)
                {
                    advhelper.currConfig.mytargetStepFloorECPMGift = AdIdsConfig.TargetGiftFloor;
                }
                if (advhelper.currConfig.mytargetStepFloorECPMGift.Length > 0)
                {
                    dicPLGift.Clear();
                    AdPlacementFull plgift = new AdPlacementFull();
                    dicPLGift.Add(PLGiftDefault, plgift);
                    plgift.placement = PLGiftDefault;
                    plgift.adECPM.idxHighPriority = -1;
                    plgift.adECPM.listFromDstring(advhelper.currConfig.mytargetStepFloorECPMGift);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads gift mytarget initGift ex=" + ex.ToString());
            }
        }

        public override string getname()
        {
            return "mytarget";
        }

        protected override void tryLoadBanner(AdPlacementBanner adpl)
        {
#if ENABLE_ADS_MYTARGET
            if (adpl != null)
            {
                if (adpl.adECPM.list.Count <= 0)
                {
                    if (adpl.cbLoad != null)
                    {
                        SdkUtil.logd($"ads bn mytarget tryLoadBanner not id");
                        var tmpcb = adpl.cbLoad;
                        adpl.cbLoad = null;
                        tmpcb(AD_State.AD_LOAD_FAIL);
                    }
                }
                else
                {
                    string idload = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
                    SdkUtil.logd($"ads bn mytarget tryLoadBanner = " + idload + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm);
                    AdsHelper.onAdLoad(adpl.loadPl, "banner", idload, "mytarget");
                    adpl.isLoading = true;
                    adpl.isloaded = false;
                    bnidLoad = idload;
                    if (AppConfig.isBannerIpad)
                    {
                        AdsMyTargetBridge.Instance.showBanner(idload, adpl.posBanner, bnWidth, (int)advhelper.bnOrien, SdkUtil.isiPad(), _membnDxCenter);
                    }
                    else
                    {
                        AdsMyTargetBridge.Instance.showBanner(idload, adpl.posBanner, bnWidth, (int)advhelper.bnOrien, false, _membnDxCenter);
                    }
                    SdkUtil.logd($"ads bn mytarget tryLoadBanner1 _membnDxCenter=" + _membnDxCenter);
                }
            }
            else
            {
                SdkUtil.logd($"ads bn mytarget tryLoadBanner not pl");
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads bn mytarget tryLoadBanner not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadBanner(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_MYTARGET
            SdkUtil.logd($"ads bn mytarget loadBanner");
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                SdkUtil.logd($"ads bn mytarget loadBanner");
                adpl.cbLoad = cb;
                if (!adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadBanner(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads bn mytarget loadBanner isProcessShow");
                }
            }
            else
            {
                if (cb != null)
                {
                    SdkUtil.logd($"ads bn mytarget loadBanner not pl");
                    cb(AD_State.AD_LOAD_FAIL);
                }
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bn mytarget loadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false)
        {
#if ENABLE_ADS_MYTARGET
            AdPlacementBanner adpl = getPlBanner(placement, 0);
            if (adpl != null)
            {
                if (!adpl.isLoading)
                {
                    adpl.adECPM.idxCurrEcpm = 0;
                }
                SdkUtil.logd($"ads bn mytarget showBanner pos=" + pos + ", dxCenter=" + dxCenter + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm + ", countecpm=" + adpl.adECPM.list.Count);
                adpl.isShow = true;
                adpl.posBanner = pos;
                adpl.setSetPlacementShow(placement);
                bnWidth = width;
                _membnDxCenter = dxCenter;
                if (!adpl.isLoading)
                {
                    int idxsh = -10;
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        AdECPMItem bnec = adpl.adECPM.list[i];
                        if (bnec.isLoaded)
                        {
                            adpl.isRealShow = true;
                            string idload = bnec.adsId;
                            SdkUtil.logd($"ads bn mytarget showBanner show pre loaded adsid=" + bnec.adsId + ", idx=" + i + ", dxCenter=" + dxCenter);
                            if (AppConfig.isBannerIpad)
                            {
                                AdsMyTargetBridge.Instance.showBanner(idload, adpl.posBanner, width, (int)advhelper.bnOrien, SdkUtil.isiPad(), dxCenter);
                            }
                            else
                            {
                                AdsMyTargetBridge.Instance.showBanner(idload, adpl.posBanner, width, (int)advhelper.bnOrien, false, dxCenter);
                            }
                            if (cb != null)
                            {
                                cb(AD_State.AD_SHOW);
                            }
                            idxsh = i;
                            break;
                        }
                    }

                    if (idxsh != -10)
                    {
                        if (cb != null)
                        {
                            cb(AD_State.AD_SHOW);
                        }
                        return true;
                    }
                    else
                    {
                        loadBanner(placement, cb);
                        return false;
                    }
                }
                else
                {
                    SdkUtil.logd($"ads bn mytarget showBanner isprocess show dxCenter=" + dxCenter);
                    AdsMyTargetBridge.Instance.setBannerPos(pos, width, dxCenter);
                    return false;
                }
            }
            else
            {
                SdkUtil.logd($"ads bn mytarget showBanner not pl");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return false;
            }
#else
            if (cb != null)
            {
                SdkUtil.logd($"ads bn mytarget tryLoadBanner not enable");
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
#endif
        }
        public override void hideBanner()
        {
#if ENABLE_ADS_MYTARGET
            SdkUtil.logd($"ads bn mytarget hideBanner");
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
            }
            AdsMyTargetBridge.Instance.hideBanner();
#endif
        }
        public override void destroyBanner()
        {
#if ENABLE_ADS_MYTARGET
            foreach (var adi in dicPLBanner)
            {
                adi.Value.isShow = false;
                adi.Value.isRealShow = false;
                adi.Value.isloaded = false;
            }
            AdsMyTargetBridge.Instance.hideBanner();
#endif
        }

        //Native

        //
        public override void clearCurrFull(string placement)
        {
#if ENABLE_ADS_MYTARGET
            if (getFullLoaded(placement) == 1)
            {
                AdsMyTargetBridge.Instance.clearCurrFull();
                AdPlacementFull adpl = getPlFull(placement, true);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
            }
#endif
        }
        public override int getFullLoaded(string placement)
        {
#if ENABLE_ADS_MYTARGET
            SdkUtil.logd($"ads full mytarget getFullLoaded");
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} mytarget getFullLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded)
                {

                    return 1;
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadFull(AdPlacementFull adpl)
        {
#if ENABLE_ADS_MYTARGET
            string idLoad = "";
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            SdkUtil.logd($"ads full mytarget tryLoadFull=" + idLoad + ", idxCurrEcpmFull=" + adpl.adECPM.idxCurrEcpm);
            int tryload = adpl.countLoad;
            fullidLoad = idLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads full mytarget tryLoadFull over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }

            SdkUtil.logd($"ads full mytarget tryLoadFull load idLoad=" + idLoad);
            if (idLoad != null)
            {
                AdsHelper.onAdLoad(adpl.loadPl, "interstitial", idLoad, "mytarget");
                FIRhelper.logAdEvent("ads_full_target_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "interstitial", idLoad, "mytarget", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsMyTargetBridge.Instance.loadFull(idLoad);
            }
            else
            {

                SdkUtil.logd($"ads full mytarget tryLoadFull id not correct");
                adpl.isloaded = false;
                adpl.isLoading = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads full mytarget tryLoadFull not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_MYTARGET
            AdPlacementFull adpl = getPlFull(placement, false);
            if (adpl == null)
            {
                SdkUtil.logd($"ads full {placement} mytarget loadFull not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                SdkUtil.logd($"ads full {placement} mytarget loadFull type=" + adsType);
                if (!adpl.isloaded && !adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryLoadFull(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads full {placement} mytarget loadFull isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
#else
            if (cb != null)
            {
               cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb)
        {
            isFull2 = isShow2;
#if ENABLE_ADS_MYTARGET
            SdkUtil.logd($"ads full {placement} mytarget showFull type={adsType} timeDelay={timeDelay}");
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getFullLoaded(adpl.placement);
                if (ss > 0)
                {
                    SdkUtil.logd($"ads full {placement} mytarget showFull type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() => {
                            AdsHelper.onAdShowStart(placement, "interstitial", "mytarget", "");
                            AdsMyTargetBridge.Instance.showFull();
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "interstitial", "mytarget", "");
                        AdsMyTargetBridge.Instance.showFull();
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        //
        public override void clearCurrGift(string placement)
        {
            if (getGiftLoaded(placement) == 1)
            {
#if ENABLE_ADS_MYTARGET
                AdsMyTargetBridge.Instance.clearCurrGift();
                AdPlacementFull adpl = getPlGift(placement);
                if (adpl != null)
                {
                    adpl.isloaded = false;
                }
#endif
            }
        }
        public override int getGiftLoaded(string placement)
        {
#if ENABLE_ADS_MYTARGET
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} mytarget getGiftLoaded not pl");
                return 0;
            }
            else
            {
                if (adpl.isloaded)
                {

                    return 1;
                }
            }
#endif
            return 0;
        }
        protected override void tryloadGift(AdPlacementFull adpl)
        {
#if ENABLE_ADS_MYTARGET
            string idLoad = "";
            if (adpl.adECPM.idxCurrEcpm >= adpl.adECPM.list.Count)
            {
                adpl.adECPM.idxCurrEcpm = 0;
            }
            idLoad = adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].adsId;
            SdkUtil.logd($"ads gift mytarget tryloadGift=" + idLoad + ", idxCurrEcpm=" + adpl.adECPM.idxCurrEcpm);
            int tryload = adpl.countLoad;
            giftidLoad = idLoad;
            if (tryload >= toTryLoad)
            {
                SdkUtil.logd($"ads gift mytarget tryloadGift over try");
                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }

            SdkUtil.logd($"ads gift mytarget tryloadGift load idLoad=" + idLoad);
            if (idLoad != null)
            {
                AdsHelper.onAdLoad(adpl.loadPl, "rewarded", idLoad, "mytarget");
                FIRhelper.logAdEvent("ads_gift_target_load");
                AppsFlyerHelperScript.logAdEvent("ads_load", "", "rewarded", idLoad, "mytarget", "");
                adpl.isLoading = true;
                adpl.isloaded = false;
                AdsMyTargetBridge.Instance.loadGift(idLoad);
            }
            else
            {

                SdkUtil.logd($"ads gift mytarget tryloadGift id not correct");
                adpl.isloaded = false;
                adpl.isLoading = false;
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads gift mytarget tryloadGift not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_MYTARGET
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads gift {placement} mytarget loadGift not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                SdkUtil.logd($"ads gift {placement} mytarget loadGift type=" + adsType);
                if (!adpl.isloaded && !adpl.isLoading)
                {
                    adpl.countLoad = 0;
                    adpl.adECPM.idxCurrEcpm = 0;
                    adpl.cbLoad = cb;
                    adpl.setSetPlacementLoad(placement);
                    tryloadGift(adpl);
                }
                else
                {
                    SdkUtil.logd($"ads gift mytarget loadGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                }
            }
#else
            if (cb != null)
            {
               cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showGift(string placement, float timeDelay, AdCallBack cb)
        {
#if ENABLE_ADS_MYTARGET
            SdkUtil.logd($"ads gift mytarget showGift type={adsType} timeDelay={timeDelay}");
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl != null)
            {
                adpl.cbShow = null;
                int ss = getGiftLoaded(adpl.placement);
                if (ss > 0)
                {
                    SdkUtil.logd($"ads gift mytarget showGift type=" + adsType);
                    adpl.countLoad = 0;
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    if (timeDelay > 0)
                    {
                        AdsProcessCB.Instance().Enqueue(() => {
                            AdsHelper.onAdShowStart(placement, "rewarded", "mytarget", "");
                            AdsMyTargetBridge.Instance.showGift();
                        }, timeDelay);
                        return true;
                    }
                    else
                    {
                        AdsHelper.onAdShowStart(placement, "rewarded", "mytarget", "");
                        AdsMyTargetBridge.Instance.showGift();
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        //-------------------------------------------------------------------------------
#if ENABLE_ADS_MYTARGET

        #region BANNER AD EVENTS

        public void OnBannerAdLoadedEvent()
        {
            SdkUtil.logd($"ads bn mytarget OnBannerAdLoadedEvent");
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", bnidLoad, "mytarget", "mytarget", true);
                if (adpl.isLoading)
                {
                    adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded = true;
                }
                adpl.isloaded = true;
                adpl.isLoading = false;
                adpl.countLoad = 0;

                if (adpl.isShow)
                {
                    if (advhelper.bnCurrShow == adsType)
                    {
                        SdkUtil.logd($"ads bn mytarget OnBannerAdLoadedEvent hide other");
                        advhelper.hideOtherBanner(adsType);
                    }
                }

                if (adpl.cbLoad != null)
                {
                    var tmpcb = adpl.cbLoad;
                    adpl.cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            if (advhelper != null)
            {
                advhelper.onBannerLoadOk(adsType);
            }
        }

        private void OnBannerAdLoadFailedEvent(string err)
        {
            SdkUtil.logd($"ads bn mytarget OnBannerAdLoadFailedEvent=" + err);
            if (dicPLBanner.ContainsKey(PLBnDefault))
            {
                AdPlacementBanner adpl = dicPLBanner[PLBnDefault];
                AdsHelper.onAdLoadResult(adpl.loadPl, "banner", bnidLoad, "mytarget", "", false);
                if (adpl.isLoading)
                {
                    if (adpl.adECPM.idxCurrEcpm < (adpl.adECPM.list.Count - 1))
                    {
                        adpl.adECPM.idxCurrEcpm++;
                        if (!adpl.adECPM.list[adpl.adECPM.idxCurrEcpm].isLoaded)
                        {
                            tryLoadBanner(adpl);
                        }
                        else
                        {
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.isLoading = false;
                            AdsProcessCB.Instance().Enqueue(() =>
                            {
                                if (adpl.cbLoad != null)
                                {
                                    var tmpcb = adpl.cbLoad;
                                    adpl.cbLoad = null;
                                    tmpcb(AD_State.AD_LOAD_OK);
                                }
                                if (advhelper != null)
                                {
                                    advhelper.onBannerLoadOk(adsType);
                                }
                            });
                        }
                    }
                    else
                    {
                        adpl.adECPM.idxCurrEcpm = 0;
                        adpl.isLoading = false;
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            if (adpl.cbLoad != null)
                            {
                                var tmpcb = adpl.cbLoad;
                                adpl.cbLoad = null;
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                            if (advhelper != null)
                            {
                                advhelper.onBannerLoadFail(adsType);
                            }
                        });
                    }
                }
            }
        }

        private void OnBannerAdPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            //
        }
        private void OnBannerAdClickEvent()
        {
            AdsHelper.onAdClick(SDKManager.Instance.currPlacement, "banner", "mytarget", "mytarget", bnidLoad);
        }
        #endregion

        #region INTERSTITIAL AD EVENTS
        private void OnInterstitialLoadedEvent()
        {
            FIRhelper.logAdEvent("ads_full_target_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", fullidLoad, "mytarget", "1");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                SdkUtil.logd($"ads full mytarget HandleInterstitialLoaded idxCurrEcpmFull=" + dicPLFull[PLFullDefault].adECPM.idxCurrEcpm);
                AdsHelper.onAdLoadResult(dicPLFull[PLFullDefault].loadPl, "interstitial", fullidLoad, "mytarget", "mytarget", true);
                dicPLFull[PLFullDefault].countLoad = 0;
                dicPLFull[PLFullDefault].isLoading = false;
                dicPLFull[PLFullDefault].isloaded = true;
                if (dicPLFull[PLFullDefault].cbLoad != null)
                {
                    var tmpcb = dicPLFull[PLFullDefault].cbLoad;
                    dicPLFull[PLFullDefault].cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            SdkUtil.logd($"ads full mytarget HandleInterstitialLoaded 1");
        }
        private void OnInterstitialFailedEvent(string err)
        {
            SdkUtil.logd($"ads full mytarget HandleInterstitialFailedToLoad=" + err);
            FIRhelper.logAdEvent("ads_full_target_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "interstitial", fullidLoad, "mytarget", "0");
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdsHelper.onAdLoadResult(dicPLFull[PLFullDefault].loadPl, "interstitial", fullidLoad, "mytarget", "", false);
                dicPLFull[PLFullDefault].isLoading = false;
                dicPLFull[PLFullDefault].isloaded = false;
                if (dicPLFull[PLFullDefault].adECPM.idxCurrEcpm < (dicPLFull[PLFullDefault].adECPM.list.Count - 1))
                {
                    dicPLFull[PLFullDefault].adECPM.idxCurrEcpm++;
                    tryLoadFull(dicPLFull[PLFullDefault]);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        dicPLFull[PLFullDefault].countLoad++;
                        tryLoadFull(dicPLFull[PLFullDefault]);
                    }, 1.0f);
                }
                SdkUtil.logd($"ads full mytarget HandleInterstitialFailedToLoad 1");
            }
        }
        private void OnInterstitialDisplayedEvent()
        {
            SdkUtil.logd($"ads full mytarget HandleInterstitialOpened");
            adsIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                spl = dicPLFull[PLFullDefault].showPl;
                if (dicPLFull[PLFullDefault].cbShow != null)
                {
                    AdCallBack tmpcb = dicPLFull[PLFullDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            FIRhelper.logAdEvent("ads_full_target_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "interstitial", fullidLoad, "mytarget", "");
        }
        private void onInterstitialFailedToShow(string err)
        {
            SdkUtil.logd($"ads full mytarget onInterstitialFailedToShow=" + err);
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                dicPLFull[PLFullDefault].isloaded = false;
                dicPLFull[PLFullDefault].isLoading = false;
                spl = dicPLFull[PLFullDefault].showPl;
                if (dicPLFull[PLFullDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(true);
                    AdCallBack tmpcb = dicPLFull[PLFullDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            AdsHelper.onAdShowEnd(spl, "interstitial", "mytarget", "mytarget", fullidLoad, false, err);
            onFullClose(PLFullDefault);
        }
        private void onInterstitialClickEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                AdPlacementFull adpl = dicPLFull[PLFullDefault];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = PLFullDefault;
            }
            AdsHelper.onAdClick(spl, "interstitial", "mytarget", "mytarget", fullidLoad);
            if (!adsIsClick)
            {
                adsIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "interstitial", fullidLoad, "mytarget", "");
            }
        }
        private void OnInterstitialDismissedEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLFull.ContainsKey(PLFullDefault))
            {
                advhelper.onCloseFullGift(true);
                dicPLFull[PLFullDefault].isloaded = false;
                dicPLFull[PLFullDefault].isLoading = false;
                spl = dicPLFull[PLFullDefault].showPl;
                if (dicPLFull[PLFullDefault].cbShow != null)
                {
                    AdCallBack tmpcb = dicPLFull[PLFullDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }
                dicPLFull[PLFullDefault].cbShow = null;
                dicPLFull[PLFullDefault].countLoad = 0;
            }
            AdsHelper.onAdShowEnd(spl, "interstitial", "mytarget", "mytarget", fullidLoad, true, "");
            onFullClose(PLFullDefault);
            SdkUtil.logd($"ads full mytarget HandleInterstitialClosed 1");
        }
        private void OnInterstitialAdPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            //
        }
        #endregion

        #region REWARDED VIDEO AD EVENTS
        private void OnRewardedAdLoadedEvent()
        {
            FIRhelper.logAdEvent("ads_gift_target_load_1");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", giftidLoad, "mytarget", "1");
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                SdkUtil.logd($"ads gift mytarget OnRewardedAdLoadedEvent");
                AdsHelper.onAdLoadResult(dicPLGift[PLGiftDefault].loadPl, "rewarded", giftidLoad, "mytarget", "mytarget", true);
                dicPLGift[PLGiftDefault].countLoad = 0;
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = true;
                if (dicPLGift[PLGiftDefault].cbLoad != null)
                {
                    var tmpcb = dicPLGift[PLGiftDefault].cbLoad;
                    dicPLGift[PLGiftDefault].cbLoad = null;
                    tmpcb(AD_State.AD_LOAD_OK);
                }
            }
            SdkUtil.logd($"ads gift mytarget OnRewardedAdLoadedEvent 1");
        }
        private void OnRewardedAdFailedEvent(string err)
        {
            SdkUtil.logd($"ads gift mytarget OnRewardedAdFailedEvent=" + err);
            FIRhelper.logAdEvent("ads_gift_target_load_0");
            AppsFlyerHelperScript.logAdEvent("ads_load_result", "", "rewarded", giftidLoad, "mytarget", "0");
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdsHelper.onAdLoadResult(dicPLGift[PLGiftDefault].loadPl, "rewarded", giftidLoad, "mytarget", "", false);
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = false;
                if (dicPLGift[PLGiftDefault].adECPM.idxCurrEcpm < (dicPLGift[PLGiftDefault].adECPM.list.Count - 1))
                {
                    SdkUtil.logd($"ads gift mytarget OnRewardedAdFailedEvent load other ecpm idxCurrEcpmGift=" + dicPLGift[PLGiftDefault].adECPM.idxCurrEcpm + ", count=" + dicPLGift[PLGiftDefault].adECPM.list.Count);
                    dicPLGift[PLGiftDefault].adECPM.idxCurrEcpm++;
                    tryloadGift(dicPLGift[PLGiftDefault]);
                }
                else
                {
                    AdsProcessCB.Instance().Enqueue(() =>
                    {
                        dicPLGift[PLGiftDefault].countLoad++;
                        tryloadGift(dicPLGift[PLGiftDefault]);
                    }, 1.0f);
                }
                SdkUtil.logd($"ads gift mytarget OnRewardedAdFailedEvent 1");
            }
        }
        private void OnRewardedAdDisplayedEvent()
        {
            adsIsClick = false;
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                SdkUtil.logd($"ads gift mytarget OnRewardedAdDisplayedEvent");
                spl = dicPLGift[PLGiftDefault].showPl;
                dicPLGift[PLGiftDefault].countLoad = 0;
                if (dicPLGift[PLGiftDefault].cbShow != null)
                {
                    SdkUtil.logd($"ads gift mytarget OnRewardedAdDisplayedEvent1");
                    var tmpcb = dicPLGift[PLGiftDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            FIRhelper.logAdEvent("ads_gift_target_imp");
            AppsFlyerHelperScript.logAdEvent("ads_impression", spl, "rewarded", giftidLoad, "mytarget", "");
            SdkUtil.logd($"ads gift mytarget OnRewardedAdDisplayedEvent2");
        }
        private void OnRewardedAdFailedToDisplayEvent(string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                SdkUtil.logd($"ads gift mytarget OnRewardedAdFailedToDisplayEvent=" + err);
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = false;
                spl = dicPLGift[PLGiftDefault].showPl;
                if (dicPLGift[PLGiftDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(false);
                    SdkUtil.logd($"ads gift mytarget _cbAD fail");
                    AdCallBack tmpcb = dicPLGift[PLGiftDefault].cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            AdsHelper.onAdShowEnd(spl, "rewarded", "mytarget", "mytarget", giftidLoad, false, err);
            onGiftClose(PLGiftDefault);
        }
        private void OnRewardedAdClickEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                AdPlacementFull adpl = dicPLGift[PLGiftDefault];
                spl = adpl.showPl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = PLGiftDefault;
            }
            AdsHelper.onAdClick(spl, "rewarded", "mytarget", "mytarget", giftidLoad);
            if (!adsIsClick)
            {
                adsIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "rewarded", giftidLoad, "mytarget", "");
            }
        }
        private void OnRewardedAdReceivedRewardEvent()
        {
            SdkUtil.logd($"ads gift mytarget OnRewardedAdReceivedRewardEvent");
            isRewardCom = true;
        }
        private void OnRewardedAdDismissedEvent()
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                SdkUtil.logd($"ads gift mytarget OnRewardedAdDismissedEvent");
                dicPLGift[PLGiftDefault].isLoading = false;
                dicPLGift[PLGiftDefault].isloaded = false;
                spl = dicPLGift[PLGiftDefault].showPl;
                if (dicPLGift[PLGiftDefault].cbShow != null)
                {
                    advhelper.onCloseFullGift(false);
                    SdkUtil.logd($"ads gift mytarget _cbAD != null");
                    AdCallBack tmpcb = dicPLGift[PLGiftDefault].cbShow;
                    if (isRewardCom)
                    {
                        SdkUtil.logd($"ads gift mytarget _cbAD reward");
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                    }
                    else
                    {
                        SdkUtil.logd($"ads gift mytarget _cbAD fail");
                        AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    }
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });

                    dicPLGift[PLGiftDefault].countLoad = 0;
                    dicPLGift[PLGiftDefault].cbShow = null;
                }
            }
            AdsHelper.onAdShowEnd(spl, "rewarded", "mytarget", "mytarget", giftidLoad, true, "");
            onGiftClose("gift_default");
            isRewardCom = false;
            SdkUtil.logd($"ads gift mytarget OnRewardedAdDismissedEvent1");
        }
        private void OnRewardedAdPaidEvent(int precisionType, string currencyCode, long valueMicros)
        {
            //
        }
        #endregion

#endif

    }
}