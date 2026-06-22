using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CenterOnChildScrollRect : ScrollRect
{
    public float springStrength = 8f;
    public float nextPageThreshold = 100f;
    GameObject mCenteredObject;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        SpringPanel.Stop(content.gameObject);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        Recenter(eventData);
    }

    public void Recenter(PointerEventData eventData)
    {
        Transform trans = content;
        if (trans.childCount == 0) return;

        // Calculate the panel's center in world coordinates
        var corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;

        // Offset this value by the momentum
        Vector3 pickingPoint = panelCenter; // Magic number based on what "feels right"

        float min = float.MaxValue;
        Transform closest = null;
        int index = 0;
        int ignoredIndex = 0;
        
        for (int i = 0, imax = trans.childCount, ii = 0; i < imax; ++i)
        {
            Transform t = trans.GetChild(i);
            if (!t.gameObject.activeInHierarchy) continue;
            float sqrDist = Vector3.SqrMagnitude(t.position - pickingPoint);

            if (sqrDist < min)
            {
                min = sqrDist;
                closest = t;
                index = i;
                ignoredIndex = ii;
            }
            ++ii;
        }
        Debug.LogError(velocity);

        // // If we have a touch in progress and the next page threshold set
        if (nextPageThreshold > 0f)
        {
            // If we're still on the same object
            if (mCenteredObject != null && mCenteredObject.transform == trans.GetChild(index))
            {
                float delta = 0f;
                if (horizontal)
                {
                    delta = velocity.x;
                }
                else if (vertical)
                {
                    delta = -velocity.y;
                }
                else
                {
                    delta = velocity.magnitude;
                }
        
                if (Mathf.Abs(delta) > nextPageThreshold)
                {
                    if (delta > nextPageThreshold)
                    {
                        // Next page
                        if (ignoredIndex > 0)
                        {
                            closest = trans.GetChild(ignoredIndex - 1);
                        }
                    }
                    else if (delta < -nextPageThreshold)
                    {
                        // Previous page
                        if (ignoredIndex < trans.childCount - 1)
                        {
                            closest = trans.GetChild(ignoredIndex + 1);
                        }
                    }
                }
            }
        }
        CenterOn(closest, panelCenter);
    }

    void CenterOn(Transform target, Vector3 panelCenter)
    {
        if (target != null)
        {
            Transform panelTrans = content.transform;
            mCenteredObject = target.gameObject;

            // Figure out the difference between the chosen child and the panel's center in local coordinates
            Vector3 cp = panelTrans.InverseTransformPoint(target.position);
            Vector3 cc = panelTrans.InverseTransformPoint(panelCenter);
            Vector3 localOffset = cp - cc;

            // Offset shouldn't occur if blocked
            if (!horizontal) localOffset.x = 0f;
            if (!vertical) localOffset.y = 0f;
            localOffset.z = 0f;


            var pos = panelTrans.localPosition - localOffset;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);
            
            StopMovement();
            SpringPanel.Begin(content.gameObject, pos, springStrength);
        }
        else
        {
            mCenteredObject = null;
        }
    }
    
    public void CenterOn (Transform target)
    {
        var corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        Vector3 panelCenter = (corners[2] + corners[0]) * 0.5f;
        CenterOn(target, panelCenter);
    }
}