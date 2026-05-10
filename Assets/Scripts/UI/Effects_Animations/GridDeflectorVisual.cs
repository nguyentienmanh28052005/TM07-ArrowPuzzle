using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(GridDeflector))]
public class GridDeflectorVisual : MonoBehaviour
{
    [Header("Spawn Effect")]
    [SerializeField] private float spawnDuration = 1.5f;
    [SerializeField] private Ease spawnScaleEase = Ease.OutBack;
    [SerializeField, Min(0f)] private float spawnSpinTurns = 3f;
    
    // Đã xóa bớt các biến StartTilt, OvershootAngle, ReturnAngle rườm rà
    // Vì DOTween OutBack sẽ tự động nội suy quán tính cực kỳ chuẩn!
    [Tooltip("Độ nảy (quá đà) khi dừng lại. Càng cao giật lại càng mạnh.")]
    [SerializeField] private float overshootPower = 1f; 

    [Header("End Game Vanish")]
    [SerializeField] private float endVanishDuration = 0.28f;
    [SerializeField] private Ease endVanishEase = Ease.InBack;

    private static readonly HashSet<GridDeflectorVisual> ActiveDeflectors = new HashSet<GridDeflectorVisual>();

    [SerializeField] private SpriteRenderer targetRenderer;

    private Vector3 _baseScale;
    private float _baseAlpha = 1f;
    private float _baseRotationZ;
    private bool _isVanishing;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        _baseScale = transform.localScale;
        _baseRotationZ = NormalizeAngle(transform.eulerAngles.z);
        
        if (targetRenderer != null)
        {
            _baseAlpha = targetRenderer.color.a;
        }
    }

    private void OnEnable()
    {
        ActiveDeflectors.Add(this);
        PlaySpawnEffect();
    }

    private void OnDisable()
    {
        ActiveDeflectors.Remove(this);
        transform.DOKill();
        if (targetRenderer != null)
        {
            targetRenderer.DOKill();
        }
    }

    private void PlaySpawnEffect()
    {
        if (targetRenderer == null) return;

        _isVanishing = false;
        _baseRotationZ = NormalizeAngle(transform.eulerAngles.z);

        transform.DOKill();
        targetRenderer.DOKill();

        // 1. SETUP TRẠNG THÁI CỐ ĐỊNH
        transform.localScale = Vector3.zero;
        
        // Ép nó nằm đúng góc Đích ngay từ đầu
        transform.rotation = Quaternion.Euler(0f, 0f, _baseRotationZ);

        Color color = targetRenderer.color;
        color.a = 0f;
        targetRenderer.color = color;

        Sequence spawnSequence = DOTween.Sequence().SetLink(gameObject);

        spawnSequence.Insert(0f, transform.DOScale(_baseScale, spawnDuration)
            .SetEase(spawnScaleEase));

        spawnSequence.Insert(0f, targetRenderer.DOFade(_baseAlpha, spawnDuration * 0.5f)
            .SetEase(Ease.OutQuad));

        // 2. ÉP XOAY TƯƠNG ĐỐI (RELATIVE)
        // Ép spawnSpinTurns thành số nguyên (ví dụ 2, 3, 5) để đảm bảo nó quay đủ vòng
        // và KHÔNG BAO GIỜ bị lệch góc khi dừng lại
        float totalDegrees = 360f * Mathf.Round(spawnSpinTurns); 

        // Thêm hàm SetRelative(true) để lách luật của Unity
        spawnSequence.Insert(0f, transform.DORotate(new Vector3(0f, 0f, totalDegrees), spawnDuration, RotateMode.FastBeyond360)
            .SetRelative(true) 
            .SetEase(Ease.OutBack, overshootPower)); 
    }

    private float PlayEndGameVanish(float delay)
    {
        if (targetRenderer == null || !isActiveAndEnabled) return 0f;
        if (_isVanishing) return delay + endVanishDuration;

        _isVanishing = true;

        transform.DOKill();
        targetRenderer.DOKill();

        transform.localScale = _baseScale;
        Color color = targetRenderer.color;
        color.a = _baseAlpha;
        targetRenderer.color = color;

        transform.DOScale(0f, endVanishDuration)
            .SetEase(endVanishEase)
            .SetDelay(delay)
            .SetLink(gameObject);

        targetRenderer.DOFade(0f, endVanishDuration * 0.9f)
            .SetEase(Ease.InQuad)
            .SetDelay(delay)
            .SetLink(gameObject);

        return delay + endVanishDuration;
    }

    public static float PlayEndGameVanishAll(float stepDelay = 0.03f)
    {
        if (ActiveDeflectors.Count == 0) return 0f;

        GridDeflectorVisual[] visuals = new GridDeflectorVisual[ActiveDeflectors.Count];
        ActiveDeflectors.CopyTo(visuals);

        float maxDuration = 0f;
        int index = 0;
        for (int i = 0; i < visuals.Length; i++)
        {
            GridDeflectorVisual visual = visuals[i];
            if (visual == null) continue;

            float doneAt = visual.PlayEndGameVanish(index * stepDelay);
            if (doneAt > maxDuration) maxDuration = doneAt;
            index++;
        }

        return maxDuration;
    }

    public static void ClearAll()
    {
        ActiveDeflectors.Clear();
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}