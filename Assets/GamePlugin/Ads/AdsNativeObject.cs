using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
using GoogleMobileAds.Api;
#endif

namespace mygame.sdk
{
    public class AdsNativeObject : MonoBehaviour
    {
        public AdsNativeWrapper parentAd;
        public bool IsLoaded = false;
        public bool isReplaceTxtCallAc = true;
        public AdsNativeAdmob adNative;
        public Image contentImg;
        public Image iconImg;
        public Image adChoicesLogoImg;
        public Text headlineTxt;
        public Text bodyTxt;
        public Text callActionTxt;
        public Text advertiserTxt;
        public Text starRatingTxt;
        public Image starRatingImg;
        public Text storeTxt;
        public Text priceTxt;
        private BoxCollider boxCollider;
        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        [SerializeField] private Button btntGame;

        bool isDestroyOb = false;

        public int adType { get; private set; }
        bool isEnableClick = true;
        PromoGameOb myGamePromo = null;

        Texture2D ttAdChoise = null;
        Texture2D ttIcon = null;
        Texture2D ttContent = null;
        

        private void Awake()
        {
            getControl();
            isDestroyOb = false;
        }

        public void initAd()
        {
            adType = 0;
            isEnableClick = true;
            getControl();
        }

        void getControl()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
                rectTransform = GetComponent<RectTransform>();
                parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
                btntGame = GetComponent<Button>();
                if (btntGame != null)
                {
                    btntGame.onClick.AddListener(onclickGamePromo);
                }
            }
            if (parentAd == null)
            {
                parentAd = transform.parent.GetComponent<AdsNativeWrapper>();
            }
        }

        private void Start()
        {
            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;

            Vector2 size = new Vector2(
                (anchorMax.x - anchorMin.x) * parentRectTransform.rect.width,
                (anchorMax.y - anchorMin.y) * parentRectTransform.rect.height
            );

            Vector2 sizeDelta = size - rectTransform.sizeDelta;
            boxCollider.size = new Vector3(Mathf.Abs(sizeDelta.x), Mathf.Abs(sizeDelta.y), 0.1f);
        }
        public void setEnableClick(bool isEnable)
        {
            getControl();
            isEnableClick = isEnable;
            SdkUtil.logd($"ads native {parentAd.placement} ntobject setEnableClick type={adType}-click={isEnableClick}");
            if (adType == 0)
            {
                boxCollider.enabled = isEnableClick;
                if (btntGame != null)
                {
                    btntGame.enabled = false;
                }
            }
            else
            {
                boxCollider.enabled = false;
                if (btntGame != null)
                {
                    btntGame.enabled = isEnableClick;
                }
            }
        }
        public void switchAdType(int type)
        {
            if (adType != type)
            {
                SdkUtil.logd($"ads native {parentAd.placement} ntobject switchAdType={type}-click={isEnableClick}");
                adType = type;
                if (adType == 0)
                {
                    boxCollider.enabled = isEnableClick;
                    if (btntGame != null)
                    {
                        btntGame.enabled = false;
                    }
                }
                else
                {
                    boxCollider.enabled = false;
                    if (btntGame != null)
                    {
                        btntGame.enabled = isEnableClick;
                    }
                }
            }
        }
        public bool isActiveIcon()
        {
            return iconImg.gameObject.activeSelf;
        }
        void onclickGamePromo()
        {
            if (myGamePromo != null)
            {
                FIRhelper.Instance.onclickNativePromo(myGamePromo);
                SDKManager.Instance.onClickAd();
            }
        }
        public void pushPromoToNative(PromoGameOb game, Texture2D ttIcon, Texture2D ttContent)
        {
            if (!isDestroyOb && game != null && (ttIcon != null || ttContent != null))
            {
                SdkUtil.logd($"ads native {parentAd.placement} admobmy pushPromoToNative");
                myGamePromo = game;
                if (ttIcon != null)
                {
                    if (this.ttIcon != null)
                    {
                        Destroy(this.ttIcon);
                        this.ttIcon = null;
                    }
                    this.ttIcon = ttIcon;
                    iconImg.sprite = Sprite.Create(ttIcon, new Rect(0, 0, ttIcon.width, ttIcon.height), Vector2.zero);
                }

                if (ttContent != null)
                {
                    if (this.ttContent != null)
                    {
                        Destroy(this.ttContent);
                        this.ttContent = null;
                    }
                    this.ttContent = ttContent;
                    contentImg.sprite = Sprite.Create(ttContent, new Rect(0, 0, ttContent.width, ttContent.height), Vector2.zero);
                }
                headlineTxt.text = game.name;
                bodyTxt.text = game.des;
                if (callActionTxt != null)
                {
                    callActionTxt.text = "INSTALL";
                }
                gameObject.SetActive(true);
            }
        }

