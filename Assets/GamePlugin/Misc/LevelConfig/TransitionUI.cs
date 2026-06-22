using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using mygame.sdk;
using System.Data;
using Random = System.Random;

public class TransitionUI : MonoBehaviour
{
    public Animation anim;   
    public TweenCallback loadAction;
    public TweenCallback loadComplete;
    public TweenCallback doneTransitionCb;
    private bool canPlayFadeSound = false;

    [SerializeField] Transform group;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] spriteBGs;
    
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject loading;
    [SerializeField] private Image imgFill;
    [SerializeField] private RectTransform ball;
    [SerializeField] private Text txtFill;
    [SerializeField] private Text txtDesc;
    

     public TransitionUI Transition(LevelType levelType, TweenCallback onLoad, TweenCallback complete, TweenCallback doneTransition = null, bool canPlayFadeSound = false)
    {
        int index = Mathf.Clamp((int)levelType, 0, spriteBGs.Length - 1);
        spriteRenderer.sprite = spriteBGs[index];
        spriteRenderer.material.SetTexture("_MainTexture", spriteBGs[index].texture);

        imgFill.fillAmount = 0;
        ball.anchoredPosition = new Vector2(Mathf.Clamp(imgFill.fillAmount * 680, 35, 645), 0);
        txtFill.text = "0%";
        loading.SetActive(false);

        this.canPlayFadeSound = canPlayFadeSound;
        gameObject.SetActive(true);
        loadAction = onLoad;
        loadComplete = complete;
        doneTransitionCb = doneTransition;
        return this;
    }

    public void ShowLoading(bool isShowProgressText, string progressText)
    {
        loading.SetActive(true);
        canvasGroup.alpha = 0;
        imgFill.fillAmount = 0;
        ball.anchoredPosition = new Vector2(Mathf.Clamp(imgFill.fillAmount * 680, 35, 645), 0);
        txtFill.gameObject.SetActive(isShowProgressText);
        txtFill.text = progressText;
        txtDesc.text = "";
        canvasGroup.DOFade(1, .25f);
    }
    
    public void UpdateProgress(float progress, string progressText)
    {
        imgFill.fillAmount = progress;
        ball.anchoredPosition = new Vector2(Mathf.Clamp(imgFill.fillAmount * 680, 35, 645), 0);
        txtFill.text = progressText;
    }

    public void UpdateDesc(string desc)
    {
        txtDesc.text = desc;
    }
    public void UpdateDesc(string key,string param)
    {
        txtDesc.SetText(key, StateCapText.None, FormatText.F_String, param);
    }
    
    public void Fade(bool isPlay)
    {
        if (anim == null)
        {
            FadeIn();
            DOVirtual.DelayedCall(.5f, () =>
            {
                FadeOut();
            });
        }
        else
        {
            anim.Play("LoadingFull");
            if(isPlay) PlayAudioIntroLevel(1f);
        }
    }

    public void FadeIn(Action onFadeComplete = null)
    {
        if(anim == null)
        {
            group.localScale = Vector3.zero;
            group.DOScale(1, 0.5f).SetEase(Ease.OutBack);
            DOVirtual.DelayedCall(.17f, () => StartFadeIn(), false);
        }
        else
        {
            anim.Play("LoadingFadeIn");
            DOVirtual.DelayedCall(anim.GetClip("LoadingFadeIn").length - .15f, () => onFadeComplete?.Invoke(), false);
        }
    }
    
    public void FadeOut(bool isPlay = false)
    {
        if(anim == null)
        {
            group.localScale = Vector3.one;
            group.DOScale(0, 0.5f).SetEase(Ease.InBack).SetDelay(.25f);
            DOVirtual.DelayedCall(.05f, () => ActionLoadInvoke(), false);
            DOVirtual.DelayedCall(.25f, () => ActionCompleteInvoke(), false);
            
            DOVirtual.DelayedCall(.55f, () =>
            {
                StartFadeOut();
                if (isPlay) PlayAudioIntroLevel(0.5f);
            }, false);
            DOVirtual.DelayedCall(.75f, () => EndAnim(), false);
        }
        else
        {
            anim.Play("LoadingFadeOut");
            if(isPlay) PlayAudioIntroLevel(1f);
        }
    }

    private void PlayAudioIntroLevel(float delay)
    {
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Car_Horn, delayPlay: delay);
        // int random = UnityEngine.Random.Range(0, 5) + 1;
        // AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Intro_Level + $"_{random}", delayPlay: delay);
    }
    
    public void ActionLoadInvoke()
    {
        loadAction?.Invoke();
    }
    public void ActionCompleteInvoke()
    {
        loadComplete?.Invoke();
    }

    public void StartFadeIn()
    {
        if (canPlayFadeSound)
        {
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Fade_In);
        }
    }

    public void StartFadeOut()
    {
        if (canPlayFadeSound)
        {
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Movement_Swoosh_Slow);
        }
    }


    public void EndAnim()
    {
        doneTransitionCb?.Invoke();
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
