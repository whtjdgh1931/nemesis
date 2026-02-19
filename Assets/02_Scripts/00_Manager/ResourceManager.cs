using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 최소 책임: Resources 폴더에서 에셋을 로드해서 보관만 한다.
/// - 딕셔너리/매핑은 만들지 않음.
/// - 나중에 Addressables로 교체할 수 있게 구현부만 수정하면 됨.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    [Header("Editor Assignment (옵션)")]
    [SerializeField] GameObject _doorPrefab;

    [Header("Runtime Loaded (Resources 폴더에서 로드)")]
    public GameObject DoorPrefab { get; private set; }

    public PlayerWeaponSet[] PlayerWeaponSets { get; private set; }
    public RoomDataSO[] RoomDataSOs { get; private set; }
    public RewardDataSO[] RewardDataSOs { get; private set; }
    public RewardDataSO_TechPack[] RewardDataSO_TechPacks { get; private set; }
    public AudioClip[] Bgm { get; private set; }
    public AudioClip[] Sfx { get; private set; }
    public Sprite[] DoorViewSprites { get; private set; }

    // 호출: 게임 시작 시 한 번만
    public void Initialize()
    {
        DoorPrefab = _doorPrefab != null
            ? _doorPrefab
            : Resources.Load<GameObject>(Constants.RESOURCES_PATH_DOOR_PREFAB);

        PlayerWeaponSets = Resources.LoadAll<PlayerWeaponSet>(Constants.RESOURCES_PATH_PLAYER_WEAPONSET);
        RoomDataSOs = Resources.LoadAll<RoomDataSO>(Constants.RESOURCES_PATH_ROOMDATASO);
        RewardDataSOs = Resources.LoadAll<RewardDataSO>(Constants.RESOURCES_PATH_REWARD_DATA_SO);
        Bgm = Resources.LoadAll<AudioClip>(Constants.RESOURCES_PATH_BGM);
        Sfx = Resources.LoadAll<AudioClip>(Constants.RESOURCES_PATH_SFX);
        DoorViewSprites = Resources.LoadAll<Sprite>("RoomImage");

        // (선택) 간단한 검증 로그
        if (DoorPrefab == null)
            Debug.LogWarning("ResourceManager: DoorPrefab이 비어있습니다.");
    }

    Dictionary<Type, Dictionary<string, UnityEngine.Object>> _resourceCache = new();
    /// <summary>
    /// 리소스를 로드합니다. 이미 캐시에 있다면 캐시에서 반환합니다.
    /// </summary>
    /// <typeparam name="T">로드할 리소스의 타입</typeparam>
    /// <param name="path">Resources 폴더 내 리소스의 경로</param>
    /// <returns>로드된 리소스 객체</returns>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        // 타입별 캐시 확인
        if (_resourceCache.TryGetValue(typeof(T), out var cache) == false)
        {
            cache = new Dictionary<string, UnityEngine.Object>();
            _resourceCache[typeof(T)] = cache;
        }

        // 경로(폴더 경로 포함한 이름)에 따른 캐시 확인
        if (cache.ContainsKey(path) == true)
        {
            return cache[path] as T;
        }

        // 캐시에 없으면 Resources.Load로 로드
        T resource = Resources.Load<T>(path);
        if (resource == null)
        {
            Debug.LogError($"{path} 경로의 리소스를 Resources 폴더에서 찾을 수 없습니다.");
        }
        else
        {
            cache[path] = resource;
        }
        return resource;

        //    // 딕셔너리에서 먼저 Type으로 필터링하고 필터링했을 때 그 Type이 있으면
        //    // 내부 딕셔너리에서 또 path를 Key로써 필터링해서 해당하는 Value가 있는지 확인한다.
        //    // 만약 있다면 해당 Value를 resource변수로 out한다.
        //    if (_resourceCache.ContainsKey(typeof(T)) && _resourceCache[typeof(T)].TryGetValue(path, out var resource))
        //    {
        //        // resource가 T 타입인지 확인하고
        //        if (resource is T cachedResource)
        //        {
        //            // 맞다면 반환
        //            return cachedResource;
        //        }
        //        else
        //        {
        //            // 아니라면 로그띄우기
        //            Debug.LogWarning($"Resource at path '{path}' is not of type {typeof(T)}.");
        //            return null;
        //        }

        //        // 처음에 이렇게하면 될줄알았는데 위가 더 안전한 방법이라는듯?
        //        //return resource as T;
        //    }

        //    // 리소스가 캐시에 없으면 Resources.Load를 사용하여 로드
        //    T loadedResource = Resources.Load<T>(path);

        //    // 만약 로드한 리소스가 null이라면 
        //    if (loadedResource == null)
        //    {
        //        // 로드 실패 로그 출력
        //        Debug.Log("Resource를 로드할 수 없습니다.");
        //        // null 반환
        //        return null;
        //    }

        //    // 만약에 내가 로드한 타입이 딕셔너리에 없었다면
        //    // 즉 처음 로드하는 타입일 경우
        //    if (!_resourceCache.ContainsKey(typeof(T)))
        //    {
        //        // 해당 타입으로 딕셔너리를 하나 만든다.
        //        _resourceCache[typeof(T)] = new Dictionary<string, UnityEngine.Object>();
        //    }

        //    // 로드한 리소스를 캐시에 저장
        //    _resourceCache[typeof(T)][path] = loadedResource;

        //    // 그리고 방금 저장한 리소스를 반환
        //    return loadedResource;
        //
    }
}