using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardClaimPanel : MonoBehaviour
{
    [SerializeField] RowRewardVisualClaim rowRewardPrefab;
    [SerializeField] RectTransform holderRow;
    [SerializeField] Button claimBtn;
    [SerializeField] LayoutGroup layoutGroup;

    public List<RowRewardVisualClaim> listRow = new List<RowRewardVisualClaim>();
    Action cbOnRewarded;
    Action cbOnHide;
    private void Awake()
    {
        rowRewardPrefab.gameObject.SetActive(false);
        claimBtn.onClick.AddListener(ClaimButton);
        
    }
    public void Initialize(DataResource[] dataResources, RectTransform btnClaimFake, Action cbOnRewarded, RectTransform rectHolder, Action cbOnHide = null)
    {
        this.cbOnRewarded = cbOnRewarded;
        this.cbOnHide = cbOnHide;
        claimBtn.gameObject.SetActive(false);
        btnClaimFake.transform.localScale = Vector3.zero;
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
            sequence.Insert(i * .08f, listReward[index].AnimAppear());
            listReward[i].ActiveFXBg(true);
        }
        sequence.OnComplete(() =>
        {
            claimBtn.gameObject.SetActive(true);
            btnClaimFake.gameObject.SetActive(true);
            btnClaimFake.DOScale(1, .5f);
            //cbOnComplete?.Invoke();
        });
        sequence.Play();
        LayoutRebuilder.ForceRebuildLayoutImmediate(holderRow);
        holderRow.anchoredPosition = RewardReceivedHub.PositionInsideContainer(holderRow, rectHolder);
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
    void ClaimButton()
    {
        cbOnRewarded?.Invoke();
        claimBtn.gameObject.SetActive(false);
        AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click);
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
            sequence.Insert(i * .1f, listReward[index].FindTarget());
            listReward[i].ActiveFXBg(false);

        }
        sequence.OnComplete(() =>
        {
            cbOnHide?.Invoke();
            Hide();
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
