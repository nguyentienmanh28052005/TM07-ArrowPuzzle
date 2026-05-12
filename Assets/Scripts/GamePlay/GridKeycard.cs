using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GridKeycard : MonoBehaviour
{
    public Color keyColor = Color.white;
    private bool _isCollected = false;
    private Coroutine _registerRoutine;
    private GridManager _registeredManager;

    private void Start()
    {
        TryRegister();
    }

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        if (_registerRoutine != null)
        {
            StopCoroutine(_registerRoutine);
            _registerRoutine = null;
        }
    }

    private void TryRegister()
    {
        var manager = GridManager.Instance;
        if (manager == null)
        {
            if (_registerRoutine == null) _registerRoutine = StartCoroutine(WaitAndRegister());
            return;
        }

        if (_registeredManager != null && _registeredManager != manager)
        {
            Vector2Int oldPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            if (_registeredManager.KeycardMap != null && _registeredManager.KeycardMap.TryGetValue(oldPos, out GridKeycard existing) && existing == this)
            {
                _registeredManager.KeycardMap.Remove(oldPos);
            }
        }

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        manager.KeycardMap[pos] = this;
        _registeredManager = manager;
    }

    private System.Collections.IEnumerator WaitAndRegister()
    {
        while (GridManager.Instance == null) yield return null;
        _registerRoutine = null;
        TryRegister();
    }

    public void Collect()
    {
        if (_isCollected) return;
        _isCollected = true;

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (GridManager.Instance != null && GridManager.Instance.KeycardMap.ContainsKey(pos))
        {
            GridManager.Instance.KeycardMap.Remove(pos);
        }

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
        if (manager == null || manager.GateMap == null) return matchingGates;

        foreach (GridLaserGate gate in manager.GateMap.Values)
        {
            if (gate == null || !gate.isActiveAndEnabled) continue;
            if (gate.MatchesColor(keyColor) && !matchingGates.Contains(gate)) matchingGates.Add(gate);
        }

        return matchingGates;
    }
}
