using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRewardImmediate : MonoBehaviour
{
    public DataResource[] dataResources;

    public void Show()
    {
        RewardReceivedHub.Instance.ShowRewardGroupImmediate(dataResources);
    }
}
