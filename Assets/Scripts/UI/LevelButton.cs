using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour, IPointerClickHandler
{
    public int level;
    public double posY;

    /// <summary>
    /// Chuyển đổi Scene sang màn hình chơi game khi nút Level hợp lệ được nhấn.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.GetComponent<Button>().interactable)
        {
            SceneController.Instance.LoadScene("GameScene", false, false);
        }
    }
}