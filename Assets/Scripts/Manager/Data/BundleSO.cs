using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBundle", menuName = "ArrowPuzzle/ShopBundle")]
public class BundleSO : ScriptableObject
{
    public string bundleId;
    public string bundleName;
    public Sprite mainIcon;
    public string mainAmountText;
    public string priceString;
    public bool isBestSeller;
    public List<RewardData> subRewards;
}