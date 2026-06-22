using UnityEngine;
using UnityEngine.UI;
namespace mygame.sdk
{
    public class WrapperChoiseLang : MonoBehaviour
    {
        [SerializeField] Button btClose = null;
        bool isCallAction = false;

        private void Start()
        {
            if (btClose == null)
            {
                Debug.LogAssertion("Must reference close button");
            }
            else
            {
                btClose.onClick.AddListener(onClose);
            }
        }
        private void onClose()
        {
            if (!isCallAction)
            {
                isCallAction = true;

                int flagShow = PlayerPrefs.GetInt("cf_show_langage", AppConfig.Flag_show_langeAge);
                if ((flagShow & 2) > 0)
                {
                    SDKManager.Instance.onChangePlacement("choose_age");
                    var prefab = Resources.Load<RectTransform>("Popup/UIChooseAge");
                    RectTransform poplangage = Instantiate(prefab, Vector3.zero, Quaternion.identity, SDKManager.Instance.PopupShowFirstAds.transform.parent);
                    poplangage.anchoredPosition = Vector2.zero;
                    poplangage.anchoredPosition3D = Vector3.zero;
                    poplangage.anchorMin = Vector2.zero;
                    poplangage.anchorMax = Vector2.one;
                    poplangage.sizeDelta = Vector2.zero;
                    poplangage.localScale = Vector3.one;
                    PlayerPrefs.SetInt("mem_show_langage", 2);
                    Destroy(this.gameObject);
                }
                else
                {
                    var flagfull = AdsHelper.Instance.getCfAdsPlacement("full_chooseage", 3);
                    int flagshow = 3;
                    if (flagfull != null)
                    {
                        flagshow = flagfull.flagShow;
                    }
                    if (flagshow > 0)
                    {
                        AdsHelper.Instance.showFull("full_chooseage", GameRes.LevelCommon(), 0, flagshow - 1, 0, false, false, false, null, false, false);
                    }
                    SDKManager.Instance.onCloseLangAge(1);
                    Destroy(this.gameObject);
                }
            }
        }
    }
}