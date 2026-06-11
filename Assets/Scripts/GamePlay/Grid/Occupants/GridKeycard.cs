using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GridKeycard : GridOccupantBehaviour, IGridTrigger
{
    public Color keyColor = Color.white;
    private bool _isCollected = false;

    public override bool IsActiveOccupant => base.IsActiveOccupant && !_isCollected;

    private void Start()
    {
        RegisterOccupantOrWait();
    }

    private void OnEnable()
    {
        RegisterOccupantOrWait();
    }

    private void OnDisable()
    {
        StopPendingOccupantRegistration();
        UnregisterOccupant();
    }

    public void Collect()
    {
        if (_isCollected) return;
        _isCollected = true;

        UnregisterOccupant();

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[GridKeycard] Collect color={keyColor} instanceId={GetInstanceID()} manager={(GridManager.Instance != null ? GridManager.Instance.GetInstanceID().ToString() : "null")}");
    #endif

        GridManager manager = GridManager.Instance;
        CardGateConnectionEffectManager effectManager = CardGateConnectionEffectManager.GetOrCreateDefault();

        bool usingConnectionEffect = manager != null && effectManager != null && effectManager.isActiveAndEnabled;
        if (usingConnectionEffect)
        {
            List<GridLaserGate> matchingGates = GetMatchingGates(manager);
            effectManager.PlayEffect(this, matchingGates, gate =>
            {
                if (gate == null) return;
                gate.RemoveAfterCardGateEffect(effectManager.DestroyGateAfterEffect, effectManager.DisableGateAfterEffect);
            });
            manager.RaiseKeyCollected(keyColor);
        }
        else if (manager != null)
        {
            manager.RaiseKeyCollected(keyColor);
        }
        
        float cardRemoveDelay = usingConnectionEffect ? Mathf.Max(0f, effectManager.CardPulseDuration) : 0f;
        transform.DOScale(0f, 0.3f).SetDelay(cardRemoveDelay).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    private List<GridLaserGate> GetMatchingGates(GridManager manager)
    {
        List<GridLaserGate> matchingGates = new List<GridLaserGate>();
        if (manager == null) return matchingGates;

        foreach (GridLaserGate gate in manager.Gates)
        {
            if (gate == null || !gate.isActiveAndEnabled) continue;
            if (gate.MatchesColor(keyColor) && !matchingGates.Contains(gate)) matchingGates.Add(gate);
        }

        return matchingGates;
    }

    public void TriggerFromGrid()
    {
        Collect();
    }
}
