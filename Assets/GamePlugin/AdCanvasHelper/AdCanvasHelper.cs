//#define ENABLE_ADCANVAS
//#define ENABLE_ADCANVASTest
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using mygame.sdk;

namespace mygame.sdk
{

    public delegate void AdCvCallBack(int statusLoad, BaseAdCanvasObject newAd, BaseAdCanvasObject oldAd);

    public class AdCanvasHelper : MonoBehaviour
    {
        public static AdCanvasHelper Instance = null;
        public static int cf_adcanvas_alshow = 1;

        public Camera mainCam;
        public bool useMyDefault = true;
        public Texture2D textureDefault;
        public List<BaseAdCanvas> listAdRes;

        public Dictionary<int, BaseAdCanvas> listAdUse { get; set; }
        List<int> ListTypeAdUse = new List<int>();
        List<int> listVideo = new List<int>();
        int idxListUse = 0;
        bool isDeviceStrong = false;
        int idxVideo = 0;

#if ENABLE_ADCANVASTest
    public BaseAdCanvas AdcanvasTest;
#endif

        public bool isEnableClick { get; private set; }
        public float lenghtRayClick { get; private set; }
        public string nameUiObIgnore { get; private set; }

        public Dictionary<AdCanvasSize, TTWithTypeAdcanvas> dicDefault = new Dictionary<AdCanvasSize, TTWithTypeAdcanvas>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                listAdUse = new Dictionary<int, BaseAdCanvas>();
#if ENABLE_ADCANVASTest
            var adtest = Instantiate(AdcanvasTest, Vector3.zero, Quaternion.identity, transform);
            listAdUse.Add((int)adtest.TypeAdCanvas, adtest);
            foreach (var ad in listAdRes)
            {
                ad.onAwake();
            }
#else
                foreach (var ad in listAdRes)
                {
                    listAdUse.Add((int)ad.TypeAdCanvas, ad);
                }
#endif

                configDefault();
                if (textureDefault != null)
                {
                    textureDefault.name = "adcanvasttdefault";
                }

