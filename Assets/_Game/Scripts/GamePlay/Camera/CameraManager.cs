using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using master;
using UniRx;
using Observer = master.Observer;
using Random = UnityEngine.Random;

public class CameraManager : master.Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera boosterCamera;
    public Camera[] CamerasGame;
 
    Tween TW_Shaking;

    [Header("Config")] [SerializeField] private float streakDecayTime = 3f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeRate = 0.2f;
    [SerializeField] private AnimationCurve shakeCurve;

    public int currentStreak = 0;
    private float lastHitTime;
    private Tween streakDecayTween;

    public Action<int> OnStreakChanged;

    private Tween cameraShakeTween;
    private float originalSize;
    private float originalFOV;
    [Header("Config")]
    public float shakeStrength = 0.2f;
    public int vibrations = 10;
    public float randomness = 40f;

    private Vector3 originalPos;
    private float shakeTimer;
    private bool isShaking;
    private float interval;
    private float nextShakeTime;
    private bool canShake = true;

    private float cameraOrinalSize;
    private IDisposable screenResizeSub;

    private void Start()
    {
        cameraOrinalSize = mainCamera.orthographicSize;
        OnScreenChange();
        var ob1 = Observer.GetObservable(ObserverName.screen_resize, 0);
        screenResizeSub = ob1.Subscribe(x => { OnScreenChange(); });
    }

    private void OnScreenChange()
    {
        var defaultPos = new Vector3(0, 25, -14.5f);
        if (Screen.safeArea.yMax < Screen.height)
        {
            var safeArea = Screen.safeArea;
            var screen = new Vector2(Screen.width, Screen.height);

            // 1. Setup and apply safe area
            var heightArea = (screen.y - safeArea.yMax) / 2.1f;
            var offset = mainCamera.ScreenToWorldPoint(new Vector2(0, heightArea)) - mainCamera.ScreenToWorldPoint(new Vector2(0, 0));
            var position = defaultPos;
            transform.position = position + offset;
            float ratio = safeArea.height / safeArea.width;
            if (ratio > 2.5f)
            {
                SetCamera(cameraOrinalSize + 2.5f);
            }else if (ratio > 2.3f)
            {
                SetCamera(cameraOrinalSize + 1);
            }
            else
            {
                SetCamera(cameraOrinalSize);
            }
        }
        else
        {
            transform.position = defaultPos;
        }
        originalPos = mainCamera.transform.localPosition;
        var uiCamera = UIManager.Instance.canvas.worldCamera;
        uiCamera.transform.position = mainCamera.transform.position;
    }
    
    public void ShakeCamera(float _duration = .5f, float _strength = .2f, int _vib = 10, float _randomness = 40f)
    {
        if (isShaking) return;
        shakeDuration = _duration;
        shakeStrength = _strength;
        vibrations = _vib;
        randomness = _randomness;
        
        shakeTimer = 0f;
        interval = _duration / Mathf.Max(1, _vib);
        nextShakeTime = 0f;
        isShaking = true;
    }

    void Update()
    {
        if (!isShaking || !canShake) return;

        shakeTimer += Time.deltaTime;

        if (shakeTimer < shakeDuration)
        {
            // rung theo nhịp vibration
            if (shakeTimer >= nextShakeTime)
            {
                nextShakeTime += interval;

                // hướng ngẫu nhiên trong không gian
                Vector3 randomDir = Random.onUnitSphere;
                randomDir.y *= 0.5f; // bớt rung theo trục Y cho đỡ khó chịu
                mainCamera.transform.localPosition = originalPos + randomDir * shakeStrength;

                // thêm chút ngẫu nhiên vào cường độ
                shakeStrength *= Random.Range(0.8f, 1.0f);
            }
        }
        else
        {
            // hoàn tất, trả về vị trí gốc
            mainCamera.transform.localPosition = originalPos;
            isShaking = false;
        }
    }

    public void Reset()
    {
        currentStreak = 0;
        lastHitTime = 0;
        streakDecayTween?.Kill();
    }

    public void Increment()
    {
        currentStreak++;
        lastHitTime = Time.time;

        //streakDecayTween?.Kill();
        /*streakDecayTween =*/
        DOVirtual.DelayedCall(streakDecayTime, () =>
        {
            if (currentStreak < 0) return;
            currentStreak--;
        }).SetId(this);

        // Trigger camera shake
        if (ShouldTriggerShake(currentStreak))
        {
            TriggerCameraShake();
        }

        OnStreakChanged?.Invoke(currentStreak);
    }

    private bool ShouldTriggerShake(int streak)
    {
        return true;
        return streak % 3 == 0;
    }


    private void TriggerCameraShake()
    {
        float intensity = shakeCurve.Evaluate(currentStreak / 10f) * shakeRate;
        if (intensity <= 0) return;
        ShakeCamera(_duration: shakeDuration, _strength: intensity);    
    }


    /*public void ShakeCamera(float _duration = .5f, float _strength = .2f, int _vib = 10, float _randomness = 40f)
    {
        if (TW_Shaking != null && TW_Shaking.IsActive())
        {
            return;
        }

        Vector3 intialPos = mainCamera.transform.position;
        TW_Shaking = mainCamera.transform
            .DOShakePosition(_duration, _strength, _vib, _randomness)
            .SetEase(Ease.Linear)
            .OnComplete(() => { mainCamera.transform.position = intialPos; });
    }*/

    public void MoveCamera(Vector3 offSet, float duration, float delay, Action onDone)
    {
        canShake = offSet != Vector3.zero;
        mainCamera.transform.DOKill();
        mainCamera.transform.DOLocalMove(originalPos + offSet, duration).SetDelay(delay).SetEase(Ease.OutQuart).SetId(this).OnComplete(() => onDone?.Invoke());
    }
    
    public Vector3 WorldToScreenOffset(Vector3 offSet)
    {
        return mainCamera.WorldToScreenPoint(originalPos + offSet) - mainCamera.WorldToScreenPoint(originalPos);
    }

    public void SetCamera(float size)
    {
        mainCamera.orthographicSize = size;
        foreach(var cam in CamerasGame)
        {
            cam.orthographicSize = size;
        }
        UIManager.Instance.canvas.worldCamera.orthographicSize = size;
    }

    private void OnDestroy()
    {
        screenResizeSub?.Dispose();
        screenResizeSub = null;
        DOTween.Kill(this);
    }
}