using System;
using time;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using mygame.sdk;
using System.Collections.Generic;
using DG.Tweening;

public class HeartDisplay : MonoBehaviour, IResourceTarget
{
    [SerializeField] Text heartRecoverTimeText;
    [SerializeField] Text heartText;
    [SerializeField] Text heartNotifyText;
    [SerializeField] GameObject objNotifyHeart;
    [SerializeField] GameObject objInfinity;
    [SerializeField] Button heartBtn;
    [SerializeField] Image Icon;
    [SerializeField] bool inGame = false;
    private IDisposable subHeartReceive;
    private void Start()
    {
        if (HeartManager.IsActiveHeart)
        {
            if (heartBtn != null)
            {
                heartBtn.onClick.AddListener(OnClickHeartBar);
            }
            if (heartNotifyText != null && objNotifyHeart != null)
            {
                subHeartReceive = HeartManager.HeartReceive.Subscribe(x =>
                {
                    heartNotifyText.SetValue(x.listDonater.Count);
                    objNotifyHeart.SetActive(x.listDonater.Count > 0);
                });
            }
        }
        else
        {
            if (heartBtn != null)
            {
                heartBtn.gameObject.SetActive(false);
            }
        }
    }
    private void OnDestroy()
    {
        this.DOKill();
        tweenScale?.Kill();
        if (heartNotifyText != null && objNotifyHeart != null)
        {
            if (subHeartReceive != null)
            {
                subHeartReceive.Dispose();
            }
        }
    }

    private const float UPDATE_INTERVAL = 0.25f;
    private float _updateTimer = UPDATE_INTERVAL;

    private void Update()
    {
        _updateTimer += Time.deltaTime;
        if (_updateTimer < UPDATE_INTERVAL) return;

        _updateTimer -= UPDATE_INTERVAL;
        
        if (HeartManager.IsActiveHeart)
        {
            if (heartText != null && heartRecoverTimeText != null)
            {
                var isPlaying = inGame && GameManager.GameState != GameState.Complete;/*&& GameManager.GameState != GameState.Defeat && GameManager.GameState != GameState.WaitRevive*/;
                if (HeartManager.InfinityEndTime > MGTime.GetUtcTime())
                {
                    heartText.gameObject.SetActive(false);
                    objInfinity.SetActive(true);
                }
                else
                {
                    if (isPlaying)
                    {
                        heartText.SetValue(HeartManager.HeartInGame);
                    }
                    else
                    {
                        heartText.SetValue(HeartManager.Heart);
                    }

                    heartText.gameObject.SetActive(true);
                    objInfinity.SetActive(false);
                }

                string txt = HeartManager.Instance.GetTimeRemaningText(isPlaying ? HeartManager.HeartInGame : -1);
                if (txt.Length < 6 && heartRecoverTimeText.fontSize != 52)
                {
                    heartRecoverTimeText.fontSize = 52;
                }
                else if (txt.Length < 8 && txt.Length >= 6 && heartRecoverTimeText.fontSize != 34)
                {
                    heartRecoverTimeText.fontSize = 34;
                }
                else if (txt.Length >= 8 && heartRecoverTimeText.fontSize != 30)
                {
                    heartRecoverTimeText.fontSize = 30;
                }

                heartRecoverTimeText.SetValue(txt);
            }
        }
    }
    public void OnClickHeartBar()
    {
        if (HeartManager.Heart >= HeartManager.MAX_HEART || HeartManager.Instance.HeartState == EHeartState.Unlimited)
        {
            // if (HeartManager.HeartReceive.Value.amount > 0 && GuildManager.CF_LevelUnlockGuild >0)
            // {
            //     UIReceiveHeart uiReceiveHeart = UIManager.Instance.ShowPopup<UIReceiveHeart>(null);
            // }
            // else
            // {
            UIGuildMoreLives uiMoreLives = UIManager.Instance.ShowPopup<UIGuildMoreLives>(null);
            // }
        }
        else
        {
            UIGuildMoreLives uiMoreLives = UIManager.Instance.ShowPopup<UIGuildMoreLives>(null);
        }
    }
    
    private static List<RES_type> rES_Types = new List<RES_type>() { RES_type.Heart, RES_type.UnlimitedHeart };
    public List<RES_type> GetResourceTypes()
    {
        return rES_Types;
    }

    public Transform GetTransform()
    {
        return Icon.transform;
    }
    Tween tweenScale = null;

    public void UpdateVisual()
    {
        if (this == null || transform == null) return;
        tweenScale?.Kill();
        tweenScale = transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.05f).SetId(this).OnComplete(() =>
        {
            if (this != null && transform != null)
            {
                transform.DOScale(Vector3.one, 0.03f).SetId(this);
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

    
}