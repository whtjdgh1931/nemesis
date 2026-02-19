using System;
using UnityEngine;

public class PlayerNormalAttackState : PlayerStateBase
{
    PlayerNormalAttacker _attacker;

    public PlayerNormalAttackState(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        if (EventBus.IsColosseumRoom)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            _player.Mover.Rotate(cameraForward);
        }

        // 현재 장착한 attacker 가져오기
        _attacker = _player.NormalAttacker;
        if (_attacker == null)
        {
            _player.SetToIdle();
            return;
        }

        // 중복 구독 방지
        _attacker.OnAttackEnded -= HandleAttackEnded;
        _attacker.OnAttackEnded += HandleAttackEnded;

        // 주의: Enter에서 RequestAttack() 호출하지 않음.
        // Input 루트가 이미 RequestAttack을 호출하고 상태 전환을 트리거했어야 함.
        // Enter는 애니메이터 세팅/상태 진입 초기화만 수행.
    }

    public override void Update()
    {
        // 상태 내부에서 추가 로직 필요하면 처리
        // 발사(화살/총알)는 애니메이션 이벤트에서 Player.OnAttackFireEvent -> Attacker.FireNow()로 처리됨
    }

    public override void Exit()
    {
        if (_attacker != null)
        {
            _attacker.OnAttackEnded -= HandleAttackEnded;
            _attacker = null;
        }
        if (_player.NormalAttacker.WeaponType == WeaponType.Rifle)
        {
            _player.Animator.Animator.ResetTrigger("OnNormalAttack");
        }
    }

    void HandleAttackEnded()
    {
        // 상태 내부에서 직접 ChangeState 하지 말고, Player에게 요청하게 하자
        
        _player.SetToIdle();
    }
}