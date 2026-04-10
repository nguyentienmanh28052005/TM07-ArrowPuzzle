using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    public void Setup(RewardData data)
    {
        if (iconImage != null) iconImage.sprite = data.rewardIcon;
        if (quantityText != null) quantityText.text = "x" + data.quantityText;
    }
}