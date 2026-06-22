using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemFlyBase : MonoBehaviour
{
    public int layerSort;

    private RectTransform rectTransform;
    public RectTransform RectTF
    {
        get
        {
            if(rectTransform == null)
            {
                rectTransform=GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }
    public virtual void Initialize()
    {

    }
    public virtual Tween AnimAppear()
    {
        return null;
    }
    public virtual Tween MoveTo(Vector3 destination,float time)
    {
        Vector3 startPos = transform.position;
        Vector3 midPoint = Vector3.zero;
        if (destination.y > startPos.y)
        {
            midPoint.y = startPos.y - 3;
            if (destination.x > startPos.x)
            {
                midPoint.x = startPos.x + 2;
            }
            else
            {
                midPoint.x = startPos.x - 2;
            }
            Vector3[] path = BezierCurve.GetPath(startPos, destination, midPoint, 20).ToArray();

            return RectTF.DOPath(path, time).SetEase(Ease.InElastic);
        }
        else
        {
            return RectTF.DOMove(destination,time);
        }
        
        
    }
    public virtual void SetUpLayer(int sortingOrder)
    {
        layerSort = sortingOrder;
    }
}
