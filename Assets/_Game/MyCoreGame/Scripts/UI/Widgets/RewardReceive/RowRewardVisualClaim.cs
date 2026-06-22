using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowRewardVisualClaim : MonoBehaviour
{
    [SerializeField] RewardVisualClaim[] rewardVisualClaims;
    [SerializeField] LayoutGroup layoutGroup;
    public void Initialize(DataResource[] dataResources)
    {
        EnableLayouGroup(true);
        for (int i = 0; i < rewardVisualClaims.Length; i++)
        {
            rewardVisualClaims[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < dataResources.Length; i++)
        {
            if (i >= rewardVisualClaims.Length)
            {
                break;
            }
            rewardVisualClaims[i].gameObject.SetActive(true);
            rewardVisualClaims[i].Initialize(dataResources[i]);
        }
    }
    public List<RewardVisualClaim> GetActiveVisual()
    {
        List<RewardVisualClaim> listRes = new List<RewardVisualClaim>();
        for(int i = 0; i < rewardVisualClaims.Length; i++)
        {
            if (rewardVisualClaims[i].gameObject.activeSelf)
            {
                listRes.Add((rewardVisualClaims[i]));
            }
        }
        return listRes;
    }
    public void EnableLayouGroup(bool flag)
    {
        layoutGroup.enabled = flag;
    }
}
