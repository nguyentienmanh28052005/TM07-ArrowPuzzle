using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardUIGroup : MonoBehaviour
{
    [SerializeField] RewardUI rewardUIPrefab;

    private void Awake()
    {
        rewardUIPrefab.gameObject.SetActive(false);
    }
    public void Initialize(DataResource[] dataResources)
    {
        StartCoroutine(IEShowVisual());
        IEnumerator IEShowVisual()
        {

            for(int i = 0; i < dataResources.Length; i++)
            {
                RewardUI rewardUI = Instantiate(rewardUIPrefab, transform);
                rewardUI.Initialize(dataResources[i]);
                rewardUI.Fly();
                rewardUI.gameObject.SetActive(true);
                int mutil = i % 2 == 0 ?-1:1;
                if (dataResources.Length == 1)
                {
                    mutil = 0;
                }
                rewardUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(mutil * Random.Range(50, 100), 0);
                yield return new WaitForSeconds(.3f);
            }
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }
    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
