using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] private GameObject gameContainer;
    [SerializeField] private Transform health;
    [SerializeField] private TextMeshProUGUI text;
    private List<GameObject> hearts;

    private int countHeart;

    void Start()
    {
        countHeart = health.childCount;
        hearts = new List<GameObject>(countHeart);
        foreach (Transform child in health)
        {
            hearts.Add(child.gameObject);
        }

        if (text != null)
        {
            text.gameObject.SetActive(false);
            text.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        MessageManager.Instance.AddSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
    }

    private void OnDisable()
    {
        MessageManager.Instance.RemoveSubscriber(ManhMessageType.OnTakeDamage, DecreaseHeart);
    }

    public void RestartGame()
    {
        SceneController.Instance.LoadScene("GameScene", false, false);
    }

    public void OutLevel()
    {
        SceneController.Instance.LoadScene("GameMenu", false, false);
    }

    public void ShowWinText()
    {
        string message = "";

        if (countHeart >= 3)
        {
            message = "Perfect!";
        }
        else if (countHeart == 2)
        {
            message = "Great!";
        }
        else
        {
            message = "Good!";
        }

        ShowText(message);
    }

    private void ShowText(string content)
    {
        if (text != null)
        {
            text.text = content;
            text.gameObject.SetActive(true);
            text.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(text.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            seq.Join(text.DOFade(1f, 0.5f));
        }
    }

    public void DecreaseHeart(object data)
    {
        if (countHeart > 0)
        {
            countHeart--;
            GameObject heartObj = hearts[countHeart];

            PlayHeartLossEffect(heartObj);

            if (countHeart <= 0)
            {
                if (gameContainer != null)
                {
                    gameContainer.SetActive(false);
                }              
                StartCoroutine(DelayOutLevel(2f));
            }
        }
    }

    public IEnumerator DelayOutLevel(float delay)
    {
        yield return new WaitForSeconds(1f);
        ShowText("Game Over");
        yield return new WaitForSeconds(delay);
        OutLevel();
    }

    private void PlayHeartLossEffect(GameObject heart)
    {
        RectTransform rect = heart.GetComponent<RectTransform>();
        Image img = heart.GetComponent<Image>();
        Vector2 originalPos = rect.anchoredPosition;

        rect.DOKill();
        if (img != null) img.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOShakeAnchorPos(0.4f, 15f, 20, 90, false));

        if (img != null)
        {
            seq.Join(img.DOColor(Color.gray, 0.2f));
        }

        seq.Append(rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));

        if (img != null)
        {
            seq.Join(img.DOFade(0f, 0.3f));
        }

        seq.OnComplete(() =>
        {
            heart.SetActive(false);

            rect.localScale = Vector3.one;
            rect.anchoredPosition = originalPos;

            if (img != null)
            {
                img.color = Color.white;
                var tempColor = img.color;
                tempColor.a = 1f;
                img.color = tempColor;
            }
        });
    }
}