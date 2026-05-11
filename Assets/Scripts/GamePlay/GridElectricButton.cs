using UnityEngine;
using DG.Tweening;

public class GridElectricButton : MonoBehaviour
{
    public Color buttonColor = Color.white;

    private bool _isPressed;
    private Coroutine _registerRoutine;
    private GridManager _registeredManager;

    private void Start()
    {
        TryRegister();
        ApplyColor();
    }

    private void OnEnable()
    {
        TryRegister();
        ApplyColor();
    }

    private void OnDisable()
    {
        if (_registerRoutine != null)
        {
            StopCoroutine(_registerRoutine);
            _registerRoutine = null;
        }
        Unregister();
    }

    public void Press()
    {
        if (_isPressed) return;
        _isPressed = true;

        Unregister();

        if (GridManager.Instance != null)
            GridManager.Instance.RaiseElectricButtonPressed(buttonColor);

        transform.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    public void SetColor(Color color)
    {
        buttonColor = color;
        ApplyColor();
    }

    private void ApplyColor()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = buttonColor;
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
            UnregisterFromManager(_registeredManager);
        }

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        manager.ElectricButtonMap[pos] = this;
        _registeredManager = manager;
    }

    private System.Collections.IEnumerator WaitAndRegister()
    {
        while (GridManager.Instance == null) yield return null;
        _registerRoutine = null;
        TryRegister();
    }

    private void Unregister()
    {
        if (_registeredManager == null) return;
        UnregisterFromManager(_registeredManager);
        _registeredManager = null;
    }

    private void UnregisterFromManager(GridManager manager)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (manager.ElectricButtonMap != null && manager.ElectricButtonMap.TryGetValue(pos, out GridElectricButton existing) && existing == this)
        {
            manager.ElectricButtonMap.Remove(pos);
        }
    }
}
