using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>

/// </summary>
public abstract class Room : MonoBehaviour
{
    [SerializeField] protected Transform[] _rewardSpawnPoints;        // 보상 오브젝트 스폰 위치들 (프리팹에 세팅)
    [SerializeField] protected Transform[] _monsterSpawnPoints = null;    // 몬스터 스폰 위치들 (프리팹에 세팅)
    [SerializeField] protected Transform _playerSpawnPoint;         // 플레이어 스폰 위치 (프리팹에 세팅)

    [Header("Door spawn points (setup on prefab)")]
    [SerializeField] protected Transform[] _doorSpawnPointsLeft;
    [SerializeField] protected Transform[] _doorSpawnPointsRight;

    protected RoomInfo _roomInfo;
    protected List<GameObject> _poolableObjectsInRoom = new List<GameObject>();

    // 구조적 데이터는 프리팹에 보관 (인스펙터에서 볼 수 있음)
    public Transform[] RewardSpawnPoints => _rewardSpawnPoints;
    public Transform[] MonsterSpawnPoints => _monsterSpawnPoints;
    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    public Transform[] DoorSpawnPointsLeft => _doorSpawnPointsLeft;
    public Transform[] DoorSpawnPointsRight => _doorSpawnPointsRight;

    // 런타임에서 RoomData로부터 필요한 값 접근
    public RoomInfo RoomInfo => _roomInfo;
    public List<GameObject> PoolableObjectsInRoom => _poolableObjectsInRoom;    

    public event Action OnRewardSelectionFinished;

    /// <summary>
    /// 단일화된 진입점: RoomInfo를 받아 초기화한다.
    /// 호출부는 이 메서드를 메인으로 사용하세요.
    /// </summary>
    public virtual void Initialize(RoomInfo roomInfo)
    {
        if (roomInfo == null)
        {
            Debug.LogWarning($"{nameof(Room)}.Initialize called with null RoomInfo on '{name}'.");
        }
        else
        {
            _roomInfo = roomInfo;
        }
    }

    /// <summary>
    /// 입력받은 count 수만큼 다음 문들이 생성될 위치들을 Transform배열로 반환
    /// - 내부적으로 배열 길이/인덱스 미존재에 대해 방어적으로 처리합니다.
    /// - 가능한 경우에만 Transform을 반환하고, 없으면 빈 배열을 반환합니다.
    /// </summary>
    public Transform[] GetNextDoorPositions(int count)
    {
        if (count < 1 || count > 3)
        {
            Debug.LogWarning("GetNextDoorPositions: 생성될 문의 개수는 1~3개 사이여야 합니다.");
            return Array.Empty<Transform>();
        }

        // 방어적 접근자: 지정 인덱스가 없으면 가능한 대체 점을 선택
        Transform GetPointSafe(Transform[] arr, int idx)
        {
            if (arr == null || arr.Length == 0) return null;
            if (idx >= 0 && idx < arr.Length) return arr[idx];
            int clamped = Mathf.Clamp(idx, 0, arr.Length - 1);
            return arr[clamped];
        }

        Transform PickOneOfFirst()
        {
            var left = GetPointSafe(_doorSpawnPointsLeft, 0);
            var right = GetPointSafe(_doorSpawnPointsRight, 0);
            if (left == null && right == null) return null;
            if (left == null) return right;
            if (right == null) return left;
            return UnityEngine.Random.value < 0.5f ? left : right;
        }

        var results = new List<Transform>();

        switch (count)
        {
            case 1:
                {
                    var chosen = PickOneOfFirst();
                    if (chosen != null) results.Add(chosen);
                    break;
                }
            case 2:
                {
                    float rand2 = UnityEngine.Random.value;
                    if (rand2 < 1f / 3f)
                    {
                        var a = GetPointSafe(_doorSpawnPointsLeft, 0);
                        var b = GetPointSafe(_doorSpawnPointsRight, 0);
                        if (a != null) results.Add(a);
                        if (b != null) results.Add(b);
                    }
                    else if (rand2 < 2f / 3f)
                    {
                        var a = GetPointSafe(_doorSpawnPointsLeft, 1);
                        var b = GetPointSafe(_doorSpawnPointsLeft, 2);
                        if (a != null) results.Add(a);
                        if (b != null && b != a) results.Add(b);
                    }
                    else
                    {
                        var a = GetPointSafe(_doorSpawnPointsRight, 1);
                        var b = GetPointSafe(_doorSpawnPointsRight, 2);
                        if (a != null) results.Add(a);
                        if (b != null && b != a) results.Add(b);
                    }
                    break;
                }
            case 3:
                {
                    float rand3 = UnityEngine.Random.value;
                    if (rand3 < 0.25f)
                    {
                        var a = GetPointSafe(_doorSpawnPointsLeft, 1);
                        var b = GetPointSafe(_doorSpawnPointsLeft, 2);
                        var c = GetPointSafe(_doorSpawnPointsRight, 1);
                        if (a != null) results.Add(a);
                        if (b != null && b != a) results.Add(b);
                        if (c != null && c != a && c != b) results.Add(c);
                    }
                    else if (rand3 < 0.5f)
                    {
                        var a = GetPointSafe(_doorSpawnPointsLeft, 1);
                        var b = GetPointSafe(_doorSpawnPointsLeft, 2);
                        var c = GetPointSafe(_doorSpawnPointsRight, 2);
                        if (a != null) results.Add(a);
                        if (b != null && b != a) results.Add(b);
                        if (c != null && c != a && c != b) results.Add(c);
                    }
                    else if (rand3 < 0.75f)
                    {
                        var a = GetPointSafe(_doorSpawnPointsRight, 1);
                        var b = GetPointSafe(_doorSpawnPointsRight, 2);
                        var c = GetPointSafe(_doorSpawnPointsLeft, 1);
                        if (a != null) results.Add(a);
                        if (b != null && b != a) results.Add(b);
                        if (c != null && c != a && c != b) results.Add(c);
                    }
                    else
                    {
                        var a = GetPointSafe(_doorSpawnPointsRight, 1);
                        var b = GetPointSafe(_doorSpawnPointsRight, 2);
                        var c = GetPointSafe(_doorSpawnPointsLeft, 2);
                        if (a != null) results.Add(a);
                        if (b != null && b != a) results.Add(b);
                        if (c != null && c != a && c != b) results.Add(c);
                    }
                    break;
                }
        }

        return results.ToArray();
    }

    public abstract IInteractable[] SpawnReward();

    public List<GameObject> GetPoolableObjectsInRoom()
    {
        return _poolableObjectsInRoom;
    }

    public void RewardSelectionFinished()
    {
        OnRewardSelectionFinished?.Invoke();

        // 스킬 진화 발동용 메서드
        EventBus.Evolution();
    }

    protected virtual void OnDisable()
    {
        GameManager.Instance.SoundManager.StopBGM();
    }
}