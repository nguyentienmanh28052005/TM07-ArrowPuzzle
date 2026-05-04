using System.Collections;
using DG.Tweening;
using Pixelplacement;
using UnityEngine;

public enum TransitionStyle
{
    ClassicFade,        // Mờ dần
    SlideLeft,          // Vuốt từ phải sang trái
    BouncyZoom,         // Nảy to rồi xẹp
    SlideRight,         // Vuốt từ trái sang phải
    SlideUp,            // Vuốt từ dưới lên
    SlideDown,          // Vuốt từ trên xuống
    ZoomFade,           // Zoom nhẹ + fade
    SpinZoom,           // Xoay + zoom
    DiagonalWipe,       // Vuốt chéo
    CurtainHorizontal,  // Rèm ngang (scale X)
    SlideLeftOvershoot, // Vuốt trái có quăng
    SlideRightOvershoot,// Vuốt phải có quăng
    SlideUpBounce,      // Vuốt lên nảy
    SlideDownBounce,    // Vuốt xuống nảy
    SpinGrow,           // Xoay + nở
    SpinShrink,         // Xoay + thu
    DriftFade,          // Trôi + fade
    DiagonalWipeReverse,// Vuốt chéo ngược
    CurtainVertical,    // Rèm dọc (scale Y)
    FlashZoom           // Flash + zoom
}

