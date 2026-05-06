using UnityEngine;
using Solo.MOST_IN_ONE;
using System.Collections.Generic;

[RequireComponent(typeof(SnakeBlock))] 
public class ArrowGuideline : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lineLength = 3000f;
    [SerializeField] private float lineWidth = 8f;
    [SerializeField] private float startOffset = 0f;
    [SerializeField] private Color lineColor = Color.gray;
    [SerializeField] private int sortingOrder = 2;
    [SerializeField, Range(0f, 0.9f)] private float portalEntryInset = 0f;
    [SerializeField, Range(0f, 0.9f)] private float portalExitInset = 0f;

    [Header("Behavior")]
    [SerializeField] private bool stopAtBlockers = false;

    private const float _pixelsPerUnit = 100f;
    private const int _maxPortalHopsSafety = 32;
    private const int _maxSegmentsSafety = 32;

    private GameObject _guidelineRoot;
    private SnakeBlock _snakeBlock;

    private Sprite _segmentSprite;
    private readonly List<Transform> _segmentVisuals = new List<Transform>();
    private readonly List<SpriteRenderer> _segmentRenderers = new List<SpriteRenderer>();

    private struct Segment
    {
        public Vector3 startWorld;
        public ArrowDir dir;
        public int steps;
        public bool startsFromPortal;
        public bool endsInPortal;
    }

    private readonly List<Segment> _segmentsCache = new List<Segment>(8);

    private void Awake()
    {
        _snakeBlock = GetComponent<SnakeBlock>();
        CreateGuidelineProcedurally();
        SetLineActive(false);
    }

    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnShowAllPaths, HandleShowAllPaths);
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnShowAllPaths, HandleShowAllPaths);
    }

    private void HandleShowAllPaths(object data)
    {
        if (data is bool isShowing)
        {
            if (_snakeBlock != null && _snakeBlock.IsMoving) return;
            SetLineActive(isShowing);
        }
    }

    private void CreateGuidelineProcedurally()
    {
        _guidelineRoot = new GameObject("Guideline_Root_Auto");
        _guidelineRoot.transform.SetParent(transform, false);

        Texture2D tex = Texture2D.whiteTexture;
        _segmentSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), _pixelsPerUnit);

        EnsureSegmentPool(1);
    }

    private void EnsureSegmentPool(int count)
    {
        count = Mathf.Clamp(count, 1, _maxSegmentsSafety);
        while (_segmentVisuals.Count < count)
        {
            GameObject visual = new GameObject($"Guideline_Segment_{_segmentVisuals.Count}");
            visual.transform.SetParent(_guidelineRoot.transform, false);

            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = _segmentSprite;
            sr.color = lineColor;
            sr.sortingOrder = sortingOrder;

            _segmentVisuals.Add(visual.transform);
            _segmentRenderers.Add(sr);
        }
    }

    private void LateUpdate()
    {
        if (_guidelineRoot != null && _guidelineRoot.activeSelf && _snakeBlock != null)
        {
            UpdateSegments();
        }
    }

    private void UpdateSegments()
    {
        if (_snakeBlock.LogicNodes == null || _snakeBlock.LogicNodes.Count == 0) return;

        BuildPredictedSegments(_segmentsCache);
        EnsureSegmentPool(_segmentsCache.Count);

        for (int i = 0; i < _segmentVisuals.Count; i++)
        {
            bool active = i < _segmentsCache.Count;
            _segmentVisuals[i].gameObject.SetActive(active);
            if (!active) continue;

            Segment seg = _segmentsCache[i];
            if (seg.steps <= 0)
            {
                _segmentVisuals[i].gameObject.SetActive(false);
                continue;
            }

            float angle = GetAngle(seg.dir);
            Vector3 moveDir = GetDirVector(seg.dir);

            float startInset = (i == 0) ? Mathf.Max(0f, startOffset) : 0f;
            if (seg.startsFromPortal) startInset += portalExitInset;
            float endInset = seg.endsInPortal ? portalEntryInset : 0f;

            float lengthWorld = seg.steps;
            float visibleLengthWorld = Mathf.Max(0f, lengthWorld - startInset - endInset);

            _segmentVisuals[i].position = seg.startWorld + (moveDir * startInset);
            _segmentVisuals[i].rotation = Quaternion.Euler(0f, 0f, angle);

            // Compensate parent scaling (SnakePrefab is scaled, but positions are in world units).
            Vector3 lossy = _segmentVisuals[i].parent != null ? _segmentVisuals[i].parent.lossyScale : Vector3.one;
            float invX = (Mathf.Abs(lossy.x) < 0.0001f) ? 1f : (1f / lossy.x);
            float invY = (Mathf.Abs(lossy.y) < 0.0001f) ? 1f : (1f / lossy.y);

            float spriteHeight = _segmentSprite.bounds.size.y; 
            float correctScaleY = visibleLengthWorld / spriteHeight;

            _segmentVisuals[i].localScale = new Vector3(lineWidth * invX, correctScaleY * invY, 1f);

            if (_segmentRenderers[i] != null)
            {
                _segmentRenderers[i].color = lineColor;
                _segmentRenderers[i].sortingOrder = sortingOrder;
            }
        }
    }

    private void BuildPredictedSegments(List<Segment> outSegments)
    {
        outSegments.Clear();
        if (_snakeBlock == null || GridManager.Instance == null) return;

        int maxSteps = Mathf.Clamp(Mathf.FloorToInt(lineLength / _pixelsPerUnit), 1, 200);

        Vector3 headPos = _snakeBlock.HeadPosition;
        ArrowDir currentDir = _snakeBlock.direction;
        Vector3 dirWorld = GetDirVector(currentDir);

        Vector3 currentWorldStart = headPos;
        Vector2Int currentCell = new Vector2Int(Mathf.RoundToInt(currentWorldStart.x), Mathf.RoundToInt(currentWorldStart.y));
        Vector2Int step = GetDirStep(currentDir);

        Segment currentSeg = new Segment { startWorld = currentWorldStart, dir = currentDir, steps = 0 };

        int portalHops = 0;
        for (int used = 0; used < maxSteps; used++)
        {
            Vector2Int nextCell = currentCell + step;

            if (Mathf.Abs(nextCell.x) > 100 || Mathf.Abs(nextCell.y) > 100)
            {
                currentSeg.steps += (maxSteps - used);
                break;
            }

            if (stopAtBlockers)
            {
                SnakeBlock obstacle = GridManager.Instance.GetSnakeAt(nextCell);
                if (obstacle != null && obstacle != _snakeBlock)
                {
                    break;
                }

                if (GridManager.Instance.GateMap != null && GridManager.Instance.GateMap.ContainsKey(nextCell))
                {
                    break;
                }
            }

            if (GridManager.Instance.PortalMap != null && GridManager.Instance.PortalMap.TryGetValue(nextCell, out GridManager.PortalLink link))
            {
                currentSeg.steps += 1;
                currentSeg.endsInPortal = true;
                if (currentSeg.steps > 0) outSegments.Add(currentSeg);

                currentCell = link.exit;
                currentWorldStart = new Vector3(link.exit.x, link.exit.y, headPos.z);

                currentDir = link.exitDir;
                step = GetDirStep(currentDir);
                currentSeg = new Segment { startWorld = currentWorldStart, dir = currentDir, steps = 0, startsFromPortal = true };

                portalHops++;
                if (portalHops >= _maxPortalHopsSafety || outSegments.Count >= _maxSegmentsSafety) break;
                continue;
            }

            if (GridManager.Instance.DeflectorMap != null && GridManager.Instance.DeflectorMap.TryGetValue(nextCell, out GridDeflector deflector))
            {
                currentSeg.steps += 1;
                if (currentSeg.steps > 0) outSegments.Add(currentSeg);

                currentCell = nextCell;
                currentWorldStart = new Vector3(currentCell.x, currentCell.y, headPos.z);

                currentDir = deflector.direction;
                step = GetDirStep(currentDir);
                currentSeg = new Segment { startWorld = currentWorldStart, dir = currentDir, steps = 0 };

                if (outSegments.Count >= _maxSegmentsSafety) break;
                continue;
            }

            currentSeg.steps += 1;
            currentCell = nextCell;
        }

        if (currentSeg.steps > 0 && outSegments.Count < _maxSegmentsSafety)
        {
            outSegments.Add(currentSeg);
        }
    }

    private static Vector2Int GetDirStep(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return new Vector2Int(0, 1);
            case ArrowDir.Down: return new Vector2Int(0, -1);
            case ArrowDir.Left: return new Vector2Int(-1, 0);
            case ArrowDir.Right: return new Vector2Int(1, 0);
            default: return Vector2Int.zero;
        }
    }

    private static float GetAngle(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return 0f;
            case ArrowDir.Down: return 180f;
            case ArrowDir.Left: return 90f;
            case ArrowDir.Right: return -90f;
            default: return 0f;
        }
    }

    private static Vector3 GetDirVector(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return Vector3.up;
            case ArrowDir.Down: return Vector3.down;
            case ArrowDir.Left: return Vector3.left;
            case ArrowDir.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    public void SetLineActive(bool isActive)
    {
        if (_guidelineRoot != null) _guidelineRoot.SetActive(isActive);
    }
}