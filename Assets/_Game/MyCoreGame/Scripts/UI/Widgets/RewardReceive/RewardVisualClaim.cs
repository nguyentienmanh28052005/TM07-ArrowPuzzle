using DG.Tweening;
using mygame.sdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardVisualClaim : MonoBehaviour
{
    [SerializeField] VisualClaimBase visualDefault;
    [SerializeField] List<VisualClaimBase> listVisual;
    RectTransform rectTF;
    DataResource dataResource;
    [SerializeField] ParticleSystem fx;
    [SerializeField] GameObject fxBG;
    [SerializeField] Animator animatorHolder;
    public RectTransform RectTF
    {
        get
        {
            if (rectTF == null)
            {
                rectTF = GetComponent<RectTransform>();
            }
            return rectTF;
        }

    }
    public DataResource DataResource { get { return dataResource; } }
    public void ActiveFXBg(bool isActive)
    {
        fxBG.gameObject.SetActive(isActive);
    }
    public void PlayFx()
    {
        fx.Play();
    }
    public void Initialize(DataResource dataResource)
    {
        visualDefault.gameObject.SetActive(false);
        SetData(dataResource);
    }
    public void SetData(DataResource dataResource)
    {
        foreach (var v in listVisual)
        {
            v.gameObject.SetActive(false);
        }
        this.dataResource = dataResource;
        var visual = GetVisual(dataResource.resType);
        visual.gameObject.SetActive(true);
        visual.Init(dataResource);
    }
    public VisualClaimBase GetVisual(mygame.sdk.RES_type rES_Type)
    {
        foreach (var visual in listVisual)
        {
            if (visual.CanVisual(rES_Type))
            {
                return visual;
            }
        }

        return visualDefault;
    }
    public Tween FindTarget(bool isWaiting=true)
    {
        StopAnimTopDown();
        var target = RewardReceivedHub.GetResourceTarget(dataResource.resType);
        if(target == null)
        {
            return AnimDisappear();
        }
        else
        {
            if(dataResource.resType == mygame.sdk.RES_type.GOLD || dataResource.resType == mygame.sdk.RES_type.Ticket)
            {
                return FlyToTarget(target, isWaiting);
            }
            if (dataResource.resType == mygame.sdk.RES_type.Heart || dataResource.resType == mygame.sdk.RES_type.UnlimitedHeart || dataResource.resType == mygame.sdk.RES_type.Star)
            {
                return MoveToTargetSlow(target);
            }
            return MoveToTarget(target);
        }
    }
    public Tween AnimDisappear()
    {
        return transform.DOScale(0, .2f).SetId(this);
    }
    public Tween AnimAppearPunch()
    {
        transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(transform.DOScale(1.1f, .2f).OnComplete(() =>
        {
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Claim);
        }));
        sequence.Append(transform.DOScale(1f, .05f));
        sequence.Append(transform.DOScale(1.05f, .05f));
        sequence.Append(transform.DOScale(1f, .05f));
        return sequence;
    }
    public Tween AnimAppear()
    {
        transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(transform.DOScale(1f, .2f).OnComplete(() =>
        {
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Claim);
        }));
        return sequence;
    }
    public void PlayFX()
    {
        ParticleSystem fx1 = Instantiate(fx, transform);
        fx1.gameObject.SetActive(true);
        fx1.Play();
    }
    public Tween MoveToTarget(IResourceTarget target)
    {
        if (IResourceTarget.IsNullOrDestroyed(target))
        {
            return AnimDisappear();
        }
        Transform targetTransform = target.GetTransform();
        if (targetTransform == null)
        {
            return AnimDisappear();
        }
        Sequence sequence = DOTween.Sequence().SetId(this);
        Vector3 destination = targetTransform.position;
        Vector3 direction = destination - transform.position;
        direction = direction.normalized;

        List<Vector3> path = new List<Vector3>()
        {
             transform.position - direction *2,destination
        };
        sequence.Append(transform.DOMove(transform.position - direction*2, .15f).SetId(this));
        sequence.Append(transform.DOMove(destination, .3f).SetId(this));
        sequence.OnComplete(() =>
        {
            if (this == null) return;
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Match_Pop);
            gameObject.SetActive(false);
            if (RewardReceivedHub.Instance != null && RewardReceivedHub.Instance.Canvas != null)
            {
                ParticleSystem fx1 = Instantiate(fx, RewardReceivedHub.Instance.Canvas.transform);
                fx1.gameObject.SetActive(true);
                fx1.Play();
                fx1.transform.position = fx.transform.position;
            }

            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Jump_End);
            if (!IResourceTarget.IsNullOrDestroyed(target))
            {
                target.UpdateVisual();
            }

            mygame.sdk.GameHelper.Instance.Vibrate(mygame.sdk.Type_vibreate.Vib_Medium);
        });
        return sequence;
    }
    public Tween MoveToTargetSlow(IResourceTarget target)
    {
        if (IResourceTarget.IsNullOrDestroyed(target))
        {
            return AnimDisappear();
        }
        Transform targetTransform = target.GetTransform();
        if (targetTransform == null)
        {
            return AnimDisappear();
        }
        Sequence sequence = DOTween.Sequence().SetId(this);
        Vector3 destination = targetTransform.position;
        Vector3 direction = destination - transform.position;
        direction = direction.normalized;
        sequence.Insert(0.15f,transform.DOMove(destination, .55f).SetId(this).SetEase(Ease.InQuad));

        sequence.Insert(0.1f,transform.DOScale(1.2f, .2f).SetId(this).SetEase(Ease.Linear));
        sequence.Insert(.3f,transform.DOScale(.6f, .4f).SetId(this).SetEase(Ease.Linear));
        sequence.OnComplete(() =>
        {
            if (this == null) return;
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Match_Pop);
            gameObject.SetActive(false);
            if (RewardReceivedHub.Instance != null && RewardReceivedHub.Instance.Canvas != null)
            {
                ParticleSystem fx1 = Instantiate(fx, RewardReceivedHub.Instance.Canvas.transform);
                fx1.gameObject.SetActive(true);
                fx1.Play();
                fx1.transform.position = fx.transform.position;
            }

            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Jump_End);
            if (!IResourceTarget.IsNullOrDestroyed(target))
            {
                target.UpdateVisual();
            }
            mygame.sdk.GameHelper.Instance.Vibrate(mygame.sdk.Type_vibreate.Vib_Medium);
        });
        return sequence;
    }
    public Tween JumpToTarget(IResourceTarget target)
    {
        if (IResourceTarget.IsNullOrDestroyed(target))
        {
            return AnimDisappear();
        }
        Transform targetTransform = target.GetTransform();
        if (targetTransform == null)
        {
            return AnimDisappear();
        }
        Sequence sequence = DOTween.Sequence().SetId(this);
        Vector3 destination = targetTransform.position;
        Vector3 direction = destination - transform.position;
        direction = direction.normalized;

        List<Vector3> path = new List<Vector3>()
        {
             transform.position - direction *2,destination
        };
        sequence.Append(transform.DOJump(destination,1,1, .5f).SetId(this));
        sequence.OnComplete(() =>
        {
            if (this == null) return;
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Match_Pop);
            gameObject.SetActive(false);
            if (RewardReceivedHub.Instance != null && RewardReceivedHub.Instance.Canvas != null)
            {
                ParticleSystem fx1 = Instantiate(fx, RewardReceivedHub.Instance.Canvas.transform);
                fx1.gameObject.SetActive(true);
                fx1.Play();
                fx1.transform.position = fx.transform.position;
            }

            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Jump_End);
            if (!IResourceTarget.IsNullOrDestroyed(target))
            {
                target.UpdateVisual();
            }
            mygame.sdk.GameHelper.Instance.Vibrate(mygame.sdk.Type_vibreate.Vib_Medium);
        });
        sequence.AppendCallback(() =>
        {
            IResourceTarget.UpdateAmount(dataResource.resType);
        });
        return sequence;
    }
    public Tween FlyToTarget(IResourceTarget target,bool isWaiting)
    {
        if (IResourceTarget.IsNullOrDestroyed(target))
        {
            return AnimDisappear();
        }
        Transform targetTransform = target.GetTransform();
        if (targetTransform == null)
        {
            return AnimDisappear();
        }
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(AnimDisappear());
        Vector2 scrPoint = Vector2.zero;
        if (RewardReceivedHub.Instance != null && RewardReceivedHub.Instance.Canvas != null && RewardReceivedHub.Instance.Canvas.worldCamera != null)
        {
            scrPoint = RewardReceivedHub.Instance.Canvas.worldCamera.WorldToScreenPoint(transform.position);
        }
        switch (dataResource.resType)
        {
            case mygame.sdk.RES_type.GOLD:
                sequence.AppendCallback(() => {
                    if (RewardReceivedHub.Instance != null)
                        RewardReceivedHub.Instance.ShowPopText($"+{dataResource.amount}", scrPoint);
                });
                if (!isWaiting)
                {
                    sequence.AppendCallback(()=>CoinFly());
                }
                else
                {
                    sequence.Append(CoinFly());
                }
                break;
            case mygame.sdk.RES_type.Ticket:
                sequence.AppendCallback(() => {
                    if (RewardReceivedHub.Instance != null)
                        RewardReceivedHub.Instance.ShowPopText($"+{dataResource.amount}", scrPoint);
                });
                if (!isWaiting)
                {
                    sequence.AppendCallback(() => TicketFly());
                }
                else
                {
                    sequence.Append(TicketFly());
                }
                break;
        }
        sequence.Play();
        Tween CoinFly()
        {
            if (IResourceTarget.IsNullOrDestroyed(target) || RewardReceivedHub.Instance == null)
            {
                return DOTween.Sequence();
            }
            return RewardReceivedHub.Instance.CoinFly(scrPoint, target.GetTransform() as RectTransform, 5, (index, total) =>
            {
                if (index == 0)
                {
                    IResourceTarget.UpdateAmount(RES_type.GOLD);
                }
                if (!IResourceTarget.IsNullOrDestroyed(target))
                {
                    target.UpdateVisual();
                }
            });
        }
        Tween TicketFly()
        {
            if (IResourceTarget.IsNullOrDestroyed(target) || RewardReceivedHub.Instance == null)
            {
                return DOTween.Sequence();
            }
            return RewardReceivedHub.Instance.TicketFly(scrPoint, target.GetTransform() as RectTransform, 5, (index, total) =>
            {
                if (index == 0)
                {
                    IResourceTarget.UpdateAmount(RES_type.Ticket);
                }
                if (!IResourceTarget.IsNullOrDestroyed(target))
                {
                    target.UpdateVisual();
                }
            });
        }
        return sequence;
    }
    Tween animTopDown;
    public void AnimTopDown()
    {
        //Sequence sequence = DOTween.Sequence().SetId(this);
        //Sequence sequence2 = DOTween.Sequence().SetId(this);
        //sequence.Append(holderReward.DOAnchorPosY(25, .5f).SetEase(Ease.Linear));
        //sequence2.Append(holderReward.DOAnchorPosY(-25, 1f).SetEase(Ease.Linear));
        //sequence2.Append(holderReward.DOAnchorPosY(25, 1f).SetEase(Ease.Linear));
        //sequence2.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        //sequence.Append(sequence2);
        //animTopDown = sequence;
        animatorHolder.SetBool("claim", true);
    }
    public void StopAnimTopDown()
    {
        animatorHolder.SetBool("claim", false);
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
