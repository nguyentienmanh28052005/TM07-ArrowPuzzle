using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BundleUI : MonoBehaviour
{
    public TextMeshProUGUI bundleNameText;
    public Image mainIconImage;
    public TextMeshProUGUI mainAmountText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public GameObject bestSellerTag;

    public Transform rewardsContainer;
    public GameObject rewardPrefab;

    private BundleSO _currentBundle;
    private Action<BundleSO> _onBuyCallback;

    public void Initialize(BundleSO bundleData, Action<BundleSO> onBuy)
    {
        _currentBundle = bundleData;
        _onBuyCallback = onBuy;

        if (bundleNameText != null) bundleNameText.text = _currentBundle.bundleName;
        if (mainAmountText != null) mainAmountText.text = "x" + _currentBundle.mainAmountText;
        if (mainIconImage != null) mainIconImage.sprite = _currentBundle.mainIcon;
        if (priceText != null) priceText.text = _currentBundle.priceString;
        if (bestSellerTag != null) bestSellerTag.SetActive(_currentBundle.isBestSeller);

        foreach (Transform child in rewardsContainer)
        {
            Destroy(child.gameObject);
        }

        if (_currentBundle.subRewards != null)
        {
            foreach (var reward in _currentBundle.subRewards)
            {
                GameObject rewardObj = Instantiate(rewardPrefab, rewardsContainer);
                RewardUI rewardUI = rewardObj.GetComponent<RewardUI>();
                if (rewardUI != null)
                {
                    rewardUI.Setup(reward);
                }
            }
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnBuyClicked()
    {
        _onBuyCallback?.Invoke(_currentBundle);
    }
}