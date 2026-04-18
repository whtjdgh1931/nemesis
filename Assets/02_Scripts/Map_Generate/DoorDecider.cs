using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DoorDecider: 다음 방 후보군을 구성하고 확률적으로 선택지를 반환하는 정책 담당 클래스.
/// 
/// 설계 원칙 및 계약 요약:
/// - DoorDecider는 '정책의 단일 진실 원천'입니다. MapController는 Decider의 결과를 신뢰하고 spawn만 담당해야 합니다.
/// - GetNextDoorCount(currentRoomIndex):
///     * 입력: currentRoomIndex ∈ [0,14]
///     * 동작: 특수 규칙 적용 (index==0 => 1, index==13 => 1, index==14 => 0), 그 외는 확률에 따라 1..3 반환.
/// - GetNextRoomTypes(count, ...):
///     * 입력: count ∈ [0,3], currentRoomIndex ∈ [0,14]
///     * 동작: 항상 길이 == count 인 RoomType[] 반환 (count==0이면 빈 배열).
///       특수 규칙: index==12 -> Shop 우선 포함, index==13 -> Boss 우선 포함(나머지 슬롯은 후보군에서 채움).
///       확률 합이 0이면 균등 분포로 폴백. 특수방(Shop, Lab, Colosseum 등)은 중복 불허, Normal은 중복 허용.
/// - GetNormalRoomTypes(n, out techSelectCount):
///     * 반환 길이는 요청한 n과 같음. 확률 합이 0이면 균등 분포로 폴백.
/// 
/// 주의:
/// - 정책 변경 시 이 클래스 내부에서만 수정하세요(중앙 집중식 규칙 관리).
/// - MapController는 Decider의 반환을 신뢰하므로 Decider 계약을 깨지 마세요.
/// </summary>
public class DoorDecider : MonoBehaviour
{
    [Header("다음 방 선택지 개수 확률 (기본값)")]
    [SerializeField] float _oneDoorChance = 0.1f; // 다음 방 1개 확률
    [SerializeField] float _twoDoorChance = 0.6f; // 다음 방 2개 확률
    [SerializeField] float _threeDoorChance = 0.3f; // 다음 방 3개 확률

    [Header("NormalRoom의 세부 타입 확률 (디폴트, Inspector에서 조정 가능)")]
    [SerializeField] float door_CreditChance = 0.2f;
    [SerializeField] float door_HealChance = 0.2f;
    [SerializeField] float door_UpgradeChance = 0.2f;
    [SerializeField] float door_ChromeChance = 0.2f;
    [SerializeField] float door_SkillPackChance = 0.2f;

    /// <summary>
    /// 초기화 훅(필요시 사용). 현재는 예약용으로 비워둠.
    /// </summary>
    public void Initialize() { /* reserved for future use */ }

    /// <summary>
    /// 현재 방 인덱스를 기반으로 다음에 생성할 문 개수를 결정하여 반환합니다.
    /// 계약:
    /// - currentRoomIndex must be in range [0,14]
    /// - 특별 규칙:
    ///   * index == 0  : 항상 1 (start room)
    ///   * index == 13 : 항상 1 (pre-boss, 보스 진입 보장)
    ///   * index == 14 : 항상 0 (boss room: 다음 생성 없음)
    /// - 그 외에는 Inspector에 설정된 확률(_oneDoorChance/_twoDoorChance/_threeDoorChance)에 따라 1..3을 반환합니다.
    /// - 전체 확률 합이 0이면 균등 폴백을 사용합니다.
    /// </summary>
    /// <param name="currentRoomIndex">현재 방 인덱스(0~14)</param>
    /// <returns>다음 생성할 문의 개수(0~3)</returns>
    public int GetNextDoorCount(int currentRoomIndex)
    {
        if (currentRoomIndex < 0 || currentRoomIndex > 14)
            throw new ArgumentOutOfRangeException(nameof(currentRoomIndex), "방 인덱스는 0~14 사이여야 합니다.");

        // 특수 규칙: 시작 방, 보스 직전, 보스 방
        if (currentRoomIndex == 1)
        {
            return 1;
        }
        if (currentRoomIndex == 13)
        {
            return 1;
        }
        if (currentRoomIndex == 14)
        {
            return 0;
        }

        float totalChance = _oneDoorChance + _twoDoorChance + _threeDoorChance;
        if (totalChance <= 0f)
        {
            Debug.LogWarning("[DoorDecider] door-count weights sum to 0, using uniform fallback.");
            int r = UnityEngine.Random.Range(0, 3); // 0..2
            return r + 1; // 1..3
        }

        float rand = UnityEngine.Random.Range(0f, totalChance);
        if (rand < _oneDoorChance) return 1;
        if (rand < _oneDoorChance + _twoDoorChance) return 2;
        return 3;
    }

