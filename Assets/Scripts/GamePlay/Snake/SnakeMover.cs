using System.Collections;
using UnityEngine;
using DG.Tweening;
using Solo.MOST_IN_ONE;

public sealed class SnakeMover
{
    private readonly SnakeBlock _owner;
    private readonly SnakeRuntime _runtime;
    private readonly SnakeRenderer2D _renderer;
    private readonly SnakeInteractions _interactions;

    private LevelController _levelController;
    private float _currentMoveSpeed;

    public SnakeMover(SnakeBlock owner, SnakeRuntime runtime, SnakeRenderer2D renderer, SnakeInteractions interactions)
    {
        _owner = owner;
        _runtime = runtime;
        _renderer = renderer;
        _interactions = interactions;
    }

    public void SetLevelController(LevelController levelController)
    {
        _levelController = levelController;
    }

    public void ResetForInitializedSnake()
    {
        _currentMoveSpeed = 0f;
    }

    public bool TryReleaseHead()
    {
        if (_runtime.IsMoving || _runtime.IsSpawning || _runtime.IsStoppedByStopBlock)
            return false;

        _owner.StartCoroutine(ProcessMovementMaster());
        return true;
    }

    public bool CanReleaseNow()
    {
        if (!_runtime.IsInitialized || _runtime.IsMoving || _runtime.IsSpawning || _runtime.IsBeingErased || _runtime.IsStoppedByStopBlock)
            return false;

        float obstacleDistance = _runtime.ScanPath(_owner, _owner.direction, _owner.MaxPathScanCells, _owner.ExitTravelDistance);
        bool canRelease = obstacleDistance == float.MaxValue || _runtime.LastObstacleType == ObstacleHitType.BlackHole;
        _runtime.ClearWarps();
        return canRelease;
    }

    public void ForceDashRelease(bool keepCurrentVisual)
    {
        BeginForcedExitRelease(keepCurrentVisual, isSpinRelease: false);
    }

    public void ForceSpinRelease(bool keepCurrentVisual)
    {
        BeginForcedExitRelease(keepCurrentVisual, isSpinRelease: true);
    }

    public void StartSpawnAnimationFromTail()
    {
        if (!_runtime.IsInitialized) return;
        if (_runtime.IsSpawning || _runtime.VisiblePoints >= _runtime.TotalPoints) return;

        _owner.StartCoroutine(PlaySpawnAnimationFromTail());
    }

    public void ForceResetToOrigin()
    {
        _owner.StopAllCoroutines();
        DOTween.Kill(_owner.GetInstanceID());

        if (_runtime.HoldingStopBlock != null)
        {
            _runtime.HoldingStopBlock.ClearHeldSnake(_owner);
            _runtime.HoldingStopBlock = null;
        }

        _runtime.AccumulatedShift = 0f;
        _runtime.IsMoving = false;
        _runtime.IsSpawning = false;
        _runtime.IsStoppedByStopBlock = false;
        _runtime.IsBeingErased = false;
        _runtime.IsBeingConsumedByBlackHole = false;
        _runtime.EraseTailTrackIdx = _runtime.TotalPoints > 0 ? _runtime.TotalPoints - 1 : 0f;
        _runtime.HasDealtDamage = false;
        _runtime.ClearWarps();
        _runtime.CopyOriginalToCurrent();

        _renderer.SyncArrowVisualPosition();
        _renderer.UpdateVisualRotation(_owner.direction);
        _runtime.UpdateGridOccupancy(_owner, _interactions);
        _renderer.RequestRedraw();
        _renderer.SetColorImmediate(_owner.snakeColor);
        _renderer.ShowArrowAtOriginalScale();
        _renderer.SetSortingOrder(10);
    }

    public void ReleaseFromStopBlock(GridStopBlock stopBlock)
    {
        if (!_runtime.IsStoppedByStopBlock) return;
        if (stopBlock != null && _runtime.HoldingStopBlock != stopBlock) return;

        _runtime.HoldingStopBlock = null;
        _runtime.IsStoppedByStopBlock = false;
        _runtime.HasDealtDamage = false;

        _renderer.SetLinePressedMaterial(false, true);
        _renderer.SetColorImmediate(_owner.snakeColor);
        _runtime.UpdateGridOccupancy(_owner, _interactions);
        _renderer.RequestRedraw();
    }

