using UnityEngine;
using DG.Tweening;

public class RotateSprite : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationDuration = 10f; 
    public bool clockwise = true;

    private void OnEnable()
    {
        transform.localRotation = Quaternion.identity;

        float direction = clockwise ? -360f : 360f;
        transform.DORotate(new Vector3(0, 0, direction), rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}