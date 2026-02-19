using System;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 사용 가능한 오브젝트들을 모아두는 풀
    private Dictionary<string, List<GameObject>> availablePools = new Dictionary<string, List<GameObject>>();
    // 현재 사용중인 오브젝트들을 모아두는 풀
    private Dictionary<string, List<GameObject>> inUsePools = new Dictionary<string, List<GameObject>>();
    // 풀을 정리하는 Dictionary
    private Dictionary<string, GameObject> poolContainers = new Dictionary<string, GameObject>();

    ResourceManager _resourceManager;

    public void Initialize(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    #region 오브젝트풀 임시 생성
    /// <summary>
    /// 풀에서 오브젝트 가져오기
    /// </summary>
    public GameObject GetFromPool(PoolableObject poolable, Vector3 position, Quaternion rotation, Transform parentTransform = null, object data = null)
    {
        GameObject prefabObject = poolable.gameObject;
        if (!availablePools.ContainsKey(prefabObject.name))
        {
            availablePools.Add(prefabObject.name, new List<GameObject>());
            if (!inUsePools.ContainsKey(prefabObject.name))
            {
                inUsePools.Add(prefabObject.name, new List<GameObject>());
            }
        }

        if (availablePools[prefabObject.name].Count == 0)
        {
            GameObject newObj = CreateNewObject(prefabObject, position, rotation, parentTransform);

            if (poolable is IInitializePoolable)
            {
                IInitializePoolable initializePoolable = newObj.GetComponent<PoolableObject>() as IInitializePoolable;
                initializePoolable.Initialize(data);
            }

            return newObj;
        }

        GameObject obj = availablePools[prefabObject.name][availablePools[prefabObject.name].Count - 1];
        availablePools[prefabObject.name].RemoveAt(availablePools[prefabObject.name].Count - 1);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        if (parentTransform != null)
        {
            obj.transform.SetParent(parentTransform);
        }

        if (poolable is IInitializePoolable)
        {
            IInitializePoolable initializePoolable = obj.GetComponent<PoolableObject>() as IInitializePoolable;
            initializePoolable.Initialize(data);
        }

        obj.SetActive(true);
        inUsePools[prefabObject.name].Add(obj);

        return obj;
    }

    /// <summary>
    /// 새 오버로드: prefabPath로 프리팹을 로드해서 풀에서 가져옵니다.
    /// - ResourceManager가 Initialize로 설정되어 있으면 LoadResource를 사용하고,
    ///   그렇지 않으면 Resources.Load로 폴백합니다.
    /// - prefabPath는 Resources 폴더 기준 경로(확장자 제외) 또는 ResourceManager에서 기대하는 경로입니다.
    /// </summary>
    public GameObject GetFromPool(string prefabPath, Vector3 position, Quaternion rotation, Transform parentTransform = null, object data = null)
    {
        // 1) 프리팹 로드 시도 (ResourceManager 우선)
        GameObject prefab = null;
        if (_resourceManager != null)
        {
            try
            {
                prefab = _resourceManager.LoadResource<GameObject>(prefabPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ResourceManager.LoadResource 실패: {ex.Message}");
            }
        }

        // 2) ResourceManager가 없거나 로드 실패 시 Resources.Load로 폴백
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>(prefabPath);
        }

        if (prefab == null)
        {
            Debug.LogError($"GetFromPool: 프리팹을 찾을 수 없습니다. 경로: {prefabPath}");
            return null;
        }

        // 3) 기존 로직과 동일하게 pool key는 prefab.name 사용
        string key = prefab.name;

        if (!availablePools.ContainsKey(key))
        {
            availablePools.Add(key, new List<GameObject>());
            if (!inUsePools.ContainsKey(key))
            {
                inUsePools.Add(key, new List<GameObject>());
            }
        }

        if (availablePools[key].Count == 0)
        {
            GameObject newObj = CreateNewObject(prefab, position, rotation, parentTransform);

            if (newObj != null && newObj.TryGetComponent<PoolableObject>(out var poolableComp))
            {
                if (poolableComp is IInitializePoolable initializePoolable)
                {
                    initializePoolable.Initialize(data);
                }
            }

            return newObj;
        }

        GameObject obj = availablePools[key][availablePools[key].Count - 1];
        availablePools[key].RemoveAt(availablePools[key].Count - 1);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        if (parentTransform != null)
        {
            obj.transform.SetParent(parentTransform);
        }

        if (obj.TryGetComponent<PoolableObject>(out var existingPoolable2))
        {
            if (existingPoolable2 is IInitializePoolable initializePoolable2)
            {
                initializePoolable2.Initialize(data);
            }
        }

        obj.SetActive(true);
        inUsePools[key].Add(obj);

        return obj;
    }

    /// <summary>
    /// 풀이 비어있을 때 새로운 오브젝트 생성
    /// </summary>
    private GameObject CreateNewObject(GameObject prefabObject, Vector3 position, Quaternion rotation, Transform parentTransform = null)
    {
        GameObject newObj;
        // 새 객체 생성
        if (parentTransform == null)
        {
            newObj = Instantiate(prefabObject);
        }
        else
        {
            newObj = Instantiate(prefabObject, parentTransform);
        }

        newObj.name = prefabObject.name;

        // 객체 저장을 위한 부모 저장용 오브젝트풀 자식 객체
        if (!poolContainers.ContainsKey(prefabObject.name))
        {
            GameObject container = new GameObject($"Pool_{prefabObject.name}");
            container.transform.SetParent(transform);
            poolContainers[prefabObject.name] = container;
        }

        newObj.transform.position = position;
        newObj.transform.rotation = rotation;

        newObj.SetActive(true);
        inUsePools[prefabObject.name].Add(newObj);
        return newObj;
    }

    /// <summary>
    /// 인터페이스를 이용한 오브젝트풀 해제
    /// </summary>
    /// <param name="poolable"></param>
    public void ReleaseToPoolByInterface(PoolableObject poolable)
    {
        GameObject obj = poolable.gameObject;

        if (poolable is IReleasePoolable)
        {
            IReleasePoolable releaseObject = obj.GetComponent<PoolableObject>() as IReleasePoolable;
            releaseObject.ReleaseObjectPool();
        }

        if (!inUsePools.ContainsKey(obj.name))
        {
            Destroy(obj);
            return;
        }

        int index = inUsePools[obj.name].IndexOf(obj);
        if (index >= 0)
        {
            inUsePools[obj.name].RemoveAt(index);
            availablePools[obj.name].Add(obj);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainers[obj.name].transform);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
        }
        else
        {
        }
    }

    #endregion
    /// <summary>
    /// 풀에 오브젝트 반환 (사용 완료)
    /// </summary>
    public void ReleaseToPool(GameObject obj)
    {
        if (!inUsePools.ContainsKey(obj.name))
        {
            Destroy(obj);
            return;
        }

        int index = inUsePools[obj.name].IndexOf(obj);
        if (index >= 0)
        {
            inUsePools[obj.name].RemoveAt(index);
            availablePools[obj.name].Add(obj);
            obj.SetActive(false);
            obj.transform.SetParent(poolContainers[obj.name].transform);
        }
        else
        {
        }
    }

    /// <summary>
    /// 특정 풀 상태 확인
    /// </summary>
    public (int available, int inUse, int total) GetPoolStatus(string poolName)
    {
        if (!availablePools.ContainsKey(poolName))
            return (0, 0, 0);

        int available = availablePools[poolName].Count;
        int inUse = inUsePools[poolName].Count;
        return (available, inUse, available + inUse);
    }

    /// <summary>
    /// 모든 풀 해제
    /// </summary>
    public void ReleaseAllPools()
    {
        foreach (var poolName in inUsePools.Keys)
        {
            var inUseList = inUsePools[poolName];
            for (int i = inUseList.Count - 1; i >= 0; i--)
            {
                GameObject obj = inUseList[i];
                ReleaseToPool(obj);
            }
        }
    }


    /// <summary>
    /// 모든 풀 초기화
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in availablePools.Values)
        {
            foreach (GameObject obj in pool)
            {
                Destroy(obj);
            }
        }
        foreach (var pool in inUsePools.Values)
        {
            foreach (GameObject obj in pool)
            {
                Destroy(obj);
            }
        }
        availablePools.Clear();
        inUsePools.Clear();

        foreach (var container in poolContainers.Values)
        {
            Destroy(container);
        }
        poolContainers.Clear();
    }
}