using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class ButtonScaler2 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2 startScale = Vector2.one;
    public Vector2 endScale = new Vector2(0.95f, 0.95f);
    [SerializeField] Transform targetTF;
    [SerializeField] protected UnityEvent eventOnPointDown;
    [SerializeField] protected UnityEvent eventOnPointUp;
    [SerializeField] private bool isActiveSound = true;
    [SerializeField] private bool ignoreVib = false;

    private bool isPointerDown = false;
    private bool isPointerInside = false;

    private void Awake()
    {
        if (targetTF == null)
        {
            targetTF = transform;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        targetTF.DOScale(endScale * .9975f, 0.125f).SetEase(Ease.OutQuad).SetUpdate(true).SetId(this).SetTarget("Button");
        eventOnPointDown?.Invoke();
        isPointerInside = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;
        isPointerDown = false;
        targetTF.DOScale(startScale, 0.125f).SetEase(Ease.OutQuad).SetUpdate(true).SetId(this).SetTarget("Button");
        eventOnPointUp?.Invoke();
        if (eventData.dragging)
        {
            return;
        }
        if (!isPointerInside)
        {
            return;
        }
        if (isActiveSound)
        {
            AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click);
        }
        if (!ignoreVib)
        {
            AudioManager.Instance.PlayVibrate();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
    }

    private void OnDisable()
    {
        targetTF.localScale = startScale;
        this.DOKill();
        isPointerDown = false;
        isPointerInside = false;
    }
}
