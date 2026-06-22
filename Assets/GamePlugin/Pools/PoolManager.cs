using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyPool
{
    public enum PoolName
    {
        Item = 0
    }
    public enum BoardNodePool
    {
    }
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance;
        public List<SpawnPool> ListPool = new List<SpawnPool>();
        Dictionary<int, SpawnPool> dicPools = new Dictionary<int, SpawnPool>();

        private void Awake()
        {
            Instance = this;
            for (int i = 0; i < ListPool.Count; i++)
            {
                dicPools.Add(i, ListPool[i]);
            }
        }

        public SpawnPool pool(PoolName _pool)
        {
            return dicPools[(int)_pool];
        }

        private void OnDestroy()
        {

        }
    }
}
