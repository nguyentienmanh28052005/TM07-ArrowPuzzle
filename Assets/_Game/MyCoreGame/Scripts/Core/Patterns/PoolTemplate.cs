using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolTemplate<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected T prefab;
    [SerializeField] protected Transform holder;
    public T Prefab => prefab;  
    public enum PoolType
    {
        Stack,
        LinkedList
    }

    public PoolType poolType;

    // Collection checks will throw errors if we try to release an item that is already in the pool.
    public bool collectionChecks = true;
    public int maxPoolSize = 10;

    IObjectPool<T> m_Pool;

    public IObjectPool<T> Pool
    {
        get
        {
            if (m_Pool != null) return m_Pool;
            if (poolType == PoolType.Stack)
                m_Pool = new ObjectPool<T>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
            else
                m_Pool = new LinkedPool<T>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
            return m_Pool;
        }
    }
    public void SetHolder(Transform holder)
    {
        this.holder = holder;
    }
    virtual protected T CreatePooledItem()
    {
        var ps = Instantiate(prefab, transform);
        return ps;
    }

    // Called when an item is returned to the pool using Release
    virtual protected void OnReturnedToPool(T system)
    {
        system.gameObject.SetActive(false);
        system.transform.SetParent(transform);
    }

    // Called when an item is taken from the pool using Get
    protected virtual void OnTakeFromPool(T system)
    {
        if (system == null)
        {
            Debug.LogError($"[PoolTemplate<{typeof(T).Name}>] OnTakeFromPool received null on {name}", this);
            return;
        }

        if (system.gameObject == null)
        {
            Debug.LogError($"[PoolTemplate<{typeof(T).Name}>] Pooled instance was destroyed before Get on {name}", this);
            return;
        }

        system.gameObject.SetActive(true);

        if (holder != null)
        {
            system.transform.SetParent(holder);
        }
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(T system)
    {
        Destroy(system.gameObject);
    }
}