    /// <summary>
    /// 주어진 컨텍스트에 따라 count개의 다음 방 타입을 결정하여 반환합니다.
    /// 계약:
    /// - count ∈ [0,3]. count == 0이면 빈 배열 반환.
    /// - 항상 길이 == count 인 배열을 반환(요청한 개수를 만족).
    /// - currentRoomIndex == 12 -> Shop을 우선 포함.
    /// - currentRoomIndex == 13 -> Boss를 우선 포함(나머지 슬롯은 후보군에서 채움).
    /// - 후보군에서 확률 합이 0이면 균등 분포로 폴백.
    /// - 특수방(Shop, Lab, Colosseum)은 중복 불허, Normal은 중복 허용.
    /// </summary>
    /// <param name="count">요청되는 선택지 개수(0~3)</param>
    /// <param name="currentRoomType">현재 방 타입</param>
    /// <param name="currentRoomIndex">현재 방 인덱스(0~14)</param>
    /// <param name="hasLabRoomAppeared">이미 Lab이 등장했는지</param>
    /// <param name="normalRoomCount">반환된 배열에서 Normal 타입의 개수 (out)</param>
    /// <returns>count 길이의 RoomType 배열(또는 count==0이면 빈 배열)</returns>
    public RoomType[] GetNextRoomTypes(int count, RoomType currentRoomType, int currentRoomIndex, bool hasLabRoomAppeared, out int normalRoomCount)
    {
        ValidateGetNextRoomTypesArgs(count, currentRoomIndex);

        // 요청이 0이면 빈 배열 반환
        if (count == 0)
        {
            normalRoomCount = 0;
            return Array.Empty<RoomType>();
        }

        // 1) 특수 규칙 적용: 우선 포함할 타입들을 확보
        var types = ApplySpecialRules(currentRoomIndex);

        // 2) 후보군 구성(지역 변수로 처리하여 부작용 방지)
        var candidate = BuildCandidateMap();

        // 3) 후보군을 컨텍스트에 맞게 정리
        PruneCandidates(candidate, currentRoomType, hasLabRoomAppeared, types);

        // 4) 남은 슬롯을 가중치 랜덤으로 채움
        FillRemainingSlots(types, candidate, count);

        normalRoomCount = types.Count(x => x == RoomType.Normal);
        return types.ToArray();
    }

    // ------------------------- 세부 헬퍼들 -------------------------

    /// <summary>
    /// 입력 값 검증. 예외 발생 시 호출자에게 알려서 버그를 빨리 잡게 함.
    /// </summary>
    void ValidateGetNextRoomTypesArgs(int count, int currentRoomIndex)
    {
        if (count < 0 || count > 3)
            throw new ArgumentOutOfRangeException(nameof(count), "문의 개수는 0~3개 사이여야 합니다.");
        if (currentRoomIndex < 0 || currentRoomIndex > 14)
            throw new ArgumentOutOfRangeException(nameof(currentRoomIndex), "방 인덱스는 0~14 사이여야 합니다.");
    }

    /// <summary>
    /// currentRoomIndex에 따른 특수 규칙을 적용하여 초기 types 리스트를 반환합니다.
    /// (여기서는 index==12 => Shop 포함, index==13 => Boss 포함)
    /// 이 메서드는 오직 '우선 포함'만 수행하고 즉시 반환하지 않습니다.
    /// </summary>
    List<RoomType> ApplySpecialRules(int currentRoomIndex)
    {
        var types = new List<RoomType>();
        if (currentRoomIndex == 1)
        {
            types.Add(RoomType.Normal);
        }
        if (currentRoomIndex == 12)
        {
            types.Add(RoomType.Shop);
        }
        if (currentRoomIndex == 13)
        {
            types.Add(RoomType.Boss);
        }
        return types;
    }

    /// <summary>
    /// 후보군을 로컬 딕셔너리로 구성합니다. (필드 대신 로컬 사용으로 부작용 방지)
    /// 수정이 필요하면 이 메서드만 고치면 됩니다.
    /// </summary>
    Dictionary<RoomType, float> BuildCandidateMap()
    {
        // DataManager의 RoomDataMap에서 확률 정보를 가져오기
        DataManager dataManager = GameManager.Instance?.DataManager;

        if (dataManager == null)
        {
            Debug.LogWarning("[DoorDecider] DataManager is null, using default candidate weights.");
        }

        var candidate = new Dictionary<RoomType, float>();
        foreach (var kv in dataManager?.RoomDataMap)
        {
            var roomType = kv.Key;
            var roomData = kv.Value;
            if (roomData == null)
            {
                Debug.LogWarning($"[DoorDecider] RoomData for {roomType} is null, skipping.");
                continue;
            }
            candidate[roomType] = roomData.BaseChance;
        }
        return candidate;
    }

