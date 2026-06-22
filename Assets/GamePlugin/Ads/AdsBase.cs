//#define TEST_ADS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace mygame.sdk
{
    public abstract class AdsBase : MonoBehaviour
    {
        public AdsHelper advhelper;

        public const string PLOpenDefault = "openad_default";
        public const string PLFullSplash = "full_splash";
        public const string PLFullResume = "full_resume";

        public const string PLBnDefault = "bn_default";
        public const string PLClDefault = "cl_default";
        public const string PLRectDefault = "rect_default";
        public const string PLNtDefault = "nt_default";
        public const string PLRectNtDefault = "rectnt_default";
        public const string PLFullDefault = "full_default";
        public const string PLFull2Default = "full_2_default";
        public const string PLGiftDefault = "gift_default";

        protected int toTryLoad = 1;
        public int adsType = 0;

        [Header("Android")]
        public string android_app_id = "";
        public string android_bn_id = "";
        public string android_bncl_id = "";
        public string android_native_id = "";
        public string android_native_full_id = "";
        public string android_full_id = "";
        public string android_gift_id = "";

        [Header("iOS")]
        public string ios_app_id = "";
        public string ios_bn_id = "";
        public string ios_bncl_id = "";
        public string ios_native_id = "";
        public string ios_native_full_id = "";
        public string ios_full_id = "";
        public string ios_gift_id = "";
#if UNITY_IOS || UNITY_IPHONE
        [Header("iOS China")]
        public string ios_bn_cn_id = "";
        public string ios_bncl_cn_id = "";
        public string ios_full_cn_id = "";
        public string ios_gift_cn_id = "";
#endif

        protected string appId = "";
        protected string bannerId = "";
        protected string bannerCollapseId = "";
        protected string nativeId = "";
        protected string nativeFullId = "";
        protected string fullId = "";
        protected string giftId = "";
        public bool isEnable { get; set; }

        protected long timeLoadBN;
        protected long timeLoadBNCollaspe;
        protected long timeLoadBNRect;
        protected int bnWidth;
        protected int bnClWidth;
        protected float bnRectWidth;

        protected float rectntWith;
        protected float rectntHeight;
        protected float rectntdx;
        protected float rectntdy;

        protected bool isOpenAd2 = false;
        protected bool isFullNt2 = false;
        protected bool isFull2 = false;

        protected Dictionary<string, AdPlacementFull> dicPLOpenAd = new Dictionary<string, AdPlacementFull>();

        protected Dictionary<string, AdPlacementBanner> dicPLBanner = new Dictionary<string, AdPlacementBanner>();
        protected Dictionary<string, AdPlacementBanner> dicPLCl = new Dictionary<string, AdPlacementBanner>();
        protected Dictionary<string, AdPlacementBanner> dicPLRect = new Dictionary<string, AdPlacementBanner>();

        protected Dictionary<string, AdPlacementNative> dicPLNative = new Dictionary<string, AdPlacementNative>();
        protected Dictionary<string, AdPlacementNative> dicPLBnNt = new Dictionary<string, AdPlacementNative>();
        protected Dictionary<string, AdPlacementNative> dicPLNativeCl = new Dictionary<string, AdPlacementNative>();
        protected Dictionary<string, AdPlacementNative> dicPLRectNt = new Dictionary<string, AdPlacementNative>();

        protected Dictionary<string, AdPlacementFull> dicPLNtFull = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLNtIcFull = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLNtPgFull = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLFull = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLFullRwInter = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLFullRwRw = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLGift = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLNtGift = new Dictionary<string, AdPlacementFull>();
        protected Dictionary<string, AdPlacementFull> dicPLGiftFull = new Dictionary<string, AdPlacementFull>();

        public abstract string getname();

        protected virtual void tryLoadOpenAd(AdPlacementFull adpl) { }
        public virtual void loadOpenAd(string placement, AdCallBack cb) { if (cb != null) { cb(AD_State.AD_LOAD_FAIL); } }
        public virtual bool showOpenAd(string placement, float timeDelay, bool isShow2, bool isDelay, AdCallBack cb) { return false; }

        protected abstract void tryLoadBanner(AdPlacementBanner adpl);
        public abstract void loadBanner(string placement, AdCallBack cb);
        public abstract bool showBanner(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter, bool highP = false);
        public abstract void hideBanner();
        public abstract void destroyBanner();

        protected virtual void tryLoadCollapseBanner(AdPlacementBanner adpl) { }
        public virtual void loadCollapseBanner(string placement, AdCallBack cb) { }
        public virtual bool showCollapseBanner(string placement, int pos, int width, int maxH, float dxCenter, AdCallBack cb)
        {
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
        }
        public virtual void hideCollapseBanner() { }
        public virtual void destroyCollapseBanner() { }

        protected virtual void tryLoadRectBanner(AdPlacementBanner adpl) { }
        public virtual void loadRectBanner(string placement, AdCallBack cb) { }
        public virtual bool showRectBanner(string placement, int pos, float width, int maxH, float dxCenter, float dyVertical, AdCallBack cb)
        {
            if (cb != null)
            {
                cb(AD_State.AD_LOAD_FAIL);
            }
            return false;
        }
        public virtual void hideRectBanner() { }
        public virtual void destroyRectBanner() { }

        protected virtual void tryLoadBnNt(AdPlacementNative adpl) { }
        public virtual bool showBnNt(string placement, int pos, int width, int maxH, AdCallBack cb, float dxCenter) { return false; }
        public virtual void loadBnNt(string placement, AdCallBack cb) { }
        public virtual void hideBnNt() { }
        public virtual void destroyBnNt() { }

        //Rect Native
        protected virtual void tryLoadRectNt(AdPlacementNative adpl) { }
        public virtual bool showRectNt(string placement, int pos, int orien, float width, float height, float dx, float dy, AdCallBack cb) { return false; }
        public virtual void loadRectNt(string placement, AdCallBack cb) { }
        public virtual void hideRectNt() { }

        //native
        protected virtual void tryLoadNative(AdPlacementNative adpl, int idxId, AdsNativeObject ad, AdCallBack cb) { }
        public virtual void loadNative(string placement, AdsNativeObject ad, AdCallBack cb) { }
        public virtual bool showNative(string placement, AdsNativeObject ad, bool isRefresh, AdCallBack cb) { return false; }
        public virtual void freeNative(AdsNativeObject adsNative) { }
        public virtual void freeAllNative() { }

        //native collapse
        protected virtual void tryLoadNtCl(AdPlacementNative adpl, AdCallBack cb) { }
        public virtual void loadNtCl(string placement, AdCallBack cb) { }
        public virtual bool showNtCl(string placement, int pos, int width, float dxCenter, bool isHideBtClose, AdCallBack cb) { return false; }
        public virtual void hideNtCl() { }

        //native full
        protected virtual void tryLoadNativeFull(AdPlacementFull adpl) { }
        public virtual void loadNativeFull(string placement, AdCallBack cb) { }
        public virtual bool showNativeFull(string placement, float timeDelay, int timeNtDl, bool isHideBtClose, bool isShow2, int timeClose, bool isAutoCloseWhenClick, AdCallBack cb) { return false; }

        //native Ic full
        protected virtual void tryLoadNativeIcFull(AdPlacementFull adpl) { }
        public virtual void loadNativeIcFull(string placement, AdCallBack cb) { }
        public virtual bool showNativeIcFull(string placement, float timeDelay, int timeNtDl, bool isHideBtClose, bool isShow2, int timeClose, bool isAutoCloseWhenClick, AdCallBack cb) { return false; }

        //NtGift
        protected virtual void tryLoadNtPgFull(AdPlacementFull adpl, int adPos) { }
        public virtual void loadNtPgFull(string placement, AdCallBack cb) { }
        public virtual bool showNtPgFull(string placement, float timeDelay, bool isHideBtClose, AdCallBack cb) { return false; }

        //full
        protected string fullAdNetwork;
        protected long timeLoadFull;
        public int adsFullSubType { get; protected set; }
        public virtual string getFullNetName() { return fullAdNetwork; }
        public abstract void clearCurrFull(string placement);
        protected abstract void tryLoadFull(AdPlacementFull adpl);
        public abstract void loadFull(string placement, AdCallBack cb);
        public abstract bool showFull(string placement, float timeDelay, bool isShow2, AdCallBack cb);

        //full Rw Inter
        protected string fullRwInterAdNetwork;
        protected long timeLoadFullRwInter;
        public virtual string getFullRwInterNetName() { return fullRwInterAdNetwork; }
        protected virtual void tryLoadFullRwInter(AdPlacementFull adpl) { }
        public virtual void loadFullRwInter(string placement, AdCallBack cb) { }
        public virtual bool showFullRwInter(string placement, float timeDelay, bool isShow2, AdCallBack cb) { return false; }

        //full Rw Rw
        protected string fullRwRwAdNetwork;
        protected long timeLoadFullRwRw;
        public virtual string getFullRwRwNetName() { return fullRwRwAdNetwork; }
        protected virtual void tryLoadFullRwRw(AdPlacementFull adpl) { }
        public virtual void loadFullRwRw(string placement, AdCallBack cb) { }
        public virtual bool showFullRwRw(string placement, float timeDelay, bool isShow2, AdCallBack cb) { return false; }

        //Gift
        protected string giftAdNetwork;
        protected long timeLoadGift;
        protected int tLoadGiftErr = 15;
        protected bool isRewardCom;
        public virtual string getGiftNetName() { return giftAdNetwork; }
        public abstract void clearCurrGift(string placement);
        protected abstract void tryloadGift(AdPlacementFull adpl);
        public abstract void loadGift(string placement, AdCallBack cb);
        public abstract bool showGift(string placement, float timeDelay, AdCallBack cb);

        //NtGift
        protected virtual void tryLoadNtGift(AdPlacementFull adpl, int adPos) { }
        public virtual void loadNtGift(string placement, AdCallBack cb) { }
        public virtual bool showNtGift(string placement, float timeDelay, bool isHideBtClose, AdCallBack cb) { return false; }

        //gift full
        protected virtual void tryLoadGiftFull(AdPlacementFull adpl) { }
        public virtual void loadGiftFull(string placement, AdCallBack cb) { }
        public virtual bool showGiftFull(string placement, float timeDelay, bool isShow2, AdCallBack cb) { return false; }

        //==========================
        public abstract void InitAds();
        public abstract void AdsAwake();

        private void Awake()
        {
#if UNITY_ANDROID
            appId = android_app_id;
            bannerId = PlayerPrefs.GetString($"mem_df{adsType}_bn_id", android_bn_id);
            bannerCollapseId = android_bncl_id;
            nativeId = android_native_id;
            nativeFullId = android_native_full_id;
            fullId = PlayerPrefs.GetString($"mem_df{adsType}_full_id", android_full_id);
            giftId = PlayerPrefs.GetString($"mem_df{adsType}_gift_id", android_gift_id);
#elif UNITY_IOS || UNITY_IPHONE
            appId = ios_app_id;
            bannerId = PlayerPrefs.GetString($"mem_df{adsType}_bn_id", ios_bn_id);
            bannerCollapseId = ios_bncl_id;
            nativeId = ios_native_id;
            nativeFullId = ios_native_full_id;
            fullId = PlayerPrefs.GetString($"mem_df{adsType}_full_id", ios_full_id);
            giftId = PlayerPrefs.GetString($"mem_df{adsType}_gift_id", ios_gift_id);
#endif
            timeLoadBN = 0;

            timeLoadFull = 0;

            timeLoadGift = 0;

            isEnable = false;
            adsFullSubType = adsType;

            AdsAwake();
        }

        public void setBannerId(string adid)
        {
            if (adid != null && adid.Length > 3)
            {
                bannerId = adid;
                PlayerPrefs.SetString($"mem_df{adsType}_bn_id", adid);
            }
        }
        public string getFullId()
        {
            return fullId;
        }
        public void setFullId(string adid)
        {
            if (adid != null && adid.Length > 3)
            {
                fullId = adid;
                PlayerPrefs.SetString($"mem_df{adsType}_full_id", adid);
            }
        }
        public string getGiftId()
        {
            return giftId;
        }
        public void setGiftId(string adid)
        {
            if (adid != null && adid.Length > 3)
            {
                fullId = adid;
                PlayerPrefs.SetString($"mem_df{adsType}_gift_id", adid);
            }
        }

        public AdPlacementFull getPlOpenAd(string placement)
        {
            string gp = placement;
            if (placement.StartsWith("full_"))
            {
                gp = placement.Replace("full_", "openad_");
            }
            else if (!placement.StartsWith("openad_"))
            {
                gp = "openad_" + placement;
            }
            AdPlacementFull adpl = null;
            if (dicPLOpenAd.ContainsKey(placement))
            {
                adpl = dicPLOpenAd[placement];
            }
            else if (dicPLOpenAd.ContainsKey(PLOpenDefault))
            {
                adpl = dicPLOpenAd[PLOpenDefault];
            }
            return adpl;
        }
        public virtual int getOpenAdLoaded(string placement)
        {
            AdPlacementFull adpl = getPlOpenAd(placement);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        public AdPlacementBanner getPlBanner(string placement, int type)
        {
            Dictionary<string, AdPlacementBanner> dic;
            string pldf;
            if (type == 0)
            {
                dic = dicPLBanner;
                pldf = PLBnDefault;
            }
            else if (type == 1)
            {
                dic = dicPLCl;
                pldf = PLClDefault;
            }
            else
            {
                dic = dicPLRect;
                pldf = PLRectDefault;
            }
            AdPlacementBanner adpl = null;
            if (dic.ContainsKey(placement))
            {
                adpl = dic[placement];
            }
            else if (dic.ContainsKey(pldf))
            {
                adpl = dic[pldf];
            }
            return adpl;
        }
        public AdPlacementNative getPlBnNt(string placement)
        {
            AdPlacementNative adpl = null;
            if (dicPLBnNt.ContainsKey(placement))
            {
                adpl = dicPLBnNt[placement];
            }
            else if (dicPLBnNt.ContainsKey(PLBnDefault))
            {
                adpl = dicPLBnNt[PLBnDefault];
            }
            return adpl;
        }
        public AdPlacementNative getPlNt(string placement)
        {
            AdPlacementNative adpl = null;
            if (dicPLNative.ContainsKey(placement))
            {
                adpl = dicPLNative[placement];
            }
            else if (dicPLNative.ContainsKey(PLNtDefault))
            {
                adpl = dicPLNative[PLNtDefault];
            }
            return adpl;
        }
        public AdPlacementNative getPlNtCl(string placement)
        {
            AdPlacementNative adpl = null;
            if (dicPLNativeCl.ContainsKey(placement))
            {
                adpl = dicPLNativeCl[placement];
            }
            else if (dicPLNativeCl.ContainsKey(PLClDefault))
            {
                adpl = dicPLNativeCl[PLClDefault];
            }
            return adpl;
        }

        public AdPlacementNative getPlRectNt(string placement)
        {
            AdPlacementNative adpl = null;
            if (dicPLRectNt.ContainsKey(placement))
            {
                adpl = dicPLRectNt[placement];
            }
            else if (dicPLRectNt.ContainsKey(PLRectNtDefault))
            {
                adpl = dicPLRectNt[PLRectNtDefault];
            }
            return adpl;
        }
        public AdPlacementFull getPlNtFull(string placement, bool is4Show)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            AdPlacementFull adpl = null;
            if (dicPLNtFull.ContainsKey(placement))
            {
                adpl = dicPLNtFull[placement];
            }
            else
            {
                if (placement.StartsWith("full_2_"))
                {
                    if (dicPLNtFull.ContainsKey(PLFull2Default))
                    {
                        adpl = dicPLNtFull[PLFull2Default];
                    }
                    else if (dicPLNtFull.ContainsKey(PLFullDefault))
                    {
                        adpl = dicPLNtFull[PLFullDefault];
                    }
                }
                else if (dicPLNtFull.ContainsKey(PLFullDefault))
                {
                    adpl = dicPLNtFull[PLFullDefault];
                }
            }
            if (is4Show && (adpl == null || !adpl.isloaded))
            {
                foreach (var dicpl in dicPLNtFull)
                {
                    if (dicpl.Value.isloaded)
                    {
                        adpl = dicpl.Value;
                        SdkUtil.logd($"ads full {placement}-{adpl.placement} adbase getPlNtFull for isshow");
                    }
                }
            }
            return adpl;
        }
        public virtual int getNativeFullLoaded(string placement)
        {
            AdPlacementFull adpl = getPlNtFull(placement, true);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        public AdPlacementFull getPlNtIcFull(string placement, bool is4Show)
        {
            if (!placement.StartsWith("full_"))
            {
                placement = "full_" + placement;
            }
            AdPlacementFull adpl = null;
            if (dicPLNtIcFull.ContainsKey(placement))
            {
                adpl = dicPLNtIcFull[placement];
            }
            else
            {
                if (placement.StartsWith("full_2_"))
                {
                    if (dicPLNtIcFull.ContainsKey(PLFull2Default))
                    {
                        adpl = dicPLNtIcFull[PLFull2Default];
                    }
                    else if (dicPLNtIcFull.ContainsKey(PLFullDefault))
                    {
                        adpl = dicPLNtIcFull[PLFullDefault];
                    }
                }
                else if (dicPLNtIcFull.ContainsKey(PLFullDefault))
                {
                    adpl = dicPLNtIcFull[PLFullDefault];
                }
            }
            if (is4Show && (adpl == null || !adpl.isloaded))
            {
                foreach (var dicpl in dicPLNtIcFull)
                {
                    if (dicpl.Value.isloaded)
                    {
                        adpl = dicpl.Value;
                        SdkUtil.logd($"ads full {placement}-{adpl.placement} adbase getPlNtFull for isshow");
                    }
                }
            }
            return adpl;
        }
        public virtual int getNativeIcFullLoaded(string placement)
        {
            AdPlacementFull adpl = getPlNtIcFull(placement, true);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }

        public AdPlacementFull getPlNtPgFull(string placement)
        {
            AdPlacementFull adpl = null;
            if (dicPLNtPgFull.ContainsKey(placement))
            {
                adpl = dicPLNtPgFull[placement];
            }
            else if (dicPLNtPgFull.ContainsKey(PLFullDefault))
            {
                adpl = dicPLNtPgFull[PLFullDefault];
            }
            return adpl;
        }
        public virtual int getNtPgFullLoaded(string placement)
        {
            AdPlacementFull adpl = getPlNtPgFull(placement);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }

        public AdPlacementFull getPlFull(string placement, bool is4Show)
        {
            AdPlacementFull adpl = null;
            if (dicPLFull.ContainsKey(placement))
            {
                adpl = dicPLFull[placement];
            }
            else
            {
                if (placement.StartsWith("full_2_"))
                {
                    if (dicPLFull.ContainsKey(PLFull2Default))
                    {
                        adpl = dicPLFull[PLFull2Default];
                    }
                    else if (dicPLFull.ContainsKey(PLFullDefault))
                    {
                        adpl = dicPLFull[PLFullDefault];
                    }
                }
                else if (dicPLFull.ContainsKey(PLFullDefault))
                {
                    adpl = dicPLFull[PLFullDefault];
                }
            }
            if (is4Show && (adpl == null || !adpl.isloaded))
            {
                foreach (var dicpl in dicPLFull)
                {
                    if (dicpl.Value.isloaded)
                    {
                        adpl = dicpl.Value;
                        SdkUtil.logd($"ads full {placement}-{adpl.placement} adbase getPlFull for isshow");
                    }
                }
            }
            return adpl;
        }
        public virtual int getFullLoaded(string placement)
        {
            AdPlacementFull adpl = getPlFull(placement, true);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        public AdPlacementFull getPlFullRwInter(string placement, bool is4Show)
        {
            AdPlacementFull adpl = null;
            if (dicPLFullRwInter.ContainsKey(placement))
            {
                adpl = dicPLFullRwInter[placement];
            }
            else
            {
                if (placement.StartsWith("full_2_"))
                {
                    if (dicPLFullRwInter.ContainsKey(PLFull2Default))
                    {
                        adpl = dicPLFullRwInter[PLFull2Default];
                    }
                    else if (dicPLFullRwInter.ContainsKey(PLFullDefault))
                    {
                        adpl = dicPLFullRwInter[PLFullDefault];
                    }
                }
                else if (dicPLFullRwInter.ContainsKey(PLFullDefault))
                {
                    adpl = dicPLFullRwInter[PLFullDefault];
                }
            }
            if (is4Show && (adpl == null || !adpl.isloaded))
            {
                foreach (var dicpl in dicPLFullRwInter)
                {
                    if (dicpl.Value.isloaded)
                    {
                        adpl = dicpl.Value;
                        SdkUtil.logd($"ads full RwInter {placement}-{adpl.placement} adbase getPlFullRwInter for isshow");
                    }
                }
            }
            return adpl;
        }
        public virtual int getFullRwInterLoaded(string placement)
        {
            AdPlacementFull adpl = getPlFullRwInter(placement, true);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        public AdPlacementFull getPlFullRwRw(string placement, bool is4Show)
        {
            AdPlacementFull adpl = null;
            if (dicPLFullRwRw.ContainsKey(placement))
            {
                adpl = dicPLFullRwRw[placement];
            }
            else
            {
                if (placement.StartsWith("full_2_"))
                {
                    if (dicPLFullRwRw.ContainsKey(PLFull2Default))
                    {
                        adpl = dicPLFullRwRw[PLFull2Default];
                    }
                    else if (dicPLFullRwRw.ContainsKey(PLFullDefault))
                    {
                        adpl = dicPLFullRwRw[PLFullDefault];
                    }
                }
                else if (dicPLFullRwRw.ContainsKey(PLFullDefault))
                {
                    adpl = dicPLFullRwRw[PLFullDefault];
                }
            }
            if (is4Show && (adpl == null || !adpl.isloaded))
            {
                foreach (var dicpl in dicPLFullRwRw)
                {
                    if (dicpl.Value.isloaded)
                    {
                        adpl = dicpl.Value;
                        SdkUtil.logd($"ads full RwRw {placement}-{adpl.placement} adbase getPlFullRwRw for isshow");
                    }
                }
            }
            return adpl;
        }
        public virtual int getFullRwRwLoaded(string placement)
        {
            AdPlacementFull adpl = getPlFullRwRw(placement, true);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        public AdPlacementFull getPlGift(string placement)
        {
            AdPlacementFull adpl = null;
            if (dicPLGift.ContainsKey(placement))
            {
                adpl = dicPLGift[placement];
            }
            else if (dicPLGift.ContainsKey(PLGiftDefault))
            {
                adpl = dicPLGift[PLGiftDefault];
            }
            return adpl;
        }
        public virtual int getGiftLoaded(string placement)
        {
            AdPlacementFull adpl = getPlGift(placement);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        //
        public AdPlacementFull getPlNtGift(string placement)
        {
            AdPlacementFull adpl = null;
            if (dicPLNtGift.ContainsKey(placement))
            {
                adpl = dicPLNtGift[placement];
            }
            else if (dicPLNtGift.ContainsKey(PLGiftDefault))
            {
                adpl = dicPLNtGift[PLGiftDefault];
            }
            return adpl;
        }
        public virtual int getNtGiftLoaded(string placement)
        {
            AdPlacementFull adpl = getPlNtGift(placement);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }
        //
        public AdPlacementFull getPlGiftFull(string placement)
        {
            AdPlacementFull adpl = null;
            if (dicPLGiftFull.ContainsKey(placement))
            {
                adpl = dicPLGiftFull[placement];
            }
            else if (dicPLGiftFull.ContainsKey(PLGiftDefault))
            {
                adpl = dicPLGiftFull[PLGiftDefault];
            }
            return adpl;
        }
        public virtual int getGiftFullLoaded(string placement)
        {
            AdPlacementFull adpl = getPlGiftFull(placement);
            if (adpl == null)
            {
                return 0;
            }
            int re = 0;
            if (adpl.isloaded)
            {
                re = 1;
            }

            return re;
        }

        protected void addAdPlacement<T>(Dictionary<string, T> dic, string data, bool isReplaceIds) where T : AdPlacementBase, new()
        {
            string[] plcf = data.Split(new char[] { ',' });
            if (plcf != null && plcf.Length == 3 && plcf[2].Length > 5)
            {
                //T plfull = (T)Activator.CreateInstance(typeof(T));
                string[] arrkeys = plcf[0].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ikey in arrkeys)
                {
                    if (ikey.Length > 2)
                    {
                        if (!dic.ContainsKey(ikey))
                        {
                            T plAd = new();
                            dic.Add(ikey, plAd);
                            plAd.placement = ikey;
                            plAd.adECPM.idxHighPriority = int.Parse(plcf[1]);
                            plAd.adECPM.listFromDstring(plcf[2]);
                        }
                        else
                        {
                            T plAd = dic[ikey];
                            plAd.adECPM.idxHighPriority = int.Parse(plcf[1]);
                            List<AdECPMItem> tmpl = new List<AdECPMItem>();
                            if (isReplaceIds && plcf[2].Length > 3)
                            {
                                tmpl.AddRange(plAd.adECPM.list);
                                plAd.adECPM.idxCurrEcpm = 0;
                                plAd.adECPM.list.Clear();
                            }
                            plAd.adECPM.listFromDstring(plcf[2]);
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
                        }
                    }
                }
            }
        }

        public void onFullClose(string placement)
        {
            if (placement != null && placement.StartsWith("full_2_"))
            {
                placement = placement.Replace("full_2_", "full_");
            }
            if (advhelper.isFullLoadWhenClose && PLFullSplash.CompareTo(placement) != 0)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    advhelper.loadFull4ThisTurn(placement, false, GameRes.LevelCommon(), 0, false);
                }, 2.0f);
            }
            else
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    advhelper.loadFull4ThisTurn(PLFullDefault, false, GameRes.LevelCommon(), 0, false);
                }, 2.0f);
            }
        }
        public void onGiftClose(string placement)
        {
            if (advhelper.isGiftLoadWhenClose)
            {
                AdsProcessCB.Instance().Enqueue(() =>
                {
                    advhelper.loadGift4ThisTurn(placement, GameRes.LevelCommon(), null);
                }, 2.0f);
            }
        }
    }

    public class AdPlacementBase
    {
        static long maxid = 0;
        public long idPl = 0;
        public long tLoad = 0;
        public string placement = "";
        public string loadPl { private set; get; }
        public string showPl { private set; get; }
        public bool isLoading = false;
        public bool isloaded = false;
        public int countLoad = 0;
        public bool isAdHigh = false;
        public bool isCheckNewIds = false;
        public short count4LoadAll = 0;
        public AdECPMs adECPM = new AdECPMs();

        public AdECPMs adECPM2 = null;
        public short count4LoadAll2 = 0;

        public AdCallBack cbLoad;

        public AdPlacementBase()
        {
            idPl = SdkUtil.CurrentTimeMilis();
            if (idPl <= maxid)
            {
                idPl = maxid + 1;
            }
            maxid = idPl;
        }

        public void setSetPlacementLoad(string pl)
        {
            loadPl = pl;
        }

        public void setSetPlacementShow(string pl)
        {
            showPl = pl;
        }

        public void setObjectAd4Id(string idloaded, object adOb)
        {
            if (idloaded != null)
            {
                for (int i = 0; i < adECPM.list.Count; i++)
                {
                    if (idloaded.CompareTo(adECPM.list[i].adsId) == 0)
                    {
                        adECPM.list[i].adObject = adOb;
                    }
                }
            }
        }

        public void setStateAd4Id(string idAd, bool isloading, bool isloaded, string nameNet, double? revenue)
        {
            if (idAd != null)
            {
                for (int i = 0; i < adECPM.list.Count; i++)
                {
                    if (idAd.CompareTo(adECPM.list[i].adsId) == 0)
                    {
                        adECPM.list[i].isLoading = isloading;
                        adECPM.list[i].isLoaded = isloaded;
                        adECPM.list[i].adnetname = nameNet;
                        if (revenue != null && revenue.HasValue)
                        {
                            adECPM.list[i].revenue = revenue.Value;
                        }
                    }
                }
            }
        }
        public bool containAdsId(string idAd)
        {
            if (idAd != null)
            {
                for (int i = 0; i < adECPM.list.Count; i++)
                {
                    if (idAd.CompareTo(adECPM.list[i].adsId) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int getPosdAd(string adId)
        {
            int re = 1;
            if (adECPM2 != null && adECPM2.list != null)
            {
                foreach (var idit in adECPM2.list)
                {
                    if (idit.adsId.CompareTo(adId) == 0)
                    {
                        return 2;
                    }
                }
            }

            return re;
        }
    }

    public class AdPlacementBanner : AdPlacementBase
    {
        public bool isShow = false;
        public bool isRealShow = false;
        public int posBanner;
        public object banner;
    }

    public class AdPlacementNative : AdPlacementBase
    {
        public bool isShow = false;
        public bool hasLoaded = false;
        public int posBanner;
        public bool flagCountShow = true;

        public void coppyFrom(AdPlacementNative other)
        {
            this.isShow = other.isShow;
            this.placement = other.placement;
            this.adECPM.coppyIdSFrom(other.adECPM);
        }

        public bool isHighAdLoaded()
        {
            if (adECPM.list.Count > 0 && adECPM.list[0].isLoaded)
            {
                return true;
            }
            return false;
        }

        public void setStatusLoad(string adid, bool _isLoaded)
        {
            SdkUtil.logd($"ads nt setStatusLoad id={adid} isLoaded={_isLoaded}");
            for (int i = 0; i < adECPM.list.Count; i++)
            {
                if (adECPM.list[i].adsId.CompareTo(adid) == 0)
                {
                    SdkUtil.logd($"ads nt setStatusLoad id={adid} isLoaded={_isLoaded} ssss");
                    adECPM.list[i].isLoading = false;
                    adECPM.list[i].isLoaded = _isLoaded;
                    break;
                }
            }
        }
    }

    public class AdPlacementFull : AdPlacementBase
    {
        public AdCallBack cbShow;
        public bool isAddCondition = false;
        private bool isShowing = false;
        private long tShow = 0;

        public void coppyFrom(AdPlacementFull other)
        {
            this.placement = other.placement;
            this.adECPM.coppyIdSFrom(other.adECPM);
        }

        public void setShowing(bool isShow)
        {
            this.isShowing = isShow;
            if (isShow)
            {
                tShow = SdkUtil.CurrentTimeMilis();
            }
        }

        public bool getShowing()
        {
            if (isShowing)
            {
                long tcurr = SdkUtil.CurrentTimeMilis();
                if ((tcurr - tShow) >= 11000)
                {
                    isShowing = false;
                }
            }
            return isShowing;
        }

        public bool isHighAdLoaded()
        {
            if (adECPM.list.Count > 0 && adECPM.list[0].isLoaded)
            {
                return true;
            }
            return false;
        }

        public void setStatusLoad(string adid, bool _isLoaded)
        {
            SdkUtil.logd($"ads full setStatusLoad id={adid} isLoaded={_isLoaded}");
            for (int i = 0; i < adECPM.list.Count; i++)
            {
                if (adECPM.list[i].adsId.CompareTo(adid) == 0)
                {
                    SdkUtil.logd($"ads full setStatusLoad id={adid} isLoaded={_isLoaded} ssss");
                    adECPM.list[i].isLoading = false;
                    adECPM.list[i].isLoaded = _isLoaded;
                    break;
                }
            }
        }
        public void setStatusLoad(int adpos, string adid, bool _isLoaded)
        {
            SdkUtil.logd($"ads gift setStatusLoad adpos={adpos} id={adid} isLoaded={_isLoaded}");
            if (adpos == 1)
            {
                for (int i = 0; i < adECPM.list.Count; i++)
                {
                    if (adECPM.list[i].adsId.CompareTo(adid) == 0)
                    {
                        SdkUtil.logd($"ads gift setStatusLoad id={adid} isLoaded={_isLoaded} ssss");
                        adECPM.list[i].isLoading = false;
                        adECPM.list[i].isLoaded = _isLoaded;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < adECPM2.list.Count; i++)
                {
                    if (adECPM2.list[i].adsId.CompareTo(adid) == 0)
                    {
                        SdkUtil.logd($"ads gift setStatusLoad id={adid} isLoaded={_isLoaded} ssss2");
                        adECPM2.list[i].isLoading = false;
                        adECPM2.list[i].isLoaded = _isLoaded;
                        break;
                    }
                }
            }
        }
    }
    //============================================================

    public class AdECPMItem
    {
        public string adsId = "";
        public string adnetname = "";
        public bool isLoading = false;
        public bool isLoaded = false;
        public long timeShow = 0;
        public long time4Count = 0;
        public long countTimeShow = 0;
        public double revenue = 0;
        public object adObject = null;

        public AdECPMItem(string _adsid)
        {
            adsId = _adsid;
            isLoaded = false;
        }

        public void coppyFrom(AdECPMItem other)
        {
            adsId = other.adsId;
            isLoaded = other.isLoaded;
            timeShow = other.timeShow;
            time4Count = other.time4Count;
            countTimeShow = other.countTimeShow;
        }
    }

    public class AdECPMs
    {
        public int idxHighPriority = -1;
        public int idxCurrEcpm = 0;
        public List<AdECPMItem> list;

        public AdECPMs()
        {
            idxHighPriority = -1;
            idxCurrEcpm = 0;
            list = new List<AdECPMItem>();
        }

        public void coppyIdSFrom(AdECPMs other)
        {
            idxHighPriority = other.idxHighPriority;
            idxCurrEcpm = other.idxCurrEcpm;
            list.Clear();
            foreach (var src in other.list)
            {
                AdECPMItem dest = new AdECPMItem(src.adsId);
                list.Add(dest);
            }
        }

        public void listFromDstring(string secmps, char csplit = ';')
        {
            string[] arrlv = secmps.Split(new char[] { csplit });
            foreach (string item in arrlv)
            {
                if (item.Length > 3)
                {
                    if (!isContainAdId(item))
                    {
                        list.Add(new AdECPMItem(item));
                    }
                }
            }
        }

        bool isContainAdId(string adid)
        {
            if (list != null)
            {
                foreach (AdECPMItem it in list)
                {
                    if (it.adsId.CompareTo(adid) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool isContain(string src)
        {
            foreach (var it in list)
            {
                if (it.adsId.CompareTo(src) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class BannerInfo
    {
        public bool isBNLoading = false;
        public bool isBNloaded = false;
        public bool isBNShow = false;
        public bool isBNRealShow = false;
        public int posBanner;
        public object banner;
    }

    public class AdCfPlacement
    {
        public int flagShow = 2;
        public int lvStart = -1;
        public int maxShow = 100;
        public int delTime = 5;
        public int numOver = 1;
        public int apply_interval = 0;
        public long timeShow = 0;
        public int lv4countShow = -1;
        public int countshow = 0;
        public int typeAd = 0;

        public AdCfPlacement()
        {

        }

        public AdCfPlacement(AdCfPlacement other)
        {
            if (other != null)
            {
                this.flagShow = other.flagShow;
                this.lvStart = other.lvStart;
                this.maxShow = other.maxShow;
                this.delTime = other.delTime;
                this.numOver = other.numOver;
                this.apply_interval = other.apply_interval;
                this.timeShow = other.timeShow;
                this.lv4countShow = other.lv4countShow;
                this.countshow = other.countshow;
            }
        }
    }
}
