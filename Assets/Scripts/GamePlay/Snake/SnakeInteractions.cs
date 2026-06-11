using UnityEngine;
using Solo.MOST_IN_ONE;

public sealed class SnakeInteractions
{
    public void TriggerCellInteractions(Vector2Int cell)
    {
        TriggerBoardCell(cell);
    }

    public void PlayDotLeaveEffect(Vector2Int cell)
    {
        PlayDotLeave(cell);
    }

    public void PlayWarpFeedbacks(SnakeRuntime runtime, float headDistance)
    {
        if (runtime == null || runtime.ActiveWarps == null || runtime.ActiveWarps.Count == 0) return;

        int passedPortalIndex = -1;
        int passedDeflectorIndex = -1;
        for (int i = 0; i < runtime.ActiveWarps.Count; i++)
        {
            if (headDistance < runtime.ActiveWarps[i].rawDistFromHead0) continue;
            if (runtime.ActiveWarps[i].isPortal) passedPortalIndex = i;
            else passedDeflectorIndex = i;
        }

        PlayDeflectorFeedbacks(runtime, passedDeflectorIndex);
        PlayPortalFeedbacks(runtime, passedPortalIndex);
    }

    public static void TriggerBoardCell(Vector2Int cell)
    {
        GridManager grid = GridManager.Instance;
        if (grid == null) return;

        grid.TriggerAt(cell);
    }

    public static void PlayDotLeave(Vector2Int cell)
    {
        if (GridDotBatchRenderer.TryPlayLeaveEffect(cell)) return;

        if (GridDot.GridMap.TryGetValue(cell, out GridDot dotToAnimate))
            dotToAnimate.PlayLeaveEffect();
    }

    private static void PlayDeflectorFeedbacks(SnakeRuntime runtime, int passedDeflectorIndex)
    {
        if (passedDeflectorIndex > runtime.LastPassedDeflectorIndex)
        {
            bool playedDeflectorFeedback = false;
            for (int i = runtime.LastPassedDeflectorIndex + 1; i <= passedDeflectorIndex; i++)
            {
                if (i < 0 || i >= runtime.ActiveWarps.Count) continue;
                if (runtime.ActiveWarps[i].isPortal) continue;

                Vector2Int deflectorCell = new Vector2Int(
                    Mathf.RoundToInt(runtime.ActiveWarps[i].portalWorldPos.x),
                    Mathf.RoundToInt(runtime.ActiveWarps[i].portalWorldPos.y));

                if (runtime.ActiveWarps[i].deflector != null)
                    runtime.ActiveWarps[i].deflector.PlayInteractionFeedback();
                else
                    GridDeflectorVisual.PlayInteractionAtCell(deflectorCell);

                playedDeflectorFeedback = true;
            }

            runtime.LastPassedDeflectorIndex = passedDeflectorIndex;
            if (playedDeflectorFeedback)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.35f, 1.35f);
                if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
            }
        }
        else if (passedDeflectorIndex < runtime.LastPassedDeflectorIndex)
        {
            runtime.LastPassedDeflectorIndex = passedDeflectorIndex;
        }
    }

    private static void PlayPortalFeedbacks(SnakeRuntime runtime, int passedPortalIndex)
    {
        if (passedPortalIndex > runtime.LastPassedPortalIndex)
        {
            for (int i = runtime.LastPassedPortalIndex + 1; i <= passedPortalIndex; i++)
            {
                if (i < 0 || i >= runtime.ActiveWarps.Count) continue;
                if (!runtime.ActiveWarps[i].isPortal) continue;

                Vector2Int entryCell = new Vector2Int(
                    Mathf.RoundToInt(runtime.ActiveWarps[i].portalWorldPos.x),
                    Mathf.RoundToInt(runtime.ActiveWarps[i].portalWorldPos.y));
                Vector2Int exitCell = new Vector2Int(
                    Mathf.RoundToInt(runtime.ActiveWarps[i].exitWorldPos.x),
                    Mathf.RoundToInt(runtime.ActiveWarps[i].exitWorldPos.y));

                GridPortalVisual.PlayEnterAtCell(entryCell);
                GridPortalVisual.PlayExitAtCell(exitCell);
            }

            runtime.LastPassedPortalIndex = passedPortalIndex;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(AudioManager.Instance.sfxArrowHit, 0.5f, 1.8f);
            if (SettingManager.Instance != null) SettingManager.Instance.PlayHaptic(MOST_HapticFeedback.HapticTypes.LightImpact);
        }
        else if (passedPortalIndex < runtime.LastPassedPortalIndex)
        {
            runtime.LastPassedPortalIndex = passedPortalIndex;
        }
    }
}
