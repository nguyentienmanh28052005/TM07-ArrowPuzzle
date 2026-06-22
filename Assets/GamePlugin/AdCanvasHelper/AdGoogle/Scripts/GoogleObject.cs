using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using mygame.sdk;
#if ENABLE_Google
using GoogleMobileAds.Api;
#endif
namespace mygame.sdk
{

    public class GoogleObject : BaseAdCanvasObject
    {
        public List<GoogleInfo> listAdInfo;
        [SerializeField] private List<GameObject> listObBoard;
        Dictionary<GameObject, GoogleInfo> dicAds = null;
        bool isAdLoaded = false;
        //Vector3 postouch;
        TTAdCanvas obTTDefault = null;
        //bool isgetAllImg = false;
        int idxAniDf = 0;
        bool isFollowY = false;
        float yTagetOrigin = 0;
        float preYtaget = 0;
        float yWillSet = 0;
        float tLookAt = 2000;
        long tloadFail = 0;
        bool isCallStart = false;
        bool isLoading = false;
        //static bool isSetTouch = false;
        int isLoadOther = 1;
        int idxFloor = 0;

        public override void initAds(BaseAdCanvas adhelper, string placement, Texture2D ttdf)
        {
            base.initAds(adhelper, placement, ttdf);
            if (dicAds == null)
            {
                //isgetAllImg = false;
                isFollowY = false;
                dicAds = new Dictionary<GameObject, GoogleInfo>();
                for (int i = 0; i < listAdInfo.Count; i++)
                {
                    dicAds.Add(listAdInfo[i].mesh.gameObject, listAdInfo[i]);
#if ENABLE_Google

#endif
                }
                hideAds();
            }

#if ENABLE_Google
        if (ttdf != null)
        {
            for (int i = 0; i < listAdInfo.Count; i++)
            {
            }
        }
#endif
            if (isAdLoaded)
            {
                showAds();
            }
            else
            {
                hideAds();
            }
            isLoadOther = PlayerPrefs.GetInt("cf_adcanvas_loadotherfgg", 1);
        }

        private void Start()
        {
            isAdLoaded = false;
            isCallStart = true;
            loadAd(true);
        }

        private void OnEnable()
        {
            if (isCallStart && !isAdLoaded && !isLoading)
            {
                long tcu = SdkUtil.CurrentTimeMilis();
                if ((tcu - tloadFail) >= 15000)
                {
                    tloadFail = tcu;
                    loadAd(true);
                }
            }
        }

        // private void OnEnable()
        // {
        //     Invoke("setTextDefault", 1);
        //     Debug.Log($"mysdk: adcv Google OnEnable isAdLoaded={isAdLoaded}");
        // }

        public override void enableMesh()
        {
            // if (listMesh != null)
            // {
            //     for (int i = 0; i < listMesh.Count; i++)
            //     {
            //         listMesh[i].enabled = true;
            //     }
            // }

            yWillSet = 0;
        }

        public override void setFollowY(bool isfl, float yTaget = -10000)
        {
            isFollowY = isfl;
            if (target != null)
            {
                if (yTaget <= -10000)
                {
                    yTagetOrigin = target.transform.position.y;
                }
                else
                {
                    yTagetOrigin = yTaget;
                }

                preYtaget = yTagetOrigin;
            }
        }

        void setTextDefault()
        {
#if ENABLE_Google
        //for (int i = 0; i < listAdInfo.Count; i++)
        //{
        //}
#endif
        }

