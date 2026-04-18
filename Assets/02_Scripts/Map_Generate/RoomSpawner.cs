using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 방을 스폰하는 역할을 하는 클래스
/// RoomInfo(타입 + 옵션)를 받아서 SO 기반으로 인스턴스화 및 초기화를 수행한다.
/// </summary>
public class RoomSpawner : MonoBehaviour
{
    public event Action<Room> OnRoomSpawned; // 방이 생성될 때 발생하는 이벤트
    public event Action OnRewardSelectionFinished;  // 보상 선택이 완료되었을 때 발생하는 이벤트

    Coroutine _roomSpawnedEventRoutine;

    /// <summary>
    /// RoomInfo를 받아 해당 Room을 생성한다. position/parent를 지정하지 않으면 (0,0,0)과 씬 루트에 생성된다.
    /// </summary>
    public Room SpawnRoom(RoomInfo roomInfo, Vector3 position = default, Transform parent = null)
    {
        if (roomInfo == null)
        {
            Debug.LogError("RoomSpawner.SpawnRoom: roomInfo is null.");
            return null;
        }

        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("RoomSpawner: GameManager.Instance is null.");
            return null;
        }

        var dataManager = gm.DataManager;
        if (dataManager == null)
        {
            Debug.LogError("RoomSpawner: DataManager is null on GameManager.");
            return null;
        }

        if (!dataManager.TryGetRoomData(roomInfo.RoomType, out var roomData) || roomData == null)
        {
            Debug.LogError($"RoomSpawner: RoomData not found for RoomType {roomInfo.RoomType}.");
            return null;
        }

        var prefab = roomData.RoomPrefab;
        if (prefab == null)
        {
            Debug.LogError($"RoomSpawner: RoomData '{roomData.name}' has no assigned RoomPrefab.");
            return null;
        }

        // Instantiate
        GameObject roomObj = null;
        try
        {
            roomObj = GameManager.Instance.PoolManager.GetFromPool($"Prefabs/Map/Rooms/" + roomInfo.RoomType.ToString(), position, Quaternion.identity, parent);
        }
        catch (Exception ex)
        {
            Debug.LogError($"RoomSpawner: Failed to instantiate prefab '{prefab.name}': {ex}");
            return null;
        }

        // Room 컴포넌트 검사 및 초기화
        var room = roomObj.GetComponent<Room>();
        if (room == null)
        {
            Debug.LogError($"RoomSpawner: Instantiated prefab '{prefab.name}' has no Room component. Destroying instance.");
            GameManager.Instance.PoolManager.ReleaseToPool(roomObj);
            return null;
        }

        // SO 기반 초기화(정책: Room 내부에서 필요한 값들을 SO로부터 적용)
        try
        {
            room.OnRewardSelectionFinished += RaiseRewardSelectionFinishedEvent;
            room.Initialize(roomInfo);
        }
        catch (Exception ex)
        {
            Debug.LogError($"RoomSpawner: Exception during InitializeFromRoomData: {ex}");
            GameManager.Instance.PoolManager.ReleaseToPool(roomObj);
            return null;
        }

        // 플레이어 입력 켜주기
        EventBus.SetCanGetInput(true);

        // 이벤트는 패널 켜지는 시간 고려해서 코루틴으로 1초 후에 호출
        if (_roomSpawnedEventRoutine != null)
        {
            StopCoroutine(_roomSpawnedEventRoutine);
        }
        _roomSpawnedEventRoutine = StartCoroutine(RoomSpawnedEventRoutine(room));
        return room;
    }

    IEnumerator RoomSpawnedEventRoutine(Room room)
    {
        yield return new WaitForSeconds(1f);
        OnRoomSpawned?.Invoke(room);
        _roomSpawnedEventRoutine = null;
    }

    public void RaiseRewardSelectionFinishedEvent()
    {
        OnRewardSelectionFinished?.Invoke();
    }
}