using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Normal Monster Prefabs"), SerializeField]
    private List<PoolableObject> normalMonsterPrefabs = new List<PoolableObject>(6);

    [Header("Elite Monster Prefabs"), SerializeField]
    private List<PoolableObject> eliteMonsterPrefabs = new List<PoolableObject>(3);

    [Header("Boss Monster Prefabs"), SerializeField]
    private List<PoolableObject> bossMonsterPrefabs = new List<PoolableObject>(1);

    [Header("Spawn Settings")]
    // 방의 최대 스폰 포인트
    private int maxSpawnPoint;

    int consecutiveFailures = 10; // 연속 실패 횟수

    private List<Transform> spawnPositions;
    private const int MAX_WAVE_POINTS = 30;
    private List<GameObject> activeMonsters = new List<GameObject>(); // 현재 필드에 있는 몬스터 리스트
    private List<List<MonsterSpawnInfo>> waitingWaves = new List<List<MonsterSpawnInfo>>(); // 대기 중인 웨이브 리스트
    private int currentWaveIndex = 0; // 현재 웨이브 인덱스
    private bool isWaveActive = false; // 현재 웨이브 진행 중인지 체크

    public List<GameObject> ActiveMonsters => activeMonsters;

    public event Action<MonsterBase> OnMonsterSpawned;
    public event Action OnAllWavesCompleted;

    private class MonsterSpawnInfo
    {
        public PoolableObject prefab;
        public int cost;

        public MonsterSpawnInfo(PoolableObject prefab, int cost)
        {
            this.prefab = prefab;
            this.cost = cost;
        }
    }

    /// <summary>
    /// 설정 후 즉시 스폰 시작
    /// </summary>
    public void InitializeAndSpawn(int maxPoint, List<Transform> positions = null)
    {
        if (maxPoint == 0 || positions == null || positions.Count == 0)
        {
            OnAllWavesCompleted?.Invoke();
            return;
        }
        SpawnerSetting(maxPoint, positions);
        StartSpawn();
    }

    /// <summary>
    /// 외부에서 최대 스폰 포인트와 스폰 위치를 설정하는 메서드
    /// </summary>
    public void SpawnerSetting(int maxPoint, List<Transform> positions)
    {
        maxSpawnPoint = maxPoint;
        spawnPositions = positions;
    }

    public void EliteSpawnerSetting(List<Transform> positions)
    {
        spawnPositions = positions;
    }

    public void EliteSpawner(List<Transform> positions, int roomNumber = 1)
    {
        EliteSpawnerSetting(positions);

        // Elite 스폰은 단일 웨이브로 처리
        isWaveActive = true;
        currentWaveIndex = 1; // Elite는 1개 웨이브로 간주
        waitingWaves.Clear();
        waitingWaves.Add(new List<MonsterSpawnInfo>()); // 빈 웨이브 추가 (카운트 체크용)

        SpawnSolo(eliteMonsterPrefabs, roomNumber);
    }

    public void BossSpawner(List<Transform> positions, int roomNumber = 1)
    {
        // Elite와 동일한 로직
        EliteSpawnerSetting(positions);

        // Boss 스폰은 단일 웨이브로 처리
        isWaveActive = true;
        currentWaveIndex = 1; // Elite는 1개 웨이브로 간주
        waitingWaves.Clear();
        waitingWaves.Add(new List<MonsterSpawnInfo>()); // 빈 웨이브 추가 (카운트 체크용)

        SpawnBoss(bossMonsterPrefabs);
    }

    /// <summary>
    /// 몬스터 스폰 시작
    /// </summary>
    public void StartSpawn()
    {
        if (normalMonsterPrefabs == null)
        {
            return;
        }

        List<MonsterSpawnInfo> monsters = GenerateRandomMonsters();

        List<List<MonsterSpawnInfo>> waves = DivideIntoWaves(monsters);

        waitingWaves.Clear();
        currentWaveIndex = 0;
        waitingWaves = waves;

        StartNextWave();
    }

    /// <summary>
    /// 최대 스폰 포인트를 채울 때까지 랜덤 몬스터 생성
    /// </summary>
    private List<MonsterSpawnInfo> GenerateRandomMonsters()
    {
        List<MonsterSpawnInfo> monsters = new List<MonsterSpawnInfo>();
        int currentTotalCost = 0;

        List<MonsterSpawnInfo> availableMonsters = new List<MonsterSpawnInfo>();
        foreach (var prefab in normalMonsterPrefabs)
        {
            MonsterBase monsterBase = prefab.GetComponent<MonsterBase>();
            if (monsterBase != null)
            {
                availableMonsters.Add(new MonsterSpawnInfo(prefab, monsterBase.GetCost()));
            }
        }

        if (availableMonsters.Count == 0)
        {
            return monsters;
        }

        // 최대 스폰 포인트를 채울 때까지 랜덤 선택
        while (currentTotalCost < maxSpawnPoint)
        {
            MonsterSpawnInfo randomMonster = availableMonsters[UnityEngine.Random.Range(0, availableMonsters.Count)];

            if (currentTotalCost + randomMonster.cost <= maxSpawnPoint)
            {
                monsters.Add(randomMonster);
                currentTotalCost += randomMonster.cost;
                consecutiveFailures = 0; // 성공 시 리셋
            }
            else
            {
                consecutiveFailures++;
                // 10번 연속 실패하면 포기
                if (consecutiveFailures >= 10)
                {
                    break;
                }
            }
        }
        return monsters;
    }

    /// <summary>
    /// 몬스터 리스트를 30포인트 단위의 웨이브로 분할
    /// </summary>
    private List<List<MonsterSpawnInfo>> DivideIntoWaves(List<MonsterSpawnInfo> monsters)
    {
        // 대기 웨이브 리스트
        List<List<MonsterSpawnInfo>> waves = new List<List<MonsterSpawnInfo>>();

        // 현재 웨이브 정보들
        List<MonsterSpawnInfo> currentWave = new List<MonsterSpawnInfo>();
        int currentWavePoint = 0;

        foreach (var monster in monsters)
        {
            // 현재 웨이브에 추가했을 때 30을 초과하면 새 웨이브 시작
            if (currentWavePoint + monster.cost > MAX_WAVE_POINTS && currentWave.Count > 0)
            {
                waves.Add(currentWave);
                currentWave = new List<MonsterSpawnInfo>();
                currentWavePoint = 0;
            }

            currentWave.Add(monster);
            currentWavePoint += monster.cost;
        }

        // 마지막 웨이브 추가
        if (currentWave.Count > 0)
        {
            waves.Add(currentWave);
        }

        return waves;
    }

    /// <summary>
    /// 다음 웨이브 시작
    /// </summary>
    private void StartNextWave()
    {
        // 모든 웨이브 완료시
        if (currentWaveIndex >= waitingWaves.Count)
        {
            OnAllWavesCompleted?.Invoke();
            return;
        }

        List<MonsterSpawnInfo> currentWave = waitingWaves[currentWaveIndex];
        currentWaveIndex++;
        isWaveActive = true;

        SpawnWave(currentWave);
    }

    /// <summary>
    /// 웨이브의 몬스터들을 스폰
    /// </summary>
    private void SpawnWave(List<MonsterSpawnInfo> wave)
    {
        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            return;
        }

        foreach (var monsterInfo in wave)
        {
            // 랜덤 스폰 위치 선택
            Transform spawnPos = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)];

            GameObject spawnedMonster = GameManager.Instance.PoolManager.GetFromPool(monsterInfo.prefab, spawnPos.position, spawnPos.rotation);
            OnMonsterSpawned?.Invoke(spawnedMonster.GetComponent<MonsterBase>());
            MonsterBase monsterbase = spawnedMonster.GetComponent<MonsterBase>();

            activeMonsters.Add(spawnedMonster);
            EventBus.RemoveMonster(monsterbase);
            EventBus.AddMonster(monsterbase);

            if (monsterbase != null)
            {
                monsterbase.OnDieEvent += () => OnMonsterDeath(spawnedMonster);
                monsterbase.OnDieEvent -= () => EventBus.RemoveMonster(monsterbase);
                monsterbase.OnDieEvent += () => EventBus.RemoveMonster(monsterbase);
            }
        }
    }

    /// <summary>
    /// 단일 몬스터 스폰
    /// </summary>
    private void SpawnSolo(List<PoolableObject> monster, int roomNumber)
    {
        if (spawnPositions == null)
        {
            return;
        }

        PoolableObject spawnRandomElite = monster[UnityEngine.Random.Range(0, monster.Count)];
        // 랜덤 스폰 위치 선택
        Transform spawnPos = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)];

        GameObject spawnedMonster = GameManager.Instance.PoolManager.GetFromPool(spawnRandomElite, spawnPos.position, spawnPos.rotation);
        OnMonsterSpawned?.Invoke(spawnedMonster.GetComponent<MonsterBase>());
        MonsterBase monsterbase = spawnedMonster.GetComponent<MonsterBase>();

        activeMonsters.Add(spawnedMonster);
        EventBus.RemoveMonster(monsterbase);
        EventBus.AddMonster(monsterbase);
        EventBus.EliteBoss = monsterbase;

        if (monsterbase != null)
        {
            monsterbase.SetEliteMaxHealth(roomNumber);
            monsterbase.OnDieEvent += () => OnMonsterDeath(spawnedMonster);
            monsterbase.OnDieEvent -= () => EventBus.RemoveMonster(monsterbase);
            monsterbase.OnDieEvent += () => EventBus.RemoveMonster(monsterbase);
        }
    }

    private void SpawnBoss(List<PoolableObject> monster, int roomNumber = 1)
    {
        if (spawnPositions == null)
        {
            return;
        }

        PoolableObject spawnBoss = monster[UnityEngine.Random.Range(0, monster.Count)];
        // 랜덤 스폰 위치 선택
        Transform spawnPos = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Count)];

        GameObject spawnedMonster = GameManager.Instance.PoolManager.GetFromPool(spawnBoss, spawnPos.position, spawnPos.rotation);
        OnMonsterSpawned?.Invoke(spawnedMonster.GetComponent<MonsterBase>());
        MonsterBase monsterbase = spawnedMonster.GetComponent<MonsterBase>();

        activeMonsters.Add(spawnedMonster);
        EventBus.RemoveMonster(monsterbase);
        EventBus.AddMonster(monsterbase);

        if (monsterbase != null)
        {
            monsterbase.SetEliteMaxHealth(roomNumber);
            monsterbase.OnDieEvent += () => OnMonsterDeath(spawnedMonster);
            monsterbase.OnDieEvent -= () => EventBus.RemoveMonster(monsterbase);
            monsterbase.OnDieEvent += () => EventBus.RemoveMonster(monsterbase);
            monsterbase.OnDieEvent -= () => EventBus.BossDead();
            monsterbase.OnDieEvent += () => EventBus.BossDead();
        }
    }

    /// <summary>
    /// 몬스터 사망 시 호출
    /// </summary>
    private void OnMonsterDeath(GameObject monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
        }

        // 모든 몬스터가 죽었는지 체크
        if (activeMonsters.Count == 0 && isWaveActive)
        {
            isWaveActive = false;

            // 현재 웨이브가 마지막 웨이브인지 확인
            if (currentWaveIndex >= waitingWaves.Count)
            {
                // Elite 스폰이거나 마지막 웨이브인 경우 즉시 완료 처리
                OnAllWavesCompleted?.Invoke();
            }
            else
            {
                // 다음 웨이브가 있는 경우 딜레이 후 시작
                StartCoroutine(StartNextWaveWithDelay(2f));
            }
        }
    }

    /// <summary>
    /// 딜레이 후 다음 웨이브 시작
    /// </summary>
    private IEnumerator StartNextWaveWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    private void CheckWaveClear()
    {
        if (currentWaveIndex >= waitingWaves.Count)
        {
            OnAllWavesCompleted?.Invoke();
            return;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 현재 필드에 있는 모든 몬스터 제거
    /// </summary>
    public void KillAllActiveMonsters()
    {
        if (activeMonsters == null || activeMonsters.Count == 0)
        {
            return;
        }

        foreach (var monsterObj in new List<GameObject>(activeMonsters))
        {
            if (monsterObj != null)
            {
                MonsterBase monsterBase = monsterObj.GetComponent<MonsterBase>();
                if (monsterBase != null)
                {
                    monsterBase.TakeDamage(9999f); // 또는 monsterBase.TakeDamage(9999)
                }
            }
        }

        activeMonsters.Clear();
        isWaveActive = false;
    }
    public List<PoolableObject> GetMonsterPrefabs()
    {
        return normalMonsterPrefabs;
    }

    public void SpawnSpecificMonster(PoolableObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject spawned = GameManager.Instance.PoolManager.GetFromPool(prefab, position, Quaternion.identity);
        MonsterBase monsterBase = spawned.GetComponent<MonsterBase>();
        if (monsterBase != null)
        {
            activeMonsters.Add(spawned);
            EventBus.AddMonster(monsterBase);
            OnMonsterSpawned?.Invoke(monsterBase);
            monsterBase.OnDieEvent += () => OnMonsterDeath(spawned);
            monsterBase.OnDieEvent += () => EventBus.RemoveMonster(monsterBase);
        }
    }
#endif


}