    public void OnOwnerDestroyed()
    {
        if (_runtime.HoldingStopBlock != null)
        {
            _runtime.HoldingStopBlock.ClearHeldSnake(_owner);
            _runtime.HoldingStopBlock = null;
        }
    }

    private void BeginForcedExitRelease(bool keepCurrentVisual, bool isSpinRelease)
    {
        if (_runtime.IsMoving || _runtime.IsSpawning || _runtime.IsStoppedByStopBlock) return;

        _runtime.IsMoving = true;
        if (!keepCurrentVisual)
        {
            _renderer.SetFocusEffect(false, 1f, 0.2f);
            _renderer.SetFocusColor(false, 0.5f);
        }

        _renderer.SetSortingOrder(20);
        _runtime.ResetMovementPose();

        Vector3 moveDir = PathScanner.GetDirVector(_owner.direction);
        _owner.StartCoroutine(isSpinRelease ? ProcessSpinExitMovement(moveDir) : ProcessDashExitMovement(moveDir));
    }

    private IEnumerator ProcessMovementMaster()
    {
        _runtime.IsMoving = true;
        _renderer.SetFocusColor(false, 0.5f);
        _renderer.SetSortingOrder(20);

        _runtime.ResetMovementPose();

        Vector3 moveDir = PathScanner.GetDirVector(_owner.direction);
        float distToObstacle = _runtime.ScanPath(_owner, _owner.direction, _owner.MaxPathScanCells, _owner.ExitTravelDistance);
        bool isGhostMode = distToObstacle == float.MaxValue;

        if (isGhostMode)
        {
            yield return _owner.StartCoroutine(ProcessExitMovement(moveDir));
        }
        else if (_runtime.LastObstacleType == ObstacleHitType.BlackHole
            && GridManager.Instance != null
            && GridManager.Instance.TryGetBlackHoleAt(_runtime.LastObstacleCell, out GridBlackHole blackHole))
        {
            yield return _owner.StartCoroutine(ProcessBlackHoleMovement(moveDir, distToObstacle, blackHole));
        }
        else
        {
            float targetMaxShift = distToObstacle * _runtime.NodesPerUnit;
            yield return _owner.StartCoroutine(ProcessBlockedMovement(moveDir, targetMaxShift, distToObstacle));
        }

        _runtime.IsMoving = false;
        _renderer.SetSortingOrder(10);
    }

    private IEnumerator ProcessExitMovement(Vector3 moveDir)
    {
        yield return _owner.StartCoroutine(ProcessExitMovementInternal(moveDir, _owner.ExitStartSpeed, _owner.ExitMaxSpeed, _owner.ExitAcceleration));
    }

    private IEnumerator ProcessDashExitMovement(Vector3 moveDir)
    {
        yield return _owner.StartCoroutine(ProcessExitMovementInternal(moveDir, _owner.DashExitStartSpeed, _owner.DashExitMaxSpeed, _owner.DashExitAcceleration));
    }

    private IEnumerator ProcessSpinExitMovement(Vector3 moveDir)
    {
        yield return _owner.StartCoroutine(ProcessExitMovementInternal(moveDir, _owner.DashExitStartSpeed, _owner.DashExitMaxSpeed, _owner.DashExitAcceleration));
    }

    private IEnumerator ProcessExitMovementInternal(Vector3 moveDir, float startSpeed, float maxSpeedValue, float accelerationValue)
    {
        _runtime.ScanPath(_owner, _owner.direction, _owner.MaxPathScanCells, _owner.ExitTravelDistance);
        _runtime.ClearFromGrid(_owner);
        if (ComboManager.Instance != null) ComboManager.Instance.AddCombo(_owner);

        _currentMoveSpeed = startSpeed;
        int lastProcessedGrid = 0;
        _runtime.Outed = false;

        float exitDistance = Mathf.Max(1f, _owner.ExitTravelDistance);
        float finalTargetShift = exitDistance * _runtime.NodesPerUnit;

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, maxSpeedValue, accelerationValue * safeDeltaTime);
            _runtime.AccumulatedShift += safeDeltaTime * _currentMoveSpeed * _runtime.NodesPerUnit;

