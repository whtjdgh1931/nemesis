using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 리팩터링된 MapController
/// - 룸 생성/소멸과 문 생성 책임을 더 명확히 분리
/// - 방어적 검사 및 로그 강화
/// - 작은 헬퍼 메서드로 가독성 향상
/// </summary>
public class MapController : MonoBehaviour
{
    [SerializeField] MonsterController _monsterController;
    [SerializeField] RoomSpawner _roomSpawner;
    [SerializeField] DoorSpawner _doorSpawner;
    [SerializeField] DoorDecider _doorDecider;

    [SerializeField] Room _currentRoom;
    [SerializeField] Door[] _currentDoors;
    [SerializeField] int _currentRoomCount = -1;
    [SerializeField] bool _hasLabRoomAppeared = false;
    [SerializeField] Player _player;

    Coroutine _goNextRoomRoutine;
    Coroutine _doorInteractionRoutine;

    public MonsterController MonsterController => _monsterController;
    public RoomSpawner RoomSpawner => _roomSpawner;
    public DoorSpawner DoorSpawner => _doorSpawner;
    public DoorDecider DoorDecider => _doorDecider;

    public Room CurrentRoom => _currentRoom;
    public Door[] CurrentDoors => _currentDoors;
    public int CurrentRoomCount => _currentRoomCount;
    public bool HasLabRoomAppeared => _hasLabRoomAppeared;

    /// <summary>
    /// 룸 로딩이 끝나고 전투 시작시 발행하는 이벤트
    /// </summary>
    public event Action OnRoomStart;

    /// <summary>
    /// 시작 방에서 나갈 때 발행하는 이벤트(시간 체크용)
    /// </summary>
    public event Action OnStartRoomExited;

    /// <summary>
    /// 문과의 상호작용과 플레이어의 이동이 모두 끝났을 때 발행하는 이벤트
    /// PlaySceneView에서 로딩 패널을 제어하기 위해 사용
    /// </summary>
    public event Action<DoorInteractor> OnDoorInteractionFinished;

    /// <summary>
    /// 현재 방 번호가 변경될 때 발행하는 이벤트
    /// </summary>
    public event Action<int> OnCurrentRoomCountChanged;

    public void Initialize(Player player)
    {
        _player = player;
        if (_roomSpawner == null) Debug.LogError("MapController.Initialize: _roomSpawner is null");
        if (_doorSpawner == null) Debug.LogError("MapController.Initialize: _doorSpawner is null");
        if (_doorDecider == null) Debug.LogError("MapController.Initialize: _doorDecider is null");

        _roomSpawner.OnRoomSpawned += OnRoomSpawned;
        _doorSpawner.DoorInteracted += OnDoorInteracted;
        _monsterController.OnAllMonsterDefeated += StartReward;

        _doorDecider.Initialize();
        _monsterController.Initialize();

        _roomSpawner.SpawnRoom(new RoomInfo(RoomType.Start, null, null));
    }

    /// <summary>
    /// 문과 상호작용 했을 때 호출되는 함수
    /// </summary>
    /// <param name="interactable"></param>
    void OnDoorInteracted(IInteractable interactable)
    {
        if (interactable is DoorInteractor doorInteractor && doorInteractor.RoomInfo != null)
        {
            if(_goNextRoomRoutine != null)
            {
                StopCoroutine(_goNextRoomRoutine);
            }
            // 여기서 방 넘어갈 때 이펙트나 로딩화면 처리
            _goNextRoomRoutine = StartCoroutine(GoNextRoomRoutine(doorInteractor));
            if(_currentRoom.RoomInfo.RoomType == RoomType.Start)
            {
                // 시작 방에서 나갈 때 시간 체크를 위한 이벤트 발행
                OnStartRoomExited?.Invoke();
            }
        }
        else
        {
            Debug.LogError("OnDoorInteracted: interactable is not a DoorInteractor or RoomInfo is null");
        }
    }

