using DG.Tweening;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPlay : MonoBehaviour,IResourceTarget
{
    [SerializeField] Image btnColor;
    [SerializeField] Image clockBar;
    [SerializeField] GameObject[] modeObjects;
    [SerializeField] Text[] txtLevel;
    [SerializeField] GameObject tagObject;
    public Transform targetTF;
    public List<RES_type> targetTypes;

    public virtual List<RES_type> GetResourceTypes()
    {
        return targetTypes;
    }

    public virtual Transform GetTransform()
    {
        return targetTF;
    }

    public virtual void UpdateVisual()
    {
        if (targetTF == null) return;
        targetTF.DOKill();
        targetTF.DOScale(new Vector3(1.1f, .95f, 1.1f), 0.06f).SetId(this).OnComplete(() =>
        {
            if (targetTF != null)
            {
                targetTF.DOScale(Vector3.one, 0.05f).SetId(this);
            }
        });
    }
    private void OnEnable()
    {
        var lvlType = LevelManager.GetLevelType(DataManager.Level);
        for (int i = 0; i < modeObjects.Length; i++)
        {
            modeObjects[i].SetActive(i == (int)lvlType);
        }

        for (int i = 0; i < txtLevel.Length; i++)
        {
            txtLevel[i].SetText("_level_x", StateCapText.FirstCap, FormatText.F_String, DataManager.Level.ToString());
        }
        
        RewardReceivedHub.RegisterTarget(this);
        DoubleRewardManager.OnAddTimeUnlimited += UpdateVisual;
    }

    private void OnDisable()
    {
        RewardReceivedHub.RemoveTarget(this);
        DoubleRewardManager.OnAddTimeUnlimited -= UpdateVisual;
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
   
}
