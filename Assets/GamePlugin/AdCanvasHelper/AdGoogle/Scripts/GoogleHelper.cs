//#define ENABLE_Google
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using mygame.sdk;
#if ENABLE_Google
using GoogleMobileAds.Api;
#endif

namespace mygame.sdk
{
    public class GoogleHelper : BaseAdCanvas
    {
        public static GoogleHelper Instance = null;

        private Dictionary<AdCanvasSize, GoogleObjectWithType> listAdsFree;
        private Dictionary<AdCanvasSize, GoogleObjectWithType> listAdsUse;
        private List<Vector3> listMem4Check;

        Camera mainCamera;

        public GoogleObject[] AdsPrefabs;

        private Vector3 lastPos;
        private Vector3 currPos;

        private string _gdprString;


        [HideInInspector] public bool Google_clicking = false;
        int stateClick = 0;

        Vector2 posClick = Vector2.zero;
#if ENABLE_Google
    long tClickDown = 0;
#endif

        public List<GooglePlacementId> listIds = new List<GooglePlacementId>();

        public override void onAwake()
        {
            Instance = this;
            listAdsFree = new Dictionary<AdCanvasSize, GoogleObjectWithType>();
            listAdsUse = new Dictionary<AdCanvasSize, GoogleObjectWithType>();
            listMem4Check = new List<Vector3>();
            for (int i = 0; i < AdsPrefabs.Length; i++)
            {
                AdsPrefabs[i].initAds(this, "cvgg_default", AdCanvasHelper.Instance.textureDefault);
                GoogleObjectWithType awtfree = new GoogleObjectWithType();
                awtfree.listAds.Add(AdsPrefabs[i]);
                listAdsFree.Add(AdsPrefabs[i].adType, awtfree);
                AdsPrefabs[i].gameObject.SetActive(false);
                GoogleObjectWithType awtuse = new GoogleObjectWithType();
                listAdsUse.Add(AdsPrefabs[i].adType, awtuse);
            }
            //List<string> TestDeviceIds = new List<string>() { "08898EE6AF9BF89D54AB6F4842B36BDD" };
            //MobileAds.SetRequestConfiguration(new RequestConfiguration
            //{
            //    TestDeviceIds = TestDeviceIds
            //});
        }

        public void initListIds(string slist)
        {
            listIds.Clear();
            string[] grlist = slist.Split(new char[] { '#' });
            foreach (string grItem in grlist)
            {
                GooglePlacementId gpi = new GooglePlacementId();
                listIds.Add(gpi);
                string[] arrl = grItem.Split(new char[] { ';' });
                foreach (string idad in arrl)
                {
                    gpi.ids.Add(idad);
                }
            }
        }

        public override void onStart()
        {
            string ssl = PlayerPrefs.GetString("mem_list_immirsive", AdIdsConfig.AdmobImmirsive);
            initListIds(ssl);
#if ENABLE_Google
        MobileAds.Initialize(initStatus =>
        {
            // Initialize the Immersive In-game Ads SDK.
            ImmersiveInGameDisplayAd.Initialize(() =>
            {
            });
        });
#endif
        }

        public override void onChangeCamera(Camera newCamera)
        {
            mainCamera = newCamera;
#if ENABLE_Google
        if (newCamera != null)
        {
            ImmersiveInGameDisplayAd.SetCamera(newCamera);
        }
#endif
        }


        public override void onUpdate()
        {
#if ENABLE_Google
        foreach (var ad in listAdsUse)
        {
            foreach (var item in ad.Value.listAds)
            {
                item.onUpdate();
            }
        }
#endif
        }

        public override void onDestroyAd()
        {

        }

        private void OnDestroy()
        {
            listAdsFree.Clear();
            listAdsUse.Clear();
            listMem4Check = null;
            Instance = null;
        }

        public override void initClick(bool isClick)
        {
#if ENABLE_Google

#endif
        }

        public void onclickGoogle()
        {
            mygame.sdk.FIRhelper.logEvent("Google_onclick");
            int flagclick = PlayerPrefs.GetInt("cf_adcanvas_click", 0xFF);
            if ((flagclick & 16) > 0)
            {
                if (GameHelper.Instance != null)
                {
                    Google_clicking = true;
                    stateClick = 1;
                    GameHelper.Instance.isAlowShowAppOpenAd = false;
                    StartCoroutine(waitResetFlagShowOpenAds());
                    SdkUtil.logd($"adcv google onclickGoogle Google_onclick={Google_clicking}");
                }
            }
        }