    /// <summary>
    /// 문을 상호작용 한 시점부터 다음 방을 생성하고 진입할때까지의 코루틴
    /// </summary>
    /// <param name="doorInteractor"></param>
    /// <returns></returns>
    IEnumerator GoNextRoomRoutine(DoorInteractor doorInteractor)
    {
        // 1. 플레이어의 문 상호작용 루틴 시작
        _doorInteractionRoutine = StartCoroutine(_player.DoorInteractionRoutine());
        // 위에 코루틴이 끝날 때까지 대기
        yield return _doorInteractionRoutine;
        // 2. 문 상호작용 루틴이 끝나면 로딩 패널을 켰다가 커주기 위해 이벤트를 하나 발행
        OnDoorInteractionFinished?.Invoke(doorInteractor);
        // 3. 1초 뒤 현재 방 오브젝트들 파괴
        yield return new WaitForSeconds(1.0f);
        DestroyCurrentRoomObjects();
        // 4. 로딩 패널이 켜지고 꺼진 이벤트를 받아서 다음 방 생성
        // 이건 PlayScene에서 이벤트 연결로 처리

        _goNextRoomRoutine = null;
        _doorInteractionRoutine = null;
    }

    public void SpawnRoom(DoorInteractor doorInteractor)
    {
        Room room = _roomSpawner.SpawnRoom(doorInteractor.RoomInfo);
        // 플레이어 스폰 위치를 정해주자
        _player.gameObject.SetActive(false);
        _player.transform.position = room.PlayerSpawnPoint.position;
        _player.gameObject.SetActive(true);
        // 콜로세움 방 진입 시 처리
        if (doorInteractor.RoomInfo.RoomType == RoomType.Colosseum)
        {
            EventBus.SetColosseumRoom(true);
        }
    }

    void OnRoomSpawned(Room room)
    {
        if (room == null)
        {
            Debug.LogError("OnRoomSpawned called with null room");
            return;
        }

        // 방 생성 후 효과 처리(Start방은 없음)
        if(room.RoomInfo.RoomType != RoomType.Start)
        {
            GameManager.Instance.SoundManager.PlayBGM(room.RoomInfo.RoomType.ToString());
        }

        // 상태 갱신
        UpdateCurrentRoomState(room);

        // 문 생성 처리
        if (EventBus.IsColosseumRoom)
        {
            // 콜로세움일경우 일단 문 생성하지말고 아래 로직 진행
        }
        else
        {
            CreateDoorsForCurrentRoom();
        }

        // 여기서 방 타입별로 스폰할 몬스터 정해주고
        // 스폰위치 업데이트 해주고 몬스터 스폰해야함.

        int roomCost = _currentRoomCount * Constants.ROOMCOSTMULTIPLIER;
        Transform[] spawnPoints = room.MonsterSpawnPoints;
        if (room.RoomInfo.RoomType == RoomType.Colosseum)
        {
            _monsterController.SpawnElite(CurrentRoomCount, spawnPoints);
            EventBus.EliteBoss.OnDieEvent += CreateDoorsForCurrentRoom;
        }
        else if( room.RoomInfo.RoomType == RoomType.Boss)
        {
            _monsterController.SpawnBoss(spawnPoints);
        }
        else
        {
            _monsterController.SpawnMonster(roomCost, spawnPoints);
        }
        EventBus.bIsRoomReady = true;
        OnRoomStart?.Invoke();
    }

    void UpdateCurrentRoomState(Room room)
    {
        _currentRoom = room;
        _currentRoomCount++;
        OnCurrentRoomCountChanged?.Invoke(_currentRoomCount);
        GameManager.Instance.CurrencyManager.ResetRoomCredit();

        if (room.RoomInfo?.RoomType == RoomType.Lab)
            _hasLabRoomAppeared = true;
    }

