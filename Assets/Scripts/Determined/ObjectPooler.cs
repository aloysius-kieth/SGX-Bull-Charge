using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour 
{
    #region SINGLETON
    public static ObjectPooler Instance { get; set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else Instance = this;
    }
    #endregion

    [System.Serializable]
    public struct ObjectPoolItem
    {
        public GameObject objectToPool;
        public string poolName;
        public int amountToPool;
        public bool Expandable;
    }

    public string rootPoolName = DEFAULTROOTOBJECTPOOLNAME;
    public const string DEFAULTROOTOBJECTPOOLNAME = "Pooled Objects";
   
    public List<ObjectPoolItem> itemsToPool;
    List<GameObject> pooledObjects;

    void Start()
    {
        if (string.IsNullOrEmpty(rootPoolName))
            rootPoolName = DEFAULTROOTOBJECTPOOLNAME;

        GetParentPoolObject(rootPoolName);

        pooledObjects = new List<GameObject>();

        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                CreatePooledObject(item);
            }
        }
    }

    GameObject GetParentPoolObject(string objPoolName)
    {
        // use root object pool name if no name specifed
        if (string.IsNullOrEmpty(objPoolName))
            objPoolName = rootPoolName;

        GameObject parentObj = GameObject.Find(objPoolName);

        // create parent object if null
        if (parentObj == null)
        {
            parentObj = new GameObject();
            parentObj.name = objPoolName;

            // add sub pools to root object pool if needed
            if (objPoolName != rootPoolName)
                parentObj.transform.parent = GameObject.Find(rootPoolName).transform;
        }
        return parentObj;
    }

    /// <summary>
    /// Return the gameobject name that you want to get from
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public GameObject GetPooledObject(string name)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].name == name)
            {
                return pooledObjects[i];
            }
        }
        foreach (ObjectPoolItem item in itemsToPool)
        {
            if (item.objectToPool.name == name)
            {
                if (item.Expandable)
                {
                    return CreatePooledObject(item);
                }
            }

        }
        return null;
    }

    public void ReturnAllToPool()
    {
        foreach (GameObject pooledObj in pooledObjects)
        {
            pooledObj.SetActive(false);
        }
    }

    GameObject CreatePooledObject(ObjectPoolItem item)
    {
        GameObject obj = Instantiate(item.objectToPool) as GameObject;

        obj.name = obj.name.Replace("(Clone)", "").Trim();

        // get parent for this pooled object and assign the new object to it
        GameObject parentPoolObj = GetParentPoolObject(item.poolName);
        obj.transform.parent = parentPoolObj.transform;

        obj.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }
}
