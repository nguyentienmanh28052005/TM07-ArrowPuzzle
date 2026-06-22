using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventGame
{
    public class BarBottomManager : MonoBehaviour
    {
        #region Config
        public static int CF_ActiveBarBottom
        {
            get => PlayerPrefs.GetInt("cf_active_bar_infor_event_bottom", 1);
            set => PlayerPrefs.SetInt("cf_active_bar_infor_event_bottom", value);
        }
        #endregion

        private static bool IsActiveBarBottom => CF_ActiveBarBottom > 0;

        [Space, Header("List Bar Infor Bottom")]
        [SerializeField] List<AbsBarInforBottom> listBarInfoEventPrefab = new List<AbsBarInforBottom>();
        [SerializeField] RectTransform rect_Spawn;
        private AbsBarInforBottom absBarCurrent;
        public PopupUI popupUI;

        public bool ShowBarInfoEvent(PopupUI popupUI)
        {
            this.popupUI = popupUI;
            if (absBarCurrent != null)
            {
                Destroy(absBarCurrent.gameObject);
            }
            if (!IsActiveBarBottom)
            {
                return false;
            }

            foreach(var barEventPrefab in listBarInfoEventPrefab)
            {
                if(barEventPrefab != null && barEventPrefab.IsEventActive())
                {
                    absBarCurrent = Instantiate(barEventPrefab, rect_Spawn);
                    absBarCurrent.Initialized(this);
                    absBarCurrent.SetUpVisual();
                    return true;
                }
            }
            return false;
        }

        public void ClosePopup() 
        { 
            popupUI.gameObject.SetActive(false);
        }

        public void ActivePopup()
        {
            popupUI.gameObject.SetActive(true);
            popupUI.Show(popupUI.OnHide);
        }

        public void AnimationShow()
        {
            if (absBarCurrent != null)
            {
                try
                {
                    
                }
                catch (Exception e)
                {
                    
                }
            }
        }
    }
}