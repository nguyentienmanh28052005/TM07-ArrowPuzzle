using DG.Tweening;
using UnityEngine;

public abstract class PoolItemScroll : MonoBehaviour
{
    protected ScrollPool scrollPool;
    public int Index;/*{ get; private set; }*/
    public void SetScrollPool(ScrollPool scrollPool)
    {
        this.scrollPool = scrollPool;
    }
    public void SetIndex(int index)
    {
        this.Index = index;
    }
    public virtual void AnimateRemove(System.Action onComplete)
    { 
        transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
        {
            onComplete?.Invoke();
        }).SetId(this);
    }
    private void OnDisable()
    {
        this.DOKill();
    }
}
