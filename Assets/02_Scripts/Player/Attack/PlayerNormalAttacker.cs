using System;
using UnityEngine;

/// <summary>
/// 플레이어의 일반공격을 구현하는 추상 클래스 (MonoBehaviour 기반)
/// - 기존의 abstract Attack()은 유지하되, RequestAttack()을 추가하여
///   Player가 안전하게 호출할 수 있도록 함.
/// </summary>
public abstract class PlayerNormalAttacker : MonoBehaviour, IAttacker
{
    [SerializeField] protected Player _player;
    [SerializeField] protected bool _isAttacking = false;
    public abstract WeaponType WeaponType { get; }

    public virtual event Action OnAttackStarted;
    public virtual event Action OnAttackEnded;

    // 공격 중인지 플레이어 상태머신에서 확인용
    public bool IsAttacking => _isAttacking;

    void Awake()
    {
        _player = GetComponentInParent<Player>();
    }

    /// <summary>
    /// Player가 호출하는 진입점.
    /// 기본 구현은 CanStartAttack() 검사 후 Attack()을 호출하고 true 반환.
    /// 파생 클래스는 필요하면 RequestAttack을 오버라이드하여 큐잉/쿨타임등을 처리.
    /// </summary>
    public virtual bool RequestAttack()
    {
        if (!CanStartAttack()) return false;
        StartAttack();
        return true;
    }

    // 내부 시작 절차 (파생 클래스는 Attack()을 구현)
    protected virtual void StartAttack()
    {
        _isAttacking = true;

        Attack();
        OnAttackStarted?.Invoke();
    }

    // 파생 클래스가 구현: 애니메이션 트리거, 발사, 콤보 로직 등
    public abstract void Attack();

    /// <summary>
    /// 공격을 종료할때 호출(애니메이션 이벤트)
    /// </summary>
    public virtual void EndAttack()
    {
        if (!_isAttacking) return;
        _isAttacking = false;
        OnAttackEnded?.Invoke();
    }

    // 기본 조건: 현재 공격 중이 아니면 시작 가능
    protected virtual bool CanStartAttack() => !_isAttacking;
}