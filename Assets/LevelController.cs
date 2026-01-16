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
            GameManager.Instance.level++;
            SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameMenu");
        }
    }
}
