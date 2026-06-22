//#define ENABLE_AdInMo
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using mygame.sdk;
#if ENABLE_AdInMo
using Adinmo;
#endif

namespace mygame.sdk
{

    public class AdInMoHelper : BaseAdCanvas
    {
        public static event Action<string> onPurchaseSuccess = null;

        private Dictionary<AdCanvasSize, AdInMoObjectWithType> listAdsFree;
        private Dictionary<AdCanvasSize, AdInMoObjectWithType> listAdsUse;
        private List<Vector3> listMem4Check;

        Camera mainCamera;

        public AdInMoObject[] AdsPrefabs;

        private Vector3 lastPos;
        private Vector3 currPos;

        private string _gdprString;


        [HideInInspector] public bool AdInMo_clicking = false;
        int stateClick = 0;

        Vector2 posClick = Vector2.zero;
#if ENABLE_AdInMo
        AdinmoTexture placementClick = null;
        long tClickDown = 0;
#endif

        public override void onAwake()
        {
            listAdsFree = new Dictionary<AdCanvasSize, AdInMoObjectWithType>();
            listAdsUse = new Dictionary<AdCanvasSize, AdInMoObjectWithType>();
            listMem4Check = new List<Vector3>();
            for (int i = 0; i < AdsPrefabs.Length; i++)
            {
                AdsPrefabs[i].initAds(this, "cvgm_default", AdCanvasHelper.Instance.textureDefault);
                AdInMoObjectWithType awtfree = new AdInMoObjectWithType();
                awtfree.listAds.Add(AdsPrefabs[i]);
                listAdsFree.Add(AdsPrefabs[i].adType, awtfree);
                AdsPrefabs[i].gameObject.SetActive(false);
                AdInMoObjectWithType awtuse = new AdInMoObjectWithType();
                listAdsUse.Add(AdsPrefabs[i].adType, awtuse);
            }
        }
        public override void onStart()
        {
#if ENABLE_AdInMo
            onChangeCamera(AdCanvasHelper.Instance.mainCam);
            string iABTCv2String = PlayerPrefs.GetString("mem_iab_tcv2", "");
            if (iABTCv2String != null && iABTCv2String.Length > 10)
            {
                //AdInMoSDK.SetGdprConsentString(iABTCv2String);
            }
            AdinmoManager.SetOnReadyCallback(OnReadyCallback);
            AdinmoManager.SetPauseGameCallback(PauseGameCallback);
            AdinmoManager.SetResumeGameCallback(ResumeGameCallback);
            AdinmoManager.SetInAppPurchaseCallback(InAppPurchaseCallback);
            //AdinmoManager.SetInAppPurchasedAlreadyCallback(InAppPurchasedAlreadyCallback);
            //AdinmoManager.SetInAppPurchaseGetPriceCallback(InAppPurchaseGetPriceCallback);
#endif
        }

        public override void onChangeCamera(Camera newCamera)
        {
            mainCamera = newCamera;
#if ENABLE_AdInMo
            if (newCamera != null)
            {
                AdinmoManager.s_manager.m_camera = newCamera;
            }
#endif
        }


        public override void onUpdate()
        {
#if ENABLE_AdInMo
            foreach (var ad in listAdsUse)
            {
                foreach (var item in ad.Value.listAds)
                {
                    item.onUpdate();
                }
            }
#endif
        }
        private void OnDestroy()
        {
            listAdsFree.Clear();
            listAdsUse.Clear();
            listMem4Check = null;
        }

        public override void onDestroyAd()
        {
        }

        public override void initClick(bool isClick)
        {
#if ENABLE_AdInMo

#endif
        }