        IEnumerator waitResetFlagShowOpenAds()
        {
            yield return new WaitForSeconds(3.5f);
            if (stateClick == 1)
            {
                stateClick = 0;
                Google_clicking = false;
                GameHelper.Instance.isAlowShowAppOpenAd = true;
                SdkUtil.logd($"adcv google click but not openLink");
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                if (Google_clicking)
                {
                    SdkUtil.logd($"adcv google OnApplicationPause Google_clicking={Google_clicking}");
                    Google_clicking = false;
                    stateClick = 0;
                    StartCoroutine(waitResetFlagShowOpenAdsAfterOpenLink());
                }
            }
            else
            {
                if (stateClick == 1 && Google_clicking)
                {
                    SdkUtil.logd($"adcv google OnApplicationPause openLink");
                    stateClick = 0;
                }
            }
        }

        IEnumerator waitResetFlagShowOpenAdsAfterOpenLink()
        {
            yield return new WaitForSeconds(0.35f);
            GameHelper.Instance.isAlowShowAppOpenAd = true;
            SdkUtil.logd($"adcv google setFlag ads open");
        }

        public override BaseAdCanvasObject genAd(AdCanvasSize type, string placement, Vector3 pos, bool isDisableObj, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false)
        {
#if ENABLE_Google
        type = AdCanvasSize.Size6x5;
        if (type == AdCanvasSize.Size_3f_6x5)
        {
            type = AdCanvasSize.Size6x5;
            stateLookat = 1;
        }
        BaseAdCanvasObject ad = getAdsWithType(type, pos, forward, _target, stateLookat, isFloowY);
        SdkUtil.logd($"adcv google genAd count={listAdsUse.Count}");
        if (ad != null)
        {
            ad.disableObj = isDisableObj;
            if (isDisableObj)
            {
                ad.setStatusObj(true);
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
            //Camera camera = PlayerController.Instance.personCamera.mainCamera;
            //if (!PointInCameraView(pos, camera))
            //{
            //    return null;
            //}
            stateLookat = 1;
            type = AdCanvasSize.Size6x5;//remove
            if (listAdsUse.ContainsKey(type) && listAdsUse[type].listAds.Count < 1)//< 15
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
            //else if (PlayerController.Instance != null)
            //{
            //    if (listAdsUse.ContainsKey(type) && listAdsUse[type].listAds.Count >= 1)
            //    {
            //        if (listAdsUse[type].listAds.Count > 0)
            //        {
            //            BaseAdCanvasObject re = listAdsUse[type].listAds[0];
            //            if (!PointInCameraView(re.pos, camera))
            //            {
            //                re.transform.position = pos;
            //                re.pos = pos;
            //                re.forward = forward;
            //                re.target = _target;
            //                re.stateLoockat = stateLookat;
            //                re.gameObject.SetActive(true);
            //                re.setPlacement(placement);
            //                re.setFollowY(isFloowY);
            //                re.enableMesh();
            //                return re;
            //            }
            //        }
            //    }
            //}
            return null;
        }

        public bool PointInCameraView(Vector3 point, Camera camera)
        {
            Vector3 viewport = camera.WorldToViewportPoint(point);
            bool inCameraFrustum = Is01(viewport.x) && Is01(viewport.y);
            bool inFrontOfCamera = viewport.z > 0;

            return inCameraFrustum && inFrontOfCamera;
        }

        public bool Is01(float a)
        {
            return a > 0 && a < 1;
        }

        GoogleObject getPrefab(AdCanvasSize type)
        {
            GoogleObject re = null;
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
            SdkUtil.logd($"adcv google onShowCmpiOS");
        }

        public override void onCMPOK(string iABTCv2String)
        {
            SdkUtil.logd($"adcv google onCMPOK=" + iABTCv2String);
#if ENABLE_Google && !UNITY_EDITOR
        //GadsmeSDK.SetGdprConsentString(iABTCv2String);
#endif
        }
    }

    public class GoogleObjectWithType
    {
        public List<BaseAdCanvasObject> listAds = new List<BaseAdCanvasObject>();
    }

    public class GooglePlacementId
    {
        public List<string> ids = new List<string>();
        public int idxCurr = 0;
    }
}