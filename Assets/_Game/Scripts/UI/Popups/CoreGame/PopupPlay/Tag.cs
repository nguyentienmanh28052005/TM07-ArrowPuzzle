using System.Collections;
using System.Collections.Generic;
using mygame.sdk;
using UnityEngine;
using UnityEngine.UI;

public class Tag : MonoBehaviour
{
    [SerializeField] Text txtReward;
    [SerializeField] Button btnInfo;
    [SerializeField] RectTransform anchorTooltip;
    [SerializeField] Animator animatorTag;
    int mutilPoint => 1;
    private void Awake()
    {
        btnInfo.onClick.AddListener(ButtonInfo);
    }
    private void OnEnable()
    {
        txtReward.text = $"x{mutilPoint}";


        // var listEvent = EventController.Instance.EventManagers;
        // List<DataResource> dataResources = new List<DataResource>();
        // for (int i = 0; i < listEvent.Count; i++)
        // {
        //     if (listEvent[i].IsEventActive() && listEvent[i].CanMultiPointEvent())
        //     {
        //         return;
        //     }
        // }
        gameObject.SetActive(false);
    }
    public void ButtonInfo()
    {
        GameHelper.Instance.Vibrate(Type_vibreate.Vib_Medium);
        animatorTag.SetTrigger("shake");
        int mutilPointGet = mutilPoint;
        // var listEvent = EventController.Instance.EventManagers;
        // List<DataResource> dataResources = new List<DataResource>();
        // for(int i = 0; i < listEvent.Count; i++)
        // {
        //     if (listEvent[i].IsEventActive() && listEvent[i].CanMultiPointEvent())
        //     {
        //         DataResource dataResource = new DataResource();
        //         dataResource.icon = listEvent[i].GetEventIcon();
        //         dataResource.amount = mutilPointGet;
        //         dataResource.resType = mygame.sdk.RES_type.NONE;
        //         dataResources.Add(dataResource);
        //     }
        // }
        // GameTooltip.Instance.ShowTooltip(anchorTooltip, null, TooltipDirection.Down,iListItem: dataResources, specialItem: null, key:"_desc_level_reward_x",param: $"{mutilPointGet}");
    }
}
