//#define ENABLE_Bidstack
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using mygame.sdk;
#if ENABLE_Bidstack
using Bidstack;
#endif

namespace mygame.sdk
{

    public class BidstackHelper : BaseAdCanvas
    {
        private Dictionary<AdCanvasSize, BidstackObjectWithType> listAdsFree;
        private Dictionary<AdCanvasSize, BidstackObjectWithType> listAdsUse;
        private List<Vector3> listMem4Check;

        Camera mainCamera;

        public BidstackObject[] AdsPrefabs;
        public List<BidstackPlacementIdWithType> listPlacementsWithType = new List<BidstackPlacementIdWithType>();


        private Vector3 lastPos;
        private Vector3 currPos;

        private string _gdprString;


        public bool Bidstack_clicking = false;

        public override void onAwake()
        {
            listAdsFree = new Dictionary<AdCanvasSize, BidstackObjectWithType>();
            listAdsUse = new Dictionary<AdCanvasSize, BidstackObjectWithType>();
            listMem4Check = new List<Vector3>();
            for (int i = 0; i < AdsPrefabs.Length; i++)
            {
                AdsPrefabs[i].initAds(this, "cvbt_default", AdCanvasHelper.Instance.textureDefault);
                BidstackObjectWithType awtfree = new BidstackObjectWithType();
                awtfree.listAds.Add(AdsPrefabs[i]);
                listAdsFree.Add(AdsPrefabs[i].adType, awtfree);
                AdsPrefabs[i].gameObject.SetActive(false);
                BidstackObjectWithType awtuse = new BidstackObjectWithType();
                listAdsUse.Add(AdsPrefabs[i].adType, awtuse);
            }
        }
        public override void onStart()
        {
#if ENABLE_Bidstack
        onChangeCamera(AdCanvasHelper.Instance.mainCam);
        string iABTCv2String = PlayerPrefs.GetString("mem_iab_tcv2", "");
        if (iABTCv2String != null && iABTCv2String.Length > 10)
        {
            //BidstackSDK.SetGdprConsentString(iABTCv2String);
        }
#endif
        }

        private void OnEnable()
        {
#if ENABLE_Bidstack
        Bidstack.InGameAds.Events.onAdApplied += OnAdApplied;
        Bidstack.InGameAds.Events.onAdLoadFailed += OnAdLoadFailed;
#endif
        }

        private void OnDisable()
        {
#if ENABLE_Bidstack

        Bidstack.InGameAds.Events.onAdApplied -= OnAdApplied;
        Bidstack.InGameAds.Events.onAdLoadFailed -= OnAdLoadFailed;
#endif
        }
#if ENABLE_Bidstack

    void OnAdApplied(Bidstack.InGameAds.AdInfo info)
    {
        Debug.LogFormat("mysdk: adcv Bidstack Ad applied: sid={0} iid={1} cid={2} auid={3}", info.sessionID, info.instanceID, info.creativeID, info.adUnitID);
        foreach (var item in listAdsUse)
        {
            for (int i = (item.Value.listAds.Count - 1); i >= 0; i--)
            {
                BidstackObject biddob = (BidstackObject)item.Value.listAds[i];
                for (int j = 0; j < biddob.listAdInfo.Count; j++)
                {
                    //Debug.Log($"mysdk: adcv Bidstack Ad applied cl=: {biddob.listAdInfo[j].placement.adUnitID}");
                    if (biddob.listAdInfo[j].placement.adUnitID.Equals(info.adUnitID))
                    {
                        biddob.showAds();
                        break;
                    }
                }
            }
        }
    }

    void OnAdLoadFailed(Bidstack.InGameAds.AdLoadFailureInfo info)
    {
        Debug.LogErrorFormat("mysdk: adcv Bidstack Ad load failure: sid={0} auid={1} reason={2}", info.sessionID, info.adUnitID, info.reason);
    }
#endif

        public override void onChangeCamera(Camera newCamera)
        {
            mainCamera = newCamera;
#if ENABLE_Bidstack
        //Bidstack.InGameAds.AdViewerCamera.mainCameraName = newCamera.name;
        //BidstackSDK.SetMainCamera(newCamera);
#endif
        }