            UpdateSnakePosition(_runtime.AccumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_runtime.AccumulatedShift / _runtime.NodesPerUnit) + 0.5f);
            while (lastProcessedGrid < currentGridProgress)
            {
                TryCollectKeycardAtGridProgress(lastProcessedGrid + 1);

                Vector2Int gridToLeave = _runtime.GetTailGridPosAtProgress(lastProcessedGrid, _owner.direction);
                _interactions.PlayDotLeaveEffect(gridToLeave);
                lastProcessedGrid++;
            }

            if (_runtime.AccumulatedShift > 2f * _runtime.NodesPerUnit && !_runtime.Outed)
            {
                if (_levelController != null) _levelController.SetCountArrowInGame();
                _renderer.BeginArrowShadowFadeAfterOwnerReleased();
                _runtime.Outed = true;
            }

            if (_runtime.AccumulatedShift >= finalTargetShift)
            {
                Object.Destroy(_owner.gameObject);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator ProcessBlockedMovement(Vector3 moveDir, float targetMaxShift, float distToObstacle)
    {
        _currentMoveSpeed = _owner.StartMoveSpeed;
        int lastProcessedGrid = 0;

        while (true)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float nextShiftAmount = safeDeltaTime * _currentMoveSpeed * _runtime.NodesPerUnit;

            if (_runtime.AccumulatedShift + nextShiftAmount >= targetMaxShift)
            {
                float currentDist = _runtime.ScanPath(_owner, _owner.direction, _owner.MaxPathScanCells, _owner.ExitTravelDistance);
                if (currentDist > distToObstacle)
                {
                    distToObstacle = currentDist;
                    targetMaxShift = distToObstacle * _runtime.NodesPerUnit;
                    if (currentDist == float.MaxValue)
                    {
                        yield return _owner.StartCoroutine(ProcessExitMovement(moveDir));
                        yield break;
                    }

                    if (_runtime.LastObstacleType == ObstacleHitType.BlackHole
                        && GridManager.Instance != null
                        && GridManager.Instance.TryGetBlackHoleAt(_runtime.LastObstacleCell, out GridBlackHole blackHole))
                    {
                        yield return _owner.StartCoroutine(ProcessBlackHoleMovement(moveDir, currentDist, blackHole));
                        yield break;
                    }
                }
                else
                {
                    if (_runtime.LastObstacleType == ObstacleHitType.StopBlock
                        && GridManager.Instance != null
                        && GridManager.Instance.TryGetActiveStopBlockAt(_runtime.LastObstacleCell, out GridStopBlock stopBlock)
                        && stopBlock.CanCapture)
                    {
                        yield return _owner.StartCoroutine(HandleStopBlockCollision(moveDir, distToObstacle, lastProcessedGrid, stopBlock));
                    }
                    else
                    {
                        yield return _owner.StartCoroutine(HandleCollision(moveDir, distToObstacle, lastProcessedGrid));
                    }
                    break;
                }
            }

            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, _owner.MaxMoveSpeed, _owner.Acceleration * safeDeltaTime);
            _runtime.AccumulatedShift += nextShiftAmount;

            UpdateSnakePosition(_runtime.AccumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_runtime.AccumulatedShift / _runtime.NodesPerUnit) + 0.5f);
            while (lastProcessedGrid < currentGridProgress)
            {
                _runtime.UpdateGridOccupancy(_owner, _interactions);
                TryCollectKeycardAtGridProgress(lastProcessedGrid + 1);

                Vector2Int gridToLeave = _runtime.GetTailGridPosAtProgress(lastProcessedGrid, _owner.direction);
                _interactions.PlayDotLeaveEffect(gridToLeave);
                lastProcessedGrid++;
            }

            yield return null;
        }
    }

    private IEnumerator ProcessBlackHoleMovement(Vector3 moveDir, float targetDistance, GridBlackHole blackHole)
    {
        _runtime.ClearFromGrid(_owner);
        if (ComboManager.Instance != null) ComboManager.Instance.AddCombo(_owner);

        _currentMoveSpeed = _owner.ExitStartSpeed;
        int lastProcessedGrid = 0;
        _runtime.Outed = false;

        float targetShift = Mathf.Max(0f, targetDistance) * _runtime.NodesPerUnit;

        while (_runtime.AccumulatedShift < targetShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, _owner.ExitMaxSpeed, _owner.ExitAcceleration * safeDeltaTime);
            float forwardStep = _currentMoveSpeed * _runtime.NodesPerUnit * safeDeltaTime;
            _runtime.AccumulatedShift = Mathf.MoveTowards(_runtime.AccumulatedShift, targetShift, forwardStep);

            UpdateSnakePosition(_runtime.AccumulatedShift, moveDir);

            int currentGridProgress = Mathf.FloorToInt((_runtime.AccumulatedShift / _runtime.NodesPerUnit) + 0.5f);
            while (lastProcessedGrid < currentGridProgress)
            {
                TryCollectKeycardAtGridProgress(lastProcessedGrid + 1);

                Vector2Int gridToLeave = _runtime.GetTailGridPosAtProgress(lastProcessedGrid, _owner.direction);
                _interactions.PlayDotLeaveEffect(gridToLeave);
                lastProcessedGrid++;
            }

            yield return null;
        }

        _runtime.AccumulatedShift = targetShift;
        UpdateSnakePosition(_runtime.AccumulatedShift, moveDir);

        if (blackHole != null) blackHole.PlayEnterFeedback();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.45f, 0.8f);
        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);

        yield return _owner.StartCoroutine(_renderer.PlayBlackHoleConsumeShrink());

        if (!_runtime.Outed)
        {
            if (_levelController != null) _levelController.SetCountArrowInGame();
            _renderer.BeginArrowShadowFadeAfterOwnerReleased();
            _runtime.Outed = true;
        }

        Object.Destroy(_owner.gameObject);
    }

    private IEnumerator HandleStopBlockCollision(Vector3 dir, float dist, int lastProcessedGrid, GridStopBlock stopBlock)
    {
        float targetShift = Mathf.Max(0f, dist) * _runtime.NodesPerUnit;
        int lastStopGrid = lastProcessedGrid;

        while (_runtime.AccumulatedShift < targetShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float forwardStep = Mathf.Max(_currentMoveSpeed, _owner.StartMoveSpeed) * _runtime.NodesPerUnit * safeDeltaTime;
            _runtime.AccumulatedShift = Mathf.MoveTowards(_runtime.AccumulatedShift, targetShift, forwardStep);

            UpdateSnakePosition(_runtime.AccumulatedShift, dir);

            int currentGridProgress = Mathf.FloorToInt((_runtime.AccumulatedShift / _runtime.NodesPerUnit) + 0.5f);
            while (lastStopGrid < currentGridProgress)
            {
                _runtime.UpdateGridOccupancy(_owner, _interactions);
                TryCollectKeycardAtGridProgress(lastStopGrid + 1);

                Vector2Int gridToLeave = _runtime.GetTailGridPosAtProgress(lastStopGrid, _owner.direction);
                _interactions.PlayDotLeaveEffect(gridToLeave);
                lastStopGrid++;
            }

            yield return null;
        }

        _runtime.AccumulatedShift = targetShift;
        UpdateSnakePosition(_runtime.AccumulatedShift, dir);
        _runtime.UpdateGridOccupancy(_owner, _interactions);

        ArrowDir stoppedDirection = _runtime.GetHeadDirectionAtDistance(dist, _owner.direction);
        if (stopBlock == null || !stopBlock.TryActivate(_owner))
        {
            yield return _owner.StartCoroutine(HandleCollision(dir, dist, lastStopGrid));
            yield break;
        }

        if (ComboManager.Instance != null) ComboManager.Instance.StopCombo();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.65f, 0.9f);
        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);

        _owner.direction = stoppedDirection;
        _runtime.CommitCurrentPoseAsOrigin();
        _renderer.SyncArrowVisualPosition();
        _renderer.UpdateVisualRotation(_owner.direction);
        _runtime.UpdateGridOccupancy(_owner, _interactions);
        _renderer.RequestRedraw();
        _runtime.IsStoppedByStopBlock = true;
        _runtime.HoldingStopBlock = stopBlock;
        _renderer.ApplyStopBlockVisual();
    }

    private IEnumerator HandleCollision(Vector3 dir, float dist, int lastProcessedGrid)
    {
        const float bumpFraction = 0.35f;
        float peakShift = (dist + bumpFraction) * _runtime.NodesPerUnit;
        int lastBounceGrid = lastProcessedGrid;

        while (_runtime.AccumulatedShift < peakShift)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float forwardStep = _currentMoveSpeed * _runtime.NodesPerUnit * safeDeltaTime;
            _runtime.AccumulatedShift = Mathf.MoveTowards(_runtime.AccumulatedShift, peakShift, forwardStep);

            UpdateSnakePosition(_runtime.AccumulatedShift, dir);

            int currentGridProgress = Mathf.FloorToInt((_runtime.AccumulatedShift / _runtime.NodesPerUnit) + 0.5f);
            if (currentGridProgress > lastBounceGrid)
            {
                _runtime.UpdateGridOccupancy(_owner, _interactions);
                lastBounceGrid = currentGridProgress;
            }

            yield return null;
        }

        if (!_runtime.HasDealtDamage)
        {
            if (MessageManager.Instance != null) MessageManager.Instance.SendMessage(ManhMessageType.OnTakeDamage, _owner);
            _runtime.HasDealtDamage = true;
        }

        if (ComboManager.Instance != null) ComboManager.Instance.StopCombo();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.8f);
        _renderer.SetColorImmediate(_owner.snakeTakeHitColor);
        if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.MediumImpact);

        while (_runtime.AccumulatedShift > 0f)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            float returnStep = _owner.ReturnMoveSpeed * _runtime.NodesPerUnit * safeDeltaTime;
            _runtime.AccumulatedShift = Mathf.MoveTowards(_runtime.AccumulatedShift, 0f, returnStep);

            UpdateSnakePosition(_runtime.AccumulatedShift, dir);

            int currentGridProgress = Mathf.FloorToInt((_runtime.AccumulatedShift / _runtime.NodesPerUnit) + 0.5f);
            if (currentGridProgress < lastBounceGrid)
            {
                _runtime.UpdateGridOccupancy(_owner, _interactions);
                lastBounceGrid = currentGridProgress;
            }

            yield return null;
        }

        _runtime.CopyOriginalToCurrent();
        _runtime.ClearWarps();
        _renderer.SyncArrowVisualPosition();
        _renderer.UpdateVisualRotation(_owner.direction);
        _runtime.UpdateGridOccupancy(_owner, _interactions);
    }

    private IEnumerator PlaySpawnAnimationFromTail()
    {
        _runtime.IsSpawning = true;
        _runtime.VisiblePoints = Mathf.Min(2f, (float)_runtime.TotalPoints);

        float progress = _runtime.VisiblePoints;
        while (_runtime.VisiblePoints < _runtime.TotalPoints)
        {
            float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            progress += safeDeltaTime * _owner.SpawnSpeed;
            _runtime.VisiblePoints = Mathf.Min(progress, (float)_runtime.TotalPoints);
            yield return null;
        }

        _renderer.FinishSpawnArrow();
        _runtime.VisiblePoints = _runtime.TotalPoints;
        _runtime.IsSpawning = false;
        _renderer.RequestRedraw();
    }

    private void TryCollectKeycardAtGridProgress(int gridProgress)
    {
        float headTrackIdx = -(gridProgress * _runtime.NodesPerUnit);
        Vector2Int headCell = _runtime.GetGridPosFromTrackIndex(headTrackIdx, _owner.direction);
        _interactions.TriggerCellInteractions(headCell);
    }

    private void UpdateSnakePosition(float shift, Vector3 moveDir)
    {
        if (!_runtime.IsInitialized) return;

        float headDist = shift / _runtime.NodesPerUnit;
        _interactions.PlayWarpFeedbacks(_runtime, headDist);

        for (int i = 0; i < _runtime.TotalPoints; i++)
        {
            float trackIdx = -shift + i;
            _runtime.CurrentPositions[i] = _runtime.GetPositionAtTrackIndex(trackIdx, _owner.direction);
        }

        _renderer.SyncArrowVisualPosition();

        ArrowDir currentHeadDir = _runtime.GetHeadDirectionAtDistance(headDist, _owner.direction);
        _renderer.UpdateArrowVisualRotation(currentHeadDir);
    }
}
