using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 enum 기반 상태 머신.
/// - PlayerStateType -> IState 인스턴스 맵을 보유하고 전환/업데이트를 담당.
/// - TransitionGuard 훅을 통해 전환 허용 조건을 중앙에서 검사할 수 있음.
/// - ChangeState는 enum 기반 overload를 권장(맵에 없는 상태 전환 방지).
/// </summary>
public class StateMachine
{
    readonly Dictionary<PlayerStateType, IState> _states = new Dictionary<PlayerStateType, IState>();
    IState _current;
    PlayerStateType _currentType;

    // 전환 허용 여부 검사 훅: (from, to) => bool. null이면 항상 허용.
    public Func<PlayerStateType, PlayerStateType, bool> TransitionGuard;

    public IState Current => _current;
    public PlayerStateType CurrentType => _currentType;
    public IReadOnlyDictionary<PlayerStateType, IState> States => _states;

    // 생성자: Player 인스턴스를 받아서 상태 인스턴스를 미리 생성(캐싱)
    public StateMachine(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));

        _states[PlayerStateType.Idle] = new PlayerIdleState(player);
        _states[PlayerStateType.Move] = new PlayerMoveState(player);
        _states[PlayerStateType.Dash] = new PlayerDashState(player);
        _states[PlayerStateType.NormalAttack] = new PlayerNormalAttackState(player);
        _states[PlayerStateType.GrenadeAttack] = new PlayerGrenadeAttackState(player);
        _states[PlayerStateType.SpecialAttack] = new PlayerSpecialAttackState(player);

        // 초기 상태 설정 및 Enter 호출
        _currentType = PlayerStateType.Idle;
        _current = _states[_currentType];
        _current?.Enter();
    }

    // enum 기반 안전한 전환 API (권장)
    public bool ChangeState(PlayerStateType nextType)
    {
        if (!_states.ContainsKey(nextType))
        {
            Debug.LogWarning($"StateMachine.ChangeState: state {nextType} not found.");
            return false;
        }

        if( _currentType == nextType)
            return false; // 이미 해당 상태인 경우 무시

        // 전환 가능 여부 검사(있으면)
        if (TransitionGuard != null && !TransitionGuard(_currentType, nextType))
            return false;

        _current?.Exit();
        _current = _states[nextType];
        _currentType = nextType;
        _current?.Enter();
        return true;
    }

    // 전환 시도 후 성공/실패 반환
    public bool TryChangeState(PlayerStateType nextType) => ChangeState(nextType);

    // 강제 전환 (가드 무시)
    public void ForceChangeState(PlayerStateType nextType)
    {
        if (!_states.ContainsKey(nextType))
        {
            Debug.LogWarning($"StateMachine.ForceChangeState: state {nextType} not found.");
            return;
        }

        _current?.Exit();
        _current = _states[nextType];
        _currentType = nextType;
        _current?.Enter();
    }

    // 매프레임 현재 상태 업데이트 호출
    public void Update() => _current?.Update();

    // 상태 맵에서 IState 얻기 (필요 시)
    public IState GetState(PlayerStateType type)
    {
        _states.TryGetValue(type, out var s);
        return s;
    }
}