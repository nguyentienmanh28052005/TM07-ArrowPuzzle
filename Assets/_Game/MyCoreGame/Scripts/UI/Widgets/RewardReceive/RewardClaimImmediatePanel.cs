using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardClaimImmediatePanel : MonoBehaviour
{
    [SerializeField] RowRewardVisualClaim rowRewardPrefab;
    [SerializeField] RectTransform holderRow;

    public List<RowRewardVisualClaim> listRow = new List<RowRewardVisualClaim>();
    Action cbOnRewarded;
    Action cbOnHide;
    private void Awake()
    {
        rowRewardPrefab.gameObject.SetActive(false);

    }
    public void Initialize(DataResource[] dataResources, Action cbOnRewarded=null,Action cbOnHide = null)
    {
        this.cbOnRewarded = cbOnRewarded;
        this.cbOnHide = cbOnHide;

        for (int i = 0; i < listRow.Count; i++)
        {
            listRow[i].gameObject.SetActive(false);
        }
        List<DataResource> resources = new List<DataResource>(dataResources);
        List<DataResource[]> listArrayDataResources = new List<DataResource[]>();
        if (dataResources.Length <= 4)
        {
            for (int i = 0; i < dataResources.Length; i += 2)
            {
                int count = Mathf.Min(2, resources.Count - i);
                DataResource[] datas = resources.GetRange(i, count).ToArray();
                listArrayDataResources.Add(datas);
            }
        }
        else
        {
            for (int i = 0; i < dataResources.Length; i += 3)
            {
                int count = Mathf.Min(3, resources.Count - i);
                DataResource[] datas = resources.GetRange(i, count).ToArray();
                listArrayDataResources.Add(datas);
            }
        }

        for (int i = 0; i < listArrayDataResources.Count; i++)
        {
            RowRewardVisualClaim row;
            if (i >= listRow.Count)
            {
                row = Instantiate(rowRewardPrefab, holderRow);
                listRow.Add(row);
            }
            else
            {
                row = listRow[i];
            }
            row.gameObject.SetActive(true);
            row.Initialize(listArrayDataResources[i]);
        }
        List<RewardVisualClaim> listReward = GetListRewardVisual();
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < listReward.Count; i++)
        {
            int index = i;
            sequence.Insert(0, listReward[index].AnimAppearPunch().OnStart(()=>listReward[index].PlayFX()));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(holderRow);
        sequence.AppendInterval(.35f);
        sequence.AppendCallback(Claim);
        sequence.Play();
    }
    public List<RewardVisualClaim> GetListRewardVisual()
    {
        List<RewardVisualClaim> listReward = new List<RewardVisualClaim>();
        for (int i = 0; i < listRow.Count; i++)
        {
            if (!listRow[i].gameObject.activeSelf)
            {
                break;
            }
            listReward.AddRange(listRow[i].GetActiveVisual());
        }
        return listReward;
    }
    void Claim()
    {
        cbOnRewarded?.Invoke();
        List<RewardVisualClaim> listReward = GetListRewardVisual();
        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < listRow.Count; i++)
        {
            if (!listRow[i].gameObject.activeSelf)
            {
                break;
            }
            listRow[i].EnableLayouGroup(false);
        }
        for (int i = 0; i < listReward.Count; i++)
        {
            int index = i;
            sequence.Insert(i * .12f, listReward[index].FindTarget());

        }
        sequence.OnComplete(() =>
        {
            Hide();
            cbOnHide?.Invoke();
        });
        sequence.Play();
    }

    public void Show()
    {
        gameObject.SetActive(true);

    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
