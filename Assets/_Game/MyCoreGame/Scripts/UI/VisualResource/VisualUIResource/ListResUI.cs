using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using mygame.sdk;
using UnityEngine;
public class ListResUI : MonoBehaviour
{
    [SerializeField] private ResUI resPf;
    [SerializeField] private Transform holdRes;
    [SerializeField] private RES_type[] ignoreType;
    [SerializeField] private RES_type[] onlyType;
    [SerializeField] private bool isOffOnNotData = false;
    private List<ResUI> listRes = new List<ResUI>();
    public List<ResUI> ListRes => listRes;
    private List<ResUI> listAnimRes = new List<ResUI>();
    private Sequence sequence;
    private void Start()
    {
        resPf.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        sequence?.Kill();
    }
    public void Init(IList<DataResource> items, bool isAnim = false, bool isPlaySound = false)
    {
        listAnimRes.Clear();
        if (items == null || items.Count == 0)
        {
            for (int i = 0; i < listRes.Count; i++)
            {
                listRes[i].gameObject.SetActive(false);
            }
            if (isOffOnNotData)
            {
                holdRes.gameObject.SetActive(false);
            }
        }
        else
        {
            var countInit = 0;
            if (holdRes == null)
            {
                holdRes = this.transform;
            }
            holdRes.gameObject.SetActive(true);
            for (int i = 0; i < items.Count; i++)
            {
                var idx = i;
                ResUI it = null;
                if (i < listRes.Count)
                {
                    it = listRes[i];
                }
                else
                {
                    it = Instantiate(resPf, holdRes);
                    listRes.Add(it);
                }

                if (ignoreType.Contains(items[idx].resType) || onlyType.Length > 0 && !onlyType.Contains(items[idx].resType))
                {
                    it.gameObject.SetActive(false);
                }
                else
                {
                    countInit++;
                    if (isAnim)
                    {
                        listAnimRes.Add(it);
                    }
                    else
                    {
                        it.gameObject.SetActive(true);
                    }

                    it.Init(items[idx]);
                }
            }
            if (countInit == 0 && isOffOnNotData)
            {
                holdRes.gameObject.SetActive(false);
            }
            if (items.Count < listRes.Count)
            {
                for (int i = items.Count; i < listRes.Count; i++)
                {
                    listRes[i].gameObject.SetActive(false);
                }
            }
            if (listAnimRes.Count > 0)
            {
                sequence?.Kill();
                for (int i = 0; i < listAnimRes.Count; i++)
                {
                    var idx = i;
                    Tween tw = DOVirtual.DelayedCall(0.15f * idx, () =>
                      {
                          listAnimRes[idx].gameObject.SetActive(true);
                          //if (isPlaySound) AudioManager.Instance.PlaySFX(EAudioClip.gachaItemCard);
                      });
                    sequence.Append(tw);
                }
            }
        }
    }
}