public class TransitionManager : Singleton<TransitionManager>
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup fadeGroup;
    [Tooltip("Kéo object 'Fade' (Image) ở trong Canvas vào đây")]
    [SerializeField] private RectTransform fadeGraphicRect; 

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private float standingPadding = 0f;
    [SerializeField] private TransitionStyle defaultStyle = TransitionStyle.SlideLeft;

    private bool _isTransitioning;
    private int _holdCount;
    private Vector2 _screenCenter = Vector2.zero;
    private Vector2 _screenRight;
    private Vector2 _screenLeft;
    private Vector2 _screenUp;
    private Vector2 _screenDown;
    private Vector2 _screenUpRight;
    private Vector2 _screenDownLeft;
    private Vector2 _screenUpLeft;
    private Vector2 _screenDownRight;

    public bool IsTransitioning => _isTransitioning;
    public bool IsHeld => _holdCount > 0;
    public event System.Action<bool> TransitionStateChanged;

    public float TransitionDuration => transitionDuration;

    private void Start()
    {
        RefreshScreenPositions();
    }

    public void RequestHold() => _holdCount++;
    public void ReleaseHold() { if (_holdCount > 0) _holdCount--; }

    public void TransitionToScreen(ScreenType target, bool force = false, TransitionStyle? overrideStyle = null)
    {
        if (_isTransitioning) return;
        TransitionStyle styleToUse = overrideStyle ?? defaultStyle;
        StartCoroutine(TransitionRoutine(target, force, styleToUse));
    }

    private IEnumerator TransitionRoutine(ScreenType target, bool force, TransitionStyle style)
    {
        SetTransitioning(true);

        RefreshScreenPositions();

        if (fadeGroup == null)
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.ShowScreen(target, force);
            }
            SetTransitioning(false);
            yield break;
        }

        if (fadeGraphicRect == null && style != TransitionStyle.ClassicFade)
        {
            style = TransitionStyle.ClassicFade;
        }

        fadeGroup.gameObject.SetActive(true);
        fadeGroup.blocksRaycasts = true;
        fadeGroup.DOKill();
        if (fadeGraphicRect != null) fadeGraphicRect.DOKill();
        ResetFadeGraphic();

        // ==========================================
        // PHASE 1: TRANSITION IN (Che màn hình lại)
        // ==========================================
        DG.Tweening.Tween inTween = BuildIntroTween(style);
        if (inTween != null) yield return inTween.SetUpdate(true).WaitForCompletion();

        // ==========================================
        // PHASE 2: SWITCH SCREEN & WAIT
        // ==========================================
        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.ShowScreen(target, force);
        }

        // Chờ các hệ thống khác (như LevelLoader) load xong map
        while (_holdCount > 0)
        {
            yield return null;
        }

        if (standingPadding > 0f)
        {
            yield return new WaitForSecondsRealtime(standingPadding);
        }

        // ==========================================
        // PHASE 3: TRANSITION OUT (Mở màn hình ra)
        // ==========================================
        DG.Tweening.Tween outTween = BuildOutroTween(style);
        if (outTween != null) yield return outTween.SetUpdate(true).WaitForCompletion();

        // Reset trạng thái
        fadeGroup.blocksRaycasts = false;
        fadeGroup.gameObject.SetActive(false);
        ResetFadeGraphic();

        SetTransitioning(false);
    }

    private void SetTransitioning(bool value)
    {
        _isTransitioning = value;
        TransitionStateChanged?.Invoke(value);
    }

    private void RefreshScreenPositions()
    {
        float screenWidth = Screen.width * 2f;
        float screenHeight = Screen.height * 2f;

        _screenRight = new Vector2(screenWidth, 0f);
        _screenLeft = new Vector2(-screenWidth, 0f);
        _screenUp = new Vector2(0f, screenHeight);
        _screenDown = new Vector2(0f, -screenHeight);
        _screenUpRight = new Vector2(screenWidth, screenHeight);
        _screenDownLeft = new Vector2(-screenWidth, -screenHeight);
        _screenUpLeft = new Vector2(-screenWidth, screenHeight);
        _screenDownRight = new Vector2(screenWidth, -screenHeight);
    }

    private void ResetFadeGraphic()
    {
        if (fadeGraphicRect == null) return;

        fadeGraphicRect.anchoredPosition = _screenCenter;
        fadeGraphicRect.localScale = Vector3.one;
        fadeGraphicRect.localRotation = Quaternion.identity;
    }

    private DG.Tweening.Tween BuildIntroTween(TransitionStyle style)
    {
        switch (style)
        {
            case TransitionStyle.ClassicFade:
                fadeGroup.alpha = 0f;
                return fadeGroup.DOFade(1f, transitionDuration).SetEase(Ease.InOutQuad);

            case TransitionStyle.SlideLeft:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenRight;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutExpo);

            case TransitionStyle.BouncyZoom:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = Vector3.zero;
                return fadeGraphicRect.DOScale(Vector3.one * 1.5f, transitionDuration).SetEase(Ease.OutBack, 1.5f);

            case TransitionStyle.SlideRight:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenLeft;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutExpo);

            case TransitionStyle.SlideUp:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenDown;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutExpo);

            case TransitionStyle.SlideDown:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenUp;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutExpo);

            case TransitionStyle.ZoomFade:
                fadeGroup.alpha = 0f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = Vector3.one * 1.2f;
                Sequence zoomFadeIn = DOTween.Sequence();
                zoomFadeIn.Join(fadeGroup.DOFade(1f, transitionDuration).SetEase(Ease.OutQuad));
                zoomFadeIn.Join(fadeGraphicRect.DOScale(Vector3.one, transitionDuration).SetEase(Ease.OutQuad));
                return zoomFadeIn;

            case TransitionStyle.SpinZoom:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = Vector3.one * 0.2f;
                fadeGraphicRect.localRotation = Quaternion.Euler(0f, 0f, -45f);
                Sequence spinIn = DOTween.Sequence();
                spinIn.Join(fadeGraphicRect.DOScale(Vector3.one, transitionDuration).SetEase(Ease.OutBack));
                spinIn.Join(fadeGraphicRect.DORotate(Vector3.zero, transitionDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
                return spinIn;

            case TransitionStyle.DiagonalWipe:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenUpRight;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutExpo);

            case TransitionStyle.CurtainHorizontal:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = new Vector3(0f, 1f, 1f);
                return fadeGraphicRect.DOScaleX(1f, transitionDuration).SetEase(Ease.OutQuad);

            case TransitionStyle.SlideLeftOvershoot:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenRight;
                fadeGraphicRect.localRotation = Quaternion.Euler(0f, 0f, 5f);
                Sequence leftOver = DOTween.Sequence();
                leftOver.Join(fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutBack, 1.6f));
                leftOver.Join(fadeGraphicRect.DORotate(Vector3.zero, transitionDuration).SetEase(Ease.OutQuad));
                return leftOver;

            case TransitionStyle.SlideRightOvershoot:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenLeft;
                fadeGraphicRect.localRotation = Quaternion.Euler(0f, 0f, -5f);
                Sequence rightOver = DOTween.Sequence();
                rightOver.Join(fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutBack, 1.6f));
                rightOver.Join(fadeGraphicRect.DORotate(Vector3.zero, transitionDuration).SetEase(Ease.OutQuad));
                return rightOver;

            case TransitionStyle.SlideUpBounce:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenDown;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutBounce);

            case TransitionStyle.SlideDownBounce:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenUp;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutBounce);

            case TransitionStyle.SpinGrow:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = Vector3.zero;
                fadeGraphicRect.localRotation = Quaternion.Euler(0f, 0f, -180f);
                Sequence spinGrow = DOTween.Sequence();
                spinGrow.Join(fadeGraphicRect.DOScale(Vector3.one, transitionDuration).SetEase(Ease.OutBack, 1.4f));
                spinGrow.Join(fadeGraphicRect.DORotate(Vector3.zero, transitionDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
                return spinGrow;

            case TransitionStyle.SpinShrink:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = Vector3.one * 2f;
                fadeGraphicRect.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Sequence spinShrink = DOTween.Sequence();
                spinShrink.Join(fadeGraphicRect.DOScale(Vector3.one, transitionDuration).SetEase(Ease.OutCubic));
                spinShrink.Join(fadeGraphicRect.DORotate(Vector3.zero, transitionDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));
                return spinShrink;

            case TransitionStyle.DriftFade:
                fadeGroup.alpha = 0f;
                fadeGraphicRect.anchoredPosition = _screenRight;
                fadeGraphicRect.localRotation = Quaternion.Euler(0f, 0f, 10f);
                fadeGraphicRect.localScale = Vector3.one * 1.1f;
                Sequence driftIn = DOTween.Sequence();
                driftIn.Join(fadeGroup.DOFade(1f, transitionDuration * 0.7f).SetEase(Ease.OutQuad));
                driftIn.Join(fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutQuad));
                driftIn.Join(fadeGraphicRect.DORotate(Vector3.zero, transitionDuration).SetEase(Ease.OutQuad));
                driftIn.Join(fadeGraphicRect.DOScale(Vector3.one, transitionDuration).SetEase(Ease.OutQuad));
                return driftIn;

            case TransitionStyle.DiagonalWipeReverse:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenDownRight;
                return fadeGraphicRect.DOAnchorPos(_screenCenter, transitionDuration).SetEase(Ease.OutExpo);

            case TransitionStyle.CurtainVertical:
                fadeGroup.alpha = 1f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = new Vector3(1f, 0f, 1f);
                return fadeGraphicRect.DOScaleY(1f, transitionDuration).SetEase(Ease.OutQuad);

            case TransitionStyle.FlashZoom:
                fadeGroup.alpha = 0f;
                fadeGraphicRect.anchoredPosition = _screenCenter;
                fadeGraphicRect.localScale = Vector3.one * 0.6f;
                Sequence flashIn = DOTween.Sequence();
                flashIn.Join(fadeGroup.DOFade(1f, transitionDuration * 0.4f).SetEase(Ease.OutQuad));
                flashIn.Join(fadeGraphicRect.DOScale(Vector3.one * 1.05f, transitionDuration * 0.4f).SetEase(Ease.OutQuad));
                flashIn.Append(fadeGraphicRect.DOScale(Vector3.one, transitionDuration * 0.6f).SetEase(Ease.OutQuad));
                return flashIn;

            default:
                fadeGroup.alpha = 0f;
                return fadeGroup.DOFade(1f, transitionDuration).SetEase(Ease.InOutQuad);
        }
    }

    private DG.Tweening.Tween BuildOutroTween(TransitionStyle style)
    {
        switch (style)
        {
            case TransitionStyle.ClassicFade:
                return fadeGroup.DOFade(0f, transitionDuration).SetEase(Ease.InOutQuad);

            case TransitionStyle.SlideLeft:
                return fadeGraphicRect.DOAnchorPos(_screenLeft, transitionDuration).SetEase(Ease.InExpo);

            case TransitionStyle.BouncyZoom:
                return fadeGraphicRect.DOScale(Vector3.zero, transitionDuration).SetEase(Ease.InBack, 1.5f);

            case TransitionStyle.SlideRight:
                return fadeGraphicRect.DOAnchorPos(_screenRight, transitionDuration).SetEase(Ease.InExpo);

            case TransitionStyle.SlideUp:
                return fadeGraphicRect.DOAnchorPos(_screenUp, transitionDuration).SetEase(Ease.InExpo);

            case TransitionStyle.SlideDown:
                return fadeGraphicRect.DOAnchorPos(_screenDown, transitionDuration).SetEase(Ease.InExpo);

            case TransitionStyle.ZoomFade:
                Sequence zoomFadeOut = DOTween.Sequence();
                zoomFadeOut.Join(fadeGroup.DOFade(0f, transitionDuration).SetEase(Ease.InQuad));
                zoomFadeOut.Join(fadeGraphicRect.DOScale(Vector3.one * 1.2f, transitionDuration).SetEase(Ease.InQuad));
                return zoomFadeOut;

            case TransitionStyle.SpinZoom:
                Sequence spinOut = DOTween.Sequence();
                spinOut.Join(fadeGraphicRect.DOScale(Vector3.zero, transitionDuration).SetEase(Ease.InBack, 1.5f));
                spinOut.Join(fadeGraphicRect.DORotate(new Vector3(0f, 0f, 45f), transitionDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
                return spinOut;

            case TransitionStyle.DiagonalWipe:
                return fadeGraphicRect.DOAnchorPos(_screenDownLeft, transitionDuration).SetEase(Ease.InExpo);

            case TransitionStyle.CurtainHorizontal:
                return fadeGraphicRect.DOScaleX(0f, transitionDuration).SetEase(Ease.InQuad);

            case TransitionStyle.SlideLeftOvershoot:
                fadeGraphicRect.localRotation = Quaternion.identity;
                return fadeGraphicRect.DOAnchorPos(_screenLeft, transitionDuration).SetEase(Ease.InBack, 1.4f);

            case TransitionStyle.SlideRightOvershoot:
                fadeGraphicRect.localRotation = Quaternion.identity;
                return fadeGraphicRect.DOAnchorPos(_screenRight, transitionDuration).SetEase(Ease.InBack, 1.4f);

            case TransitionStyle.SlideUpBounce:
                return fadeGraphicRect.DOAnchorPos(_screenUp, transitionDuration).SetEase(Ease.InBack, 1.2f);

            case TransitionStyle.SlideDownBounce:
                return fadeGraphicRect.DOAnchorPos(_screenDown, transitionDuration).SetEase(Ease.InBack, 1.2f);

            case TransitionStyle.SpinGrow:
                Sequence spinGrowOut = DOTween.Sequence();
                spinGrowOut.Join(fadeGraphicRect.DOScale(Vector3.zero, transitionDuration).SetEase(Ease.InBack, 1.5f));
                spinGrowOut.Join(fadeGraphicRect.DORotate(new Vector3(0f, 0f, 180f), transitionDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
                return spinGrowOut;

            case TransitionStyle.SpinShrink:
                Sequence spinShrinkOut = DOTween.Sequence();
                spinShrinkOut.Join(fadeGraphicRect.DOScale(Vector3.one * 2f, transitionDuration).SetEase(Ease.InCubic));
                spinShrinkOut.Join(fadeGraphicRect.DORotate(new Vector3(0f, 0f, -90f), transitionDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
                return spinShrinkOut;

            case TransitionStyle.DriftFade:
                Sequence driftOut = DOTween.Sequence();
                driftOut.Join(fadeGroup.DOFade(0f, transitionDuration * 0.7f).SetEase(Ease.InQuad));
                driftOut.Join(fadeGraphicRect.DOAnchorPos(_screenLeft, transitionDuration).SetEase(Ease.InQuad));
                driftOut.Join(fadeGraphicRect.DORotate(new Vector3(0f, 0f, -10f), transitionDuration).SetEase(Ease.InQuad));
                driftOut.Join(fadeGraphicRect.DOScale(Vector3.one * 1.05f, transitionDuration).SetEase(Ease.InQuad));
                return driftOut;

            case TransitionStyle.DiagonalWipeReverse:
                return fadeGraphicRect.DOAnchorPos(_screenUpLeft, transitionDuration).SetEase(Ease.InExpo);

            case TransitionStyle.CurtainVertical:
                return fadeGraphicRect.DOScaleY(0f, transitionDuration).SetEase(Ease.InQuad);

            case TransitionStyle.FlashZoom:
                Sequence flashOut = DOTween.Sequence();
                flashOut.Join(fadeGroup.DOFade(0f, transitionDuration).SetEase(Ease.InQuad));
                flashOut.Join(fadeGraphicRect.DOScale(Vector3.one * 1.1f, transitionDuration).SetEase(Ease.InQuad));
                return flashOut;

            default:
                return fadeGroup.DOFade(0f, transitionDuration).SetEase(Ease.InOutQuad);
        }
    }
}