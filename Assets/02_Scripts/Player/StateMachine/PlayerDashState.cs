using System;
using UnityEngine;

/// <summary>
/// 수정된 PlayerDashState.Enter: 애니메이션 길이를 읽어 Dasher에 duration으로 전달
/// </summary>
public class PlayerDashState : PlayerStateBase
{
    float _dashDuration = 0.5f; // 기본 fallback duration
    float _dashDistance;   // 대시 거리 (예시)

    bool _subscribed = false;

    // 애니메이터에서 사용할 클립 이름 (Animator Controller에서 사용하는 Dash 클립의 정확한 이름)
    // 프로젝트에 따라 "Dash", "Roll", "Dash_Loop" 등 이름을 맞춰주세요.
    const string DashClipName = "RollForward";

    public PlayerDashState(Player player, float dashDuration = 0.5f) : base(player)
    {
        _dashDistance = GameManager.Instance.PlayerStatManager.playerDashDistance*GameManager.Instance.PlayerStatManager.playerDashDistanceMulti;
        GameManager.Instance.PlayerStatManager.OnPlayerDashDistanceMultiChange += SetDashDistance;
        _dashDuration = dashDuration;
    }

    public void SetDashDistance()
    {
        _dashDistance = GameManager.Instance.PlayerStatManager.playerDashDistance * GameManager.Instance.PlayerStatManager.playerDashDistanceMulti;
    }

    public override void Enter()
    {
        var dasher = _player.Dasher;
        if (dasher == null)
        {
            _player.SetToIdle();
            return;
        }

        // 이벤트 구독
        dasher.DashEnded += OnDashEnded;
        _subscribed = true;

        // 대시 방향: 입력 벡터가 있으면 그쪽, 아니면 플레이어 앞 방향
        Vector3 dir3 = _player.MoveInput.sqrMagnitude > 0.01f ? new Vector3(_player.MoveInput.x, 0f, _player.MoveInput.z).normalized : _player.transform.forward;

        // 애니메이션 기반 duration 계산 시도
        float durationToUse = _dashDuration;

        var playerAnimator = _player.Animator;
        if (playerAnimator != null)
        {
            // 런타임에서 클립 길이 읽기
            float clipLength = playerAnimator.GetClipLengthByName(DashClipName);
            clipLength = clipLength * 0.8f; // 약간 조정 (필요시)

            if (clipLength > 0f)
            {
                // 기본적으로 animator.speed를 반영하여 실제 재생 시간을 계산
                float animatorSpeed = 2f;
                //var animComp = playerAnimator.Animator;
                //if (animComp != null) animatorSpeed = Mathf.Max(0.0001f, animComp.speed);

                durationToUse = clipLength / animatorSpeed;

                // 참고: 상태별 speed multiplier(Animator State의 Speed)나 트랜지션 블렌딩을 정확히 반영하려면
                // AnimationEvent를 사용하여 애니메이션 끝에 직접 FinishDashFromAnimation을 호출하는 것이 가장 정확합니다.
            }
        }

        // 요청: 시작 성공 시 대시가 실제로 시작된다
        bool started = dasher.RequestDash(dir3, _dashDistance, durationToUse);

        // 대시 애니메이션 실행 (Animator 트리거/메서드 호출)
        // Animator 내부에서 클립 이름과 트리거가 다르면 OnDash 구현을 맞춰주세요.
        _player.Animator.OnDash();

        if (!started)
        {
            // 실패 시 구독 해제 및 Idle로 복귀
            dasher.DashEnded -= OnDashEnded;
            _subscribed = false;
            _player.SetToIdle();
        }
    }

    public override void Update()
    {
        // 상태 자체는 대시 진행 타이머를 갖지 않음
    }

    public override void Exit()
    {
        var dasher = _player.Dasher;
        if (_subscribed && dasher != null)
        {
            dasher.DashEnded -= OnDashEnded;
            _subscribed = false;
        }

        // 강제 종료 시 Dasher 중단
        if (dasher != null && dasher.IsDashing)
        {
            dasher.Interrupt();
        }
    }

    void OnDashEnded()
    {
        _player.SetToIdle();
    }
}