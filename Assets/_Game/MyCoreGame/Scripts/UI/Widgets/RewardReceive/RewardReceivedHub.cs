using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mygame.sdk;
using UnityEngine.UI;
using System;
using DG.Tweening;
/// <summary>
/// place for resource fly to
/// </summary>
public interface IResourceTarget
{
    public static Action<RES_type, float> OnItemChange = delegate { };
    public abstract List<RES_type> GetResourceTypes();
    public abstract Transform GetTransform();

    public abstract void UpdateVisual();

    public bool IsValid(RES_type type)
    {
        if (GetResourceTypes().Count == 0) return false;
        return GetResourceTypes().Contains(type);
    }
    public static void UpdateAmount(RES_type type)
    {
        OnItemChange?.Invoke(type, .75f);
    }
    public static bool IsNullOrDestroyed(IResourceTarget target)
    {
        if (target == null) return true;
        if (target is UnityEngine.Object unityObj)
        {
            return unityObj == null;
        }
        return false;
    }
}
public enum ChestType
{
    Bronze=0,
    Silver=1,
    Gold=2,
    Purple=3,
}
public enum ChestSkeleton
{
    Default = 0,
    WinChest = 1,
    SafeChest = 2,

}
public class RewardReceivedHub : master.Singleton<RewardReceivedHub>
{
    [SerializeField] RewardUIGroup rewardUIGroupPrefab;
    [SerializeField] RewardClaimChestPanel rewardClaimChestPanel;
    [SerializeField] RewardClaimPanel rewardClaimPanel;
    [SerializeField] RewardClaimImmediatePanel rewardClaimImmediatePanel;
    [SerializeField] ItemReceiveFly coinReceiveFly;
    [SerializeField] ItemReceiveFly ticketReceiveFly;
    [SerializeField] ItemReceiveFly heartReceiveFly;
    [SerializeField] List<RES_type> listPriorityRestype;
    [SerializeField] PopTextController popTextController;
    [SerializeField] PopTextRewardController popTextRewardController;
    [SerializeField] CanvasGroup topBarCanvasGrp;
    [SerializeField] ItemDisplay coinTarget;
    [SerializeField] ItemDisplay ticketTarget;
    [SerializeField] HeartDisplay heartTarget;
    /// <summary>
    /// When use claim and want to show gift appear sequencely immediate 
    /// </summary>
    /// <param name="dataResources"></param>
    /// <param name="screenPos">the position you want to show reward</param>
    public static List<IResourceTarget> resourceTargets = new List<IResourceTarget>();
    [SerializeField] Canvas canvas;
    public Canvas Canvas { get { return canvas; } }
    public static Dictionary<RES_type, int> cacheResType = new Dictionary<RES_type, int>();
    
    public static int GetCacheValue(RES_type rES_Type)
    {
        if (!cacheResType.ContainsKey(rES_Type))
        {
            return 0;
        }
        return cacheResType[rES_Type];
    }
    public static void AddCacheValue(RES_type rES_Type,int value)
    {
        if (!cacheResType.ContainsKey(rES_Type))
        {
            cacheResType[rES_Type] = 0;
        }
        cacheResType[rES_Type] += value;
    }
    public static void RemoveCacheValue(RES_type rES_Type)
    {
        cacheResType[rES_Type] = 0;
    }
    public static bool RegisterTarget(IResourceTarget resourceTarget)
    {
        if (resourceTargets.Contains(resourceTarget))
        {
            return false;
        }
        resourceTargets.Add(resourceTarget);
        return true;
    }
    public static bool RemoveTarget(IResourceTarget resourceTarget)
    {
        return resourceTargets.Remove(resourceTarget);
    }
    public static IResourceTarget GetResourceTarget(RES_type type)
    {
        for (int i = resourceTargets.Count - 1; i >= 0; i--)
        {
            if (IResourceTarget.IsNullOrDestroyed(resourceTargets[i]))
            {
                resourceTargets.RemoveAt(i);
                continue;
            }
            if (resourceTargets[i].IsValid(type))
            {
                return resourceTargets[i];
            }
        }
        return null;
    }
    protected override void Awake()
    {
        base.Awake();
        rewardClaimChestPanel.Initialize();
    }

