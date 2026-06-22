using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestRewardChest : MonoBehaviour
{
    public DataResource[] dataResources;
    public RectTransform rectTransformShow;
    public RectTransform btnClaim;

    public void Show()
    {
        gameObject.SetActive(true);
        RewardReceivedHub.Instance.ShowRewardGroupChest(dataResources, rectTransformShow,btnClaim,Hide,chestSkeleton: ChestSkeleton.SafeChest,skinName:"default");
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
