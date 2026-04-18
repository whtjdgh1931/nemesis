using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RoomType -> RoomDataSO 매핑을 보관하고, Instantiate 편의 메서드를 제공한다.
/// </summary>
public class DataManager : MonoBehaviour
{
    [SerializeField] RoomDataSO[] _roomDatasFromInspector; // optional: 에디터에서 직접 할당 가능

    Dictionary<RoomType, RoomDataSO> _roomDataMap = new();
    Dictionary<WeaponType, PlayerWeaponSet> _weaponSetMap = new();
    Dictionary<RewardType, RewardDataSO> _rewardDataMap = new();
    Dictionary<TechSelectPackType, RewardDataSO_TechPack> _techPackDataMap = new();
    Dictionary<string, Sprite> _roomImageMap = new();

    // 딕셔너리는 public Dictionary => private dictionary 형태로 노출하면 안됨.
    // 외부에서 Add/Remove 함수 등으로 딕셔너리를 변경할 수 있기 때문.
    // 따라서 IReadOnlyDictionary 인터페이스로 노출.
    public IReadOnlyDictionary<RoomType, RoomDataSO> RoomDataMap => _roomDataMap;
    public IReadOnlyDictionary<WeaponType, PlayerWeaponSet> WeaponSetMap => _weaponSetMap;
    public IReadOnlyDictionary<RewardType, RewardDataSO> RewardDataMap => _rewardDataMap;
    public IReadOnlyDictionary<TechSelectPackType, RewardDataSO_TechPack> TechPackDataMap => _techPackDataMap;
    public IReadOnlyDictionary<string, Sprite> RoomImageMap => _roomImageMap;

    /// <summary>
    /// ResourceManager.Initialize() 이후 호출.
    /// </summary>
    public void Initialize(ResourceManager resources)
    {
        if (resources == null)
        {
            Debug.LogError("DataManager.Initialize: resources is null");
            return;
        }

        // 우선 ResourceManager에서 로드한 SO 배열을 사용. Inspector에 직접 넣어둔 값이 있다면 합칠 수도 있음.
        var roomDatas = resources.RoomDataSOs ?? _roomDatasFromInspector;
        BuildRoomDataMap(roomDatas);
        BuildWeaponSetMap(resources.PlayerWeaponSets);
        BuildRewardMap(resources.RewardDataSOs);
        BuildRoomImageMap(resources.DoorViewSprites);
    }

    void BuildRoomDataMap(RoomDataSO[] roomDatas)
    {
        _roomDataMap.Clear();
        if (roomDatas == null) return;

        foreach (var rd in roomDatas)
        {
            if (rd == null) continue;

            if (_roomDataMap.ContainsKey(rd.RoomType))
            {
                Debug.LogWarning($"DataManager: 이미 {rd.RoomType}에 RoomData가 등록되어 있습니다. '{rd.name}' 무시");
                continue;
            }

            if (!rd.IsValid(out var reason))
            {
                Debug.LogWarning($"DataManager: RoomData '{rd.name}' 유효성 실패: {reason}. 무시합니다.");
                continue;
            }

            _roomDataMap[rd.RoomType] = rd;
        }

        // (옵션) 누락 타입 경고
        foreach (RoomType rt in System.Enum.GetValues(typeof(RoomType)))
        {
            if (!_roomDataMap.ContainsKey(rt))
                Debug.LogWarning($"DataManager: RoomType '{rt}'에 매핑된 RoomData가 없습니다.");
        }
    }

    void BuildWeaponSetMap(PlayerWeaponSet[] sets)
    {
        _weaponSetMap.Clear();
        if (sets == null) return;

        foreach (var s in sets)
        {
            if (s == null) continue;
            if (_weaponSetMap.ContainsKey(s.WeaponType))
            {
                Debug.LogWarning($"DataManager: 이미 {s.WeaponType} 타입의 무기 세트가 존재합니다.");
                continue;
            }
            _weaponSetMap[s.WeaponType] = s;
        }
    }

    void BuildRewardMap(RewardDataSO[] rewards)
    {
        // 만약 RewardDataSO의 RewardType이 TechSelectPack이면 RewardDataSO_TechPack으로 캐스팅해서 _techPackDataMap에 넣어야 함.
        if (rewards == null)
        {
            Debug.Log("DataManager: 보상 데이터가 없습니다.");
            return;
        }
        _rewardDataMap.Clear();
        _techPackDataMap.Clear();
        foreach (var reward in rewards) {
            if (reward.RewardType == RewardType.TechSelectPack) {
                // TechSelectPack 딕셔너리에 넣기
                RewardDataSO_TechPack rewardDataSO_TechPack = reward as RewardDataSO_TechPack;
                if (rewardDataSO_TechPack != null) {
                    _techPackDataMap[rewardDataSO_TechPack.TechPackType] = rewardDataSO_TechPack;
                } else {
                    Debug.LogWarning($"DataManager: RewardDataSO '{reward.name}'을 RewardDataSO_TechPack으로 캐스팅하는 데 실패했습니다.");
                }
                continue;
            }
            // 일반 RewardDataSO 딕셔너리에 넣기
            if (_rewardDataMap.ContainsKey(reward.RewardType)) {
                Debug.LogWarning($"DataManager: 이미 {reward.RewardType} 타입의 보상 데이터가 존재합니다.");
                continue;
            }
            _rewardDataMap[reward.RewardType] = reward;
        }

    }

    void BuildRoomImageMap(Sprite[] roomImages)
    {
        _roomImageMap.Clear();
        if (roomImages == null) return;
        foreach (var img in roomImages)
        {
            if (img == null) continue;
            if (_roomImageMap.ContainsKey(img.name))
            {
                Debug.LogWarning($"DataManager: 이미 '{img.name}' 이름의 룸 이미지가 존재합니다.");
                continue;
            }
            _roomImageMap[img.name] = img;
        }
    }

    // 조회 API
    public bool TryGetRoomData(RoomType type, out RoomDataSO roomData) => _roomDataMap.TryGetValue(type, out roomData);
    public bool TryGetWeaponSet(WeaponType wt, out PlayerWeaponSet set) => _weaponSetMap.TryGetValue(wt, out set);
}