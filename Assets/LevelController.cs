using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private int countArrowInGame;

    void Awake()
    {
        countArrowInGame = GameManager.Instance.levelDataSOs[GameManager.Instance.level - 1].snakes.Count;
    }

    void Update()
    {
        
    }

    public void SetCountArrowInGame()
    {
        countArrowInGame--;
        if(countArrowInGame <= 0)
        {
            Debug.Log("Level Complete");
            if(GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
            {
                GameManager.Instance.level++;
                SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
            }
            FindObjectOfType<WinEffectManager>().PlayWinEffect();
            StartCoroutine(DelayLoadMenu(3f));
        }
    }

    public IEnumerator DelayLoadMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneController.Instance.LoadScene("GameMenu", false, false);
    }
}
