using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartGame()
    {
        SceneController.Instance.LoadScene("GameScene", false, false);
    }

    public void OutLevel()
    {
        SceneController.Instance.LoadScene("GameMenu", false, false);
    }
}
