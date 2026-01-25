using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private int countArrowInGame;

    void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.levelDataSOs != null)
        {
            countArrowInGame = GameManager.Instance.levelDataSOs[GameManager.Instance.level - 1].snakes.Count;
        }
    }

    public void SetCountArrowInGame()
    {
        countArrowInGame--;
        if (countArrowInGame <= 0)
        {
            Debug.Log("Level Complete");
            if (GameManager.Instance != null && GameManager.Instance.level < GameManager.Instance.currentMaxLevel)
            {
                GameManager.Instance.level++;
                SaveDataPlayer.Instance.Save(1, GameManager.Instance.level);
            }

            CameraController cam = FindObjectOfType<CameraController>();
            if (cam != null)
            {
                cam.ZoomToMax();
            }

            float effectDuration = 0f;
            WinEffectManager winEffect = FindObjectOfType<WinEffectManager>();
            if (winEffect != null)
            {
                effectDuration = winEffect.PlayWinEffect();
            }

            StartCoroutine(SequenceWinGame(effectDuration));
        }
    }

    public IEnumerator SequenceWinGame(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        GameCanvas canvas = FindObjectOfType<GameCanvas>();
        if (canvas != null)
        {
            canvas.ShowWinText();
        }

        yield return new WaitForSeconds(2f);

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene("GameMenu", false, false);
        }
    }
}