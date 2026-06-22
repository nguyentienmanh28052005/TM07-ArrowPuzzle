using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideMechanic : MonoBehaviour
{
    [SerializeField] MechanicGuideType mechanicGuideType;
    [SerializeField] GameObject lockObj;
    [SerializeField] GameObject normalObj;
    [SerializeField] Text txtLevelUnlock;
    [SerializeField] Text txtTipContent;
    [SerializeField] Text txtNameTip;
    [SerializeField] Text txtDesc;
    [SerializeField] string tipContent;
    [SerializeField] string nameTip;
    [SerializeField] string desc;
    public MechanicGuideType guideType => mechanicGuideType;
    private void OnEnable()
    {
        if (DataManager.Level < GetLevelUnlock())
        {
            lockObj.SetActive(true);
            normalObj.SetActive(false);
            txtLevelUnlock.SetText("desc_unlock_at_level_x", stateCap: mygame.sdk.StateCapText.None, stateFormat: mygame.sdk.FormatText.F_Int, GetLevelUnlock());
        }
        else
        {
            lockObj.SetActive(false);
            normalObj.SetActive(true);
        }
        txtTipContent.SetText(tipContent);
        txtNameTip.SetText(nameTip);
        txtDesc.SetText(desc);
    }
    public int GetLevelUnlock()
    {
        return 1;
    }
}