                foreach (var ad in listAdUse)
                {
                    ad.Value.onAwake();
                }
                GameAdsHelperBridge.CBRequestGDPR += onShowCmp;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != Instance) Destroy(gameObject);
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            string cfadcanvas = PlayerPrefs.GetString("cf_list_adcanvas", "0,1,0");
            initListAdcanvas(cfadcanvas);
            isDeviceStrong = SdkUtil.isDevicesStrong();
            string slvdeo = PlayerPrefs.GetString("cf_adcanvas_video", "0,1");
            initListVideo(slvdeo);
            foreach (var ad in listAdUse)
            {
                ad.Value.onStart();
            }
        }

        // Update is called once per frame
        void Update()
        {
#if ENABLE_ADCANVAS
            foreach (var ad in listAdUse)
            {
                ad.Value.onUpdate();
            }
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
#if ENABLE_ADCANVAS
                foreach (var ad in listAdUse)
                {
                    ad.Value.onDestroyAd();
                }
#endif
            }
        }

        void addRes2Use()
        {
            ListTypeAdUse.Clear();
            foreach (var ad in listAdRes)
            {
                ListTypeAdUse.Add((int)ad.TypeAdCanvas);
            }
        }

        public void initListAdcanvas(string datalist)
        {
            //datalist = "0,1,6";
            SdkUtil.logd($"adcv initListAdcanvas={datalist}");
            ListTypeAdUse.Clear();
            if (PlayerPrefs.GetInt("cf_adcanvas_enable", 1) != 1)
            {
                return;
            }
            if (datalist == null || datalist.Length == 0)
            {
                addRes2Use();
            }
            else
            {
                string[] arrtype = datalist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrtype != null && arrtype.Length > 0)
                {
                    for (int i = 0; i < arrtype.Length; i++)
                    {
                        int type = -1;
                        if (int.TryParse(arrtype[i], out type))
                        {
                            if (listAdUse.ContainsKey(type))
                            {
                                ListTypeAdUse.Add(type);
                            }
                        }
                    }
                    if (datalist == null || datalist.Length == 0)
                    {
                        addRes2Use();
                    }
                }
                else
                {
                    addRes2Use();
                }
            }
        }

        public void initListVideo(string datalist)
        {
            listVideo.Clear();
            string[] arrtype = datalist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (arrtype != null && arrtype.Length > 0)
            {
                for (int i = 0; i < arrtype.Length; i++)
                {
                    int type = -1;
                    if (int.TryParse(arrtype[i], out type))
                    {
                        listVideo.Add(type);
                    }
                }
            }
        }

        public void onShowCmp(int state, string des)
        {
            if (state == 0)
            {
                onShowCmpNative();
            }
            else if (state == 1)
            {
                if (des != null && des.Length > 5)
                {
                    onCMPOK(des);
                }
            }
        }

        public void onShowCmpNative()
        {
            foreach (var ad in listAdUse)
            {
                ad.Value.onShowCmpNative();
            }
        }

        public void onCMPOK(string iABTCv2String)
        {
            PlayerPrefs.SetString("mem_iab_tcv2", iABTCv2String);
            foreach (var ad in listAdUse)
            {
                ad.Value.onCMPOK(iABTCv2String);
            }
        }

        void _onChangeCamera(Camera newCamera)
        {
            mainCam = newCamera;
            foreach (var ad in listAdUse)
            {
                ad.Value.onChangeCamera(newCamera);
            }
        }

        BaseAdCanvasObject _genAd(AdCanvasSize type, string placement, Vector3 pos, bool isDisableObj, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false, int typeExcluse = -1)
        {
            if (ListTypeAdUse.Count > 0 && PlayerPrefs.GetInt("cf_adcanvas_enable", 1) == 1)
            {
                if (idxListUse >= ListTypeAdUse.Count)
                {
                    idxListUse = 0;
                }

                BaseAdCanvasObject re = null;
                int n = listAdUse.Count;
                if (idxVideo >= listVideo.Count)
                {
                    idxVideo = 0;
                }
                while (re == null && n > 0)
                {
                    if (!listAdUse.ContainsKey(typeExcluse))
                    {
                        if (type.Equals(AdCanvasSize.Size6x5)
                        && (listAdUse[ListTypeAdUse[idxListUse]].TypeAdCanvas.Equals(AdCanvasType.GadsMe)
                        || listAdUse[ListTypeAdUse[idxListUse]].TypeAdCanvas.Equals(AdCanvasType.AdVerty)
                        || listAdUse[ListTypeAdUse[idxListUse]].TypeAdCanvas.Equals(AdCanvasType.AdInMo)
                        ))
                        {
                            int typevd = 0;
                            if (isDeviceStrong && listVideo.Count > 0)
                            {
                                typevd = listVideo[idxVideo];
                                idxVideo++;
                                if (idxVideo >= listVideo.Count)
                                {
                                    idxVideo = 0;
                                }
                            }
                            if (typevd == 1)
                            {
                                re = listAdUse[ListTypeAdUse[idxListUse]].genAd(AdCanvasSize.Size4x3Video, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY);
                            }
                            else
                            {
                                re = listAdUse[ListTypeAdUse[idxListUse]].genAd(type, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY);
                            }
                        }
                        else
                        {
                            re = listAdUse[ListTypeAdUse[idxListUse]].genAd(type, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY);
                        }
                    }

                    idxListUse++;
                    n--;
                    if (idxListUse >= ListTypeAdUse.Count)
                    {
                        idxListUse = 0;
                    }
                }
                return re;
            }
            else
            {
                return null;
            }
        }

        BaseAdCanvasObject _genAdCircle(int countLoad, int idxgen, int idxVd, AdCanvasSize type, string placement, Vector3 pos, bool isDisableObj, Vector3 forward, AdCvCallBack cbcv, Transform _target = null, int stateLookat = 0, bool isFloowY = false, int typeExcluse = -1)
        {
            SdkUtil.logd($"adcv _genAdCircle countLoad={countLoad} idxgen={idxgen} idxVd={idxVd}");
            if (idxgen >= ListTypeAdUse.Count)
            {
                idxgen = 0;
            }
            BaseAdCanvasObject re = null;
            int n = listAdUse.Count;
            if (idxVd >= listVideo.Count)
            {
                idxVd = 0;
            }
            while (re == null && n > 0)
            {
                if (!listAdUse.ContainsKey(typeExcluse))
                {
                    if (type.Equals(AdCanvasSize.Size6x5)
                        && (listAdUse[ListTypeAdUse[idxgen]].TypeAdCanvas.Equals(AdCanvasType.GadsMe)
                        || listAdUse[ListTypeAdUse[idxgen]].TypeAdCanvas.Equals(AdCanvasType.AdVerty)
                        || listAdUse[ListTypeAdUse[idxgen]].TypeAdCanvas.Equals(AdCanvasType.AdInMo)
                        ))
                    {
                        int typevd = 0;
                        if (isDeviceStrong && listVideo.Count > 0)
                        {
                            typevd = listVideo[idxVd];
                            idxVd++;
                            if (idxVd >= listVideo.Count)
                            {
                                idxVd = 0;
                            }
                        }
                        if (typevd == 1)
                        {
                            re = listAdUse[ListTypeAdUse[idxgen]].genAd(AdCanvasSize.Size4x3Video, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY);
                        }
                        else
                        {
                            re = listAdUse[ListTypeAdUse[idxgen]].genAd(type, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY);
                        }
                    }
                    else
                    {
                        re = listAdUse[ListTypeAdUse[idxgen]].genAd(type, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY);
                    }

                    void _cbAdCv(int statusLoad, BaseAdCanvasObject newAd, BaseAdCanvasObject oldAd)
                    {
                        SdkUtil.logd($"adcv cbAdCv statusLoad={statusLoad} countLoad={countLoad}");
                        if (statusLoad == 0 && countLoad > 0)
                        {
                            SdkUtil.logd($"adcv cbAdCv load fail and next load idxgen={idxgen} idxVd={idxVd}");
                            var adnew = _genAdCircle(countLoad, idxgen, idxVd, type, placement, pos, isDisableObj, forward, _cbAdCv, _target, stateLookat, isFloowY, typeExcluse);
                            if (cbcv != null)
                            {
                                cbcv(statusLoad, adnew, re);
                            }
                        }
                    }
                }
                idxgen++;
                countLoad--;
                n--;
                if (idxgen >= ListTypeAdUse.Count)
                {
                    idxgen = 0;
                }
            }

            return re;
        }

        void _initClick(bool isClick, float lenghtRay, string nameUiIgnore = "")
        {
            SdkUtil.logd($"adcv initClick isClick={isClick} lenghtRay={lenghtRay}");
            int tclick = PlayerPrefs.GetInt("cf_adcanvas_click", 0xFF);
            if (tclick <= 0)
            {
                isEnableClick = false;
                foreach (var ad in listAdUse)
                {
                    ad.Value.initClick(false);
                }
                return;
            }
            if (mygame.sdk.AdsHelper.isRemoveAds(0))
            {
                // isClick = false;
            }
            SdkUtil.logd($"adcv initClick 2 isClick={isClick} lenghtRay={lenghtRay}");
            if (!isClick)
            {
                isEnableClick = false;
                lenghtRayClick = lenghtRay;
                foreach (var ad in listAdUse)
                {
                    ad.Value.initClick(false);
                }
            }
            else
            {
                isEnableClick = true;
                lenghtRayClick = lenghtRay;
                nameUiObIgnore = nameUiIgnore;
                foreach (var ad in listAdUse)
                {
                    ad.Value.initClick(true);
                }
            }
        }

        void _freeAd(BaseAdCanvasObject adrm)
        {
            foreach (var ad in listAdUse)
            {
                if (ad.Value.TypeAdCanvas == adrm.adCanvasHelper.TypeAdCanvas)
                {
                    ad.Value.freeAd(adrm);
                    break;
                }
            }
        }

        void _freeAll()
        {
            foreach (var ad in listAdUse)
            {
                ad.Value.freeAll();
            }
        }

        public static void OnChangeCamera(Camera newCamera)
        {
            if (Instance != null)
            {
                Instance._onChangeCamera(newCamera);
            }
        }

        public static BaseAdCanvasObject GenAd(AdCanvasSize type, string placement, Vector3 pos, bool isDisableObj, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false, int typeExcluse = -1)
        {
            if (Instance != null)
            {
                return Instance._genAd(type, placement, pos, isDisableObj, forward, _target, stateLookat, isFloowY, typeExcluse);
            }
            else
            {
                return null;
            }
        }

        public static void InitClick(bool isClick, float lenghtRay, string nameUiIgnore = "")
        {
            if (Instance != null)
            {
                Instance._initClick(isClick, lenghtRay, nameUiIgnore);
            }
        }

        public static void FreeAd(BaseAdCanvasObject adrm)
        {
            if (Instance != null)
            {
                Instance._freeAd(adrm);
            }
        }

        public static void FreeAll()
        {
            if (Instance != null)
            {
                Instance._freeAll();
            }
        }
        //-------------------------------
        public void configDefault()
        {
            dicDefault.Clear();
            string stringcf = PlayerPrefs.GetString("adcanvas_v1_6x5", "");
            if (stringcf.Length > 0)
            {
                SdkUtil.logd($"adcv adcanvas_v1_6x5=" + stringcf);
                string[] arrtype = stringcf.Split('#');
                TTWithTypeAdcanvas obType = new TTWithTypeAdcanvas();
                for (int i = 0; i < arrtype.Length; i++)
                {
                    string[] arrad = arrtype[i].Split(';');
                    if (arrad.Length > 0)
                    {
                        TTAdCanvas dd = new TTAdCanvas();
                        obType.list.Add(dd);
                        string[] imgs = arrad[0].Split(',');
                        for (int j = 0; j < imgs.Length; j++)
                        {
                            dd.urlImgs.Add(imgs[j]);
                        }
                        if (arrad.Length == 2)
                        {
                            dd.urlStore = arrad[1];
                        }
                    }
                }
                obType.idx = PlayerPrefs.GetInt("adcanvas_6x5_idx", 0);
                if (obType.idx >= obType.list.Count)
                {
                    obType.idx = 0;
                }
                dicDefault.Add(AdCanvasSize.Size6x5, obType);
            }
            if (dicDefault != null && dicDefault.Count > 0)
            {
                cf_adcanvas_alshow = PlayerPrefs.GetInt("cf_adcanvas_alshow", 1);
            }
            else
            {
                cf_adcanvas_alshow = 0;
            }
        }
        public TTAdCanvas getTTDefault(BaseAdCanvas adcvhelper, AdCanvasSize type, Action<TTAdCanvas, Texture2D> cb = null)
        {
            SdkUtil.logd($"adcv getTTDefault");
            TTAdCanvas re = null;
            if (dicDefault.ContainsKey(type))
            {
                TTWithTypeAdcanvas ob = dicDefault[type];
                if (ob.list.Count > 0)
                {
                    if (ob.idx >= ob.list.Count)
                    {
                        ob.idx = 0;
                    }
                    SdkUtil.logd($"adcv getTTDefault idx=" + ob.idx);
                    re = ob.list[ob.idx];
                    if (ob.list[ob.idx].textures != null && ob.list[ob.idx].textures.Count > 0 && ob.list[ob.idx].textures[0] != null)
                    {
                        cb?.Invoke(ob.list[ob.idx], ob.list[ob.idx].textures[0]);
                    }
                    else
                    {
                        int idx = ob.idx;
                        ImageLoader.loadImageTexture(ob.list[idx].urlImgs[0], 60, 50, (tt) =>
                        {
                            tt.name = "adcanvasttdefault";
                            if (ob.list[idx].textures.Count > 0)
                            {
                                ob.list[idx].textures[0] = tt;
                            }
                            else
                            {
                                ob.list[idx].textures.Add(tt);
                            }
                            cb?.Invoke(ob.list[idx], tt);
                        });
                    }
                    ob.idx++;
                    if (ob.idx >= ob.list.Count)
                    {
                        ob.idx = 0;
                    }
                    if (type == AdCanvasSize.Size6x5)
                    {
                        PlayerPrefs.SetInt("adcanvas_6x5_idx", ob.idx);
                    }
                }
            }
            return re;
        }
    }

    public class TTWithTypeAdcanvas
    {
        public int idx = 0;
        public List<TTAdCanvas> list = new List<TTAdCanvas>();
    }

    public class TTAdCanvas
    {
        public List<string> urlImgs = new List<string>();
        public string urlStore = "";
        public List<Texture2D> textures = new List<Texture2D>();
    }
}
