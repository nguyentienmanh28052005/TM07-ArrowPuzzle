using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mygame.sdk
{
    partial class AdsAdmobMy
    {
        bool ntpgfullIsClick = false;
        int countNtFullpgImp = 0;
        AdPlacementFull adplNtFullpgShow = null;
        private void initNativePgFull()
        {
            if (adsType != 0)
            {
                return;
            }
            try
            {
                Debug.Log("mysdk: ads fullntpg admobmy init adCfPlacementFullNtPg=" + advhelper.currConfig.adCfPlacementNtPgFull);
                if (advhelper.currConfig.adCfPlacementNtPgFull.Length > 0)
                {
                    string[] listpl = advhelper.currConfig.adCfPlacementNtPgFull.Split(new char[] { '#' });
                    foreach (string plitem in listpl)
                    {
                        addAdPlacementFullNtPg(plitem, true);
                    }
                }
                string[] listpldf = AdIdsConfig.AdmobPlNtPgFull.Split(new char[] { '#' });
                foreach (string plitem in listpldf)
                {
                    addAdPlacementFullNtPg(plitem, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: ads fullntpg admobmy initNativePgFull ex=" + ex.ToString());
            }
        }
        private void addAdPlacementFullNtPg(string data, bool isReplaceIds)
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
                            if (!dicPLNtPgFull.ContainsKey(ikey))
                            {
                                AdPlacementFull plAd = new();
                                dicPLNtPgFull.Add(ikey, plAd);
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
                                AdPlacementFull plAd = dicPLNtPgFull[ikey];
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

        //fullntpg
        public override int getNtPgFullLoaded(string placement)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtPgFull(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullntpg {placement} admobmy getNtPgFullLoaded not pl");
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
        protected override void tryLoadNtPgFull(AdPlacementFull adpl, int adPos)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            if ((flagLoadAll & 32) == 0)
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
                        SdkUtil.logd($"ads fullntpg {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtPgFull adpos={adPos} id={idLoad} idxCurrEcpm={adpl.adECPM.idxCurrEcpm} plid={adpl.idPl}");
                        AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                        adECPM.list[adECPM.idxCurrEcpm].isLoading = true;
                        if (timeDeltaLoad <= 0 || adpl.adECPM.idxCurrEcpm == 0)
                        {
                            AdsAdmobMyBridge.Instance.loadNativePgFull(adpl.placement, adPos, idLoad, adECPM.idxCurrEcpm);
                        }
                        else
                        {
                            if (timeDeltaLoad > 30)
                            {
                                timeDeltaLoad = 30;
                            }
                            AdsProcessCB.Instance().Enqueue(() =>
                            {
                                AdsAdmobMyBridge.Instance.loadNativePgFull(adpl.placement, adPos, idLoad, adECPM.idxCurrEcpm);
                            }, timeDeltaLoad);
                        }
                    }
                    else
                    {
                        SdkUtil.logd($"ads fullntpg {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtPgFull adpos={adPos} id not correct");
                        if (adECPM.list[adECPM.idxCurrEcpm].isLoaded)
                        {
                            adpl.countLoad--;
                            if (adpl.countLoad <= 0 && adpl.cbLoad != null)
                            {
                                adpl.isLoading = false;
                                var tmp = adpl.cbLoad;
                                adpl.cbLoad = null;
                                if (getNtPgFullLoaded(adpl.placement) > 0)
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
                            if (getNtPgFullLoaded(adpl.placement) > 0)
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
                            SdkUtil.logd($"ads fullntpg {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtPgFull adpos=1 id={idLoad}, idxload={i}");
                            AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                            adpl.adECPM.list[i].isLoading = true;
                            AdsAdmobMyBridge.Instance.loadNativePgFull(adpl.placement, 1, idLoad, i);
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
                            SdkUtil.logd($"ads fullntpg {adpl.loadPl}-{adpl.placement} admobmy tryLoadNtPgFull adpos=2 id={idLoad}, idxload={i}");
                            AdsHelper.onAdLoad(adpl.loadPl, "native_full", idLoad, "admob");
                            adpl.adECPM2.list[i].isLoading = true;
                            AdsAdmobMyBridge.Instance.loadNativePgFull(adpl.placement, 2, idLoad, i);
                        }
                    }
                }
            }
#else
            if (adpl != null && adpl.cbLoad != null)
            {
                SdkUtil.logd($"ads fullntpg {adpl.placement} admobmy tryLoadNtPgFull adpos={adPos} not enable");
                var tmpcb = adpl.cbLoad;
                adpl.cbLoad = null;
                tmpcb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override void loadNtPgFull(string placement, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtPgFull(placement);
            if (adpl == null)
            {
                SdkUtil.logd($"ads fullntpg {placement} admobmy loadNtPgFull not placement");
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
                    if ((flagLoadAll & 32) == 0)
                    {
                        if (!adpl.isloaded && !adpl.isLoading)
                        {
                            SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy loadFullNtPg type={adsType}");
                            adpl.cbLoad = cb;
                            adpl.isLoading = true;
                            ntpgfullisnew = false;
                            adpl.countLoad = 2;
                            adpl.adECPM.idxCurrEcpm = 0;
                            adpl.adECPM2.idxCurrEcpm = 0;
                            adpl.setSetPlacementLoad(placement);
                            tryLoadNtPgFull(adpl, 1);
                            tryLoadNtPgFull(adpl, 2);
                        }
                        else
                        {
                            SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy loadFullNtPg isloading={adpl.isLoading} or isloaded={adpl.isloaded}");
                        }
                    }
                    else
                    {
                        if (!adpl.isHighAdLoaded())
                        {
                            SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy loadFullNtPg all");
                            adpl.cbLoad = cb;
                            ntpgfullisnew = false;
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
                            tryLoadNtPgFull(adpl, 3);
                        }
                        else
                        {
                            SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy loadFullNtPg isHighAdLoaded={adpl.isHighAdLoaded()} or count4LoadAll={adpl.count4LoadAll}");
                        }
                    }
                }
                else
                {
                    SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy loadFullNtPg showing={adpl.getShowing()}");
                }
            }
#else
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
#endif
        }
        public override bool showNtPgFull(string placement, float timeDelay, bool isHideBtClose, AdCallBack cb)
        {
#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
            AdPlacementFull adpl = getPlNtPgFull(placement);
            if (adpl != null)
            {
                //adpl.cbShow = null;
                countNtFullpgImp = 0;
                adplNtFullpgShow = adpl;
                int ss = getNtPgFullLoaded(adpl.placement);
                if (ss > 0 && !adpl.getShowing())
                {
                    SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy showNtPgFull call show timeDelay={timeDelay}");
                    adpl.cbShow = cb;
                    adpl.setSetPlacementShow(placement);
                    AdsHelper.onAdShowStart(placement, "native_full", "admob", "");
                    int timeAd = PlayerPrefs.GetInt("cf_time_ntfullpg", 10);
                    bool iss = AdsAdmobMyBridge.Instance.showNativePgFull(adpl.placement, !isHideBtClose, timeAd, (int)(timeDelay * 1000));
                    adpl.setShowing(iss);
                    return iss;
                }
                else
                {
                    SdkUtil.logd($"ads fullntpg {placement}-{adpl.placement} admobmy showNtPgFull not load or showing={adpl.getShowing()}");
                }
            }
            else
            {
                SdkUtil.logd($"ads fullntpg {placement} admobmy showNtPgFull not pl");
            }
#endif
            return false;
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY
        #region Full Native PG AD EVENTS
        private void OnNativePgFullLoadedEvent(string placement, string adsId, string adnet)
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
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                SdkUtil.logd($"ads fullntpg {adpl.loadPl}-{placement} admobmy OnNativePgFullLoadedEvent adpos={adpos} plid={adpl.idPl}");
                string adsource = FIRhelper.getAdsourceAdmob(adnet);
                AdsHelper.onAdLoadResult(adpl.loadPl, "native_full", adsId, "admob", adsource, true);
                ntpgfullisnew = false;
                if ((flagLoadAll & 32) != 0)
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
                    if (getNtPgFullLoaded(placement) > 0)
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
                        if (getNtPgFullLoaded(placement) > 0)
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
                SdkUtil.logd($"ads fullntpg {placement} admobmy OnNativePgFullLoadedEvent adpos={adpos} not pl");
            }
        }
        private void OnNativePgFullFailedEvent(string placement, string adsId, string err)
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
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                SdkUtil.logd($"ads fullntpg {adpl.loadPl}-{placement} admobmy onload adpos={adpos} fail=" + err);
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
                if ((flagLoadAll & 32) == 0)
                {
                    adpl.setStatusLoad(adpos, adsId, false);
                    if (adECPM.idxCurrEcpm < (adECPM.list.Count - 1))
                    {
                        if (ntpgfullisnew)
                        {
                            ntpgfullisnew = false;
                            adECPM.idxCurrEcpm = 0;
                        }
                        else
                        {
                            adECPM.idxCurrEcpm++;
                        }
                        AdsProcessCB.Instance().Enqueue(() =>
                        {
                            tryLoadNtPgFull(adpl, adpos);
                        }, 1);
                    }
                    else
                    {
                        adpl.countLoad--;
                        if (adpl.countLoad <= 0)
                        {
                            adpl.isLoading = false;
                            if (getNtPgFullLoaded(placement) > 0)
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
                SdkUtil.logd($"ads fullntpg {placement} admobmy onload adpos={adpos} fail=" + err);
            }
        }
        private void OnNativePgFullDisplayedEvent(string placement, string adsId, string adnet)
        {
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                SdkUtil.logd($"ads fullntpg {adpl.showPl}-{placement} admobmy onshow ids={adsId} net={adnet}");
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
                SdkUtil.logd($"ads fullntpg {placement} admobmy onshow not pl ids={adsId} net={adnet}");
            }
        }
        private void OnNativePgFullImpresstionEvent(string placement, string adsId, string adnet)
        {
            SdkUtil.logd($"ads fullntpg {placement} admobmy OnNativePgFullImpresstionEvent");
        }
        private void OnNativePgFullFailedToShow(string placement, string adsId, string adnet, string err)
        {
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                SdkUtil.logd($"ads fullntpg {adpl.showPl}-{placement} admobmy OnNativePgFullFailedToShow=" + err);
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
                SdkUtil.logd($"ads fullntpg {placement} admobmy not pl OnNativePgFullFailedToShow=" + err);
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, false, err);
            onFullClose(placement);
            advhelper.onCloseFullGift(false);
        }
        private void OnNativePgFullClickEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logEvent("show_ads_full_ntpg_click");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                spl = adpl.showPl;
                adplNtFullpgShow = adpl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            if (adplNtFullpgShow != null)
            {
                int adpos = adplNtFullpgShow.getPosdAd(adsId);
                if (countNtFullpgImp == 1)
                {
                    FIRhelper.logEvent($"ntfullpg_click_1_{adpos}");
                }
                else
                {
                    FIRhelper.logEvent($"ntfullpg_click_2_{adpos}");
                }
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdClick(spl, "native_full", "admob", adsource, adsId);
            if (!ntpgfullIsClick)
            {
                ntpgfullIsClick = true;
                AppsFlyerHelperScript.logAdEvent("ads_click", spl, "native_full", adsId, "admob", "");
            }
        }
        private void OnNativePgFullDismissedEvent(string placement, string adsId, string adnet)
        {
            FIRhelper.logEvent("show_ads_total_imp");
            FIRhelper.logEvent("show_ads_reward_imp");
            FIRhelper.logEvent("show_ads_reward_imp_0_nt");
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                SdkUtil.logd($"ads fullntpg {adpl.showPl}-{placement} admobmy OnNativePgFullDismissedEvent id={adsId}");
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
                SdkUtil.logd($"ads fullntpg {placement} admobmy onclose not pl");
            }
            string adsource = FIRhelper.getAdsourceAdmob(adnet);
            AdsHelper.onAdShowEnd(spl, "native_full", "admob", adsource, adsId, true, "");
            onFullClose(placement);
            advhelper.onCloseFullGift(false);
        }
        private void OnNativePgFullFinishShowEvent(string placement, string adsId, string err)
        {
            //advhelper.onCloseFullGift(true);
        }
        private void OnNativePgFullAdPaidEvent(string placement, string adsId, string adNet, int precisionType, string currencyCode, long valueMicros)
        {
            ntpgfullIsClick = false;
            long originva = valueMicros;
            AdsHelper.Instance.setEcpmNtFull4Fb(originva / 1000);
            string spl = SDKManager.Instance.currPlacement;
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                AdPlacementFull adpl = dicPLNtPgFull[placement];
                spl = adpl.showPl;
                adplNtFullpgShow = adpl;
            }
            if (spl == null || spl.Length <= 1)
            {
                spl = placement;
            }
            countNtFullpgImp++;
            if (adplNtFullpgShow != null)
            {
                int adpos = adplNtFullpgShow.getPosdAd(adsId);
                if (countNtFullpgImp == 1)
                {
                    FIRhelper.logEvent($"ntfullpg_imp_1_{adpos}");
                }
                else
                {
                    FIRhelper.logEvent($"ntfullpg_imp_2_{adpos}");
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