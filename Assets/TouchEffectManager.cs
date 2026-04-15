using UnityEngine;
using DG.Tweening;
using Pixelplacement;

public class TouchEffectManager : Singleton<TouchEffectManager>
{
    [Header("References")]
    [SerializeField] private GameObject touchRipplePrefab;

    [Header("Ripple Settings")]
    [SerializeField] private float targetScale = 2.5f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private Ease expandEase = Ease.OutCirc;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SpawnRipple(mousePos);
        }
    }

    private void SpawnRipple(Vector2 position)
    {
        if (touchRipplePrefab == null) return;

        GameObject ripple = Instantiate(touchRipplePrefab, position, Quaternion.identity);
        SpriteRenderer sr = ripple.GetComponent<SpriteRenderer>();

        // Fallback cleanup: guarantees destroy even if tween is interrupted.
        DOVirtual.DelayedCall(duration + 0.2f, () =>
        {
            if (ripple != null) Destroy(ripple);
        }, true);

        ripple.transform.localScale = Vector3.one * 0.2f;
        ripple.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        if (sr != null)
        {
            Color startColor = sr.color;
            startColor.a = 0.8f;
            sr.color = startColor;

            Sequence seq = DOTween.Sequence();
            seq.Append(ripple.transform.DOScale(targetScale, duration).SetEase(expandEase));
            seq.Join(sr.DOFade(0f, duration).SetEase(fadeEase));
            seq.SetUpdate(true);
            seq.OnComplete(() =>
            {
                if (ripple != null) Destroy(ripple);
            });
            seq.OnKill(() =>
            {
                if (ripple != null) Destroy(ripple);
            });
            return;
        }

        // If no SpriteRenderer exists on prefab, still auto-destroy to avoid leaked objects.
        ripple.transform.DOScale(targetScale, duration)
            .SetEase(expandEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (ripple != null) Destroy(ripple);
            });
    }
}