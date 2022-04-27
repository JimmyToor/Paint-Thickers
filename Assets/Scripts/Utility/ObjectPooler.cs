using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectPool
{
    public List<GameObject> pool = new List<GameObject>();
    public GameObject objectToPool;
    public int poolSize;
}

public class ObjectPooler : Singleton<ObjectPooler>
{
    public List<ObjectPool> objectPools = new List<ObjectPool>();
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject newObject;
        foreach (var obj in objectPools)
        {
            for (int i = 0; i < obj.poolSize; i++)
            {
                newObject = Instantiate(obj.objectToPool, transform, true);
                newObject.SetActive(false);
                obj.pool.Add(newObject);
            }
        }
    }

    private ObjectPool GetObjectPool(string objTag)
    {
        foreach (var currPool in objectPools)
        {
            if (currPool.objectToPool.CompareTag(objTag))
            {
                return currPool;
            }
        }
        Debug.LogFormat("No Object Pool found for object with tag {0}", objTag);
        return null;
    }
    
    public GameObject GetObjectFromPool(String objTag)
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