        void loadAd(bool isStart)
        {
#if ENABLE_Google
        isLoading = true;
        foreach (var item in dicAds)
        {
            int[] wad = { 300, 300, 320, 1024, 300, 1200 };
            int[] had = { 250, 250, 50, 768, 600, 628 };
            //int w = 1200;
            //int h = 1200;
            //int[] wad = { w, w, w, w, w, w };
            //int[] had = { h, h, h, h, h, h };
            string idLoad = "";
            if (GoogleHelper.Instance != null)
            {
                if (GoogleHelper.Instance.listIds.Count > 0)
                {
                    if (isStart)
                    {
                        idxFloor = 0;
                    }
                    if (idxFloor >= GoogleHelper.Instance.listIds.Count)
                    {
                        idxFloor = 0;
                    }
                    GooglePlacementId plid = GoogleHelper.Instance.listIds[idxFloor];
                    if (plid.ids.Count > 0)
                    {
                        if (plid.idxCurr >= plid.ids.Count)
                        {
                            plid.idxCurr = 0;
                        }
                        idLoad = plid.ids[plid.idxCurr];
                        plid.idxCurr++;
                        if (plid.idxCurr >= plid.ids.Count)
                        {
                            plid.idxCurr = 0;
                        }
                    }
                }
            }
            if (idLoad.Length <= 0)
            {
                if (isLoadOther == 1)
                {
                    AdCanvasHelper.GenAd(adType, pos, false, forward, target, 1, false, 3);
                }
            }
            else
            {
                SdkUtil.logd($"adcv Google loadAd={idLoad}");
                ImmersiveInGameDisplayAdAspectRatio adSize = new ImmersiveInGameDisplayAdAspectRatio(wad[(int)adType], wad[(int)adType]);
                AdLoader adLoader = new AdLoader.Builder(idLoad)
                        .ForImmersiveInGameDisplayAd()
                        .SetImmersiveInGameDisplayAdAspectRatio(adSize)
                        .Build();
                GoogleInfo finfo = item.Value;
                adLoader.OnImmersiveInGameDisplayAdLoaded += (sender, args) =>
                {
                    SdkUtil.logd($"adcv Google OnImmersiveInGameDisplayAdLoaded {finfo.loPos}");
                    if (finfo.mesh != null)
                    {
                        if (item.Value.placement != null)
                        {
                            item.Value.placement.Destroy();
                        }
                        for (int ii = 0; ii < finfo.mesh.gameObject.transform.parent.childCount; ii++)
                        {
                            var cc = finfo.mesh.gameObject.transform.parent.GetChild(ii);
                            if (!cc.name.StartsWith("Def") && !cc.name.StartsWith("ad"))
                            {
                                Destroy(cc.gameObject);
                            }
                        }
                        args.ImmersiveInGameDisplayAd.SetParent(finfo.mesh.gameObject.transform.parent.gameObject);
                    }
                    item.Value.placement = args.ImmersiveInGameDisplayAd;
                    item.Value.placement.SetLocalPosition(finfo.loPos);
                    item.Value.placement.SetLocalRotation(Quaternion.Euler(finfo.loRot));
                    item.Value.placement.SetLocalScale(finfo.loScale);
                    item.Value.placement.ShowAd();

                    isAdLoaded = true;
                    isLoading = false;
                    if (dicAds.ContainsKey(finfo.mesh.gameObject))
                    {
                        dicAds[finfo.mesh.gameObject].isAdLoaded = true;
                    }
                    showAds();
                };
                adLoader.OnAdFailedToLoad += (sender, args) =>
                {
                    SdkUtil.logd($"adcv Google HandleAdFailedToLoad={args.LoadAdError.GetMessage()}");
                    isLoading = false;
                    if (isAdLoaded)
                    {
                        showAds();
                    }
                    else
                    {
                        tloadFail = SdkUtil.CurrentTimeMilis();
                        bool isConLoad = false;
                        if (GoogleHelper.Instance != null && GoogleHelper.Instance.listIds.Count > 1)
                        {
                            if (idxFloor < (GoogleHelper.Instance.listIds.Count - 1))
                            {
                                isConLoad = true;
                                idxFloor++;
                                loadAd(false);
                            }
                        }
                        if (isLoadOther == 1 && !isConLoad)
                        {
                            AdCanvasHelper.GenAd(adType, pos, false, forward, target, 1, false, 3);
                        }
                    }

                };
                adLoader.OnImmersiveInGameDisplayAdClicked += (sender, args) =>
                {
                    SdkUtil.logd($"adcv Google AdClicked");
                    if (GoogleHelper.Instance != null)
                    {
                        GoogleHelper.Instance.onclickGoogle();
                    }
                };
                adLoader.OnImmersiveInGameDisplayAdImpression += (sender, args) =>
                {
                    SdkUtil.logd($"adcv Google AdImpression");
                    FIRhelper.logEvent("show_ads_cv_gg");
                };
                var request = new AdRequest();
                adLoader.LoadAd(request);
            }
        }
#endif
        }

        void showAds()
        {
            hideBillboards(isHideBillboard);
            hideDefault(isHideDefault);
            foreach (var item in dicAds)
            {
                item.Value.mesh.transform.parent.GetComponent<MeshRenderer>().enabled = !isHideDefault;
                item.Value.mesh.enabled = true;
                if (item.Value.defaultScaleFace == Vector3.zero)
                {
                    item.Value.defaultScaleFace = item.Value.mesh.transform.parent.localScale;
                }
                item.Value.mesh.transform.parent.localScale = new Vector3(item.Value.defaultScaleFace.x * scaleAdSize.x, item.Value.defaultScaleFace.y * scaleAdSize.y, item.Value.defaultScaleFace.z);
                foreach (var ibill in listObBoard)
                {
                    if (defaultYBillBoard < -5000)
                    {
                        defaultYBillBoard = ibill.transform.localPosition.y;
                    }
                    ibill.transform.localScale = new Vector3(scaleAdSize.x, scaleAdSize.y, 1);
                    ibill.transform.localPosition = new Vector3(0, defaultYBillBoard * scaleAdSize.y, -0.8f);
                }
                if (item.Value.defaultMesh != null)
                {
                    if (!isHideDefault)
                    {
                        item.Value.defaultMesh.enabled = true;
                    }
                    else
                    {
                        item.Value.defaultMesh.gameObject.SetActive(false);
                    }
                }
            }
#if ENABLE_Google
        for (int i = 0; i < listAdInfo.Count; i++)
        {
            var pp = listAdInfo[i].placement;
            if (pp != null)
            {
                pp.ShowAd();
            }
        }
#endif
        }

