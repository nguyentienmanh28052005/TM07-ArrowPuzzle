using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool ntGiftIsClick = false;
        int countNtGiftpgImp = 0;
        AdPlacementFull adplNtGiftpgShow = null;
        private void initNativeGift()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads giftnt admobmy init adCfPlacementNtGift=" + advhelper.currConfig.adCfPlacementNtGift);
                if (advhelper.currConfig.adCfPlacementNtGift.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementNtGift.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacementNtGift(plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlNtGift.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacementNtGift(plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads giftnt admobmy initNativeGift ex=" + ex.ToString());
            }
        }
        private void addAdPlacementNtGift(string data, bool isReplaceIds)
        {
            string[] plcf = data.Split(new char[] { ',' });
            if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
            {
                string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ikey in arrkeys)
                {
                    if (ikey.Length > 2)
                    {
                        string[] arrids = plcf[2].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrids.Length > 0)
                        {
                            if (!dicPLNtGift.ContainsKey(ikey))
                            {
                                AdPlacementFull plAd = new();
                                dicPLNtGift.Add(ikey, plAd);
                                plAd.placement = ikey;
                                plAd.adECPM.idxHighPriority = int.Parse(plcf[1]);
                                plAd.adECPM.listFromDstring(arrids[0], '|');
                                //
                                plAd.adECPM2 = new AdECPMs();
                                plAd.count4LoadAll2 = 0;
                                plAd.adECPM2.idxHighPriority = int.Parse(plcf[1]);
                                if (arrids.Length > 1)
                                {
                                    plAd.adECPM2.listFromDstring(arrids[1], '|');
                                }
                                else
                                {
                                    plAd.adECPM2.coppyIdSFrom(plAd.adECPM);
                                }
                            }
                            else
                            {
                                AdPlacementFull plAd = dicPLNtGift[ikey];
                                plAd.adECPM.idxHighPriority = int.Parse(plcf[1]);
                                List<AdECPMItem> tmpl = new List<AdECPMItem>();
                                if (isReplaceIds && arrids[0].Length > 3)
                                {
                                    tmpl.AddRange(plAd.adECPM.list);
                                    plAd.adECPM.idxCurrEcpm = 0;
                                    plAd.adECPM.list.Clear();
                                }
                                plAd.adECPM.listFromDstring(arrids[0], '|');
                                if (tmpl.Count > 0)
                                {
                                    for (int ii = 0; ii < tmpl.Count; ii++)
                                    {
                                        for (int jj = 0; jj < plAd.adECPM.list.Count; jj++)
                                        {
                                            if (plAd.adECPM.list[jj].adsId.CompareTo(tmpl[ii].adsId) == 0)
                                            {
                                                plAd.adECPM.list[jj].coppyFrom(tmpl[ii]);
                                                tmpl.RemoveAt(ii);
                                                ii--;
                                                break;
                                            }
                                        }
                                    }
                                    plAd.adECPM.list.AddRange(tmpl);
                                    tmpl.Clear();
                                }
                                //
                                if (plAd.adECPM2 == null)
                                {
                                    plAd.adECPM2 = new AdECPMs();
                                }
                                plAd.count4LoadAll2 = 0;
                                plAd.adECPM2.idxHighPriority = int.Parse(plcf[1]);
                                if (isReplaceIds && arrids.Length > 1 && arrids[1].Length > 3)
                                {
                                    tmpl.AddRange(plAd.adECPM2.list);
                                    plAd.adECPM2.idxCurrEcpm = 0;
                                    plAd.adECPM2.list.Clear();
                                }
                                plAd.adECPM2.listFromDstring(arrids[1], '|');
                                if (tmpl.Count > 0)
                                {
                                    for (int ii = 0; ii < tmpl.Count; ii++)
                                    {
                                        for (int jj = 0; jj < plAd.adECPM2.list.Count; jj++)
                                        {
                                            if (plAd.adECPM2.list[jj].adsId.CompareTo(tmpl[ii].adsId) == 0)
                                            {
                                                plAd.adECPM2.list[jj].coppyFrom(tmpl[ii]);
                                                tmpl.RemoveAt(ii);
                                                ii--;
                                                break;
                                            }
                                        }
                                    }
                                    plAd.adECPM2.list.AddRange(tmpl);
                                    tmpl.Clear();
                                }
                            }
                        }
                    }
                }
            }
        }

        //gift nt
        public override int getNtGiftLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy getNtGiftLoaded not pl");
                return 0;
            }
            else
            {
                int countLoaded = 0;
                for (int i = 0; i < adpl.adECPM.list.Count; i++)
                {
                    if (adpl.adECPM.list[i].isLoaded)
                    {
                        countLoaded++;
                    }
                }
                for (int i = 0; i < adpl.adECPM2.list.Count; i++)
                {
                    if (adpl.adECPM2.list[i].isLoaded)
                    {
                        countLoaded++;
                    }
                }
                if (countLoaded >= 2)
                {
                    return 1;
                }
            }
