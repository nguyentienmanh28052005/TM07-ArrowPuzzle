using UnityEngine;

public class SnakeInput : MonoBehaviour
{
    private void OnMouseDown()
    {
        SnakeBlock parentScript = GetComponentInParent<SnakeBlock>();
        if (parentScript != null)
        {
            parentScript.OnHeadClicked();
        }
    }
}