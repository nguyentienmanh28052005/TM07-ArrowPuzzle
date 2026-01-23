using UnityEngine;
using System.Collections;

public class GridDot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    public void PlayWinAnimation(Color targetColor, float delay, float scaleAmount, float duration)
    {
        StartCoroutine(CoWinAnimation(targetColor, delay, scaleAmount, duration));
    }

    private IEnumerator CoWinAnimation(Color targetColor, float delay, float scaleAmount, float totalDuration)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        float halfDuration = totalDuration / 2f;
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Vector3 targetScaleVec = originalScale * scaleAmount;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localScale = Vector3.Lerp(originalScale, targetScaleVec, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            transform.localScale = Vector3.Lerp(targetScaleVec, Vector3.zero, t);
            spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        spriteRenderer.color = targetColor;
    }
}