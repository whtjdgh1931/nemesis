using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField] MonsterSpawner _monsterSpawner;    // 몬스터 소환기

    public event Action OnAllMonsterDefeated;

    public MonsterSpawner MonsterSpawner => _monsterSpawner;

    public void Initialize()
    {
        _monsterSpawner.OnAllWavesCompleted += AllMonsterDefeated;
    }

    /// <summary>
    /// 일반 잡몹 소환
    /// </summary>
    public void SpawnMonster(int roomCost, Transform[] monsterSpawnPoints)
    {
        List<Transform> tempList = new List<Transform>(monsterSpawnPoints);
        _monsterSpawner.InitializeAndSpawn(roomCost, tempList);
    }

    public void SpawnElite(int currentRoomCount, Transform[] monsterSpawnPoints)
    {
        List<Transform> tempList = new List<Transform>(monsterSpawnPoints);
        _monsterSpawner.EliteSpawner(tempList, currentRoomCount);
    }

    public void SpawnBoss(Transform[] monsterSpawnPoints)
    {
        List<Transform> tempList = new List<Transform>(monsterSpawnPoints);
        _monsterSpawner.BossSpawner(tempList);
    }

    public void AllMonsterDefeated()
    {
        OnAllMonsterDefeated?.Invoke();
    }
}