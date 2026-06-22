using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRewardReceive : MonoBehaviour
{
    public DataResource[] dataResources;
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ReceiveData();
        }
    }
#endif
    public void ReceiveData()
    {
        GameEvent.OnReceiveResource?.Invoke(LogEvent.ReasonItem.reward, "hack", dataResources, DataManager.Level);
        RewardReceivedHub.Instance.ShowRewardReceive(dataResources,new Vector2(Screen.width/2,Screen.height/2));
    }
}
