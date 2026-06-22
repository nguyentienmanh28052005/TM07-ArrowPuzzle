using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mygame.sdk;
using DG.Tweening;

public class TargetRewardReceived : MonoBehaviour,IResourceTarget
{
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
        RewardReceivedHub.RegisterTarget(this);
    }
    private void OnDisable()
    {
        RewardReceivedHub.RemoveTarget(this);
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