    void CreateDoorsForCurrentRoom()
    {
        if (_currentRoom == null)
        {
            Debug.LogError("CreateDoorsForCurrentRoom: _currentRoom is null");
            return;
        }

        if (_currentRoomCount == 1)
        {
            // TechSelectPackType을 랜덤으로 선택하여 TechSelect 방 생성
            int rand = UnityEngine.Random.Range(0, (int)TechSelectPackType.Count);
            TechSelectPackType randomTechPackType = (TechSelectPackType)rand;
            Door door = TrySpawnDoor(_currentRoom.GetNextDoorPositions(1)[0], new RoomInfo(RoomType.Normal, NormalRoomType.TechSelect, randomTechPackType), _currentRoom?.transform);
            return;
        }

        // 다음 문 개수(nextDoorCount) 결정 (DoorDecider에 current index 전달)
        int nextDoorCount = _doorDecider.GetNextDoorCount(_currentRoomCount);

        if (nextDoorCount <= 0)
        {
            Debug.Log("[MapController] nextDoorCount <= 0, skipping door spawn");
            _currentDoors = Array.Empty<Door>();
            return;
        }

        // 다음 문 개수(nextDoorCount)에 따라 문 위치 배열(doorPoisitions) 얻기
        var doorPositions = _currentRoom.GetNextDoorPositions(nextDoorCount) ?? Array.Empty<Transform>();
        if (doorPositions.Length < nextDoorCount)
        {
            Debug.LogWarning($"CreateDoorsForCurrentRoom: requested {nextDoorCount} positions but got {doorPositions.Length}. Clamping.");
            nextDoorCount = doorPositions.Length;
        }

        // 문 타입 / 일반방 타입 / 기술팩 타입 결정 (분리해서 얻기)
        int normalRoomCount;
        RoomType[] doorTypes = _doorDecider.GetNextRoomTypes(
            nextDoorCount,
            _currentRoom.RoomInfo?.RoomType ?? RoomType.Normal,
            _currentRoomCount,
            _hasLabRoomAppeared,
            out normalRoomCount) ?? Array.Empty<RoomType>();

        // 안전: doorTypes 길이가 nextDoorCount보다 작으면 남는 슬롯을 마지막 선택 타입으로 채움
        if (doorTypes.Length != nextDoorCount)
        {
            Debug.LogWarning($"GetNextRoomTypes returned {doorTypes.Length} items but expected {nextDoorCount}. Filling remaining slots.");
            doorTypes = FillToCount(doorTypes, nextDoorCount);
        }

        NormalRoomType[] normalRoomTypes = Array.Empty<NormalRoomType>();
        int techSelectPackCount = 0;
        if (normalRoomCount > 0)
            normalRoomTypes = _doorDecider.GetNormalRoomTypes(normalRoomCount, out techSelectPackCount) ?? Array.Empty<NormalRoomType>();

        TechSelectPackType[] techPackTypes = Array.Empty<TechSelectPackType>();
        if (techSelectPackCount > 0)
            techPackTypes = GameManager.Instance?.skillManager?.GetSkillPackTypes(techSelectPackCount) ?? Array.Empty<TechSelectPackType>();

        // 소비 인덱스
        int normalIdx = 0;
        int techIdx = 0;

        List<Door> created = new List<Door>(nextDoorCount);

        for (int i = 0; i < nextDoorCount; i++)
        {
            RoomType rt = doorTypes.Length > i ? doorTypes[i] : RoomType.Normal;

            NormalRoomType? nrt = null;
            TechSelectPackType? tpt = null;

            if (rt == RoomType.Normal)
            {
                if (normalIdx < normalRoomTypes.Length)
                {
                    nrt = normalRoomTypes[normalIdx++];
                    if (nrt == NormalRoomType.TechSelect)
                    {
                        if (techIdx < techPackTypes.Length)
                            tpt = techPackTypes[techIdx++];
                        else
                            Debug.LogWarning("CreateDoorsForCurrentRoom: techPackTypes 부족");
                    }
                }
                else
                {
                    Debug.LogWarning("CreateDoorsForCurrentRoom: normalRoomTypes 부족, defaulting to Normal");
                }
            }

            var nextRoomInfo = new RoomInfo(rt, nrt, tpt);

            // DoorSpawner가 parent 파라미터를 지원하면 전달하고, 아니면 SetParent로 처리
            Door door = TrySpawnDoor(doorPositions[i], nextRoomInfo, _currentRoom?.transform);
            if (door != null)
            {
                created.Add(door);
            }
            else
            {
                Debug.LogWarning($"[MapController] Failed to spawn door at index {i}");
            }
        }

        _currentDoors = created.ToArray();

        // Lab 포함 여부 갱신(선택지에 lab 포함돼있으면 표시)
        if (Array.Exists(doorTypes, t => t == RoomType.Lab))
            _hasLabRoomAppeared = true;

        if (EventBus.IsColosseumRoom)
        {
            EventBus.EliteBoss.OnDieEvent -= CreateDoorsForCurrentRoom;
        }
    }

    // DoorSpawner가 parent 파라미터를 지원하는 경우를 고려해 안전하게 호출
    Door TrySpawnDoor(Transform position, RoomInfo info, Transform parent)
    {
        if (_doorSpawner == null)
        {
            Debug.LogError("TrySpawnDoor: _doorSpawner is null");
            return null;
        }

        Door door = _doorSpawner.SpawnDoor(position, info);
        if (door == null) return null;

        // 구독: 반드시 같은 구독이 중복으로 등록되지 않도록 방어
        if (_currentRoom != null)
        {
            _currentRoom.OnRewardSelectionFinished -= door.OnRewardSelectionCompleted; // 중복 등록 방지
            _currentRoom.OnRewardSelectionFinished += door.OnRewardSelectionCompleted;
        }

        //if (_currentRoom?.RoomInfo?.RoomType == RoomType.Start || _currentRoom?.RoomInfo?.RoomType == RoomType.Shop)
        //{
        //    door.OnRewardSelectionCompleted();
        //}
        if (_currentRoom?.RoomInfo?.RoomType == RoomType.Shop)
        {
            door.OnRewardSelectionCompleted();
        }

        if (door != null && parent != null)
            door.transform.SetParent(parent, worldPositionStays: true);

        return door;
    }