    /// <summary>
    /// 후보군에서 컨텍스트(이미 등장한 Lab, 현재 방 타입, 이미 선택된 특수 타입)에 따라 항목을 제거합니다.
    /// </summary>
    void PruneCandidates(Dictionary<RoomType, float> candidate, RoomType currentRoomType, bool hasLabRoomAppeared, List<RoomType> alreadyChosen)
    {
        if (hasLabRoomAppeared)
            candidate.Remove(RoomType.Lab);

        foreach (var special in new[] { RoomType.Shop, RoomType.Lab, RoomType.Colosseum })
        {
            if (currentRoomType == special)
                candidate.Remove(special);
        }

        // 이미 선택된 특수 타입은 후보에서 제거(중복 방지)
        foreach (var t in alreadyChosen.Where(t => t != RoomType.Normal).ToArray())
            candidate.Remove(t);
    }

    /// <summary>
    /// types에 이미 포함된 항목을 기준으로 남은 슬롯(targetCount)을 안전하게 채웁니다.
    /// - candidate가 비어있으면 Normal로 채움.
    /// - candidate의 가중치 합이 0이면 균등 분포로 대체(ChooseWeightedRandom 내부 처리).
    /// - 특수 타입은 중복 불가(선택 즉시 후보에서 제거).
    /// </summary>
    void FillRemainingSlots(List<RoomType> types, Dictionary<RoomType, float> candidate, int targetCount)
    {
        for (int i = types.Count; i < targetCount; i++)
        {
            if (candidate.Count == 0)
            {
                types.Add(RoomType.Normal);
                Debug.LogWarning("[DoorDecider] candidate empty -> filling with Normal");
                continue;
            }

            var chosen = ChooseWeightedRandom(candidate);
            types.Add(chosen);

            if (chosen != RoomType.Normal)
                candidate.Remove(chosen);
        }
    }

    /// <summary>
    /// normalRoomCount만큼 NormalRoomType을 반환합니다. (확률 합 0이면 균등 분포로 폴백)
    /// techSelectPackCount는 선택된 TechSelect의 개수를 out으로 반환합니다.
    /// </summary>
    public NormalRoomType[] GetNormalRoomTypes(int normalRoomCount, out int techSelectPackCount)
    {
        if (normalRoomCount < 0 || normalRoomCount > 3)
            throw new ArgumentOutOfRangeException(nameof(normalRoomCount), "일반방 개수는 0~3개 사이여야 합니다.");

        var result = new List<NormalRoomType>();
        techSelectPackCount = 0;
        if (normalRoomCount == 0) return result.ToArray();

        float totalChance = door_CreditChance + door_HealChance + door_UpgradeChance + door_ChromeChance + door_SkillPackChance;
        bool useUniform = totalChance <= 0f;
        if (useUniform)
            Debug.LogWarning("[DoorDecider] GetNormalRoomTypes: normal-type weights sum to 0, using uniform fallback.");

        for (int i = 0; i < normalRoomCount; i++)
        {
            if (useUniform)
            {
                int idx = UnityEngine.Random.Range(0, 5);
                switch (idx)
                {
                    case 0: result.Add(NormalRoomType.Credit); break;
                    case 1: result.Add(NormalRoomType.Heal); break;
                    case 2: result.Add(NormalRoomType.TechUpgrade); break;
                    case 3: result.Add(NormalRoomType.Chrome); break;
                    default: result.Add(NormalRoomType.TechSelect); techSelectPackCount++; break;
                }
                continue;
            }

            float r = UnityEngine.Random.Range(0f, totalChance);
            if (r < door_CreditChance) result.Add(NormalRoomType.Credit);
            else if (r < door_CreditChance + door_HealChance) result.Add(NormalRoomType.Heal);
            else if (r < door_CreditChance + door_HealChance + door_UpgradeChance) result.Add(NormalRoomType.TechUpgrade);
            else if (r < door_CreditChance + door_HealChance + door_UpgradeChance + door_ChromeChance) result.Add(NormalRoomType.Chrome);
            else { result.Add(NormalRoomType.TechSelect); techSelectPackCount++; }
        }

        return result.ToArray();
    }

    /// <summary>
    /// 안전한 가중 랜덤 선택 유틸리티.
    /// - weights가 비어있으면 예외.
    /// - 모든 가중치 합이 0이면 균등 분포로 대체합니다.
    /// </summary>
    private static T ChooseWeightedRandom<T>(Dictionary<T, float> weights)
    {
        if (weights == null || weights.Count == 0)
            throw new ArgumentException("weights must not be null or empty", nameof(weights));

        float total = 0f;
        foreach (var kv in weights) total += kv.Value;

        Dictionary<T, float> useWeights = weights;
        if (total <= 0f)
        {
            // 균등 분포로 대체
            useWeights = new Dictionary<T, float>();
            float per = 1f / weights.Count;
            foreach (var k in weights.Keys) useWeights[k] = per;
            total = 1f;
        }

        float r = UnityEngine.Random.Range(0f, total);
        float cum = 0f;
        foreach (var kv in useWeights)
        {
            cum += kv.Value;
            if (r <= cum) return kv.Key;
        }

        // 수치적 이유로 여기에 도달하면 마지막 키 반환
        return useWeights.Keys.Last();
    }
}