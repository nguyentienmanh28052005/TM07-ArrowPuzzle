using UnityEngine;
using UnityEngine.UI;
namespace mygame.sdk
{
    public class WrapperChoiseAge : MonoBehaviour
    {
        [SerializeField] Button btClose = null;
        [SerializeField] int TimeAutoClose = -1;
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
            if (TimeAutoClose > 5)
            {
                Invoke("onClose", TimeAutoClose);
            }
        }

        private void onClose()
        {
            if (!isCallAction)
            {
                isCallAction = true;
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

                SDKManager.Instance.onCloseLangAge(2);
                Destroy(this.gameObject);
            }
        }
    }
}