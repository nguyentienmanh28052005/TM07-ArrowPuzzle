using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<BundleSO> shopBundles;
    public GameObject bundlePrefab;
    public Transform contentContainer;

    private void Start()
    {
        PopulateShop();
    }

    private void PopulateShop()
    {
        if (contentContainer != null)
        {
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (shopBundles == null || bundlePrefab == null || contentContainer == null) return;

        foreach (var bundle in shopBundles)
        {
            GameObject bundleObj = Instantiate(bundlePrefab, contentContainer);
            BundleUI bundleUI = bundleObj.GetComponent<BundleUI>();
            
            if (bundleUI != null)
            {
                bundleUI.Initialize(bundle, TryPurchaseBundle);
            }
        }
    }

    private void TryPurchaseBundle(BundleSO bundleToBuy)
    {
        Debug.Log("Bat dau quy trinh thanh toan cho Gói: " + bundleToBuy.bundleId);
    }
}