using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestRewardPanel : MonoBehaviour
{
    public DataResource[] dataResources;
    public RectTransform rectTransformShow;
    public RectTransform btnClaim;

    public void Show()
    {
        gameObject.SetActive(true);
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "hack", dataResources, DataManager.Level);
        RewardReceivedHub.Instance.ShowRewardGroup(dataResources, rectTransformShow, btnClaim, Hide);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