        void hideAds()
        {
            //Debug.Log($"mysdk: adcv Google hideAds");
            foreach (var item in dicAds)
            {
                item.Value.mesh.transform.parent.GetComponent<MeshRenderer>().enabled = false;
                item.Value.mesh.enabled = false;
                if (item.Value.defaultMesh != null)
                {
                    item.Value.defaultMesh.enabled = false;
                }
            }
            foreach (var item in listObBoard)
            {
                item.SetActive(false);
            }
        }

        public void OnAdLoaded()
        {
            SdkUtil.logd($"adcv google OnAdLoaded");
            showAds();
        }

        public void OnAdLoadFail()
        {
            SdkUtil.logd($"adcv google OnAdLoadFail");
        }

        public void OnAdImplession()
        {
            SdkUtil.logd($"adcv google OnAdImplession");
        }

        public void OnAdClick()
        {
            SdkUtil.logd($"adcv google OnAdClick");
        }

        public override void setScaleAd(Vector2 scale)
        {
            scaleAdSize = scale;
        }

        public override void hideDefault(bool isHide)
        {
            isHideDefault = isHide;
            foreach (var item in listAdInfo)
            {
                if (item.defaultMesh != null)
                {
                    item.defaultMesh.gameObject.SetActive(!isHide);
                    if (isHide)
                    {
                        item.defaultMesh.transform.parent.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }
            if (isHide)
            {
                hideBillboards(true);
            }
        }

        public override void hideBillboards(bool isHide)
        {
            isHideBillboard = isHide;
            foreach (var item in listObBoard)
            {
                item.SetActive(!isHide);
            }
        }

        public override bool isLoaded()
        {
            return isAdLoaded;
        }

        public override void onUpdate()
        {
            //transform.position += forward * 2 * Time.deltaTime;
            if (target != null)
            {
                if (isFollowY)
                {
                    float dy = target.transform.position.y - yTagetOrigin;
                    if (dy > 1f || dy < -1f)
                    {
                        dy = target.transform.position.y - preYtaget;
                        preYtaget = target.transform.position.y;
                        yWillSet += dy;
                    }
                    if (yWillSet > 0.1f || yWillSet < -0.1f)
                    {
                        if (yWillSet > 0)
                        {
                            dy = 7.5f * Time.deltaTime;
                            if (yWillSet < dy)
                            {
                                dy = yWillSet;
                                yWillSet = 0;
                            }
                            else
                            {
                                yWillSet -= dy;
                            }
                        }
                        else
                        {
                            dy = -7.5f * Time.deltaTime;
                            if (yWillSet > dy)
                            {
                                dy = yWillSet;
                                yWillSet = 0;
                            }
                            else
                            {
                                yWillSet -= dy;
                            }
                        }
                        transform.position = new Vector3(transform.position.x, transform.position.y + dy, transform.position.z);
                    }
                }
                if (stateLoockat > 0)
                {
                    transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
                    if (stateLoockat == 2)
                    {
                        stateLoockat = 0;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (dicAds != null)
            {
                foreach (var item in dicAds)
                {
                    item.Value.mesh = null;
                }
                dicAds.Clear();
                dicAds = null;
            }
        }

#if ENABLE_Google

    IEnumerator AniDefault()
    {
        yield return new WaitForSeconds(0.1f);
        idxAniDf++;
        //bool isc = false;
        //if (idxAniDf >= obTTDefault.textures.Count)
        //{
        //    idxAniDf = 0;
        //    isc = true;
        //}
        //if (obTTDefault.textures[idxAniDf] != null)
        //{
        //    for (int i = 0; i < listAdInfo.Count; i++)
        //    {
        //    }
        //}
        //if (isc)
        //{
        //    yield return new WaitForSeconds(3.5f);
        //}
        //StartCoroutine(AniDefault());
    }

#endif

    }

    [Serializable]
    public class GoogleInfo : BaseAdInfo
    {
#if ENABLE_Google
    //public GooglePlacement placement;
    public ImmersiveInGameDisplayAd placement;
    public Vector3 loPos;
    public Vector3 loRot;
    public float loScale;
#endif
    }
}