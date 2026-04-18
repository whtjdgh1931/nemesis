using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NormalRoom: 풀링을 사용하는 보상 생성 로직 (정리 및 안전성 강화)
/// </summary>
public class NormalRoom : Room
{
    // NormalRoomType -> Pool/Resource 키 매핑 (프로젝트 규칙에 맞춰 값 설정)
    Dictionary<NormalRoomType, string> _normalRewardMap = new Dictionary<NormalRoomType, string>()
    {
        {NormalRoomType.Heal, Constants.RESOURCES_PATH_REWARDS + "/HealPack" },
        {NormalRoomType.Credit, Constants.RESOURCES_PATH_REWARDS + "/Credit" },
        {NormalRoomType.TechUpgrade, Constants.RESOURCES_PATH_REWARDS + "/TechUpgradePack" },
        {NormalRoomType.Chrome, Constants.RESOURCES_PATH_REWARDS + "/Chrome" },
        {NormalRoomType.TechSelect, Constants.RESOURCES_PATH_REWARDS + "/TechSelectPack" },
    };
    Dictionary<TechSelectPackType, string> _techSelectPackMap = new Dictionary<TechSelectPackType, string>()
    {
        {TechSelectPackType.Company1, Constants.RESOURCES_PATH_REWARDS + "/TechSelectPack_Company1" },
        {TechSelectPackType.Company2, Constants.RESOURCES_PATH_REWARDS + "/TechSelectPack_Company2" },
        {TechSelectPackType.Company3, Constants.RESOURCES_PATH_REWARDS + "/TechSelectPack_Company3" },
        {TechSelectPackType.Company4, Constants.RESOURCES_PATH_REWARDS + "/TechSelectPack_Company4" },
        {TechSelectPackType.Company5, Constants.RESOURCES_PATH_REWARDS + "/TechSelectPack_Company5" },
    };

    public override IInteractable[] SpawnReward()
    {
        // 보상 스폰 포인트 유효성 검사
        if (_rewardSpawnPoints == null || _rewardSpawnPoints.Length == 0)
        {
            Debug.LogWarning($"NormalRoom.SpawnReward: reward spawn points are not set. (room={name})");
            return System.Array.Empty<IInteractable>();
        }

        // Normal 타입인지 확인하고 NormalType 값 획득
        if (!_roomInfo.TryGetNormal(out var normalType))
        {
            // Normal 타입이 아니면 보상 없음
            return System.Array.Empty<IInteractable>();
        }

        Vector3 spawnPos = _rewardSpawnPoints[0].position;
        Quaternion spawnRot = Quaternion.identity;
        GameObject instance = null;

        var poolMgr = GameManager.Instance?.PoolManager;
        var resourceMgr = GameManager.Instance?.ResourceManager;

        // TechSelect은 별도 매핑 처리
        if (normalType == NormalRoomType.TechSelect)
        {
            if (!_roomInfo.TryGetTechSelect(out var packType))
            {
                Debug.LogWarning($"NormalRoom.SpawnReward: TechSelectPackType이 설정되지 않았습니다. (room={name})");
                return System.Array.Empty<IInteractable>();
            }

            if (_techSelectPackMap.TryGetValue(packType, out var techKey) && !string.IsNullOrEmpty(techKey))
            {
                if (poolMgr != null)
                {
                    instance = poolMgr.GetFromPool(techKey, spawnPos, spawnRot, transform);
                }

                //// 풀에서 못가져오면 폴백(리소스매니저/Instantiate 등)을 여기서 추가할 수 있음
                //if (instance == null && resourceMgr != null)
                //{
                //    var prefab = resourceMgr.LoadResource<GameObject>(techKey);
                //    if (prefab != null)
                //        instance = Instantiate(prefab, spawnPos, spawnRot, transform);
                //}

                //if (instance == null)
                //{
                //    Debug.LogWarning($"NormalRoom.SpawnReward: Failed to spawn TechSelectPack for key '{techKey}' (room={name})");
                //    return System.Array.Empty<IInteractable>();
                //}
            }
            else
            {
                Debug.LogWarning($"NormalRoom.SpawnReward: No mapping for TechSelectPackType {packType} (room={name})");
                return System.Array.Empty<IInteractable>();
            }
        }
        else
        {
            // 일반 매핑 처리
            if (!_normalRewardMap.TryGetValue(normalType, out var prefabKey) || string.IsNullOrEmpty(prefabKey))
            {
                Debug.LogWarning($"NormalRoom.SpawnReward: No prefab key mapped for NormalRoomType {normalType}. (room={name})");
                return System.Array.Empty<IInteractable>();
            }

            if (poolMgr != null)
            {
                instance = poolMgr.GetFromPool(prefabKey, spawnPos, spawnRot, transform);
            }

            //if (instance == null && resourceMgr != null)
            //{
            //    var prefab = resourceMgr.LoadResource<GameObject>(prefabKey);
            //    if (prefab != null)
            //        instance = Instantiate(prefab, spawnPos, spawnRot, transform);
            //}

            //if (instance == null)
            //{
            //    Debug.LogWarning($"NormalRoom.SpawnReward: Failed to spawn reward for key '{prefabKey}' (room={name})");
            //    return System.Array.Empty<IInteractable>();
            //}
        }

        // Spawn 성공: RewardInteractableObject 찾기 (루트 또는 자식)
        var reward = instance.GetComponent<RewardInteractableObject>()
                     ?? instance.GetComponentInChildren<RewardInteractableObject>(true);

        //if (reward == null)
        //{
        //    Debug.LogError($"NormalRoom.SpawnReward: Spawned instance '{instance.name}' does not contain RewardInteractableObject. Releasing/destroying instance. (room={name})");
        //    if (poolMgr != null)
        //        poolMgr.ReleaseToPool(instance);
        //    else
        //        Destroy(instance);

        //    return System.Array.Empty<IInteractable>();
        //}

        reward.OnRewardGiven += RewardSelectionFinished;
        reward.Initialize();

        // 마지막으로 보상 참조를 배열로 담아 반환 (이 배열이 '새로운 오브젝트 생성'을 의미하는 것은 아님)
        return new IInteractable[] { reward };
    }
}