    // doorTypes 배열을 요청 개수(count)만큼 채움: 마지막 값 또는 Normal로 채움
    RoomType[] FillToCount(RoomType[] original, int count)
    {
        if (original == null) return Array.Empty<RoomType>();
        if (original.Length >= count) return original;

        var result = new RoomType[count];
        for (int i = 0; i < original.Length; i++) result[i] = original[i];

        RoomType fill = original.Length > 0 ? original[original.Length - 1] : RoomType.Normal;
        for (int i = original.Length; i < count; i++) result[i] = fill;
        return result;
    }

    void DestroyCurrentRoomObjects()
    {
        // 0) 안전: _currentRoom이 null이면 할 일 없음
        if (_currentRoom == null)
        {
            Debug.LogWarning("[MapController] DestroyCurrentRoomObjects: no current room");
            _currentDoors = null;
            return;
        }

        // 1) 먼저 문들이 _currentRoom 이벤트(구독)에서 안전하게 언구독되도록 처리
        if (_currentDoors != null && _currentRoom != null)
        {
            foreach (var door in _currentDoors)
            {
                if (door == null) continue;
                try
                {
                    _currentRoom.OnRewardSelectionFinished -= door.OnRewardSelectionCompleted;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to unsubscribe door handler: {ex}");
                }
            }
        }

        // 2) 관련 코루틴 정리 (플레이어 상호작용 등)
        if (_doorInteractionRoutine != null)
        {
            StopCoroutine(_doorInteractionRoutine);
            _doorInteractionRoutine = null;
        }
        if (_goNextRoomRoutine != null)
        {
            StopCoroutine(_goNextRoomRoutine);
            _goNextRoomRoutine = null;
        }

        // 3) PoolableObject 찾아서 반환 처리 (PoolManager가 즉시 Destroy/Deactivate 한다면 이후 순서 중요)
        var poolables = _currentRoom.PoolableObjectsInRoom;
        if (poolables != null)
        {
            foreach (var poolableObj in poolables)
            {
                try
                {
                    if (GameManager.Instance?.PoolManager != null)
                        GameManager.Instance.PoolManager.ReleaseToPool(poolableObj);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"ReleaseToPool failed for {poolableObj?.name}: {ex}");
                }
            }
        }

        // 4) 먼저 문들 파괴 (이미 이벤트 언구독했으므로 안전)
        if (_currentDoors != null)
        {
            foreach (var door in _currentDoors)
            {
                if (door == null) continue;
                if (door.gameObject.scene.IsValid())
                {
                    GameManager.Instance.PoolManager.ReleaseToPool(door.gameObject);   
                }
                else
                {
                    Debug.LogWarning($"[MapController] Door appears to be an asset: {door.name}, skipping Destroy");
                }
            }
            _currentDoors = null;
        }

        // 5) 그 다음 방 파괴
        if (_currentRoom != null)
        {
            if (_currentRoom.gameObject.scene.IsValid())
            {
                GameManager.Instance.PoolManager.ReleaseToPool(_currentRoom.gameObject);
            }
            else
            {
                Debug.LogWarning($"[MapController] CurrentRoom appears to be an asset: {_currentRoom.name}, skipping Destroy");
            }
            _currentRoom = null;
        }
    }

    void StartReward()
    {
        _currentRoom.SpawnReward();
    }

    private void OnDisable()
    {
        // 코루틴 정리
        if (_doorInteractionRoutine != null)
        {
            StopCoroutine(_doorInteractionRoutine);
            _doorInteractionRoutine = null;
        }
        if (_goNextRoomRoutine != null)
        {
            StopCoroutine(_goNextRoomRoutine);
            _goNextRoomRoutine = null;
        }

    }

#if UNITY_EDITOR
    public void KillAllMonsters()
    {
        _monsterController.MonsterSpawner?.KillAllActiveMonsters();
    }

#endif

}