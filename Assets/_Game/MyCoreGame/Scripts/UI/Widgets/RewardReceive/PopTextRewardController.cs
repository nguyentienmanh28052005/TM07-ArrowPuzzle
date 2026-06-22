using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopTextRewardController : MonoBehaviour
{
    [SerializeField] List<PopTextReward> listPopText;
    [SerializeField] PopTextReward popTextPrefab;
    private RectTransform rectTransform;
    public RectTransform RectTF
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }
    private void Awake()
    {
        popTextPrefab.gameObject.SetActive(false);
    }
    public PopTextReward GetPopText()
    {
        PopTextReward popText = null;
        for (int i = 0; i < listPopText.Count; i++)
        {
            if (!listPopText[i].gameObject.activeInHierarchy)
            {
                popText = listPopText[i];
                break;
            }
        }
        if (popText == null)
        {
            popText = Instantiate(popTextPrefab, transform);
            listPopText.Add(popText);
        }
        popText.gameObject.SetActive(true);
        return popText;
    }
    public void ShowPopText(DataResource dataResource, Vector2 scrPoint,float scale =1)
    {
        PopTextReward popText = GetPopText();
        popText.transform.localScale = Vector3.one * scale;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTF, scrPoint, RewardReceivedHub.Instance.Canvas.worldCamera, out Vector2 localPoint);
        popText.RectTF.anchoredPosition = localPoint;
        popText.Initialize(dataResource);
    }
}