#endif
            return 0;
        }
        protected override void tryLoadNtGift(AdPlacementFull adpl, int adPos)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 8) == 0)
            {
                string idLoad = "";
                AdECPMs adECPM;
                if (adPos == 1)
                {
                    adECPM = adpl.adECPM;
                }
                else
                {
                    adECPM = adpl.adECPM2;
                }
                if (adECPM.idxCurrEcpm >= adECPM.list.Count)
                {
                    adECPM.idxCurrEcpm = 0;
                }
                if (!adECPM.list[adECPM.idxCurrEcpm].isLoaded && !adECPM.list[adECPM.idxCurrEcpm].isLoading)
                {
                    idLoad = adECPM.list[adECPM.idxCurrEcpm].adsId;
                    if (idLoad != null && idLoad.Contains("ca-app-pub"))
                    {
                        SdkUtil.logd($"ads giftnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtGift adpos={adPos} id={idLoad} idxCurrEcpm={adpl.adECPM.idxCurrEcpm} plid={adpl.idPl}");
                        AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                        adECPM.list[adECPM.idxCurrEcpm].isLoading = true;
                        if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                        {
                            AdsAdmobMyBridge.Instance.loadNativeGift(adpl.placement, adPos, idLoad, adECPM.idxCurrEcpm);
                        }
                        else
                        {
                            if (timeDeltaLoad > 30)
                            {
                                timeDeltaLoad = 30;
                            }
                            AdsProcessCB.Instance().Enqueue(() =>
                            {
                                AdsAdmobMyBridge.Instance.loadNativeGift(adpl.placement, adPos, idLoad, adECPM.idxCurrEcpm);
                            }, timeDeltaLoad);
                        }
                    }
                    else
                    {
                        SdkUtil.logd($"ads giftnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtGift adpos={adPos} id not correct");
                        if (adECPM.list[adECPM.idxCurrEcpm].isLoaded)
                        {
                            adpl.countLoad--;
                            if (adpl.countLoad <= 0 && adpl.cbLoad != null)
                            {
                                adpl.isLoading = false;
                                var tmp = adpl.cbLoad;
                                adpl.cbLoad = null;
                                if (getNtGiftLoaded(adpl.placement) > 0)
                                {
                                    tmp(AD_State.AD_LOAD_OK);
                                }
                                else
                                {
                                    tmp(AD_State.AD_LOAD_FAIL);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (adECPM.list[adECPM.idxCurrEcpm].isLoaded)
                    {
                        adpl.countLoad--;
                        if (adpl.countLoad <= 0 && adpl.cbLoad != null)
                        {
                            adpl.isLoading = false;
                            var tmp = adpl.cbLoad;
                            adpl.cbLoad = null;
                            if (getNtGiftLoaded(adpl.placement) > 0)
                            {
                                tmp(AD_State.AD_LOAD_OK);
                            }
                            else
                            {
                                tmp(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    }
                }
            }
            else
            {
                if (adpl.count4LoadAll <= 0)
                {
                    for (int i = 0; i < adpl.adECPM.list.Count; i++)
                    {
                        if (!adpl.adECPM.list[i].isLoaded && !adpl.adECPM.list[i].isLoading)
                        {
                            adpl.count4LoadAll++;
                            string idLoad = adpl.adECPM.list[i].adsId;
                            SdkUtil.logd($"ads giftnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtGift adpos=1 id={idLoad}, idxload={i}");
                            AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                            adpl.adECPM.list[i].isLoading = true;
                            AdsAdmobMyBridge.Instance.loadNativeGift(adpl.placement, 1, idLoad, i);
                        }
                    }
                }
                if (adpl.count4LoadAll2 <= 0)
                {
                    for (int i = 0; i < adpl.adECPM2.list.Count; i++)
                    {
                        if (!adpl.adECPM2.list[i].isLoaded && !adpl.adECPM2.list[i].isLoading)
                        {
                            adpl.count4LoadAll2++;
                            string idLoad = adpl.adECPM2.list[i].adsId;
                            SdkUtil.logd($"ads giftnt {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtGift adpos=2 id={idLoad}, idxload={i}");
                            AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                            adpl.adECPM2.list[i].isLoading = true;
                            AdsAdmobMyBridge.Instance.loadNativeGift(adpl.placement, 2, idLoad, i);
                        }
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads giftnt {adpl.placement} admobmy tryLoadNtGift adpos={adPos} not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNtGift(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtGift(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy loadNtGift not placement");
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_FAIL);
                }
                return;
            }
            else
            {
                if (!adpl.getShowing())
                {
                    if ((flagLoadAll & 8) == 0)
                    {
                        if (!adpl.isloaded && !adpl.isLoading)
                        {
                            SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy loadNtGift type={adsType}");
                            adpl.cbLoad = cb;
                            adpl.isLoading = true;
                            giftnativeisnew = false;
                            adpl.countLoad = 2;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.adECPM2.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNtGift(adpl, 1);
                            tryLoadNtGift(adpl, 2);
                        }
                        else
                        {
                            SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy loadNtGift isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                    else
                    {
                        if (!adpl.isHighAdLoaded())
                        {
                            SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy loadNtGift all");
                            adpl.cbLoad = cb;
                            giftnativeisnew = false;
                            if (adpl.count4LoadAll <= 0)
                            {
                                adpl.count4LoadAll = 0;
                                adpl.adECPM.idxCurrEcpm = 0;
                            }
                            if (adpl.count4LoadAll2 <= 0)
                            {
                                adpl.count4LoadAll2 = 0;
                                adpl.adECPM2.idxCurrEcpm = 0;
                            }
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNtGift(adpl, 3);
                        }
                        else
                        {
                            SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy loadNtGift isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy loadNtGift showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showNtGift(string placement, float timeDelay, bool isHideBtClose, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtGift(placement);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                countNtGiftpgImp = 0;
                adplNtGiftpgShow = adpl;
                int ss = getNtGiftLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy showNtGift call show timeDelay={timeDelay}");
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    AdsHelper.onAdShowStart(placement, "native_full", "admob", "");
                    int timeAd = PlayerPrefs.GetInt("cf_time_ntgiftpg", 15);
                    bool iss = AdsAdmobMyBridge.Instance.showNativeGift(adpl.placement, !isHideBtClose, timeAd, (int)(timeDelay * 1000));
                    adpl.setShowing(iss);
                    return iss;
                }
                else
                {
                    SdkUtil.logd($"ads giftnt {placement}-{adpl.placement} admobmy showNtGift not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy showNtGift not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Gift Native AD EVENTS
        private void OnNativeGiftLoadedEvent(string placement, string adsId, string adnet)
        {
            if (adnet != null && adnet.EndsWith("@@@"))
            {
                adnet = adnet.Replace("@@@", "");
                advhelper.isQcThu = true;
                if (advhelper.islogttttttt == 0)
                {
                    advhelper.islogttttttt = 1;
                    PlayerPrefs.SetInt("mem_is_log_ttt", advhelper.islogttttttt);
                    FIRhelper.logEvent("ads_test");
                    string dv = $"atn{GameHelper.Instance.AdsIdentify}";
                    dv = dv.Replace("-", "");
                    FIRhelper.logEvent(dv);
                }
            }
            int adpos;
            if (placement.Contains("@1"))
            {
                adpos = 1;
                placement = placement.Replace("@1", "");
            }
            else
            {
                adpos = 2;
                placement = placement.Replace("@2", "");
            }
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                SdkUtil.logd($"ads giftnt {adpl.loadPl}-{placement} admobmy OnNativeGiftLoadedEvent adpos={adpos} plid={adpl.idPl}");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", adsource, true);
                giftnativeisnew = false;
                if ((flagLoadAll & 8) != 0)
                {
                    if (adpos == 1)
                    {
                        adpl.count4LoadAll--;
                    }
                    else
                    {
                        adpl.count4LoadAll2--;
                    }
                    adpl.setStatusLoad(adpos, adsId, true);
                    if (getNtGiftLoaded(placement) > 0)
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;
                            tmpcb(AD_State.AD_LOAD_OK);
                        }
                    }
                    else
                    {
                        if (adpl.count4LoadAll <= 0 && adpl.count4LoadAll2 <= 0)
                        {
                            if (adpl.cbLoad != null)
                            {
                                var tmpcb = adpl.cbLoad;
                                adpl.cbLoad = null;
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    }
                }
                else
                {
                    adpl.countLoad--;
                    adpl.setStatusLoad(adpos, adsId, true);
                    if (adpl.countLoad <= 0)
                    {
                        adpl.isLoading = false;
                        if (getNtGiftLoaded(placement) > 0)
                        {
                            adpl.isloaded = true;
                        }
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;
                            if (adpl.isloaded)
                            {
                                tmpcb(AD_State.AD_LOAD_OK);
                            }
                            else
                            {
                                tmpcb(AD_State.AD_LOAD_FAIL);
                            }
                        }
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy OnNativeGiftLoadedEvent adpos={adpos} not pl");
            }
        }
        private void OnNativeGiftFailedEvent(string placement, string adsId, string err)
        {
            int adpos;
            if (placement.Contains("@1"))
            {
                adpos = 1;
                placement = placement.Replace("@1", "");
            }
            else
            {
                adpos = 2;
                placement = placement.Replace("@2", "");
            }
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                SdkUtil.logd($"ads giftnt {adpl.loadPl}-{placement} admobmy onload adpos={adpos} fail=" + err);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", "", false);
                AdECPMs adECPM;
                if (adpos == 1)
                {
                    adECPM = adpl.adECPM;
                }
                else
                {
                    adECPM = adpl.adECPM2;
                }
                if ((flagLoadAll & 8) == 0)
                {
                    adpl.setStatusLoad(adpos, adsId, false);
                    if (adECPM.idxCurrEcpm < (adECPM.list.Count - 1))
                    {
                        if (giftnativeisnew)
                        {
                            giftnativeisnew = false;
                            adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryLoadNtGift(adpl, adpos);
                        }, 1);
                    }
                    else
                    {
                        adpl.countLoad--;
                        if (adpl.countLoad <= 0)
                        {
                            adpl.isLoading = false;
                            if (getNtGiftLoaded(placement) > 0)
                            {
                                adpl.isloaded = true;
                            }
                            if (adpl.cbLoad != null)
                            {
                                var tmp = adpl.cbLoad;
                                adpl.cbLoad = null;
                                if (adpl.isloaded)
                                {
                                    tmp(AD_State.AD_LOAD_OK);
                                }
                                else
                                {
                                    tmp(AD_State.AD_LOAD_FAIL);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (adpos == 1)
                    {
                        adpl.count4LoadAll--;
                    }
                    else
                    {
                        adpl.count4LoadAll2--;
                    }
                    adpl.setStatusLoad(adpos, adsId, false);

                    if (adpl.count4LoadAll <= 0 && adpl.count4LoadAll2 <= 0)
                    {
                        if (adpl.cbLoad != null)
                        {
                            var tmpcb = adpl.cbLoad;
                            adpl.cbLoad = null;
                            tmpcb(AD_State.AD_LOAD_FAIL);
                        }
                    }
                }
            }
            else
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy onload adpos={adpos} fail=" + err);
            }
        }
        private void OnNativeGiftDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                SdkUtil.logd($"ads giftnt {adpl.showPl}-{placement} admobmy onshow ids={adsId} net={adnet}");
                string[] arrids = adsId.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < arrids.Length; i++)
                {
                    string[] gids = arrids[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (gids.Length >= 2)
                    {
                        string adid = gids[0];
                        int pos = int.Parse(gids[1]);
                        adpl.setStatusLoad(pos, adid, false);
                    }
                }
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW); });
                }
            }
            else
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy onshow not pl ids={adsId} net={adnet}");
            }
        }
        private void OnNativeGiftImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads giftnt {placement} admobmy OnNativeGiftImpresstionEvent");
        }
        private void onNativeGiftFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                SdkUtil.logd($"ads giftnt {adpl.showPl}-{placement} admobmy onNativeGiftFailedToShow=" + err);
                adpl.isLoading = false;
                adpl.isloaded = false;
                spl = adpl.showPl;
                adpl.setShowing(false);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_FAIL); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_SHOW_FAIL); });
                }
            }
            else
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy not pl onNativeGiftFailedToShow=" + err);
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, false, err);
            onGiftClose(placement);
            advhelper.onCloseFullGift(false);
        }
        private void OnNativeGiftClickEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logEvent("show_ads_gift_nt_click");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                spl = adpl.showPl;
                adplNtGiftpgShow = adpl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            if (adplNtGiftpgShow != null)
            {
                int adpos = adplNtGiftpgShow.getPosdAd(adsId);
                if (countNtGiftpgImp == 1)
                {
                    FIRhelper.logEvent($"ntgiftpg_click_1_{adpos}");
                }
                else
                {
                    FIRhelper.logEvent($"ntgiftpg_click_2_{adpos}");
                }
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "native_full", "admob", adsource, adsId);
            if (!ntGiftIsClick)
            {
                ntGiftIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "admob", "");
            }
        }
        private void OnNativeGiftDismissedEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logEvent("show_ads_total_imp");
            FIRhelper.logEvent("show_ads_reward_imp");
            FIRhelper.logEvent("show_ads_reward_imp_0_nt");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                SdkUtil.logd($"ads giftnt {adpl.showPl}-{placement} admobmy OnNativeGiftDismissedEvent id={adsId}");
                adpl.isLoading = false;
                adpl.isloaded = false;
                spl = adpl.showPl;
                adpl.setShowing(false);
                if (adpl.cbShow != null)
                {
                    AdCallBack tmpcb = adpl.cbShow;
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_REWARD_OK); });
                    AdsProcessCB.Instance().Enqueue(() => { tmpcb(AD_State.AD_CLOSE); });
                }

                adpl.cbShow = null;
            }
            else
            {
                SdkUtil.logd($"ads giftnt {placement} admobmy OnNativeGiftDismissedEvent not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, true, "");
            onGiftClose(placement);
            advhelper.onCloseFullGift(false);
        }
        private void OnNativeGiftFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativeGiftAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            ntGiftIsClick = false;
            long originva = valueMicros;
            AdsHelper.Instance.setEcpmNtFull4Fb(originva / 1000);
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtGift.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtGift[placement];
                spl = adpl.showPl;
                adplNtGiftpgShow = adpl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            countNtGiftpgImp++;
            if (adplNtGiftpgShow != null)
            {
                int adpos = adplNtGiftpgShow.getPosdAd(adsId);
                if (countNtGiftpgImp == 1)
                {
                    FIRhelper.logEvent($"ntgiftpg_imp_1_{adpos}");
                }
                else
                {
                    FIRhelper.logEvent($"ntgiftpg_imp_2_{adpos}");
                }
            }
            string adformat = FIRhelper.getAdformatAdmob(9);
            string adsource = FIRhelper.getAdsourceAdmob(adNet);
            var dicpr = TiktokBusiness.getAdmobParam(currencyCode);
            FIRhelper.logEventAdsPaidAdmob(spl, adformat, adsource, adsId, valueMicros, originva, dicpr["currency_code"]);
            TiktokBusiness.logAdRevenueAdmob(adformat, adsource, adsId, precisionType, valueMicros / 1000, dicpr);
            float realValue = ((float)valueMicros) / 1000000000.0f;
            AdsHelper.onAdImpression(spl, adsId, adformat, "admob", adsource, realValue, originva);
        }
        #endregion
#endif
    }
}