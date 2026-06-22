using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2 startScale = Vector2.one;
    public Vector2 endScale = new Vector2(0.9f, 0.9f);
    [SerializeField] protected UnityEvent eventOnPointDown;
    [SerializeField] protected UnityEvent eventOnPointUp;
    [SerializeField] protected bool isActiveSound = true;
    [SerializeField] protected bool ignoreVib = false;
    private bool isPointerDown = false;
    private bool isPointerInside = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        transform.DOScale(endScale, 0.075f).SetEase(Ease.OutQuad).SetUpdate(true).SetId(this);
        eventOnPointDown?.Invoke();
        isPointerInside = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;
        isPointerDown = false;
        transform.DOScale(startScale, 0.075f).SetEase(Ease.OutQuad).SetUpdate(true).SetId(this);
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
        transform.localScale = startScale;
        this.DOKill();
        isPointerDown = false;
        isPointerInside = false;
    }
}
