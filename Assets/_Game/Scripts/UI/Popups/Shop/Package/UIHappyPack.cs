using mygame.sdk;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;


public class UIHappyPack : PopupUI
{
    [SerializeField] private CanvasPropertyOverrider canvasPropertyOverrider;
    [SerializeField] UIPackage uiPackage;
    [SerializeField] SkeletonGraphic skeleton;
    
    [SerializeField] protected Text txtTime;

    private PackageData.PackageInfo packageInfo;
    
    protected long timeStart;
    protected long timeActive;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        uiPackage.onBuySuccess += Hide;
        PlayOpen();
        skeleton.AnimationState.Complete += OnAnimationComplete;
        uiPackage.SetUp(30);
        packageInfo = uiPackage.baseItem;
    }
    
    public virtual void SetTimeText(long start, long active)
    {
        timeStart = start;
        timeActive = active;
    }

    private void Update()
    {
        if (txtTime)
        {
            txtTime.CountTime(timeStart, timeActive, () => { txtTime.text = "Finish"; });
        }
    }

    public void PlayOpen()
    {
        skeleton.AnimationState.SetAnimation(0, "open", false);
    }

    private void OnAnimationComplete(TrackEntry entry)
    {
        // Khi anim "Open" chạy xong → tự về "Idle"
        if (entry.Animation.Name == "open")
        {
            skeleton.AnimationState.SetAnimation(0, "idle_3", true);
        }
    }

    public void IAPShowAction(LogEvent.IAP_ShowAction showAction)
    {
        uiPackage.IAPShowAction(showAction, LogEvent.IAP_ShowPosition.home_popup);
    }
}