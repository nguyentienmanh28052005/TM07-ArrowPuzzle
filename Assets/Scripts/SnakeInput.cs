using UnityEngine;

public class SnakeInput : MonoBehaviour
{
    // Khi click vào cái Đầu (Con), hàm này sẽ chạy
    private void OnMouseDown()
    {
        // Tìm script SnakeBlock ở object Cha và gọi hàm xử lý
        SnakeBlock parentScript = GetComponentInParent<SnakeBlock>();
        if (parentScript != null)
        {
            parentScript.OnHeadClicked();
        }
    }
}