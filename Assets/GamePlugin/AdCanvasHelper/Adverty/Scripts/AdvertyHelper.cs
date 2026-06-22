//#define ENABLE_Adverty
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using mygame.sdk;
#if ENABLE_Adverty
using Adverty5;
using Adverty5.Checks;
#endif

namespace mygame.sdk
{

    public class AdvertyHelper : BaseAdCanvas
    {
        private Dictionary<AdCanvasSize, AdvertyObjectWithType> listAdsFree;
        private Dictionary<AdCanvasSize, AdvertyObjectWithType> listAdsUse;
        //private List<Vector3> listMem4Check;

        public Camera mainCamera;

        public AdvertyObject[] AdsPrefabs;


        //private Vector3 lastPos;
        //private Vector3 currPos;

        private string _gdprString;
        public bool Adverty_click = false;

#if ENABLE_Adverty
        [MonoPInvokeCallback(typeof(AdReadyCallback))]
        private static void OnAdReadyCallback(double price, int width, int height)
        {
            SdkUtil.logd($"adcv adverty AD paid={price} width={width} height={height}");
        }

        [MonoPInvokeCallback(typeof(BrowserOpenCallback))]
        private static void OnBrowserOpened()
        {
            SdkUtil.logd($"adcv adverty On browser opened");
        }

        [MonoPInvokeCallback(typeof(BrowserClosedCallback))]
        private static void OnBrowserClosed()
        {
            SdkUtil.logd($"adcv adverty On browser closed");
        }
#endif

        public override void onAwake()
        {
            initSdk();

            listAdsFree = new Dictionary<AdCanvasSize, AdvertyObjectWithType>();
            listAdsUse = new Dictionary<AdCanvasSize, AdvertyObjectWithType>();
            //listMem4Check = new List<Vector3>();
            for (int i = 0; i < AdsPrefabs.Length; i++)
            {
                AdsPrefabs[i].initAds(this, "cvav_default", AdCanvasHelper.Instance.textureDefault);
                AdvertyObjectWithType awtfree = new AdvertyObjectWithType();
                awtfree.listAds.Add(AdsPrefabs[i]);
                listAdsFree.Add(AdsPrefabs[i].adType, awtfree);
                AdsPrefabs[i].gameObject.SetActive(false);
                AdvertyObjectWithType awtuse = new AdvertyObjectWithType();
                listAdsUse.Add(AdsPrefabs[i].adType, awtuse);
            }
        }
        public override void onStart()
        {
#if ENABLE_Adverty
            onChangeCamera(AdCanvasHelper.Instance.mainCam);
#endif
        }

        private void initSdk()
        {
#if ENABLE_Adverty
            onChangeCamera(mainCamera);
            string APIKey = AdIdsConfig.AdvertyApiKey;
            bool isAdCanvasSandbox = false;
            if (SDKManager.Instance != null && SDKManager.Instance.isAdCanvasSandbox || AdvertySettings.SandboxMode)
            {
                isAdCanvasSandbox = true; //Sandbox mode enabled if we are using Development build or we are in editor
            }
            string iABTCv2String = PlayerPrefs.GetString("mem_iab_tcv2", "");
            UserData userData = new UserData() { consented = true, consentString = iABTCv2String, gppConsentString = iABTCv2String };
            LaunchData launchData = new LaunchData
            {
                apiKey = APIKey,
                sandboxMode = isAdCanvasSandbox,
                userData = userData,
                callbacks = { OnAdReadyCallback = OnAdReadyCallback, OnBrowserOpenCallback = OnBrowserOpened, OnBrowserClosedCallback = OnBrowserClosed }
            };
            Adverty.Start(launchData);

#endif
        }

        public override void onChangeCamera(Camera newCamera)
        {
            if (newCamera == null)
            {
                newCamera = Camera.main;
            }
            mainCamera = newCamera;
#if ENABLE_Adverty
            if (mainCamera == null)
            {
                Debug.LogError(
                    "Adverty was not able to detect a MainCamera. AdvertyLooper wasn't attached. Please add it manually to your Game Camera.");
                return;
            }

            GameObject currentCameraObject = mainCamera.gameObject;
            if (!currentCameraObject.GetComponent<AdvertyLooper>())
            {
                currentCameraObject.AddComponent<AdvertyLooper>();
            }
            if (!currentCameraObject.GetComponent<AdvertyViewabilityCheck>())
            {
                currentCameraObject.AddComponent<AdvertyViewabilityCheck>();
            }
#endif
        }


        public override void onUpdate()
        {
#if ENABLE_Adverty
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
            //listMem4Check = null;
        }

        public override void onDestroyAd()
        {
#if ENABLE_Adverty
            Adverty.Stop();
#endif
        }

        public override void initClick(bool isClick)
        {
            Adverty_click = isClick;
#if ENABLE_Adverty
            if (listAdsUse.Count > 0)
            {
                foreach (var ad in listAdsUse)
                {
                    foreach (var item in ad.Value.listAds)
                    {
                        item.initClick(isClick && AdCanvasHelper.Instance.isEnableClick);
                    }
                }
            }
#endif
        }

        public override BaseAdCanvasObject genAd(AdCanvasSize type, string placement, Vector3 pos, bool isHideBillboard, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false)
        {
#if ENABLE_Adverty
            BaseAdCanvasObject ad = getAdsWithType(type, placement, pos, forward, _target, stateLookat, isFloowY);
            SdkUtil.logd($"adcv adverty genAd count={listAdsUse.Count}");
            if (ad != null)
            {
                ad.isHideBillboard = isHideBillboard;
                ad.initClick(Adverty_click && AdCanvasHelper.Instance.isEnableClick);
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

        AdvertyObject getPrefab(AdCanvasSize type)
        {
            AdvertyObject re = null;
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

#if ENABLE_Adverty

#endif

        public override void onShowCmpNative()
        {
            SdkUtil.logd($"adcv adverty onShowCmpiOS");
        }

        public override void onCMPOK(string iABTCv2String)
        {
            SdkUtil.logd($"adcv adverty onCMPOK=" + iABTCv2String);
#if ENABLE_Adverty && !UNITY_EDITOR
        //Adverty.UserData userData = new Adverty.UserData(AgeSegment.Unknown, Gender.Unknown, iABTCv2String);
        //AdvertySDK.UpdateUserData(userData);
#endif
        }
    }

    public class AdvertyObjectWithType
    {
        public List<BaseAdCanvasObject> listAds = new List<BaseAdCanvasObject>();
    }
}