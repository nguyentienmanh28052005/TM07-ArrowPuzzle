using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Solo.MOST_IN_ONE;

public class DashManager : MonoBehaviour
{
    public static DashManager Instance;

    [Header("Dash Settings")]
    [Tooltip("Delay between each launched arrow.")]
    public float delayBetweenLaunches = 0.15f;

    [Header("Release Highlight")]
    public Color releaseHighlightColor = new Color(1f, 0.92f, 0.25f, 1f);
    public float releaseHighlightScale = 1.2f;
    public float releaseHighlightInDuration = 0.18f;

    private List<SnakeBlock> _queuedDashSnakes;
    private Coroutine _dashReleaseRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ExecuteDash(ArrowDir targetDir)
    {
        if (GameplayInputLock.IsLocked || Time.timeScale == 0f) return;

        PrepareDashRelease(targetDir);

        MessageManager.Instance.SendMessage(ManhMessageType.OnSelectDashDirection, false);
        if (BoosterTutorialManager.Instance != null)
            BoosterTutorialManager.Instance.NotifyDashDirectionSelected(targetDir);
    }

    public void ExecuteDashFromDirectionUI(ArrowDir targetDir)
    {
        if (GameplayInputLock.IsLocked || Time.timeScale == 0f) return;

        PrepareDashRelease(targetDir);
        if (BoosterTutorialManager.Instance != null)
            BoosterTutorialManager.Instance.NotifyDashDirectionSelected(targetDir);

        BeginQueuedDashRelease();
    }

    public void TriggerDash()
    {
        if (GameplayInputLock.IsLocked || Time.timeScale == 0f) return;
        if (BoosterTutorialManager.Instance != null &&
            (BoosterTutorialManager.Instance.IsWaitingForBoosterRewardClaim || !BoosterTutorialManager.Instance.IsDashUnlocked)) return;

        if (CurrencyManager.Instance.SpendDashTool(1))
        {
            MessageManager.Instance.SendMessage(ManhMessageType.OnSelectDashDirection, true);
            if (BoosterTutorialManager.Instance != null)
                BoosterTutorialManager.Instance.NotifyDashTriggered();
        }
    }

    public void DashUp() { ExecuteDash(ArrowDir.Up); }
    public void DashDown() { ExecuteDash(ArrowDir.Down); }
    public void DashLeft() { ExecuteDash(ArrowDir.Left); }
    public void DashRight() { ExecuteDash(ArrowDir.Right); }

    public void BeginQueuedDashRelease()
    {
        if (_queuedDashSnakes == null || _queuedDashSnakes.Count == 0) return;

        if (_dashReleaseRoutine != null)
            StopCoroutine(_dashReleaseRoutine);

        List<SnakeBlock> snakesToRelease = _queuedDashSnakes;
        _queuedDashSnakes = null;
        _dashReleaseRoutine = StartCoroutine(DashReleaseRoutine(snakesToRelease));
    }

    private void PrepareDashRelease(ArrowDir targetDir)
    {
        GameplayInputLock.SetLock(GameplayLockReason.BoosterActive, true);
        _queuedDashSnakes = null;

        SnakeBlock[] allSnakes = FindObjectsOfType<SnakeBlock>();
        List<SnakeBlock> matchingSnakes = new List<SnakeBlock>();

        foreach (var snake in allSnakes)
        {
            if (snake != null && snake.direction == targetDir && !snake.IsMoving && !snake.IsStoppedByStopBlock)
            {
                matchingSnakes.Add(snake);
            }
        }

        if (matchingSnakes.Count == 0)
        {
            Debug.Log($"<color=yellow>DASH: No arrow points {targetDir} on the board.</color>");
            GameplayInputLock.SetLock(GameplayLockReason.BoosterActive, false);
            return;
        }

        SortSnakesForSafeLaunch(matchingSnakes, targetDir);
        _queuedDashSnakes = matchingSnakes;

        if (SettingManager.Instance != null)
            SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.RigidImpact);
    }

    private IEnumerator DashReleaseRoutine(List<SnakeBlock> matchingSnakes)
    {
        yield return StartCoroutine(PlayReleaseHighlightAndLaunchRoutine(matchingSnakes));

        yield return new WaitForSeconds(0.5f);
        GameplayInputLock.SetLock(GameplayLockReason.BoosterActive, false);
        _dashReleaseRoutine = null;
    }

    private IEnumerator PlayReleaseHighlightAndLaunchRoutine(List<SnakeBlock> snakes)
    {
        if (snakes == null) yield break;

        float highlightDuration = Mathf.Max(0f, releaseHighlightInDuration);
        float launchDelay = Mathf.Max(0f, delayBetweenLaunches);
        int validCount = 0;
        for (int i = 0; i < snakes.Count; i++)
        {
            SnakeBlock snake = snakes[i];
            if (snake != null && !snake.IsMoving && !snake.IsStoppedByStopBlock) validCount++;
        }

        if (validCount == 0) yield break;

        int startedCount = 0;
        bool startedAnyLaunch = false;

        for (int i = 0; i < snakes.Count; i++)
        {
            SnakeBlock snake = snakes[i];
            if (snake == null || snake.IsMoving || snake.IsStoppedByStopBlock) continue;

            startedAnyLaunch = true;
            startedCount++;
            StartCoroutine(HighlightThenLaunch(snake, highlightDuration));

            if (startedCount < validCount && launchDelay > 0f)
                yield return new WaitForSeconds(launchDelay);
        }

        if (startedAnyLaunch && highlightDuration > 0f)
            yield return new WaitForSeconds(highlightDuration);
    }

    private IEnumerator HighlightThenLaunch(SnakeBlock snake, float highlightDuration)
    {
        if (snake == null || snake.IsMoving || snake.IsStoppedByStopBlock) yield break;

        snake.PlayDashReadyVisual(releaseHighlightColor, releaseHighlightScale, releaseHighlightInDuration);
        if (highlightDuration > 0f) yield return new WaitForSeconds(highlightDuration);

        if (snake == null || snake.IsMoving || snake.IsStoppedByStopBlock) yield break;

        snake.ForceDashRelease(keepCurrentVisual: true);
    }

    private void SortSnakesForSafeLaunch(List<SnakeBlock> snakes, ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up:
                snakes.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
                break;
            case ArrowDir.Down:
                snakes.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
                break;
            case ArrowDir.Right:
                snakes.Sort((a, b) => b.transform.position.x.CompareTo(a.transform.position.x));
                break;
            case ArrowDir.Left:
                snakes.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
                break;
        }
    }
}
