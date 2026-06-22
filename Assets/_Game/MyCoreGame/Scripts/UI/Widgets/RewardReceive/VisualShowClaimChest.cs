using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
#if UNITY_EDITOR
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(VisualShowClaimChest))]
public class VisualShowClaimChestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VisualShowClaimChest script = (VisualShowClaimChest)target;

        if (GUILayout.Button("Fetch All Child Rewards"))
        {
            FetchChildRewards(script);
        }
    }

    private void FetchChildRewards(VisualShowClaimChest script)
    {
        script.rewardVisualHolder = script.GetComponentsInChildren<RectTransform>();
        EditorUtility.SetDirty(script); 
    }
}
#endif
public class VisualShowClaimChest : MonoBehaviour
{
    public RectTransform[] rewardVisualHolder;
    private RewardVisualClaim rewardVisualPrefab;
    public RewardVisualClaim[] rewardVisual;
    Vector2[] initializeAnchoredPoses;
    public int numReward => rewardVisualHolder.Length;
    public bool isWaiting;
    public void Initialize(RewardVisualClaim rewardVisualClaim)
    {
        initializeAnchoredPoses = new Vector2[numReward];
        for(int i = 0; i < initializeAnchoredPoses.Length; i++)
        {
            initializeAnchoredPoses[i] = rewardVisualHolder[i].anchoredPosition;
        }
        this.rewardVisualPrefab = rewardVisualClaim;
        rewardVisual = new RewardVisualClaim[rewardVisualHolder.Length];
    }
    public void Hide()
    {
        for (int i = 0; i < rewardVisualHolder.Length; i++)
        {
            if (rewardVisual[i] != null)
            {
                Destroy(rewardVisual[i].gameObject);
            }
            rewardVisualHolder[i].gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void SetResources(DataResource[] dataResources)
    {
        for (int i = 0; i < rewardVisual.Length; i++)
        {
            if(rewardVisual[i] == null)
            {
                rewardVisual[i] = Instantiate(rewardVisualPrefab,transform);
            }
            rewardVisual[i].Initialize(dataResources[i]);
        }
    }
    public Sequence AnimAppear(Vector3 startPos)
    {
        for(int i = 0; i < rewardVisual.Length; i++)
        {
            rewardVisual[i].transform.localScale = Vector3.zero;
            rewardVisual[i].transform.position = startPos;
            rewardVisual[i].gameObject.SetActive(true);
            rewardVisual[i].ActiveFXBg(false);
        }
        Sequence sequence = DOTween.Sequence().SetId(this);
        for (int i = 0; i < rewardVisual.Length; i++)
        {
            int index = i;
            Vector2 direction = initializeAnchoredPoses[index] - rewardVisual[index].RectTF.anchoredPosition;
            direction = direction.normalized;
            sequence.InsertCallback(index * .06f + 0.2f, () =>
            {
                AudioManager.Instance.PlayOneShot(AUDIO_CLIP_NAME.SFX_UI_Button_Click_Pop, 1);
            });
            sequence.Insert(i * .06f, rewardVisual[index].RectTF.DOAnchorPos(initializeAnchoredPoses[index] + direction*50, .4f));
            Sequence sequence2 = DOTween.Sequence();
            sequence2.Append(rewardVisual[index].RectTF.DOAnchorPos(initializeAnchoredPoses[index], .1f));
            sequence2.AppendCallback(rewardVisual[index].AnimTopDown);
            sequence.Insert(index * .06f+0.4f, sequence2);
            sequence.Insert(index * .06f, rewardVisual[index].RectTF.DOScale(1, .3f));
            
        }
        //sequence.AppendCallback(() =>
        //{
        //    for (int i = 0; i < rewardVisual.Length; i++)
        //    {
        //        rewardVisual[i].AnimTopDown();

        //    }
        //});
        sequence.Play();
        return sequence;
    }
    public void ReceiveReward(Action cbOnComplete)
    {
        Sequence sequence = DOTween.Sequence().SetId(this);
        for (int i = 0; i < rewardVisual.Length; i++)
        {
            int index = rewardVisual.Length-1-i;
            sequence.Insert(i * .12f,rewardVisual[index].FindTarget(isWaiting));

        }
        sequence.AppendCallback(() =>
        {
            Hide();
            cbOnComplete?.Invoke();
        });
        sequence.Play();
    }
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
