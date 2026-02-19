using System.Collections.Generic;
using UnityEngine;

public class ShopRoom : Room
{
    Dictionary<ShopItemType, string> _shopItemPathMap = new()
    {
        { ShopItemType.MutantPack , "Prefabs/ShopItems/MutantPack_Shop" },
        {ShopItemType.HealPack , "Prefabs/ShopItems/HealPack_Shop" },
        {ShopItemType.TechSelectPack , "Prefabs/ShopItems/TechSelectPack_Shop" },
        {ShopItemType.TechUpgradePack , "Prefabs/ShopItems/TechUpgradePack_Shop" },
    };

    Dictionary<TechSelectPackType, string> _techSelectPackPathMap = new()
    {
        {TechSelectPackType.Company1, "Prefabs/ShopItems/TechSelectPack_Company1_Shop" },
        {TechSelectPackType.Company2, "Prefabs/ShopItems/TechSelectPack_Company2_Shop" },
        {TechSelectPackType.Company3, "Prefabs/ShopItems/TechSelectPack_Company3_Shop" },
        {TechSelectPackType.Company4, "Prefabs/ShopItems/TechSelectPack_Company4_Shop" },
        {TechSelectPackType.Company5, "Prefabs/ShopItems/TechSelectPack_Company5_Shop" },
    };

    public override IInteractable[] SpawnReward()
    {
        int count = 4;
        var types = DecideShopItemsAllowDuplicates(count);
        var spawned = new List<IInteractable>();

        // _spawnPoints 배열이 유효하면 그것을 사용, 아니면 기존 가운데 정렬된 spacing 방식으로 폴백
        bool useSpawnPoints = _rewardSpawnPoints != null && _rewardSpawnPoints.Length >= count;

        float spacing = 1.8f;
        float startX = -spacing * (count - 1) / 2f;

        for (int i = 0; i < types.Length; i++)
        {
            ShopItemType type = types[i];
            string prefabPath = null;
            object initData = null;

            if (type == ShopItemType.TechSelectPack)
            {
                var keys = new List<TechSelectPackType>(_techSelectPackPathMap.Keys);
                if (keys.Count == 0)
                {
                    Debug.LogError("ShopRoom: _techSelectPackPathMap이 비어있습니다.");
                    continue;
                }
                var chosenSubtype = keys[Random.Range(0, keys.Count)];
                prefabPath = _techSelectPackPathMap[chosenSubtype];
                initData = chosenSubtype; // 프리팹 내부 초기화에 서브타입 정보 전달
            }
            else
            {
                if (!_shopItemPathMap.TryGetValue(type, out prefabPath))
                {
                    Debug.LogError($"ShopRoom: _shopItemPathMap에 경로가 없습니다. Type={type}");
                    continue;
                }
            }

            Vector3 spawnPos;
            Quaternion spawnRot = Quaternion.identity;
            Transform parentTransform = null;

            if (useSpawnPoints)
            {
                var sp = _rewardSpawnPoints[i];
                if (sp == null)
                {
                    // 안전장치: 해당 인덱스가 null이면 폴백 위치 사용
                    spawnPos = transform.position + new Vector3(startX + i * spacing, 0f, 0f);
                    spawnRot = Quaternion.identity;
                    parentTransform = transform;
                }
                else
                {
                    spawnPos = sp.position;
                    spawnRot = sp.rotation;
                    parentTransform = sp;
                }
            }
            else
            {
                spawnPos = transform.position + new Vector3(startX + i * spacing, 0f, 0f);
                spawnRot = Quaternion.identity;
                parentTransform = transform;
            }

            // 사용자가 제공한 GetFromPool(prefabPath, position, rotation, parent, data) 사용
            GameObject obj = GameManager.Instance.PoolManager.GetFromPool(prefabPath, spawnPos, spawnRot, parentTransform, initData);
            if (obj == null)
            {
                Debug.LogError($"ShopRoom: 프리팹 소환 실패. 경로={prefabPath}");
                continue;
            }

            var interact = obj.GetComponent<IInteractable>();
            if (interact == null)
            {
                Debug.LogWarning($"ShopRoom: 소환된 오브젝트에 IInteractable 없음. prefabPath={prefabPath}, name={obj.name}");
                // 필요시 obj를 다시 풀에 반환하거나 비활성화 처리할 수 있음
                continue;
            }
            RewardInteractableObject rewardInteractableObject = interact as RewardInteractableObject;
            rewardInteractableObject.Initialize();

            spawned.Add(interact);
        }

        RewardSelectionFinished();
        return spawned.ToArray();
    }

    ShopItemType[] DecideShopItemsAllowDuplicates(int count = 4)
    {
        var selected = new List<ShopItemType>
    {
        ShopItemType.HealPack,
        ShopItemType.TechSelectPack
    };

        ShopItemType[] possibleTypes = new ShopItemType[]
        {
        ShopItemType.TechUpgradePack,
        ShopItemType.MutantPack,
        ShopItemType.TechSelectPack, // 중복을 허용하려면 포함
                                     // 필요하면 HealPack도 포함 가능
        };

        while (selected.Count < count)
        {
            var randomType = possibleTypes[Random.Range(0, possibleTypes.Length)];
            selected.Add(randomType);
        }

        return selected.ToArray();
    }
}
