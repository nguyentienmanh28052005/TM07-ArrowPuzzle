using System.Collections.Generic;
using UnityEngine;

public sealed class LevelEditorObjectPool
{
    private readonly Dictionary<GameObject, Queue<GameObject>> objectPool = new Dictionary<GameObject, Queue<GameObject>>();

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null) return null;

        if (!objectPool.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            objectPool[prefab] = queue;
        }

        GameObject obj = null;
        while (queue.Count > 0)
        {
            obj = queue.Dequeue();
            if (obj != null)
            {
                break;
            }
        }

        if (obj == null)
        {
            obj = UnityEngine.Object.Instantiate(prefab);
            PooledObject pooled = obj.AddComponent<PooledObject>();
            pooled.prefabReference = prefab;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(parent, false);
        obj.SetActive(true);

        // Re-enable components that might have been disabled during preview
        MonoBehaviour[] scripts = obj.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour script in scripts)
        {
            if (script == null) continue;
            if (script is IGridOccupant || script is IPreviewDisableable)
            {
                script.enabled = true;
            }
        }

        return obj;
    }

    public void Recycle(GameObject obj)
    {
        if (obj == null) return;

        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled != null && pooled.prefabReference != null)
        {
            obj.SetActive(false);
            obj.transform.SetParent(null); // Detach from container
            
            if (!objectPool.TryGetValue(pooled.prefabReference, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                objectPool[pooled.prefabReference] = queue;
            }
            queue.Enqueue(obj);
        }
        else
        {
            UnityEngine.Object.Destroy(obj);
        }
    }
}