        void onclickAdInMo()
        {
            mygame.sdk.FIRhelper.logEvent("AdInMo_onclick");
            int flagclick = PlayerPrefs.GetInt("cf_adcanvas_click", 0xFF);
            if ((flagclick & 1) > 0)
            {
                if (GameHelper.Instance != null)
                {
                    AdInMo_clicking = true;
                    stateClick = 1;
                    GameHelper.Instance.isAlowShowAppOpenAd = false;
                    StartCoroutine(waitResetFlagShowOpenAds());
                    SdkUtil.logd($"adcv adInMo onclickadInMo AdInMo_clicking={AdInMo_clicking}");
                }
            }
        }

        IEnumerator waitResetFlagShowOpenAds()
        {
            yield return new WaitForSeconds(3.5f);
            if (stateClick == 1)
            {
                stateClick = 0;
                AdInMo_clicking = false;
                GameHelper.Instance.isAlowShowAppOpenAd = true;
                SdkUtil.logd($"adcv adInMo click but not openLink");
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (AdInMo_clicking)
                {
                    SdkUtil.logd($"adcv adInMo OnApplicationPause AdInMo_clicking={AdInMo_clicking}");
                    AdInMo_clicking = false;
                    stateClick = 0;
                    StartCoroutine(waitResetFlagShowOpenAdsAfterOpenLink());
                }
            }
            else
            {
                if (stateClick == 1 && AdInMo_clicking)
                {
                    SdkUtil.logd($"adcv adInMo OnApplicationPause openLink");
                    stateClick = 0;
                }
            }
        }

        IEnumerator waitResetFlagShowOpenAdsAfterOpenLink()
        {
            yield return new WaitForSeconds(0.35f);
            GameHelper.Instance.isAlowShowAppOpenAd = true;
            SdkUtil.logd($"adcv adInMo setFlag ads open");
        }

        public override BaseAdCanvasObject genAd(AdCanvasSize type, string placement, Vector3 pos, bool isHideBillboard, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false)
        {
#if ENABLE_AdInMo
            if (type == AdCanvasSize.Size960x540)
            {
                type = AdCanvasSize.Size4x3Video;
            }
            BaseAdCanvasObject ad = getAdsWithType(type, placement, pos, forward, _target, stateLookat, isFloowY);
            SdkUtil.logd($"adcv adInMo genAd count={listAdsUse.Count}");
            if (ad != null)
            {
                ad.isHideBillboard = isHideBillboard;
                if (isHideBillboard)
                {
                    ad.hideBillboards(true);
                }
                ad.transform.localScale = new Vector3(1, 1, 1);
            }

            return ad;
#else
        return null;
#endif
        }
        private BaseAdCanvasObject getAdsWithType(AdCanvasSize type, string placement, Vector3 pos, Vector3 forward, Transform _target, int stateLookat, bool isFloowY)
        {
            if (listAdsUse.ContainsKey(type) && listAdsUse[type].listAds.Count < 15)
            {
                if (listAdsFree[type].listAds.Count > 0)
                {
                    BaseAdCanvasObject re = null;
                    for (int i = 0; i < listAdsFree[type].listAds.Count; i++)
                    {
                        if (listAdsFree[type].listAds[i].isLoaded())
                        {
                            re = listAdsFree[type].listAds[i];
                            listAdsFree[type].listAds.RemoveAt(i);
                            break;
                        }
                    }
                    if (re == null)
                    {
                        re = listAdsFree[type].listAds[0];
                        listAdsFree[type].listAds.RemoveAt(0);
                    }
                    re.transform.position = pos;
                    re.pos = pos;
                    re.forward = forward;
                    re.target = _target;
                    re.stateLoockat = stateLookat;
                    listAdsUse[type].listAds.Add(re);
                    re.gameObject.SetActive(true);
                    re.setPlacement(placement);
                    re.setFollowY(isFloowY);
                    re.enableMesh();
                    return re;
                }
                else
                {
                    var prefabsads = getPrefab(type);
                    if (prefabsads != null)
                    {
                        var re = Instantiate(prefabsads, pos, Quaternion.identity, prefabsads.transform.parent);
                        re.initAds(this, placement, AdCanvasHelper.Instance.textureDefault);
                        for (int i = 0; i < re.listAdInfo.Count; i++)
                        {
                            re.listAdInfo[i].isLoadDefault = false;
                        }
                        re.transform.position = pos;
                        re.pos = pos;
                        re.forward = forward;
                        re.target = _target;
                        re.stateLoockat = stateLookat;
                        listAdsUse[type].listAds.Add(re);
                        re.gameObject.SetActive(true);
                        re.setFollowY(isFloowY);
                        re.enableMesh();
                        return re;
                    }
                }
            }
            return null;
        }

