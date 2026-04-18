using System;

/// <summary>
/// 공격 컴포넌트 공통 인터페이스
/// - RequestAttack: 입력으로 시도
/// - EndAttack: 외부(피격/인터럽트)에서 강제 중단
/// - OnAnimationFire/OnAnimationEnd: 애니메이션 이벤트 진입점
/// - 이벤트로 시작/종료 통지
/// </summary>
public interface IAttacker
{
    WeaponType WeaponType { get; }
    bool IsAttacking { get; }
    bool RequestAttack();
    void EndAttack();

    event Action OnAttackStarted;
    event Action OnAttackEnded;
}