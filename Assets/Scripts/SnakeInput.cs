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

    private bool isPressed = false;
    private SnakeBlock parentScript;
    private Coroutine holdCoroutine;

    private void Awake()
    {
        parentScript = GetComponentInParent<SnakeBlock>();
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

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(WaitAndScale());
    }

    private void HandleInputUp()
    {
        if (!isPressed) return;

        isPressed = false;
        CameraController.IsGameplayBlocking = false;

        if (holdCoroutine != null) StopCoroutine(holdCoroutine);

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

    private void LateUpdate()
    {
        if (isPressed)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(transform.position, mousePos) > clickRadius)
            {
                isPressed = false;
                CameraController.IsGameplayBlocking = false;

                if (holdCoroutine != null) StopCoroutine(holdCoroutine);

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