#if ENABLE_ADS_ADMOB && USE_ADSMOB_MY && USE_NATIVE_UNITY
        public virtual void pushNative2GameObject(string placement, NativeAd nativeAd, AdCallBack cb)
        {
            if (!isDestroyOb && nativeAd != null)
            {
                SdkUtil.logd($"ads native {placement} admobmy pushNative2GameObject-{adNative}");
                AdsHelper.Instance.freeNative(this);
                adNative.nativeAd = nativeAd;
                Texture2D iconTexture = nativeAd.GetIconTexture();
                Texture2D contentTexture = null;
                List<Texture2D> ltt = nativeAd.GetImageTextures();
                if (ltt != null && ltt.Count > 0)
                {
                    contentTexture = ltt[0];
                }
                Texture2D adchoiseTT = nativeAd.GetAdChoicesLogoTexture();
                if (adchoiseTT != null)
                {
                    adChoicesLogoImg.sprite = Sprite.Create(adchoiseTT, new Rect(0, 0, adchoiseTT.width, adchoiseTT.height), Vector2.zero);
                }

                if (iconTexture != null)
                {
                    iconImg.sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), Vector2.zero);
                }

                if (contentTexture != null)
                {
                    contentImg.sprite = Sprite.Create(contentTexture, new Rect(0, 0, contentTexture.width, contentTexture.height), Vector2.zero);
                }

                headlineTxt.text = nativeAd.GetHeadlineText();
                bodyTxt.text = nativeAd.GetBodyText();
                if (isReplaceTxtCallAc)
                {
                    callActionTxt.text = nativeAd.GetCallToActionText();
                }

                if (advertiserTxt != null)
                {
                    advertiserTxt.text = nativeAd.GetAdvertiserText();
                }
                if (storeTxt != null)
                {
                    storeTxt.text = nativeAd.GetStore();
                }
                if (priceTxt != null)
                {
                    priceTxt.text = nativeAd.GetPrice();
                }
                if (starRatingTxt != null)
                {
                    starRatingTxt.text = $"{nativeAd.GetPrice():0.0}";
                }
                if (starRatingImg != null)
                {
                    starRatingImg.fillAmount = (float)nativeAd.GetStarRating() / 5.0f;
                }

                // Register GameObject that will display icon asset of native ad.
                if (!nativeAd.RegisterIconImageGameObject(gameObject))
                {
                    SdkUtil.logd($"ads native {placement} admobmy pushNative2GameObject RegisterIconImageGameObject fail");
                }
                else
                {
                    //SdkUtil.logd($"ads native {placement} admobmy pushNative2GameObject BoxCollider={cl.size} - {cl.center}");
                }
                gameObject.SetActive(true);
                if (cb != null)
                {
                    cb(AD_State.AD_LOAD_OK);
                }
            }
            else
            {
                SdkUtil.logd($"ads native admobmy pushNative2GameObject ob = null || ob.gameObject = null || nativeAd = null");
                gameObject.SetActive(false);
                if (cb != null)
                {
                    var tmpcb = cb;
                    cb = null;
                    tmpcb(AD_State.AD_LOAD_FAIL);
                }
            }
        }
#endif
        private void OnDestroy()
        {
            isDestroyOb = true;
            if (ttAdChoise != null)
            {
                Destroy(ttAdChoise);
                ttAdChoise = null;
            }
            if (ttIcon != null)
            {
                Destroy(ttIcon);
                ttIcon = null;
            }
            if (ttContent != null)
            {
                Destroy(ttContent);
                ttContent = null;
            }
        }
    }
}