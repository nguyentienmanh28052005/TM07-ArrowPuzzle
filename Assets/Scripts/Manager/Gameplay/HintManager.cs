using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;
using TMPro;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Hint Settings")]
    public Color hintGlowColor = Color.yellow;
    [SerializeField] private float hintDuration = 2f;
    [SerializeField] private float hintGlowScale = 1.18f;
    [SerializeField] private float hintGlowPulseDuration = 0.4f;
    [SerializeField] private float hintGlowRestoreDuration = 0.15f;

    [Header("Hint Camera")]
    [SerializeField] private bool moveCameraToHintTarget = true;
    [SerializeField] private float hintCameraMoveDuration = 0.35f;
    
    private bool _isHinting = false;
    private Sequence _currentHintSeq; 

    [SerializeField] private TextMeshProUGUI hintCountText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerHint()
    {
        if (_isHinting || Time.timeScale == 0f) return; 
        
        if (EraseManager.Instance != null && EraseManager.Instance.IsExecutingErase) return;

        if(CurrencyManager.Instance.SpendHintTool(1))
        {
            SnakeBlock bestMoveSnake = FindBestMove();
            if (bestMoveSnake != null)
            {
                PlayHintAnimation(bestMoveSnake);
            }
            else
            {
                Debug.Log("HINT: Không tìm thấy nước đi nào tốt hơn!");
            }

            if (BoosterTutorialManager.Instance != null)
                BoosterTutorialManager.Instance.NotifyHintTriggered();
        }
    }

    private SnakeBlock FindBestMove()
    {
        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();

        if (GridManager.Instance == null) return null;

        foreach (var snake in allSnakes)
        {
            // ĐÃ SỬA: Check LogicNodes.Count thay vì bodySegments.Count
            if (snake == null || snake.IsMoving || snake.LogicNodes == null || snake.LogicNodes.Count == 0) continue;

            Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(snake.HeadPosition.x), Mathf.RoundToInt(snake.HeadPosition.y));
            ArrowDir currentDir = snake.direction;
            Vector2Int step = GetDirStep(currentDir);

            bool isBlocked = false;
            for (int d = 1; d < 50; d++)
            {
                Vector2Int checkPos = currentPos + step;

                if (Mathf.Abs(checkPos.x) > 100 || Mathf.Abs(checkPos.y) > 100)
                {
                    break;
                }

                SnakeBlock obstacle = GridManager.Instance.GetSnakeAt(checkPos);
                if (obstacle != null && obstacle != snake)
                {
                    isBlocked = true;
                    break;
                }

                if (GridManager.Instance.GateMap != null && GridManager.Instance.GateMap.ContainsKey(checkPos))
                {
                    isBlocked = true;
                    break;
                }

                if (GridManager.Instance.ElectricWallMap != null && GridManager.Instance.ElectricWallMap.ContainsKey(checkPos))
                {
                    isBlocked = true;
                    break;
                }

                if (GridManager.Instance.HasActiveCountdownBlockAt(checkPos))
                {
                    isBlocked = true;
                    break;
                }

                if (GridManager.Instance.PortalMap != null && GridManager.Instance.PortalMap.TryGetValue(checkPos, out GridManager.PortalLink link))
                {
                    currentPos = link.exit;
                    currentDir = link.exitDir;
                    step = GetDirStep(currentDir);
                    continue;
                }

                if (GridManager.Instance.DeflectorMap != null && GridManager.Instance.DeflectorMap.TryGetValue(checkPos, out GridDeflector deflector))
                {
                    currentPos = checkPos;
                    currentDir = deflector.direction;
                    step = GetDirStep(currentDir);
                    continue;
                }

                currentPos = checkPos;
            }

            if (!isBlocked) return snake;
        }
        
        return null;
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

    private void PlayHintAnimation(SnakeBlock snake)
    {
        _isHinting = true;

        if (moveCameraToHintTarget)
        {
            CameraController camController = FindObjectOfType<CameraController>();
            if (camController != null)
            {
                camController.FocusOnWorldPosition(snake.HeadPosition, hintCameraMoveDuration, blockCameraInput: true);
            }
        }
        
        var guideline = snake.GetComponent<ArrowGuideline>();
        Color originalColor = snake.snakeColor;

        if (_currentHintSeq != null && _currentHintSeq.IsActive()) _currentHintSeq.Kill();

        float safePulseDuration = Mathf.Max(0.05f, hintGlowPulseDuration);
        float halfPulseDuration = safePulseDuration * 0.5f;
        int loopCount = Mathf.Max(1, Mathf.CeilToInt(hintDuration / safePulseDuration));

        if (guideline != null) guideline.SetLineActive(true);
        snake.BeginHintGlowVisual(hintGlowScale, halfPulseDuration);

        _currentHintSeq = DOTween.Sequence();

        _currentHintSeq.Append(DOVirtual.Color(originalColor, hintGlowColor, halfPulseDuration, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));
        _currentHintSeq.Append(DOVirtual.Color(hintGlowColor, originalColor, halfPulseDuration, (c) => snake.SetColorImmediate(c)).SetEase(Ease.InOutSine));

        _currentHintSeq.SetLoops(loopCount);
        
        _currentHintSeq.OnKill(() => {
            _isHinting = false; 
            if (snake != null)
            {
                if (guideline != null) guideline.SetLineActive(false);
                snake.EndHintGlowVisual(originalColor, hintGlowRestoreDuration);
            }
        });
        
        _currentHintSeq.SetLink(snake.gameObject);
    }

    public void StopHintImmediate()
    {
        if (_currentHintSeq != null && _currentHintSeq.IsActive())
        {
            _currentHintSeq.Kill();
        }
    }

    private Vector3 GetDirVector(ArrowDir dir)
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
}
