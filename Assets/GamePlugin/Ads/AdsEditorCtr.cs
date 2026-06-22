using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace mygame.sdk
{
    public class AdsEditorCtr : MonoBehaviour
    {
        public Transform adsEditor;
        public FullEditorCtr fullEditor;
        public GiftEditorCtr giftEditor;
        public LoadingAdsCtr loadingAdsCtr;
        public RectTransform rectEditor;
        public bool isFullEditorLoaded { get; private set; }
        public bool isFullEditorLoading { get; private set; }
        public bool isGiftEditorLoaded { get; private set; }
        public bool isGiftEditorLoading { get; private set; }

        private void Start()
        {
            var list = FindObjectsOfType<EventSystem>();
            if (list == null || list.Count() == 0)
            {
                transform.Find("EventSystem").gameObject.SetActive(true);
            }
        }

        public void loadFullEditor()
        {
#if UNITY_EDITOR
            fullEditor.loadAds();
#endif
        }
        public void showFullEditor()
        {
            adsEditor.gameObject.SetActive(true);
            fullEditor.gameObject.SetActive(true);
            giftEditor.gameObject.SetActive(false);
        }
        public void loadGiftEditor()
        {
            SdkUtil.logd("ads helper RequestRewardBasedVideo editor");
#if UNITY_EDITOR
            giftEditor.loadAds();
#endif
        }
        public void showGiftEditor()
        {
            adsEditor.gameObject.SetActive(true);
            fullEditor.gameObject.SetActive(false);
            giftEditor.gameObject.SetActive(true);
        }

        public void showRect(AD_BANNER_POS location, float width, int maxH, float dxCenter, float dyVertical)
        {
            adsEditor.gameObject.SetActive(true);
            rectEditor.gameObject.SetActive(true);
            float dpi = Screen.dpi * 1.5f;
            float wbn = 300 * dpi / 160;
            float hbn = 250 * dpi / 160;
            rectEditor.sizeDelta = new Vector2(wbn, hbn);
            if (location == AD_BANNER_POS.TOP)
            {
                rectEditor.pivot = new Vector2(0.5f, 1);
                rectEditor.anchorMin = new Vector2(0.5f, 1);
                rectEditor.anchorMax = new Vector2(0.5f, 1);
            }
            else
            {
                rectEditor.pivot = new Vector2(0.5f, 0);
                rectEditor.anchorMin = new Vector2(0.5f, 0);
                rectEditor.anchorMax = new Vector2(0.5f, 0);
            }
            float mmm = dyVertical * Screen.height;
            float nnn = dxCenter * Screen.width;
            rectEditor.anchoredPosition = new Vector2(nnn, mmm);
        }
        public void hideRect()
        {
            rectEditor.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public void onFullLoaded()
        {
            SdkUtil.logd("ads helper onFullLoaded");
            isFullEditorLoading = false;
            isFullEditorLoaded = true;
        }
        public void onFullLoadFail()
        {
            SdkUtil.logd("ads helper onFullLoadFail");
            isFullEditorLoading = false;
            isFullEditorLoaded = false;
        }
        public void onFullShow()
        {
            SdkUtil.logd("ads helper onFullShow");
        }
        public void onFullClose()
        {
            SdkUtil.logd("ads helper onFullClose");
            isFullEditorLoading = false;
            isFullEditorLoaded = false;
            AdsHelper.Instance.EditorOnFullClose();
        }
        public void onGifLoaded()
        {
            SdkUtil.logd("ads helper rw onGifLoaded");
            isGiftEditorLoading = false;
            isGiftEditorLoaded = true;
        }
        public void onGifLoadFail()
        {
            SdkUtil.logd("ads helper rw onGifLoadFail");
            isGiftEditorLoaded = false;
            isGiftEditorLoading = false;
        }
        public void onGifShow()
        {
            SdkUtil.logd("ads helper rw onGifShow");
        }
        public void onGiftReward()
        {
            SdkUtil.logd("ads helper rw onGiftReward");
        }
        public void onGifClose(bool isrw)
        {
            SdkUtil.logd("ads helper rw onGifClose");
            isGiftEditorLoaded = false;
            isGiftEditorLoading = false;
            AdsHelper.Instance.EditorOnGiftClose(isrw);
        }
#endif
    }
}