        public override void onUpdate()
        {
#if ENABLE_Bidstack
        //if (listAdsUse.Count > 0 && AdCanvasHelper.Instance.isEnableClick)
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        placementClick = null;
        //        tClickDown = 0;
        //        posClick = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        //        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        //        eventDataCurrentPosition.position = new Vector2(posClick.x, posClick.y);

        //        //check ui elements
        //        bool isIgnoreClick = false;
        //        if (EventSystem.current != null)
        //        {
        //            List<RaycastResult> results = new List<RaycastResult>();
        //            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        //            foreach (var re in results)
        //            {
        //                Debug.Log($"mysdk: adcv Bidstack click 000 ui=" + re.gameObject.name);
        //                if (!AdCanvasHelper.Instance.nameUiObIgnore.Contains(re.gameObject.name))
        //                {
        //                    isIgnoreClick = true;
        //                    break;
        //                }
        //            }
        //        }
        //        if (!isIgnoreClick)
        //        {
        //            Ray ray;
        //            if (mainCamera != null)
        //            {
        //                ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //            }
        //            else
        //            {
        //                if (Camera.main != null)
        //                {
        //                    ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        //                }
        //                else
        //                {
        //                    return;
        //                }
        //            }
        //            RaycastHit hit;
        //            if (Physics.Raycast(ray, out hit, AdCanvasHelper.Instance.lenghtRayClick, LayerMask.GetMask("Default", "AdCanvasLayer")))
        //            {
        //                placementClick = hit.collider.GetComponent<BidstackPlacement>();
        //                tClickDown = SdkUtil.CurrentTimeMilis();
        //            }
        //        }
        //    }
        //    if (Input.GetMouseButtonUp(0))
        //    {
        //        if (placementClick != null)
        //        {
        //            Vector2 dclick = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - posClick;
        //            long tup = SdkUtil.CurrentTimeMilis();
        //            if ((tup - tClickDown) <= 2000 && Mathf.Abs(dclick.x) <= 10 && Mathf.Abs(dclick.y) <= 10)
        //            {
        //                placementClick.Interact();
        //                onclickBidstack();
        //            }
        //        }
        //    }
        //}
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

        public string getPlacementId(BidstackPlacementType type)
        {
            string re = null;
            foreach (var item in listPlacementsWithType)
            {
                if (item.type == type)
                {
                    if (item.idx < item.listPlacements.Count)
                    {
                        re = item.listPlacements[item.idx];
                        item.idx++;
                        if (item.idx >= item.listPlacements.Count)
                        {
                            item.idx = 0;
                        }
                    }
                    break;
                }
            }
            return re;
        }

        public override void initClick(bool isClick)
        {
#if ENABLE_Bidstack

#endif
        }

        public override BaseAdCanvasObject genAd(AdCanvasSize type, string placement, Vector3 pos, bool isDisableObj, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false)
        {
#if ENABLE_Bidstack
        BaseAdCanvasObject ad = getAdsWithType(type, pos, forward, _target, stateLookat, isFloowY);
        Debug.Log($"mysdk: adcv Bidstack genAd count={listAdsUse.Count}");
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

        BidstackObject getPrefab(AdCanvasSize type)
        {
            BidstackObject re = null;
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
            Debug.Log($"mysdk: adcv Bidstack onShowCmpiOS");
        }

        public override void onCMPOK(string iABTCv2String)
        {
            Debug.Log($"mysdk: adcv Bidstack onCMPOK=" + iABTCv2String);
#if ENABLE_Bidstack && !UNITY_EDITOR
        //BidstackSDK.SetGdprConsentString(iABTCv2String);
#endif
        }
    }

    [Serializable]
    public class BidstackPlacementIdWithType
    {
        public BidstackPlacementType type = BidstackPlacementType.P_300x250;
        public List<string> listPlacements = new List<string>();
        public int idx = 0;
    }

    public class BidstackObjectWithType
    {
        public List<BaseAdCanvasObject> listAds = new List<BaseAdCanvasObject>();
    }
}
