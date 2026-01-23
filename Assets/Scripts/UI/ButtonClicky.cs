using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ButtonClicky : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Visual related")]
    [SerializeField] private Sprite _default, _pressed;
    private Image _image;
    
    [Space(10)]
    [Header("Scales")]
    [SerializeField] private float _pointerHoverScale = 1.1f;
    [SerializeField] private float _pointerReleaseScale = 1f;
    [SerializeField] private float _pointerClickScale = 0.9f;

    [Space(10)]
    [Header("Audio Clips")]
    //[SerializeField] private GameEnum.AudioEnum _clickSound;
    // [SerializeField] private AudioEnum _hoverSound;
    
    private RectTransform _rectTransform;
    private float _changeY = 5.6f;
    
   
    
    private Vector3 _originalLocalScale;
    
    private void Awake() 
    {
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
        // _image.sprite = _default;
        
        _originalLocalScale = this.transform.localScale;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerUp");
        //MessageManager.Instance.SendMessage(new Message(ManhMessageType.OnButtonClick));
        //transform.localScale = _originalLocalScale;
        //_image.sprite = _default;
        transform.DOScale(_originalLocalScale, 0.2f);
        // DOTween.Sequence().Append(transform.DOScale(_originalLocalScale, 0.25f))
        //     .OnComplete(() =>
        //     {
        //     });

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        //transform.localScale = _originalLocalScale * _pointerClickScale;
        //_image.sprite = _pressed;
        transform.DOScale(_originalLocalScale * _pointerClickScale, 0.2f);
        // DOTween.Sequence()
        //     .Append(transform.DOScale(_originalLocalScale * _pointerClickScale, 0.45f))
        //     .OnComplete((() =>
        //     {
        //     }));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        //Module.MediumVibrate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        // //AudioManager.Instance.Play(_hoverSound);
        //DOTween.Sequence().Append(transform.DOScale(_originalLocalScale * _pointerHoverScale, 0.2f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
        //DOTween.Sequence().Append(transform.DOScale(_originalLocalScale, 0.2f));
    }

    
    // detect and handle events when a pointer (e.g., mouse cursor or touch) moves over a UI element
    public void OnPointerMove(PointerEventData eventData)
    {
        //Debug.Log("OnPointerMove");
        // transform.localScale = localScaleOld * _pointerHoverScale;
    }
}

