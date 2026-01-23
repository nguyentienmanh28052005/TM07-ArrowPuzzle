using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour, IPointerClickHandler
{
    public int level;
    public double posY;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.GetComponent<Button>().interactable)
        {
            //GameManager.Instance.level = level;
            SceneController.Instance.LoadScene("GameScene", false, false);
        }
    }
}
