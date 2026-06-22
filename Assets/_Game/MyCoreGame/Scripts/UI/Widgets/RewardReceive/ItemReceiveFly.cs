using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using master;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ItemReceiveFly : MonoBehaviour
{
    [SerializeField] ItemFlyBase itemObjPrefab;
    [SerializeField] ParticleSystem fx;
    [SerializeField] SortingGroup fxSorting;
    private List<ItemFlyBase> listIcon;
    private RectTransform rectTransform;
    RectTransform RectTF
    {
        get
        {
            if(rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }
    private void Awake()
    {
        itemObjPrefab.gameObject.SetActive(false);
    }
    private List<ItemFlyBase> CreateList(int amount)
    {
        List<ItemFlyBase> list = new List<ItemFlyBase>();
        for (int i = 0; i < amount; i++)
        {
            var item = GetItem();
            item.gameObject.SetActive(true);
            list.Add(item);
        }
        return list;
    }
    private ItemFlyBase GetItem()
    {
        if (listIcon == null)
        {
            listIcon = new List<ItemFlyBase>();
        }
        ItemFlyBase item = null;
        for(int i = 0; i < listIcon.Count; i++)
        {
            if (!listIcon[i].gameObject.activeInHierarchy)
            {
                item = listIcon[i];
                break;
            }
        }
        if (!item)
        {
            item = Instantiate(itemObjPrefab, transform);
            listIcon.Add(item);
        }
        return item;
    }
    public Tween Animate(Vector2 scrStartPoint,RectTransform target, int amount, Action<int, int> complete)
    {
        if (target == null)
        {
            complete?.Invoke(0, amount);
            return DOTween.Sequence();
        }
        var list = CreateList(amount);
        var cacheList = new List<ItemFlyBase>(list);
        
        Camera worldCamera = (RewardReceivedHub.Instance != null && RewardReceivedHub.Instance.Canvas != null) ? RewardReceivedHub.Instance.Canvas.worldCamera : null;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTF, scrStartPoint, worldCamera, out Vector2 localPoint);
        float baseRadius = 60f;
        float angleStep = 360f / amount;
        var canvas = target.GetComponentInParent<Canvas>();
        int sortingOrder = 200;
        if (canvas != null)
        {
            sortingOrder = canvas.sortingOrder+20;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            list[i].Initialize();
            list[i].SetUpLayer(sortingOrder);

            float angle = i * angleStep + Random.Range(-15f, 15f);
            float radius = baseRadius + Random.Range(-20f, 20f); 

            Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            list[i].RectTF.anchoredPosition = localPoint + offset;
        }
        list.Shuffle();
        Sequence sequence = DOTween.Sequence().SetId(this);
        Vector3 targetPosition = target.position;
        float firstItemDist = (list.Count > 0 && list[0] != null) ? Vector3.Distance(list[0].transform.position, targetPosition) : 0f;
        float duration = Mathf.Min(firstItemDist / 15f, 1.1f);
        for (int i = 0; i < list.Count; i++)
        {
            int index = i;
            if (list[index] == null) continue;
            
            sequence.Insert(i * .1f, list[index].AnimAppear());
            sequence.Insert(i * .1f+.35f, list[index].MoveTo(targetPosition, duration).OnComplete(()=> {
                if (list[index] == null) return;
                list[index].gameObject.SetActive(false);
                cacheList.Remove(list[index]);
                if (fx != null) fx.transform.position = targetPosition;
                if (fxSorting != null) fxSorting.sortingOrder = list[index].layerSort;
                if (fx != null) fx.Play();
                AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_Item_Jump_End_1,1);
                mygame.sdk.GameHelper.Instance.Vibrate(mygame.sdk.Type_vibreate.Vib_Medium);
                complete?.Invoke(index, list.Count);
            }));
        }
        sequence.OnUpdate(() =>
        {
            if(target == null || !target.gameObject.activeInHierarchy)
            {
                sequence.Kill();
            }
        });
        sequence.OnKill(() =>
        {
            for (int i = 0; i < cacheList.Count; i++)
            {
                if (cacheList[i] != null && cacheList[i].gameObject != null && cacheList[i].gameObject.activeInHierarchy)
                {
                    cacheList[i].gameObject.SetActive(false);
                }
            }
        });
        return sequence;
    }
   
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
