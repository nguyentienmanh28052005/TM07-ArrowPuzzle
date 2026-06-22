using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopTextController : MonoBehaviour
{
    [SerializeField] List<PopText> listPopText;
    [SerializeField] PopText popTextPrefab;
    private RectTransform rectTransform;
    public RectTransform RectTF
    {
        get {
            if(rectTransform == null)
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
    public PopText GetPopText()
    {
        PopText popText = null;
        for(int i = 0; i < listPopText.Count; i++)
        {
            if (!listPopText[i].gameObject.activeInHierarchy)
            {
                popText = listPopText[i];
                break;
            }
        }
        if(popText == null)
        {
            popText = Instantiate(popTextPrefab, transform);
            listPopText.Add(popText);
        }
        popText.gameObject.SetActive(true);
        return popText;
    }
    public void ShowPopText(string msg, Vector2 scrPoint,int layerSorting = 160)
    {
        PopText popText = GetPopText();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTF, scrPoint, RewardReceivedHub.Instance.Canvas.worldCamera, out Vector2 localPoint);
        popText.RectTF.anchoredPosition = localPoint;
        popText.Initialize(msg, layerSorting);
    }
}
