using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [System.Serializable]
    public class LoopScrollPrefabSource
    {
        public GameObject item;
        private Stack<Transform> pool = new Stack<Transform>();
        private LoopScrollRectBase ls;


        private LoopScrollPrefabSource()
        {
        }

        public LoopScrollPrefabSource(GameObject trans, LoopScrollRectBase scrollRect)
        {
            item = trans;
            ls = scrollRect;
        }

        public GameObject GetObject()
        {
            if (pool.Count == 0)
            {
                return Object.Instantiate(item, ls.content);
            }

            Transform candidate = pool.Pop();
            candidate.gameObject.SetActive(true);
            return candidate.gameObject;
        }

        public void ReturnObject(Transform trans)
        {
            trans.gameObject.SetActive(false);
            trans.SetParent(ls.transform);
            pool.Push(trans);
        }
    }
}