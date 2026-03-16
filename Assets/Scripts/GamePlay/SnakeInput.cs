using UnityEngine;
using DG.Tweening;

public class SnakeInput : MonoBehaviour
{
    [Header("Effect Settings")]
    public float scaleFactor = 1.3f;
    public float duration = 0.2f;
    public float holdThreshold = 0.15f;

    [Header("Input Settings")]
    public float clickRadius = 0.8f;
    public bool useHaptics = true;

    [Header("Guide Line")]
    [SerializeField] private float guideLineLength = 50f;
    [SerializeField] private float guideLineWidth = 0.05f;
    [SerializeField] private Color guideLineColor = new Color(1f, 1f, 1f, 0.35f);

    private bool isPressed = false;
    private SnakeBlock parentScript;
    private Coroutine holdCoroutine;
    private LineRenderer guideLine;

    private void Awake()
    {
        parentScript = GetComponentInParent<SnakeBlock>();
        CreateGuideLine();
    }

    private void CreateGuideLine()
    {
        GameObject guideObj = new GameObject("GuideLine");
        guideObj.transform.SetParent(transform);
        guideObj.transform.localPosition = Vector3.zero;

        guideLine = guideObj.AddComponent<LineRenderer>();
        guideLine.useWorldSpace = true;
        guideLine.alignment = LineAlignment.TransformZ;
        guideLine.startWidth = guideLineWidth;
        guideLine.endWidth = guideLineWidth;
        guideLine.material = new Material(Shader.Find("Sprites/Default"));
        guideLine.startColor = guideLineColor;
        guideLine.endColor = new Color(guideLineColor.r, guideLineColor.g, guideLineColor.b, 0f);
        guideLine.sortingOrder = 5;
        guideLine.positionCount = 2;
        guideLine.numCornerVertices = 0;
        guideLine.numCapVertices = 0;
        guideLine.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleInputDown();
        if (Input.GetMouseButtonUp(0)) HandleInputUp();
    }

    private void HandleInputDown()
    {
        if (CameraController.IsDragging) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dist = Vector2.Distance(transform.position, mousePos);

        if (dist > clickRadius) return;
        if (!IsClosestToClick(mousePos)) return;

        isPressed = true;
        CameraController.IsGameplayBlocking = true;

        if (parentScript != null)
        {
            parentScript.SetFocusColor(true, duration);
        }

        ShowGuideLine(true);

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(WaitAndScale());
    }

    private void HandleInputUp()
    {
        if (!isPressed) return;

        isPressed = false;
        CameraController.IsGameplayBlocking = false;

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

        ShowGuideLine(false);

        if (parentScript != null)
        {
            parentScript.SetFocusEffect(false, 1f, duration);
        }

        bool willMove = false;

        if (!CameraController.IsDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, mousePos) <= clickRadius)
            {
                willMove = true;
                if (parentScript != null)
                {
                    parentScript.OnHeadClicked();
                }
            }
        }

        if (parentScript != null)
        {
            if (!willMove)
            {
                parentScript.SetFocusColor(false, duration);
            }
        }
    }

    private void ShowGuideLine(bool show)
    {
        if (guideLine == null || parentScript == null) return;

        guideLine.enabled = show;

        if (show)
        {
            UpdateGuideLinePositions();
        }
    }

    private void UpdateGuideLinePositions()
    {
        if (guideLine == null || parentScript == null) return;

        Vector3 headPos = transform.position;
        Vector3 dir = parentScript.GetDirVector(parentScript.direction);
        Vector3 endPos = headPos + dir * guideLineLength;

        guideLine.SetPosition(0, headPos);
        guideLine.SetPosition(1, endPos);
    }

    private void LateUpdate()
    {
        if (isPressed)
        {
            // Update guide line position while held
            if (guideLine != null && guideLine.enabled)
            {
                UpdateGuideLinePositions();
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, mousePos) > clickRadius)
            {
                isPressed = false;
                CameraController.IsGameplayBlocking = false;

                if (holdCoroutine != null) StopCoroutine(holdCoroutine);

                ShowGuideLine(false);

                if (parentScript != null)
                {
                    parentScript.SetFocusEffect(false, 1f, duration);
                    parentScript.SetFocusColor(false, duration);
                }
            }
        }
    }

    private bool IsClosestToClick(Vector2 clickPos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(clickPos, clickRadius);
        float myDistance = Vector2.Distance(transform.position, clickPos);
        foreach (var hit in hits)
        {
            SnakeInput other = hit.GetComponent<SnakeInput>();
            if (other != null && other != this && Vector2.Distance(other.transform.position, clickPos) < myDistance) return false;
        }
        return true;
    }

    private System.Collections.IEnumerator WaitAndScale()
    {
        yield return new WaitForSeconds(holdThreshold);
        if (isPressed && parentScript != null)
        {
            parentScript.SetFocusEffect(true, scaleFactor, duration);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (isPressed) CameraController.IsGameplayBlocking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, clickRadius);
    }
}