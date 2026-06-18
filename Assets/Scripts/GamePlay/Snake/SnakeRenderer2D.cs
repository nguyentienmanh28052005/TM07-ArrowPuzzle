using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public sealed class SnakeRenderer2D
{
    private readonly SnakeBlock _owner;
    private readonly SnakeRuntime _runtime;

    private readonly List<Vector3> _renderPointsCache = new List<Vector3>(100);
    private readonly List<Vector3> _smoothedPointsCache = new List<Vector3>(200);
    private readonly List<float> _renderTrackIdxCache = new List<float>(100);
    private readonly List<List<Vector3>> _visualSegmentsCache = new List<List<Vector3>>(8);
    private readonly List<Vector3[]> _linePositionsArrayCache = new List<Vector3[]>(8);
    private readonly List<LineRenderer> _lineRenderers = new List<LineRenderer>();

    private LineRenderer _lineRenderer;
    private float _originalWidthMultiplier = 1f;
    private Vector3 _originalArrowScale = Vector3.one;
    private Tweener _colorTweener;
    private Color _currentLineColor;
    private Color _lastFocusTargetColor;
    private bool _hasFocusVisualState;
    private bool _isFocusVisualActive;
    private bool _forceRedraw;
    private SpriteRenderer _arrowSpriteRenderer;
    private Material _originalLineMaterial;
    private bool _isFocusingColorTweenRunning;
    private bool _pendingUnfocus;
    private float _pendingUnfocusDuration;
    private bool _isLinePressedMaterialActive;
    private ArrowShadowVisual _arrowShadowVisual;
    private Coroutine _transparentRevealFlashRoutine;

    public SnakeRenderer2D(SnakeBlock owner, SnakeRuntime runtime)
    {
        _owner = owner;
        _runtime = runtime;
    }

    public void SetupLineRenderer()
    {
        _lineRenderer = _owner.GetComponent<LineRenderer>();
        _lineRenderer.startWidth = _owner.lineWidth;
        _lineRenderer.endWidth = _owner.lineWidth;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.alignment = LineAlignment.TransformZ;
        _lineRenderer.textureMode = LineTextureMode.Tile;
        _lineRenderer.numCornerVertices = 0;
        _lineRenderer.numCapVertices = 10;
        _currentLineColor = _owner.snakeColor;
        _lineRenderer.startColor = _owner.snakeColor;
        _lineRenderer.endColor = _owner.snakeColor;
        _lineRenderer.sortingOrder = 10;
        _originalWidthMultiplier = _lineRenderer.widthMultiplier;
        _originalLineMaterial = _lineRenderer.sharedMaterial;

        _lineRenderers.Clear();
        _lineRenderers.Add(_lineRenderer);
    }

    public void InitializeVisualState(Color color, ArrowDir direction)
    {
        _originalArrowScale = _owner.ArrowVisual != null ? _owner.ArrowVisual.localScale : Vector3.one;
        _hasFocusVisualState = false;
        _isFocusVisualActive = false;
        SetColorImmediate(color);
        SetLinePressedMaterial(false, true);
        UpdateVisualRotation(direction);
        RequestRedraw();
    }

    public void RedrawIfNeeded()
    {
        if (_runtime == null) return;
        if (_runtime.IsMoving || _forceRedraw || _runtime.IsSpawning)
        {
            RedrawLine();
            if (!_runtime.IsMoving && !_runtime.IsSpawning)
                _forceRedraw = false;
        }
    }

    public void RequestRedraw()
    {
        _forceRedraw = true;
    }

    public void SetFocusEffect(bool isFocused, float scaleFactor, float duration)
    {
        if (_runtime.IsStoppedByStopBlock) return;

        float targetWidth = isFocused ? (_originalWidthMultiplier * scaleFactor) : _originalWidthMultiplier;

        foreach (LineRenderer lr in _lineRenderers)
        {
            if (lr == null) continue;

            lr.DOKill();
            DOTween.To(() => lr.widthMultiplier, x =>
            {
                lr.widthMultiplier = x;
                RequestRedraw();
            }, targetWidth, duration)
                .SetEase(isFocused ? Ease.OutBack : Ease.OutQuad)
                .SetTarget(lr)
                .SetLink(_owner.gameObject);
        }

        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual != null)
        {
            Vector3 targetScale = isFocused ? (_originalArrowScale * scaleFactor) : _originalArrowScale;
            arrowVisual.DOKill();
            arrowVisual.DOScale(targetScale, duration)
                .SetEase(isFocused ? Ease.OutBack : Ease.OutQuad)
                .SetLink(arrowVisual.gameObject);
        }
    }

    public void SetFocusColor(bool isFocusing, float duration)
    {
        if (_runtime.IsStoppedByStopBlock) return;

        Color targetColor = isFocusing 
            ? _owner.snakeMoveColor 
            : (_runtime.HasCollided ? _owner.snakeTakeHitColor : _owner.snakeColor);

        if (isFocusing)
        {
            _pendingUnfocus = false;
            _isFocusingColorTweenRunning = true;
            SetLinePressedMaterial(true);

            if (_hasFocusVisualState && _isFocusVisualActive == isFocusing && _lastFocusTargetColor == targetColor)
                return;

            _hasFocusVisualState = true;
            _isFocusVisualActive = isFocusing;
            _lastFocusTargetColor = targetColor;

            if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();

            if (duration <= 0f || _currentLineColor == targetColor)
            {
                _currentLineColor = targetColor;
                ApplyColorToAll(_currentLineColor);
                _isFocusingColorTweenRunning = false;
                return;
            }

            _colorTweener = DOTween.To(() => _currentLineColor, x => _currentLineColor = x, targetColor, duration)
                .OnUpdate(() => ApplyColorToAll(_currentLineColor))
                .OnComplete(() =>
                {
                    _isFocusingColorTweenRunning = false;
                    if (_pendingUnfocus)
                    {
                        _pendingUnfocus = false;
                        _hasFocusVisualState = true;
                        _isFocusVisualActive = false;
                        Color restoreColor = _runtime.HasCollided ? _owner.snakeTakeHitColor : _owner.snakeColor;
                        _lastFocusTargetColor = restoreColor;
                        SetLinePressedMaterial(false);
                        RunColorTween(restoreColor, _pendingUnfocusDuration);
                    }
                })
                .SetLink(_owner.gameObject);
        }
        else
        {
            if (_isFocusingColorTweenRunning)
            {
                _pendingUnfocus = true;
                _pendingUnfocusDuration = duration;
            }
            else
            {
                _pendingUnfocus = false;
                SetLinePressedMaterial(false);

                if (_hasFocusVisualState && _isFocusVisualActive == isFocusing && _lastFocusTargetColor == targetColor)
                    return;

                _hasFocusVisualState = true;
                _isFocusVisualActive = isFocusing;
                _lastFocusTargetColor = targetColor;
                RunColorTween(targetColor, duration);
            }
        }
    }

    public void PlayDashReadyVisual(Color highlightColor, float scaleFactor, float duration)
    {
        if (!_runtime.IsInitialized || _runtime.IsMoving || _runtime.IsSpawning || _runtime.IsStoppedByStopBlock) return;

        _hasFocusVisualState = false;
        SetLinePressedMaterial(true);
        SetFocusEffect(true, scaleFactor, duration);
        RunColorTween(highlightColor, duration);
    }

    public void BeginHintGlowVisual(float scaleFactor, float duration)
    {
        if (!_runtime.IsInitialized || _runtime.IsMoving || _runtime.IsSpawning || _runtime.IsStoppedByStopBlock) return;

        _hasFocusVisualState = false;
        SetLinePressedMaterial(true);
        SetFocusEffect(true, scaleFactor, duration);
    }

    public void EndHintGlowVisual(Color restoreColor, float duration)
    {
        if (!_runtime.IsInitialized) return;

        _hasFocusVisualState = false;
        SetLinePressedMaterial(false);
        SetFocusEffect(false, 1f, duration);
        RunColorTween(restoreColor, duration);
    }

    public void SetColorImmediate(Color color)
    {
        _isFocusingColorTweenRunning = false;
        _pendingUnfocus = false;
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        if (_flashTweener != null && _flashTweener.IsActive()) _flashTweener.Kill();
        _hasFocusVisualState = false;
        _currentLineColor = color;
        ApplyColorToAll(_currentLineColor);
    }

    private Tween _flashTweener;

    public void FlashRed(float duration)
    {
        _isFocusingColorTweenRunning = false;
        _pendingUnfocus = false;
        if (!_runtime.IsInitialized || _runtime.IsMoving || _runtime.IsStoppedByStopBlock) return;

        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        if (_flashTweener != null && _flashTweener.IsActive()) _flashTweener.Kill();

        _currentLineColor = _owner.snakeTakeHitColor;
        ApplyColorToAll(_currentLineColor);

        Color restoreColor = _runtime.HasCollided ? _owner.snakeTakeHitColor : _owner.snakeColor;

        _flashTweener = DOTween.To(() => _currentLineColor, x => _currentLineColor = x, restoreColor, duration)
            .OnUpdate(() => ApplyColorToAll(_currentLineColor))
            .SetLink(_owner.gameObject);
    }

    public void BeginEraseVisual()
    {
        _isFocusingColorTweenRunning = false;
        _pendingUnfocus = false;
        if (!_runtime.IsInitialized || _runtime.TotalPoints <= 0) return;

        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();
        if (_flashTweener != null && _flashTweener.IsActive()) _flashTweener.Kill();
        SetLinePressedMaterial(false);

        _runtime.IsBeingErased = true;
        _runtime.EraseTailTrackIdx = _runtime.TotalPoints - 1;

        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual != null)
        {
            arrowVisual.gameObject.SetActive(true);
            arrowVisual.DOKill();
            arrowVisual.localScale = _originalArrowScale;
        }

        RequestRedraw();
    }

    public void EraseVisualAtWorldPosition(Vector3 eraserWorldPosition, float brushRadius)
    {
        if (!_runtime.IsInitialized || _runtime.TotalPoints <= 0) return;
        if (!_runtime.IsBeingErased) BeginEraseVisual();

        float closestTrackIdx = _runtime.GetClosestTrackIndex(eraserWorldPosition);
        float brushTrackRadius = Mathf.Max(0f, brushRadius) * Mathf.Max(1, _runtime.NodesPerUnit);
        float targetTailTrackIdx = Mathf.Clamp(closestTrackIdx - brushTrackRadius, 0f, _runtime.TotalPoints - 1);

        _runtime.EraseTailTrackIdx = Mathf.Min(_runtime.EraseTailTrackIdx, targetTailTrackIdx);
        UpdateArrowEraseVisual(brushTrackRadius);
        RequestRedraw();
    }

    public void CompleteEraseVisual()
    {
        if (!_runtime.IsInitialized) return;

        _runtime.IsBeingErased = true;
        _runtime.EraseTailTrackIdx = 0f;
        HideAllLineRenderers();

        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual != null)
        {
            arrowVisual.DOKill();
            arrowVisual.localScale = Vector3.zero;
        }
    }

    public bool IsVisualAlphaZero(float threshold = 0.01f)
    {
        if (!_runtime.IsInitialized || _runtime.IsBeingErased || _runtime.IsBeingConsumedByBlackHole) return false;

        float maxAlpha = 0f;
        bool hasVisual = false;

        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            LineRenderer lr = _lineRenderers[i];
            if (lr == null) continue;

            maxAlpha = Mathf.Max(maxAlpha, lr.startColor.a, lr.endColor.a);
            hasVisual = true;
        }

        CacheArrowRenderer();
        if (_arrowSpriteRenderer != null)
        {
            maxAlpha = Mathf.Max(maxAlpha, _arrowSpriteRenderer.color.a);
            hasVisual = true;
        }

        if (!hasVisual) maxAlpha = _currentLineColor.a;
        return maxAlpha <= Mathf.Clamp01(threshold);
    }

    public void FlashIfVisualAlphaZero(float flashAlpha, float duration, Color flashTint, float threshold = 0.01f)
    {
        if (!IsVisualAlphaZero(threshold)) return;
        if (_transparentRevealFlashRoutine != null) return;

        _transparentRevealFlashRoutine = _owner.StartCoroutine(TransparentRevealFlashRoutine(flashAlpha, duration, flashTint));
    }

    public void SetLinePressedMaterial(bool isPressed, bool force = false)
    {
        if (_lineRenderer == null) return;

        bool shouldUsePressedMaterial = isPressed && _owner.LinePressedMaterial != null;
        if (!force && _isLinePressedMaterialActive == shouldUsePressedMaterial) return;

        Material targetMaterial = shouldUsePressedMaterial ? _owner.LinePressedMaterial : _originalLineMaterial;
        foreach (LineRenderer lr in _lineRenderers)
        {
            if (lr != null) lr.sharedMaterial = targetMaterial;
        }

        _isLinePressedMaterialActive = shouldUsePressedMaterial;
    }

    public void SetSortingOrder(int sortingOrder)
    {
        foreach (LineRenderer lr in _lineRenderers)
        {
            if (lr != null) lr.sortingOrder = sortingOrder;
        }
    }

    public void ApplyStopBlockVisual()
    {
        SetLinePressedMaterial(false, true);
        Color faded = _owner.snakeColor;
        faded.a = Mathf.Clamp01(_owner.StopBlockAlpha);
        SetColorImmediate(faded);
    }

    public void PrepareArrowForSpawn()
    {
        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual == null || _runtime.CurrentPositions == null || _runtime.CurrentPositions.Length == 0) return;

        arrowVisual.position = _runtime.CurrentPositions[0];
        arrowVisual.localScale = Vector3.zero;
    }

    public void FinishSpawnArrow()
    {
        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual == null) return;

        SyncArrowVisualPosition();
        arrowVisual.DOKill();
        arrowVisual.localScale = Vector3.zero;
        arrowVisual.DOScale(_originalArrowScale, 0.4f)
            .SetEase(Ease.OutBack)
            .SetLink(arrowVisual.gameObject);
    }

    public void ShowArrowAtOriginalScale()
    {
        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual == null) return;

        arrowVisual.gameObject.SetActive(true);
        arrowVisual.localScale = _originalArrowScale;
    }

    public IEnumerator PlayBlackHoleConsumeShrink()
    {
        float duration = Mathf.Max(0.01f, _owner.BlackHoleConsumeShrinkDuration);
        float elapsed = 0f;

        CacheArrowRenderer();

        Transform arrowVisual = _owner.ArrowVisual;
        Vector3 arrowStartScale = arrowVisual != null ? arrowVisual.localScale : Vector3.one;
        _runtime.IsBeingConsumedByBlackHole = true;
        _runtime.BlackHoleConsumeHeadTrackIdx = -_runtime.AccumulatedShift;
        float startTailTrackIdx = -_runtime.AccumulatedShift + (_runtime.TotalPoints - 1);
        _runtime.BlackHoleConsumeTailTrackIdx = startTailTrackIdx;

        while (elapsed < duration)
        {
            elapsed += Mathf.Min(Time.deltaTime, 0.033f);
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);

            _runtime.BlackHoleConsumeTailTrackIdx = Mathf.Lerp(startTailTrackIdx, _runtime.BlackHoleConsumeHeadTrackIdx, eased);
            if (arrowVisual != null)
                arrowVisual.localScale = Vector3.Lerp(arrowStartScale, Vector3.zero, Mathf.Clamp01(eased * 1.35f));

            RequestRedraw();
            RedrawLine();
            yield return null;
        }

        _runtime.BlackHoleConsumeTailTrackIdx = _runtime.BlackHoleConsumeHeadTrackIdx;
        RedrawLine();
        HideAllLineRenderers();
        if (arrowVisual != null) arrowVisual.localScale = Vector3.zero;
    }

    public void CreateOrRefreshArrowShadow(ArrowDir direction, Color snakeColor)
    {
        if (!_runtime.HasArrowShadow || _runtime.LogicNodes == null || _runtime.LogicNodes.Count == 0)
        {
            if (_arrowShadowVisual != null)
            {
                _arrowShadowVisual.DestroyIfNotCounting();
                _arrowShadowVisual = null;
            }
            return;
        }

        if (_arrowShadowVisual == null)
        {
            GameObject shadowObject = new GameObject("ArrowShadow");
            shadowObject.transform.SetParent(_owner.transform, false);
            shadowObject.transform.localPosition = Vector3.zero;
            shadowObject.transform.localRotation = Quaternion.identity;
            shadowObject.transform.localScale = Vector3.one;
            _arrowShadowVisual = shadowObject.AddComponent<ArrowShadowVisual>();
        }

        _runtime.ArrowShadowReleased = false;
        _arrowShadowVisual.Initialize(
            _runtime.LogicNodes,
            direction,
            snakeColor,
            _owner.ArrowVisual,
            _lineRenderer,
            _owner.lineWidth,
            _owner.ArrowShadowWidthMultiplier,
            _owner.ArrowShadowAlpha,
            _owner.ArrowShadowHeadScaleMultiplier,
            _owner.ArrowShadowTurnsToFade);
    }

    public void BeginArrowShadowFadeAfterOwnerReleased()
    {
        if (!_runtime.HasArrowShadow || _arrowShadowVisual == null || _runtime.ArrowShadowReleased) return;

        _runtime.ArrowShadowReleased = true;
        Transform stableParent = _owner.transform.parent;
        _arrowShadowVisual.BeginFadeAfterOwnerReleased(stableParent);
        _arrowShadowVisual = null;
    }

    public void OnOwnerDestroyed()
    {
        if (_arrowShadowVisual != null && !_runtime.ArrowShadowReleased)
        {
            _arrowShadowVisual.DestroyIfNotCounting();
            _arrowShadowVisual = null;
        }
    }

    public void SyncArrowVisualPosition()
    {
        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual != null && _runtime.CurrentPositions != null && _runtime.CurrentPositions.Length > 0)
            arrowVisual.position = _runtime.CurrentPositions[0];
    }

    public void UpdateVisualRotation(ArrowDir direction)
    {
        ApplyArrowRotation(_owner.ArrowVisual, direction);
    }

    public void UpdateArrowVisualRotation(ArrowDir direction)
    {
        ApplyArrowRotation(_owner.ArrowVisual, direction);
    }

    public void RedrawLine()
    {
        if (_lineRenderer == null || _runtime.TotalPoints <= 0 || !_runtime.IsInitialized) return;

        float headTrackIdx = -_runtime.AccumulatedShift;
        float tailTrackIdx = -_runtime.AccumulatedShift + (_runtime.TotalPoints - 1);

        if (_runtime.IsBeingConsumedByBlackHole)
        {
            headTrackIdx = _runtime.BlackHoleConsumeHeadTrackIdx;
            tailTrackIdx = Mathf.Max(headTrackIdx, _runtime.BlackHoleConsumeTailTrackIdx);
        }
        else if (_runtime.IsSpawning)
        {
            tailTrackIdx = _runtime.TotalPoints - 1;
            headTrackIdx = tailTrackIdx - (_runtime.VisiblePoints - 1);
        }
        else if (_runtime.IsBeingErased)
        {
            tailTrackIdx = Mathf.Min(tailTrackIdx, _runtime.EraseTailTrackIdx);
        }

        _renderPointsCache.Clear();
        _renderTrackIdxCache.Clear();

        _renderPointsCache.Add(_runtime.GetPositionAtTrackIndex(headTrackIdx, _owner.direction));
        _renderTrackIdxCache.Add(headTrackIdx);

        int firstStatic = Mathf.CeilToInt(headTrackIdx);
        int lastStatic = Mathf.FloorToInt(tailTrackIdx);

        for (int i = firstStatic; i <= lastStatic; i++)
        {
            if (i > headTrackIdx + 0.001f && i < tailTrackIdx - 0.001f)
            {
                if (i < _runtime.TotalPoints) _renderPointsCache.Add(_runtime.GetPositionAtTrackIndex(i, _owner.direction));
                if (i < _runtime.TotalPoints) _renderTrackIdxCache.Add(i);
            }
        }

        if (tailTrackIdx > headTrackIdx + 0.001f)
        {
            _renderPointsCache.Add(_runtime.GetPositionAtTrackIndex(tailTrackIdx, _owner.direction));
            _renderTrackIdxCache.Add(tailTrackIdx);
        }

        InsertPortalSplitPoints(headTrackIdx, tailTrackIdx);

        int visualSegmentCount = BuildVisualSegments();
        SmoothVisualSegments(visualSegmentCount);
        EnsureLineRenderersCount(visualSegmentCount);
        ApplyVisualSegments(visualSegmentCount);
    }

    public static void ApplyArrowRotation(Transform arrowVisual, ArrowDir direction)
    {
        if (arrowVisual == null) return;
        arrowVisual.localRotation = Quaternion.Euler(0f, 0f, GetAngle(direction));
    }

    public static float GetAngle(ArrowDir direction)
    {
        switch (direction)
        {
            case ArrowDir.Up: return 0f;
            case ArrowDir.Down: return 180f;
            case ArrowDir.Left: return 90f;
            case ArrowDir.Right: return -90f;
            default: return 0f;
        }
    }

    public static Vector3 GetDirVector(ArrowDir direction)
    {
        return PathScanner.GetDirVector(direction);
    }

    private void EnsureLineRenderersCount(int count)
    {
        if (_lineRenderers.Count == 0 && _lineRenderer != null) _lineRenderers.Add(_lineRenderer);

        while (_lineRenderers.Count < count)
        {
            GameObject child = new GameObject("LineSegment_" + _lineRenderers.Count);
            child.transform.SetParent(_owner.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            LineRenderer lr = child.AddComponent<LineRenderer>();

            lr.startWidth = _lineRenderer.startWidth;
            lr.endWidth = _lineRenderer.endWidth;
            lr.widthMultiplier = _lineRenderer.widthMultiplier;
            lr.widthCurve = _lineRenderer.widthCurve;
            lr.useWorldSpace = _lineRenderer.useWorldSpace;
            lr.alignment = _lineRenderer.alignment;
            lr.textureMode = _lineRenderer.textureMode;
            lr.numCornerVertices = _lineRenderer.numCornerVertices;
            lr.numCapVertices = _lineRenderer.numCapVertices;
            lr.startColor = _currentLineColor;
            lr.endColor = _currentLineColor;
            lr.sortingOrder = _lineRenderer.sortingOrder;
            lr.sharedMaterial = _lineRenderer.sharedMaterial;

            _lineRenderers.Add(lr);
        }
    }

    private void InsertPortalSplitPoints(float headTrackIdx, float tailTrackIdx)
    {
        if (_runtime.ActiveWarps == null || _runtime.ActiveWarps.Count == 0 || _renderPointsCache.Count <= 1)
            return;

        const float epsilonTrack = 0.0001f;
        for (int w = 0; w < _runtime.ActiveWarps.Count; w++)
        {
            if (!_runtime.ActiveWarps[w].isPortal) continue;

            float warpTrackIdx = -_runtime.ActiveWarps[w].rawDistFromHead0 * _runtime.NodesPerUnit;
            if (warpTrackIdx <= headTrackIdx + epsilonTrack || warpTrackIdx >= tailTrackIdx - epsilonTrack)
                continue;

            int insertAt = _renderTrackIdxCache.Count;
            for (int k = 0; k < _renderTrackIdxCache.Count; k++)
            {
                if (_renderTrackIdxCache[k] > warpTrackIdx)
                {
                    insertAt = k;
                    break;
                }
            }

            Vector3 portalCenter = _runtime.ActiveWarps[w].portalWorldPos;
            portalCenter.z = _renderPointsCache[0].z;
            Vector3 exitCenter = _runtime.ActiveWarps[w].exitWorldPos;
            exitCenter.z = _renderPointsCache[0].z;

            _renderTrackIdxCache.Insert(insertAt, warpTrackIdx - epsilonTrack);
            _renderPointsCache.Insert(insertAt, exitCenter);

            _renderTrackIdxCache.Insert(insertAt + 1, warpTrackIdx + epsilonTrack);
            _renderPointsCache.Insert(insertAt + 1, portalCenter);
        }
    }

    private int BuildVisualSegments()
    {
        int visualSegmentCount = 0;
        List<Vector3> currentSegment = GetReusableVisualSegment(visualSegmentCount);

        for (int i = 0; i < _renderPointsCache.Count; i++)
        {
            if (currentSegment.Count == 0)
            {
                currentSegment.Add(_renderPointsCache[i]);
            }
            else
            {
                float dist = Vector3.Distance(currentSegment[currentSegment.Count - 1], _renderPointsCache[i]);
                if (dist > 1.5f)
                {
                    if (currentSegment.Count > 1) visualSegmentCount++;
                    currentSegment = GetReusableVisualSegment(visualSegmentCount);
                }
                currentSegment.Add(_renderPointsCache[i]);
            }
        }

        if (currentSegment.Count > 1) visualSegmentCount++;
        return visualSegmentCount;
    }

    private void SmoothVisualSegments(int visualSegmentCount)
    {
        if (_owner.CornerRadius <= 0f) return;

        for (int s = 0; s < visualSegmentCount; s++)
        {
            List<Vector3> segment = _visualSegmentsCache[s];
            if (segment.Count <= 2) continue;

            BuildSmoothedPositionsForRenderCached(segment, _smoothedPointsCache);
            segment.Clear();
            for (int p = 0; p < _smoothedPointsCache.Count; p++)
                segment.Add(_smoothedPointsCache[p]);
        }
    }

    private void ApplyVisualSegments(int visualSegmentCount)
    {
        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            if (i > 0 && _lineRenderers[i] != null)
                _lineRenderers[i].widthMultiplier = _lineRenderer.widthMultiplier;

            if (i < visualSegmentCount && _visualSegmentsCache[i].Count > 1)
            {
                if (!_lineRenderers[i].gameObject.activeSelf) _lineRenderers[i].gameObject.SetActive(true);
                _lineRenderers[i].enabled = true;
                ApplyCachedLinePositions(_lineRenderers[i], i, _visualSegmentsCache[i]);
            }
            else
            {
                _lineRenderers[i].positionCount = 0;
                _lineRenderers[i].enabled = false;
                if (i > 0) _lineRenderers[i].gameObject.SetActive(false);
            }
        }
    }

    private List<Vector3> GetReusableVisualSegment(int index)
    {
        while (_visualSegmentsCache.Count <= index)
            _visualSegmentsCache.Add(new List<Vector3>(32));

        List<Vector3> segment = _visualSegmentsCache[index];
        segment.Clear();
        return segment;
    }

    private void ApplyCachedLinePositions(LineRenderer targetRenderer, int segmentIndex, List<Vector3> positions)
    {
        int pointCount = positions.Count;
        targetRenderer.positionCount = pointCount;

        Vector3[] buffer = GetLinePositionsArray(segmentIndex, pointCount);
        for (int i = 0; i < pointCount; i++)
            buffer[i] = positions[i];

        targetRenderer.SetPositions(buffer);
    }

    private Vector3[] GetLinePositionsArray(int segmentIndex, int pointCount)
    {
        while (_linePositionsArrayCache.Count <= segmentIndex)
            _linePositionsArrayCache.Add(null);

        Vector3[] buffer = _linePositionsArrayCache[segmentIndex];
        if (buffer == null || buffer.Length != pointCount)
        {
            buffer = new Vector3[pointCount];
            _linePositionsArrayCache[segmentIndex] = buffer;
        }

        return buffer;
    }

    private void BuildSmoothedPositionsForRenderCached(List<Vector3> input, List<Vector3> output)
    {
        output.Clear();
        if (input.Count < 3)
        {
            output.AddRange(input);
            return;
        }

        output.Add(input[0]);
        const float angleThreshold = 15f;

        for (int i = 1; i < input.Count - 1; i++)
        {
            Vector3 prev = input[i - 1];
            Vector3 curr = input[i];
            Vector3 next = input[i + 1];

            Vector3 dirIn = curr - prev;
            Vector3 dirOut = next - curr;

            if (dirIn.sqrMagnitude > 2.25f || dirOut.sqrMagnitude > 2.25f || dirIn.sqrMagnitude < 0.0001f || dirOut.sqrMagnitude < 0.0001f)
            {
                output.Add(curr);
                continue;
            }

            float angle = Vector3.Angle(dirIn, dirOut);
            if (angle > angleThreshold)
            {
                float distIn = dirIn.magnitude;
                float distOut = dirOut.magnitude;
                float r = Mathf.Min(_owner.CornerRadius, distIn * 0.4f, distOut * 0.4f);

                Vector3 p0 = curr - dirIn.normalized * r;
                Vector3 p1 = curr;
                Vector3 p2 = curr + dirOut.normalized * r;

                if (output.Count > 0 && Vector3.SqrMagnitude(output[output.Count - 1] - p0) < 0.001f)
                    output.RemoveAt(output.Count - 1);

                int steps = Mathf.Max(3, _owner.CornerSmoothSteps);
                for (int s = 0; s <= steps; s++)
                {
                    float t = (float)s / steps;
                    Vector3 pt = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
                    output.Add(pt);
                }
            }
            else
            {
                output.Add(curr);
            }
        }

        output.Add(input[input.Count - 1]);
    }

    private void HideAllLineRenderers()
    {
        for (int i = 0; i < _lineRenderers.Count; i++)
        {
            LineRenderer lr = _lineRenderers[i];
            if (lr == null) continue;

            lr.positionCount = 0;
            lr.enabled = false;
            if (i > 0) lr.gameObject.SetActive(false);
        }
    }

    private void UpdateArrowEraseVisual(float brushTrackRadius)
    {
        Transform arrowVisual = _owner.ArrowVisual;
        if (arrowVisual == null) return;

        float hideDistance = Mathf.Max(0.001f, brushTrackRadius);
        float visibleRatio = Mathf.Clamp01(_runtime.EraseTailTrackIdx / hideDistance);
        arrowVisual.localScale = _originalArrowScale * visibleRatio;
    }

    private void RunColorTween(Color targetColor, float duration)
    {
        _isFocusingColorTweenRunning = false;
        _pendingUnfocus = false;
        if (_colorTweener != null && _colorTweener.IsActive()) _colorTweener.Kill();

        if (duration <= 0f || _currentLineColor == targetColor)
        {
            _currentLineColor = targetColor;
            ApplyColorToAll(_currentLineColor);
            return;
        }

        _colorTweener = DOTween.To(() => _currentLineColor, x => _currentLineColor = x, targetColor, duration)
            .OnUpdate(() => ApplyColorToAll(_currentLineColor))
            .SetLink(_owner.gameObject);
    }

    private void ApplyColorToAll(Color color)
    {
        foreach (LineRenderer lr in _lineRenderers)
        {
            if (lr == null) continue;

            lr.startColor = color;
            lr.endColor = color;
        }

        CacheArrowRenderer();
        if (_arrowSpriteRenderer != null) _arrowSpriteRenderer.color = color;
    }

    private void CacheArrowRenderer()
    {
        if (_arrowSpriteRenderer != null || _owner.ArrowVisual == null) return;

        _arrowSpriteRenderer = _owner.ArrowVisual.GetComponentInChildren<SpriteRenderer>(true);
    }

    private IEnumerator TransparentRevealFlashRoutine(float flashAlpha, float duration, Color flashTint)
    {
        float halfDuration = Mathf.Max(0.01f, duration * 0.5f);
        Color flashColor = Color.Lerp(_owner.snakeColor, flashTint, 0.45f);
        flashColor.a = Mathf.Clamp01(flashAlpha);

        int rendererCount = _lineRenderers.Count;
        Color[] originalStartColors = new Color[rendererCount];
        Color[] originalEndColors = new Color[rendererCount];
        bool[] hasLineRenderer = new bool[rendererCount];

        for (int i = 0; i < rendererCount; i++)
        {
            LineRenderer lr = _lineRenderers[i];
            if (lr == null) continue;

            originalStartColors[i] = lr.startColor;
            originalEndColors[i] = lr.endColor;
            hasLineRenderer[i] = true;
        }

        CacheArrowRenderer();
        bool hasArrowRenderer = _arrowSpriteRenderer != null;
        Color originalArrowColor = hasArrowRenderer ? _arrowSpriteRenderer.color : Color.clear;

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Mathf.Min(Time.deltaTime, 0.033f);
            float t = Mathf.Clamp01(elapsed / halfDuration);
            ApplyTransparentRevealFlashColor(t, originalStartColors, originalEndColors, hasLineRenderer, hasArrowRenderer, originalArrowColor, flashColor);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Mathf.Min(Time.deltaTime, 0.033f);
            float t = Mathf.Clamp01(elapsed / halfDuration);
            ApplyTransparentRevealFlashColor(1f - t, originalStartColors, originalEndColors, hasLineRenderer, hasArrowRenderer, originalArrowColor, flashColor);
            yield return null;
        }

        RestoreTransparentRevealFlashColors(originalStartColors, originalEndColors, hasLineRenderer, hasArrowRenderer, originalArrowColor);
        _transparentRevealFlashRoutine = null;
    }

    private void ApplyTransparentRevealFlashColor(
        float flashWeight,
        Color[] originalStartColors,
        Color[] originalEndColors,
        bool[] hasLineRenderer,
        bool hasArrowRenderer,
        Color originalArrowColor,
        Color flashColor)
    {
        float t = Mathf.Clamp01(flashWeight);

        for (int i = 0; i < hasLineRenderer.Length; i++)
        {
            if (!hasLineRenderer[i]) continue;
            LineRenderer lr = _lineRenderers[i];
            if (lr == null) continue;

            lr.startColor = Color.Lerp(originalStartColors[i], flashColor, t);
            lr.endColor = Color.Lerp(originalEndColors[i], flashColor, t);
        }

        if (hasArrowRenderer && _arrowSpriteRenderer != null)
            _arrowSpriteRenderer.color = Color.Lerp(originalArrowColor, flashColor, t);
    }

    private void RestoreTransparentRevealFlashColors(
        Color[] originalStartColors,
        Color[] originalEndColors,
        bool[] hasLineRenderer,
        bool hasArrowRenderer,
        Color originalArrowColor)
    {
        for (int i = 0; i < hasLineRenderer.Length; i++)
        {
            if (!hasLineRenderer[i]) continue;
            LineRenderer lr = _lineRenderers[i];
            if (lr == null) continue;

            lr.startColor = originalStartColors[i];
            lr.endColor = originalEndColors[i];
        }

        if (hasArrowRenderer && _arrowSpriteRenderer != null)
            _arrowSpriteRenderer.color = originalArrowColor;
    }
}
