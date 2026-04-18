using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Pixelplacement;

public class ScreenJuiceManager : Singleton<ScreenJuiceManager>
{
    [Header("Camera & Visuals")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image flashOverlay;

    [Header("Damage Profile")]
    [SerializeField] private float dmgShakeDuration = 0.3f;
    [SerializeField] private float dmgShakeStrength = 0.8f;
    [SerializeField] private float dmgHitStop = 0.1f;
    [SerializeField] private Color dmgFlashColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("Combo Profile")]
    [SerializeField] private float comboShakeDuration = 0.15f;
    [SerializeField] private float comboShakeBaseStrength = 0.2f;
    [SerializeField] private float comboHitStop = 0f;
    [SerializeField] private Color comboFlashColor = new Color(1f, 1f, 1f, 0f);

    private Vector3 _preShakePos;
    private DG.Tweening.Tween _shakeTween;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        //if (mainCamera != null) _originalCameraPos = mainCamera.transform.localPosition;
        
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            flashOverlay.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnTakeDamage, PlayDamageJuice);
        //MessageManager.Instance.AddSubscriber(ManhMessageType.OnPlayComboJuice, HandlePlayComboJuice);
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnTakeDamage, PlayDamageJuice);
        //MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnPlayComboJuice, HandlePlayComboJuice);
    }

    public void PlayDamageJuice(object data)
    {
        PlayCustomJuice(dmgShakeDuration, dmgShakeStrength, dmgHitStop, dmgFlashColor);
    }

    public void PlayComboJuice(int comboCount = 2)
    {
        float dynamicStrength = Mathf.Clamp(comboShakeBaseStrength + (comboCount * 0.05f), 0.2f, 0.6f);
        PlayCustomJuice(comboShakeDuration, dynamicStrength, comboHitStop, comboFlashColor);
    }

    public void PlayCustomJuice(float duration, float strength, float hitStop, Color flashColor)
    {
        if (hitStop > 0f) StartCoroutine(HitStopRoutine(hitStop));

        if (mainCamera != null)
        {
            if (_shakeTween == null || !_shakeTween.IsActive())
            {
                _preShakePos = mainCamera.transform.localPosition;
            }

            _shakeTween?.Kill();
            
            mainCamera.transform.localPosition = _preShakePos;
            
            _shakeTween = mainCamera.transform.DOShakePosition(duration, strength, 20, 90f, false, true)
                .SetUpdate(true)
                .OnComplete(() => mainCamera.transform.localPosition = _preShakePos);
        }

        if (flashOverlay != null)
        {
            flashOverlay.DOKill();
            flashOverlay.color = flashColor;
            flashOverlay.DOFade(0f, 0.15f).SetUpdate(true);
        }
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.01f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}