    public void ShowRewardReceive(DataResource[] dataResources, Vector2 screenPos, float scale = 1)
    {
        RewardUIGroup rewardUIGroup = Instantiate(rewardUIGroupPrefab, transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), screenPos, canvas.worldCamera, out Vector2 localPoint);
        rewardUIGroup.gameObject.SetActive(true);
        rewardUIGroup.GetComponent<RectTransform>().anchoredPosition = localPoint;
        rewardUIGroup.Initialize(dataResources);
        rewardUIGroup.transform.localScale = Vector3.one * scale;
        foreach(var data in dataResources)
        {
            IResourceTarget target = GetResourceTarget(data.resType);
            if(target != null)
            {
                IResourceTarget.UpdateAmount(data.resType);

            }
        }
    }

    /// /// <summary>
    /// When use claim and want to show list gift in your panel
    /// </summary>
    /// <param name="rectTransform">the panel you want to show reward</param>
    public void ShowRewardGroup(DataResource[] dataResources, RectTransform rectHolder, RectTransform btnClaimFake, Action cbOnRewarded, Action cbOnHide = null)
    {
        rewardClaimPanel.Show();
        rewardClaimPanel.Initialize(StandardizationDataResources(dataResources), btnClaimFake, cbOnRewarded, rectHolder, cbOnHide);
    }
    public void ShowRewardGroupImmediate(DataResource[] dataResources, Action cbOnRewarded = null, Action cbOnHide = null)
    {
        rewardClaimImmediatePanel.Show();
        rewardClaimImmediatePanel.Initialize(StandardizationDataResources(dataResources), cbOnRewarded, cbOnHide);
    }
    /// /// <summary>
    /// When use claim and want to show list gift in your panel with a chest , max is 9 data
    /// </summary>
    /// /// <param name="rectTransform">the panel you want to show reward</param>
    /// /// <param name="startJumpPos">if you want to give start pos for chest jump</param>
    /// /// <param name="cbOnRewarded">call back will action when player click to claim</param>
    /// /// <param name="cbOnHide">call back will action when claiming reward is completed </param>
    /// /// <param name="startScale"> The intial scale of chest </param>
    public void ShowRewardGroupChest(DataResource[] dataResources, RectTransform rectHolderChest, RectTransform btnClaimFake, Action cbOnRewarded, Vector2 startscrPos, Action cbOnHide = null, int typeChest = 0, float startScale = 0,string skinName ="",ChestSkeleton chestSkeleton= ChestSkeleton.Default,bool isWaiting  = true)
    {
        rewardClaimChestPanel.Show();
        DataResource[] resources = /*StandardizationDataResources(dataResources);*/dataResources;
        if (resources.Length > 9)
        {
            resources = new List<DataResource>(resources).GetRange(0, 9).ToArray();
        }
        rewardClaimChestPanel.ShowRewardGroup(resources, rectHolderChest, btnClaimFake, cbOnRewarded, startscrPos, cbOnHide, typeChest, startScale,skinName,chestSkeleton, isWaiting);
    }
    public void ShowRewardGroupChest(DataResource[] dataResources, RectTransform rectHolderChest, RectTransform btnClaimFake, Action cbOnRewarded, Action cbOnHide = null, int typeChest = 0, float startScale = 0, string skinName="", ChestSkeleton chestSkeleton = ChestSkeleton.Default, bool isWaiting = true)
    {
        rewardClaimChestPanel.Show();
        DataResource[] resources = /*StandardizationDataResources(dataResources);*/dataResources;
        if (resources.Length > 9)
        {
            resources = new List<DataResource>(resources).GetRange(0, 9).ToArray();
        }
        rewardClaimChestPanel.ShowRewardGroup(resources, rectHolderChest, btnClaimFake, cbOnRewarded, cbOnHide, typeChest, startScale, skinName, chestSkeleton, isWaiting);

    }

    /// <summary>
    /// Use to visual claim gold
    /// </summary>
    /// <param name="scrStartPoint"></param>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    /// <param name="complete">action with index object fly and total object list </param>
    /// <returns></returns>
    float timeTopBar;
    public Tween CoinFly(Vector2 scrStartPoint, RectTransform target, int amount, Action<int, int> complete,int numShowText = 0)
    {
        bool useCoinTarget = false;
        var mainMenu = UIManager.Instance.GetScreenActive<MainMenuScreen>();
        amount = Mathf.Min(8, amount);

        if (mainMenu != null)
        {
            var currentPanel = mainMenu.GetCurrentPanel();
            if (currentPanel != null)
            {
                var resTarget = currentPanel.itemDisplay;
                if (resTarget != null)
                {
                    target = resTarget.GetTransform() as RectTransform;
                }
                else
                {
                    for (int i = 0; i < resourceTargets.Count; i++)
                    {
                        if (coinTarget != null)
                        {
                            target = coinTarget.GetTransform() as RectTransform;
                            useCoinTarget =  true;
                        }
                    }
                }
            }
        }
        if (target == null || (coinTarget != null && target == coinTarget.GetTransform() && !useCoinTarget))
        {
            float timeShow = 0;
            IResourceTarget resourceTarget = GetResourceTarget(RES_type.GOLD);
            
            if (IResourceTarget.IsNullOrDestroyed(resourceTarget))
            {
                resourceTarget = coinTarget;
            }
            if (IResourceTarget.IsNullOrDestroyed(resourceTarget))
            {
                return DOTween.Sequence();
            }
            if (coinTarget != null && resourceTarget.GetTransform() == coinTarget.GetTransform())
            {
                timeShow = Time.time;
                timeTopBar = timeShow;
                if (topBarCanvasGrp != null)
                {
                    topBarCanvasGrp.gameObject.SetActive(true);
                    topBarCanvasGrp.DOKill();
                    topBarCanvasGrp.DOFade(1, .5f).SetId(this);
                }
            }
            target = resourceTarget.GetTransform() as RectTransform;
            complete += (index, total) =>
            {
                if (IResourceTarget.IsNullOrDestroyed(resourceTarget)) return;
                
                if (index == 0)
                {
                    IResourceTarget.UpdateAmount(RES_type.GOLD);
                }
                resourceTarget.UpdateVisual();
                if (index == total - 1)
                {
                    if (topBarCanvasGrp != null && Mathf.Abs(timeShow - timeTopBar) < .01f)
                    {
                        topBarCanvasGrp.DOFade(0, .5f).SetDelay(1.2f).SetId(this);
                    }
                }
            };
        }
        if (target == null) return DOTween.Sequence();
        
        DOVirtual.DelayedCall(1f, () =>
        {
            IResourceTarget.UpdateAmount(RES_type.GOLD);
        });
        
        if (numShowText>0)
        {
            var canvas = target.GetComponentInParent<Canvas>();
            int sortingOrder = 200;
            if (canvas != null)
            {
                sortingOrder = canvas.sortingOrder + 20;
            }
            ShowPopText($"+{numShowText}", scrStartPoint, sortingOrder);
        }
        return coinReceiveFly.Animate(scrStartPoint, target, amount, complete);
    }
    /// <summary>
    /// Use to visual claim ticket
    /// </summary>
    /// <param name="scrStartPoint"></param>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    /// <param name="complete"></param>
    public Tween TicketFly(Vector2 scrStartPoint, RectTransform target, int amount, Action<int, int> complete,int numShowText=0)
    {
        amount = Mathf.Min(8, amount);
        if (target == null)
        {
            float timeShow = 0;
            IResourceTarget resourceTarget = GetResourceTarget(RES_type.Ticket);
            if (IResourceTarget.IsNullOrDestroyed(resourceTarget))
            {
                resourceTarget = ticketTarget;

            }
            if (IResourceTarget.IsNullOrDestroyed(resourceTarget))
            {
                return DOTween.Sequence();
            }
            if (ticketTarget != null && resourceTarget.GetTransform() == ticketTarget.GetTransform())
            {
                timeShow = Time.time;
                timeTopBar = timeShow;
                if (topBarCanvasGrp != null)
                {
                    topBarCanvasGrp.gameObject.SetActive(true);
                    topBarCanvasGrp.DOKill();
                    topBarCanvasGrp.alpha = 0;
                    topBarCanvasGrp.DOFade(1, .5f).SetId(this);
                }
            }
            target = resourceTarget.GetTransform() as RectTransform;

            complete += (index, total) =>
            {
                if (IResourceTarget.IsNullOrDestroyed(resourceTarget)) return;
                if (index == 0)
                {
                    IResourceTarget.UpdateAmount(RES_type.Ticket);
                }
                resourceTarget.UpdateVisual();
                if (index == total - 1)
                {
                    if (topBarCanvasGrp != null && Mathf.Abs(timeShow - timeTopBar) < .01f)
                    {
                        topBarCanvasGrp.DOFade(0, .5f).SetDelay(.7f).SetId(this);
                    }
                }
            };
        }
        if (target == null) return DOTween.Sequence();
        if (numShowText > 0)
        {
            ShowPopText($"+{numShowText}", scrStartPoint);
        }
        return ticketReceiveFly.Animate(scrStartPoint, target, amount, complete);
    }

    public Tween HeartFly(Vector2 scrStartPoint, RectTransform target, int amount, Action<int, int> complete)
    {
        amount = Mathf.Min(8, amount);
        if (target == null)
        {
            float timeShow = 0;
            IResourceTarget resourceTarget = GetResourceTarget(RES_type.Heart);
            if (IResourceTarget.IsNullOrDestroyed(resourceTarget))
            {
                resourceTarget = heartTarget;

            }
            if (IResourceTarget.IsNullOrDestroyed(resourceTarget))
            {
                return DOTween.Sequence();
            }
            if (heartTarget != null && resourceTarget.GetTransform() == heartTarget.GetTransform())
            {
                timeShow = Time.time;
                timeTopBar = timeShow;
                if (topBarCanvasGrp != null)
                {
                    topBarCanvasGrp.gameObject.SetActive(true);
                    topBarCanvasGrp.DOKill();
                    topBarCanvasGrp.alpha = 0;
                    topBarCanvasGrp.DOFade(1, .5f).SetId(this);
                }
            }
            target = resourceTarget.GetTransform() as RectTransform;

            complete += (index, total) =>
            {
                if (IResourceTarget.IsNullOrDestroyed(resourceTarget)) return;
                if(index == 0)
                {
                    IResourceTarget.UpdateAmount(RES_type.Ticket);
                }
                resourceTarget.UpdateVisual();
                if (index == total - 1)
                {
                    if (topBarCanvasGrp != null && Mathf.Abs(timeShow - timeTopBar) < .01f)
                    {
                        topBarCanvasGrp.DOFade(0, .5f).SetDelay(.7f).SetId(this);
                    }
                }
            };
        }
        if (target == null) return DOTween.Sequence();
        return heartReceiveFly.Animate(scrStartPoint, target, amount, complete);
    }

    public void ShowPopText(string msg, Vector2 scrPoint,int layerSorting = 160)
    {
        popTextController.ShowPopText(msg, scrPoint,layerSorting);
    }
    public void ShowPopTextReward(DataResource dataResource, Vector2 scrPoint,float scale =1)
    {
        popTextRewardController.ShowPopText(dataResource, scrPoint,scale);
    }



    public static Vector2 PositionInsideContainer(RectTransform target, RectTransform container)
    {
        Vector2 result = Vector2.zero;


        if (target.rect.height > container.rect.height)
        {
            return container.anchoredPosition;
        }
        Vector2 anchorContainer = container.anchoredPosition;
        float maxY = anchorContainer.y + container.rect.height / 2 - target.rect.height / 2;
        float minY = anchorContainer.y - container.rect.height / 2 + target.rect.height / 2;
        result.y = Mathf.Clamp(result.y, minY, maxY);



        return result;
    }
    /// <summary>
    /// Normalize data
    /// </summary>
    /// <param name="dataResources"></param>
    /// <returns></returns>
    public DataResource[] StandardizationDataResources(DataResource[] dataResources)
    {
        List<DataResource> resources = new List<DataResource>();
        for (int i = 0; i < listPriorityRestype.Count; i++)
        {
            RES_type rES_Type = listPriorityRestype[i];
            for (int j = 0; j < dataResources.Length; j++)
            {
                DataResource curData = dataResources[j];
                if (dataResources[j].resType == rES_Type)
                {
                    DataResource data = resources.Find(x => x.resType == rES_Type);
                    if (data == null)
                    {
                        data = new DataResource();
                        data.CopyData(curData);
                        resources.Add(data);
                    }
                    else
                    {
                        data.amount += curData.amount;
                    }
                }
            }
        }
        for (int j = 0; j < dataResources.Length; j++)
        {
            DataResource curData = dataResources[j];
            if (!listPriorityRestype.Contains(curData.resType))
            {
                DataResource data = resources.Find(x => x.resType == curData.resType);
                if (data == null)
                {
                    data = new DataResource();
                    data.CopyData(curData);
                    resources.Add(data); ;
                }
                else
                {
                    data.amount += curData.amount;
                }
            }
        }
        return resources.ToArray();
    }
}
