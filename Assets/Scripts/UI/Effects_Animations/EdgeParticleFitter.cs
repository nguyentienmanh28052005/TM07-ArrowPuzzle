using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class EdgeParticleFitter : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Nhóm Particle Bên Trái")]
    public ParticleSystem[] leftParticles = new ParticleSystem[0];

    [Header("Nhóm Particle Bên Phải")]
    public ParticleSystem[] rightParticles = new ParticleSystem[0];

    [Header("Cài Đặt Vị Trí")]
    [Tooltip("Khoảng cách thụt vào từ viền (đơn vị Unity)")]
    public float paddingX = 0.5f;

    private Camera _cam;

    private void OnEnable()
    {
        RefreshCamera();
        FitParticlesToEdges();
    }

    private void OnValidate()
    {
        _cam = null;
        RefreshCamera();
        FitParticlesToEdges();
    }

    void LateUpdate()
    {
        RefreshCamera();
        if (_cam == null) return;

        FitParticlesToEdges();
    }

    private void RefreshCamera()
    {
        if (targetCamera != null)
        {
            _cam = targetCamera;
            return;
        }

        if (_cam == null) _cam = Camera.main;
        if (_cam == null) _cam = GetComponent<Camera>();
    }

    private void FitParticlesToEdges()
    {
        if (_cam == null) return;

        if (_cam.orthographic)
        {
            float halfHeight = _cam.orthographicSize;
            float halfWidth = _cam.orthographicSize * _cam.aspect;
            Vector3 camPos = _cam.transform.position;

            float leftX = camPos.x - halfWidth + paddingX;
            float rightX = camPos.x + halfWidth - paddingX;

            FitSideOrtho(leftParticles, leftX, halfHeight);
            FitSideOrtho(rightParticles, rightX, halfHeight);
            return;
        }

        FitSidePerspective(leftParticles, true);
        FitSidePerspective(rightParticles, false);
    }

    private void FitSideOrtho(ParticleSystem[] particles, float targetX, float halfHeight)
    {
        if (particles == null) return;

        foreach (var p in particles)
        {
            if (p == null) continue;

            Vector3 worldPos = p.transform.position;
            p.transform.position = new Vector3(targetX, worldPos.y, worldPos.z);

            var shape = p.shape;
            shape.radius = halfHeight;
        }
    }

    private void FitSidePerspective(ParticleSystem[] particles, bool isLeft)
    {
        if (particles == null) return;

        Transform camTransform = _cam.transform;
        Vector3 camPos = camTransform.position;
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        float edgeX = isLeft ? 0f : 1f;
        float padSign = isLeft ? 1f : -1f;

        foreach (var p in particles)
        {
            if (p == null) continue;

            Vector3 worldPos = p.transform.position;
            float depth = Vector3.Dot(worldPos - camPos, camForward);
            if (depth <= 0.001f) continue;

            Vector3 viewportPos = _cam.WorldToViewportPoint(worldPos);
            viewportPos.x = edgeX;
            viewportPos.z = depth;

            Vector3 edgeWorld = _cam.ViewportToWorldPoint(viewportPos);
            edgeWorld += camRight * (paddingX * padSign);
            p.transform.position = edgeWorld;

            Vector3 bottom = _cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth));
            Vector3 top = _cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
            float halfHeight = 0.5f * Vector3.Distance(bottom, top);

            var shape = p.shape;
            shape.radius = halfHeight;
        }
    }
}