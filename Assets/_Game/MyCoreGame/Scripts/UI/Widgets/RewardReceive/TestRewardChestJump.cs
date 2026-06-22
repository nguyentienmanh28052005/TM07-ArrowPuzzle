using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestRewardChestJump : MonoBehaviour
{
    public DataResource[] dataResources;
    public RectTransform rectTransformShow;
    public RectTransform rectClaim;
    public RectTransform rectStartJump;
    public UnityEvent Action2;
    public void Show()
    {
        gameObject.SetActive(true);
        RewardReceivedHub.Instance.ShowRewardGroupChest(dataResources, rectTransformShow, rectClaim, Hide, RewardReceivedHub.Instance.Canvas.worldCamera.WorldToScreenPoint(rectStartJump.position));
    }
    public void Show2()
    {
        gameObject.SetActive(true);
        RewardReceivedHub.Instance.ShowRewardGroupChest(dataResources, rectTransformShow, rectClaim, ()=>Active(false), RewardReceivedHub.Instance.Canvas.worldCamera.WorldToScreenPoint(rectStartJump.position), ()=>Action2?.Invoke());
    }
    public void Active(bool flag)
    {
        gameObject.SetActive(flag);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