        AdInMoObject getPrefab(AdCanvasSize type)
        {
            AdInMoObject re = null;
            for (int i = 0; i < AdsPrefabs.Length; i++)
            {
                if (AdsPrefabs[i].adType == type)
                {
                    re = AdsPrefabs[i];
                    break;
                }
            }

            return re;
        }
        public override void freeAll()
        {
            foreach (var item in listAdsUse)
            {
                for (int i = (item.Value.listAds.Count - 1); i >= 0; i--)
                {
                    listAdsFree[item.Value.listAds[i].adType].listAds.Add(item.Value.listAds[i]);
                    item.Value.listAds[i].gameObject.SetActive(false);
                    item.Value.listAds.RemoveAt(i);
                }
            }
        }
        public override void freeAd(BaseAdCanvasObject ad)
        {
            if (ad != null)
            {
                if (listAdsUse[ad.adType].listAds.Contains(ad))
                {
                    listAdsUse[ad.adType].listAds.Remove(ad);
                    listAdsFree[ad.adType].listAds.Add(ad);
                    ad.gameObject.SetActive(false);
                }
            }
        }

        public override void onShowCmpNative()
        {
            SdkUtil.logd($"adcv adInMo onShowCmpiOS");
        }

        public override void onCMPOK(string iABTCv2String)
        {
            SdkUtil.logd($"adcv adInMo onCMPOK=" + iABTCv2String);
#if ENABLE_AdInMo && !UNITY_EDITOR
        //AdinmoManager.SetGdprConsentString(iABTCv2String);
#endif
        }

#if ENABLE_AdInMo
        public void OnReadyCallback(string status)
        {
            SdkUtil.logd($"adcv adInMo OnReadyCallback={status}");
        }
        public void PauseGameCallback()
        {
            SdkUtil.logd($"adcv adInMo PauseGameCallback");
            onclickAdInMo();
        }
        public void ResumeGameCallback()
        {
            SdkUtil.logd($"adcv adInMo ResumeGameCallback");
        }
        public void InAppPurchaseCallback(string iap_id)
        {
            SdkUtil.logd($"adcv adInMo InAppPurchaseCallback={iap_id}");
#if ENABLE_INAPP
            string skuid = InappHelper.Instance.getSkuIdBySku(iap_id);
            InappHelper.Instance.BuyPackage(skuid, "by_adinmo", (status) =>
            {
                if (status.status == 1)
                {
                    if (onPurchaseSuccess != null)
                    {
                        onPurchaseSuccess(skuid);
                    }
                }
            });
#endif
        }
        public InAppAlreadyPurchasedReply InAppPurchasedAlreadyCallback(string iap_id)
        {
            SdkUtil.logd($"adcv adInMo InAppPurchasedAlreadyCallback={iap_id}");
            InAppAlreadyPurchasedReply re = new InAppAlreadyPurchasedReply(true);
            return re;
        }
        public string InAppPurchaseGetPriceCallback(string iap_id)
        {
            SdkUtil.logd($"adcv adInMo InAppPurchaseGetPriceCallback={iap_id}");
            if (InappHelper.Instance != null)
            {
                return InappHelper.Instance.getPrice(iap_id);
            }
            else
            {
                return "";
            }
        }
#endif
    }

    public class AdInMoObjectWithType
    {
        public List<BaseAdCanvasObject> listAds = new List<BaseAdCanvasObject>();
    }
}