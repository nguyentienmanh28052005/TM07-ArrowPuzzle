using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
public class ItemFlyCurveUI : MonoBehaviour
{
    [SerializeField] List<Image> images;
    List<Animation> animations = new List<Animation>();
    List<Vector2> anchorOrg = new List<Vector2>();
    [SerializeField] Text txtPoint;
    public float jumpPower =.65f;
    public float timeDelay=.08f;
    public float timeFade=.25f;
    [SerializeField] ParticleSystem fxExplosion;
    [SerializeField] Image tagImg;
    [SerializeField] private ParticleSystem fxText;
    Vector2 orgPosTag;
    private void Awake()
    {
        for (int i = 0; i < images.Count; i++)
        {
            images[i].gameObject.SetActive(false);
        }
        tagImg.gameObject.SetActive(false);
        txtPoint.gameObject.SetActive(false);
    }
    void Initialize()
    {
        for (int i = 0; i < images.Count; i++)
        {
            anchorOrg.Add(images[i].rectTransform.anchoredPosition);
        }
        for (int i = 0; i < images.Count; i++)
        {
            animations.Add(images[i].GetComponent<Animation>());
        }
        orgPosTag = tagImg.rectTransform.anchoredPosition;
    }

    public Tween FlyTo(RectTransform target, int count, Action OnCompleted, bool isDoubleReward = false, Action onFirstItemComplete = null)
    {
        if (anchorOrg.Count == 0) Initialize();

        DOTween.Kill(this);
        Color c;

        for (int i = 0; i < images.Count; i++) images[i].gameObject.SetActive(false);

        txtPoint.gameObject.SetActive(true);
        txtPoint.transform.localScale = Vector3.zero;

        tagImg.rectTransform.anchoredPosition = orgPosTag;
        tagImg.gameObject.SetActive(isDoubleReward);

        for (int i = 0; i < count; i++)
        {
            if (i >= images.Count) break;
            var img = images[i];
            img.gameObject.SetActive(true);
            c = img.color; c.a = 0; img.color = c;
            img.rectTransform.anchoredPosition = anchorOrg[i];
            img.DOFade(1, timeFade * 2).SetEase(Ease.Linear).SetDelay(timeDelay * i).SetId(this);
        }

        int value = isDoubleReward ? count / 2 : count;
        txtPoint.SetValue($"+{value}");
        txtPoint.color = Color.white;
        c = txtPoint.color; c.a = 0; txtPoint.color = c;
        c = tagImg.color; c.a = 0; tagImg.color = c;

        Vector2 oldPos = txtPoint.rectTransform.anchoredPosition;
        oldPos.y = 0;
        txtPoint.rectTransform.anchoredPosition = oldPos;

        var sequence = DOTween.Sequence().SetId(this);

        sequence.Insert(0, txtPoint.rectTransform.DOScale(1.1f, timeFade).SetEase(Ease.Linear).OnComplete(() =>
        {
            txtPoint.rectTransform.DOScale(1f, timeFade).SetEase(Ease.Linear).SetId(this);
        }));

        sequence.Insert(timeFade, txtPoint.DOFadeAllShadow(1, timeFade * 1.3f, 0, Ease.OutQuad, this));

        if (isDoubleReward)
        {
            sequence.Insert(timeFade, tagImg.DOFade(1, timeFade * 0.2f).SetId(this));
            sequence.Insert(timeFade * 1.2f, tagImg.transform.DOJump(txtPoint.transform.position, 1, 1, timeFade).OnComplete(() =>
            {
                tagImg.gameObject.SetActive(false);
                txtPoint.SetValue($"+{count}");
                fxText.Play();
                txtPoint.color = Color.green;
                txtPoint.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            }).SetId(this));
        }

        sequence.Append(txtPoint.DOFadeAllShadow(0, timeFade * 3f, timeFade * 2, Ease.Linear, this));
        sequence.Insert(timeFade * 3, txtPoint.rectTransform.DOAnchorPosY(90, timeFade * 4f).SetEase(Ease.Linear).SetId(this));

        // Jump animation sequence
        int countJump = Mathf.Min(count, images.Count);
        float jumpStartDelay = timeFade * 1.75f;
        float jumpInterval = timeDelay;
        float jumpDuration = 0.35f;

        for (int i = 0; i < countJump; i++)
        {
            int index = i;
            float jumpTime = jumpStartDelay + i * jumpInterval;

            sequence.InsertCallback(jumpTime - jumpInterval * 0.25f, () =>
            {
                if (index < animations.Count)
                {
                    animations[index].Play("ScalePunch");
                    AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Fly_In);
                }
            });

            sequence.Insert(jumpTime, images[i].transform.DOJump(target.transform.position, jumpPower, 1, jumpDuration, false)
                .OnStart(() =>
                {
                // Optional: pre-jump logic
            })
                .OnComplete(() =>
                {
                    images[index].gameObject.SetActive(false);

                    target.transform.DOKill();
                    target.transform.localScale = Vector3.one;
                    target.DOScale(1.1f, 0.15f).OnComplete(() =>
                    {
                        target.DOScale(1f, 0.15f).SetTarget(this);
                    }).SetTarget(this);

                    mygame.sdk.GameHelper.Instance.Vibrate(mygame.sdk.Type_vibreate.Vib_Medium);
                    if(index ==0)
                    {
                        onFirstItemComplete?.Invoke();
                    }
                    if (index == countJump - 1)
                    {
                        OnCompleted?.Invoke();
                        fxExplosion.transform.position = target.transform.position;
                        fxExplosion.Play();
                        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Jump_End_1);
                    }
                }).SetId(this));
        }

        sequence.Play();

        return sequence;
    }

    private void OnDisable()
    {
        StopJump();

    }
    public void StopJump() {
        DOTween.Kill(this);
        for (int i = 0; i < images.Count; i++)
        {
            images[i].gameObject.SetActive(false);
        }
        tagImg.gameObject.SetActive(false);
        txtPoint.gameObject.SetActive(false);
    }
}
