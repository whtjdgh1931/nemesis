using UnityEngine;

public class PlayerSpecialAttackState : PlayerStateBase
{
    public PlayerSpecialAttackState(Player player) : base(player) { }

    // 상태 진입 시 호출
    public override void Enter()
    {
        // 상태 플래그 설정
        _player.SetIsSpecialAttacking(true);

        // 애니메이션 시작 트리거 (애니 이벤트는 비주얼 동기화용)
        _player.Animator.OnSpecialAttack();

        // SpecialAttacker가 owner(=Player)로 Initialize 되어 있어야 함
        if (_player.SpecialAttacker == null)
        {
            Debug.LogWarning("PlayerSpecialAttackState.Enter: SpecialAttacker가 설정되어 있지 않습니다.");
            return;
        }

        // 차지 시작 (owner.StartCoroutine 내부에서 작업)
        _player.SpecialAttacker.StartCharge();

        // 필요하면 charge 값 업데이트용 이벤트 구독 (UI 갱신 등)
        _player.SpecialAttacker.OnSpecialChargeUpdated += HandleChargeUpdated;

        // 입력 해제(취소/발사)를 상태가 직접 듣도록 하려면 Player 또는 InputHandler에서 이벤트를 제공하고 구독하세요.
        _player.SpecialAttacker.OnSpecialFired += OnReleaseFire;
        // 예: _player.InputHandler.OnSpecialAttackCanceled += HandleInputCanceled;
    }

    // 매 프레임 상태 로직 (필요시)
    public override void Update()
    {
        // 상태 내부에서 특별히 처리할 것이 있다면 구현.
        // 예: 일정 조건으로 자동 풀차지 발사시 state 전환 처리(그러나 보통 attacker가 자동발사를 직접 호출함)
    }

    // 상태 탈출 시 호출
    public override void Exit()
    {
        // 구독 해제
        if (_player.SpecialAttacker != null)
        {
            _player.SpecialAttacker.OnSpecialChargeUpdated -= HandleChargeUpdated;
        }

        // 만약 차지 중이라면 취소(혹은 이미 발사되었으면 아무것도 안 함)
        if (_player.SpecialAttacker != null && _player.SpecialAttacker.IsCharging)
        {
            _player.SpecialAttacker.CancelCharge();
        }

        // 상태 플래그 해제
        _player.SetIsSpecialAttacking(false);

        _player.Animator.OnSpecialAttackEnd();

        // 입력 이벤트 등도 구독 해제 필요
        // 예: _player.InputHandler.OnSpecialAttackCanceled -= HandleInputCanceled;
    }

    // 외부(예: Player가 InputHandler 이벤트를 받아 호출하거나, 애니메이션 이벤트에서 호출)
    // 버튼 해제 시 호출: 차지 중지하고 발사
    public void OnReleaseFire()
    {
        if (_player.SpecialAttacker == null)
        {
            Debug.Log("PlayerSpecialAttackState: OnReleaseFire called but SpecialAttacker is null.");
            return;
        }

        // StopChargeAndFire는 현재 charge ratio로 발사 처리
        _player.SpecialAttacker.StopChargeAndFire();

        // 발사 후 상태 전환: 보통 Idle 또는 FireState로 전환
        _player.SetToIdle();
    }

    // 애니메이션 이벤트로 발사 타이밍을 동기화해야 한다면 애니메이션에서 호출해서 사용
    public void OnSpecialFireAnimEvent()
    {
        // (선택) 발사 이펙트 동기화용. 실제 데미지는 StopChargeAndFire에서 처리하는 게 안전.
    }

    // charge 값 업데이트 받는 콜백(예: UI 갱신)
    void HandleChargeUpdated(float ratio)
    {
        // 예: _player.UpdateSpecialChargeUI(ratio);
    }

    // (선택) 입력 해제 이벤트 핸들러 - Player가 이 상태로 이벤트를 라우팅하면 호출
    void HandleInputCanceled()
    {
        OnReleaseFire();
    }
}