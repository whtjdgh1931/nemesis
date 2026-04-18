using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 게임의 다양한 이벤트들의 허브 역할
/// </summary>
public static class EventBus
{
    /// <summary>
    /// 몬스터 적중시 이벤트 <무기 타입,공격 타입, 적 트랜스폼, 공격자 트랜스폼(플레이어, 총알 드론 등)
    /// </summary>
    public static event Action<WeaponType, ATTACKTYPE, Transform, Transform> OnMonsterHit;
    public static event Action<int> OnFailBuy;

    public static void MonsterHit(WeaponType weaponType, ATTACKTYPE attackType, Transform monster, Transform attacker)
    {
        OnMonsterHit?.Invoke(weaponType, attackType, monster, attacker);
    }

    /// <summary>
    /// 방 2개 넘어갈 시 진화 스킬 발동을 위한 이벤트
    /// </summary>
    public static event Action OnEvolution;

    public static void Evolution()
    {
        OnEvolution?.Invoke();
    }

    public static void FailBuy(int price)
    {
        OnFailBuy?.Invoke(price);
    }

    /// <summary>
    /// 몬스터 넉백 시 발생하는 이벤트
    /// </summary>
    public static event Action<Vector3> OnMonsterKnockBack;

    public static void MonsterKnockBack(Vector3 monsterPosition)
    {
        OnMonsterKnockBack?.Invoke(monsterPosition);
    }

    /// <summary>
    /// 유탄 폭발 이벤트
    /// </summary>
    public static event Action<Vector3> OnGrenadeBomb;
    public static void GrenadeBomb(Vector3 bombPosition)
    {
        OnGrenadeBomb?.Invoke(bombPosition);
    }

    public static bool IsColosseumRoom = false;
    public static event Action<bool> IsColosseumChanged;
    public static void SetColosseumRoom(bool isColosseum)
    {
        IsColosseumRoom = isColosseum;
        IsColosseumChanged?.Invoke(isColosseum);
    }

    public static event Action OnBossDead;
    public static void BossDead()
    {
        OnBossDead?.Invoke();
    }

    public static MonsterBase EliteBoss { get; set; }

    public static bool IsRewardSelecting = false;
    public static void SetIsRewardSelecting(bool isSelecting)
    {
        IsRewardSelecting = isSelecting;
    }

    public static bool HasMutant1 { get; set; } // 유탄 1
    public static bool HasMutant2 { get; set; } // 유탄 2
    public static bool HasMutant3 { get; set; } // 일반공격 유도
    public static bool HasMutant4 { get; set; } // 특수공격 에너지구체

    // --- 몬스터 리스트 관리 (null-safe, 캡슐화) ---
    // 외부에서 직접 리스트를 교체하지 못하도록 internal로 초기화하고, 추가/제거 API 제공
    private static readonly List<MonsterBase> _currentMonsterList = new List<MonsterBase>();
    public static IReadOnlyList<MonsterBase> CurrentMonsterList => _currentMonsterList.AsReadOnly();

    public static void AddMonster(MonsterBase m)
    {
        if (m == null) return;
        if (!_currentMonsterList.Contains(m))
            _currentMonsterList.Add(m);
    }

    public static void RemoveMonster(MonsterBase m)
    {
        if (m == null) return;
        _currentMonsterList.Remove(m);
    }

    public static void ClearMonsters()
    {
        _currentMonsterList.Clear();
    }

    public static Transform GetNearestMonsterFromMe(Transform player)
    {
        if (CurrentMonsterList == null || CurrentMonsterList.Count == 0)
        {
            return null;
        }
        MonsterBase nearestMonster = null;
        float nearestDistance = float.MaxValue;
        foreach (var monster in CurrentMonsterList)
        {
            float distance = Vector3.Distance(player.position, monster.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestMonster = monster;
            }
        }
        return nearestMonster != null ? nearestMonster.transform : null;
    }

    static bool _canGetInput = true;
    public static bool CanGetInput => _canGetInput;
    public static void SetCanGetInput(bool canGetInput)
    {
        _canGetInput = canGetInput;
    }

    static bool _canTimerun = true;
    public static bool CanTimeRun => _canTimerun;
    public static void SetCanTimeRun(bool canTimeRun)
    {
        _canTimerun = canTimeRun;
    }


    public static void ResetEvent()
    {
        OnMonsterHit = null;
        HasMutant1 = false;
        HasMutant2 = false;
        HasMutant3 = false;
        HasMutant4 = false;
        OnEvolution = null;
        OnMonsterKnockBack = null;
        OnGrenadeBomb = null;
        _canGetInput = true;
        IsRewardSelecting = false;
        _canTimerun = true;
        IsColosseumRoom = false;
        ClearMonsters();

		}

    public static bool bIsRoomReady = false;
}



