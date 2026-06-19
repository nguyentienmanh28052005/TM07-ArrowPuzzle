using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoPlayManager : MonoBehaviour
{
    public static AutoPlayManager Instance;

    [Header("Auto Play")]
    [SerializeField] private float delayBetweenReleases = 0.08f;
    [SerializeField] private bool waitWhileGameplayBlocked = true;

    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI toggleLabel;
    [SerializeField] private string enabledLabel = "AUTO: ON";
    [SerializeField] private string disabledLabel = "AUTO: OFF";

    private readonly List<SnakeBlock> _snakeBuffer = new List<SnakeBlock>(128);
    private Coroutine _autoPlayRoutine;
    private WaitForSeconds _releaseDelayWait;
    private float _cachedReleaseDelay = -1f;

    public bool IsAutoPlaying => _autoPlayRoutine != null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateToggleLabel();
    }

    private void OnDisable()
    {
        StopAutoPlay();
    }

    [ContextMenu("Toggle Auto Play")]
    public void ToggleAutoPlay()
    {
        if (IsAutoPlaying) StopAutoPlay();
        else StartAutoPlay();
    }

    public void StartAutoPlay()
    {
        if (IsAutoPlaying || Time.timeScale == 0f) return;

        if (HintManager.Instance != null)
        {
            HintManager.Instance.StopHintImmediate();
        }

        _autoPlayRoutine = StartCoroutine(AutoPlayRoutine());
        UpdateToggleLabel();
    }

    public void StopAutoPlay()
    {
        if (_autoPlayRoutine != null)
        {
            StopCoroutine(_autoPlayRoutine);
            _autoPlayRoutine = null;
        }

        _snakeBuffer.Clear();
        UpdateToggleLabel();
    }

    private IEnumerator AutoPlayRoutine()
    {
        while (true)
        {
            if (Time.timeScale == 0f) break;
            if (GameManager.Instance != null && GameManager.Instance.isGameOver) break;

            if (EraseManager.Instance != null && (EraseManager.Instance.IsEraseModeActive || EraseManager.Instance.IsExecutingErase))
            {
                break;
            }

            if (waitWhileGameplayBlocked && GameplayInputLock.IsLocked)
            {
                yield return null;
                continue;
            }

            SnakeBlock snake = FindNextReleasableSnake();
            if (snake == null)
            {
                if (HasMovingSnake())
                {
                    yield return null;
                    continue;
                }

                break;
            }

            if (!snake.OnHeadClicked())
            {
                yield return null;
                continue;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowTap, 0.8f);
            }

            if (delayBetweenReleases > 0f)
            {
                yield return GetReleaseDelayWait();
            }
        }

        _autoPlayRoutine = null;
        _snakeBuffer.Clear();
        UpdateToggleLabel();
    }

    private SnakeBlock FindNextReleasableSnake()
    {
        _snakeBuffer.Clear();

        IReadOnlyList<SnakeBlock> activeSnakes = SnakeBlock.ActiveSnakes;
        for (int i = 0; i < activeSnakes.Count; i++)
        {
            SnakeBlock snake = activeSnakes[i];
            if (snake == null) continue;
            _snakeBuffer.Add(snake);
        }

        _snakeBuffer.Sort(CompareSnakesForAutoPlay);

        for (int i = 0; i < _snakeBuffer.Count; i++)
        {
            SnakeBlock snake = _snakeBuffer[i];
            if (snake != null && snake.CanReleaseNow())
            {
                return snake;
            }
        }

        return null;
    }

    private static bool HasMovingSnake()
    {
        IReadOnlyList<SnakeBlock> activeSnakes = SnakeBlock.ActiveSnakes;
        for (int i = 0; i < activeSnakes.Count; i++)
        {
            SnakeBlock snake = activeSnakes[i];
            if (snake != null && snake.IsMoving)
            {
                return true;
            }
        }

        return false;
    }

    private static int CompareSnakesForAutoPlay(SnakeBlock a, SnakeBlock b)
    {
        if (a == b) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        Vector3 aPos = a.HeadPosition;
        Vector3 bPos = b.HeadPosition;

        int yCompare = bPos.y.CompareTo(aPos.y);
        if (yCompare != 0) return yCompare;

        int xCompare = aPos.x.CompareTo(bPos.x);
        if (xCompare != 0) return xCompare;

        return a.GetInstanceID().CompareTo(b.GetInstanceID());
    }

    private WaitForSeconds GetReleaseDelayWait()
    {
        float delay = Mathf.Max(0f, delayBetweenReleases);
        if (_releaseDelayWait == null || !Mathf.Approximately(_cachedReleaseDelay, delay))
        {
            _cachedReleaseDelay = delay;
            _releaseDelayWait = new WaitForSeconds(delay);
        }

        return _releaseDelayWait;
    }

    private void UpdateToggleLabel()
    {
        if (toggleLabel != null)
        {
            toggleLabel.text = IsAutoPlaying ? enabledLabel : disabledLabel;
        }
    }
}
