using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility
{
    public class ObjectPooler : Singleton<ObjectPooler>
    {
        [Serializable]
        public class ObjectPool
        {
            public List<GameObject> pool = new List<GameObject>();
            public GameObject objectToPool;
            public int poolSize;
        }
    
        public List<ObjectPool> objectPools = new List<ObjectPool>();
    
        // Start is called before the first frame update
        void Start()
        {
            foreach (var obj in objectPools)
            {
                for (int i = 0; i < obj.poolSize; i++)
                {
                    var newObject = Instantiate(obj.objectToPool, transform, true);
                    newObject.SetActive(false);
                    obj.pool.Add(newObject);
                }
            }
        }

        private ObjectPool GetObjectPool(string objTag)
        {
            foreach (var currPool in objectPools.Where(currPool => currPool.objectToPool.CompareTag(objTag)))
            {
                return currPool;
            }
            Debug.LogFormat("No Object Pool found for object with tag {0}", objTag);
            return null;
        }
    
        public GameObject GetObjectFromPool(string objTag)
        {
            ObjectPool objectPool = GetObjectPool(objTag);
        
            if (objectPool != null)
            {
                foreach (var currObject in objectPool.pool)
                {
                    if (!currObject.activeInHierarchy)
                    {
                        return currObject;
                    }
                }
                Debug.LogFormat("No available objects found in pool for object with tag {0}", objTag);
            }
        
            return null;
        }
    
    }
}
