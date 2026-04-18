
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 보상형 상호작용의 기본 구현 (IInteractable의 TryInteract 패턴에 맞춤)
/// TryInteract은 즉시 시작 성공 여부를 반환한다 (true 시작, false 거부)
/// </summary>
public abstract class RewardInteractableObject : InteractableObject
{
    protected Coroutine _rewardCoroutine;
    protected Transform _player;
    bool _isInteracting = false;

    public override InteractableType InteractableType => InteractableType.Reward;

    public abstract event Action OnRewardGiven;
    public override event Action<IInteractable> OnInteracted;

    public virtual void Initialize()
    {
        // 기본 구현 없음, 파생 클래스에서 필요시 오버라이드
    }

    // TryInteract은 호출자에게 상호작용이 시작되었는지 알려주는 동기적 bool 반환
    public override bool TryInteract(Transform subject)
    {
        // 이미 상호작용 중이면 거부
        if (_isInteracting) return false;

        _isInteracting = true;
        _player = subject;

        // 보상 실행 코루틴 시작
        _rewardCoroutine = StartCoroutine(RewardCoroutineWrapper());

        // 감지/프롬프트 시스템에 상호작용 시작 알림
        OnInteracted?.Invoke(this);

        return true;
    }

    // 파생 클래스는 실제 보상 로직을 구현 (UI 열기, 선택 대기, 연출 등)
    protected abstract IEnumerator RewardCoroutine();

    // 내부 래퍼: 코루틴 끝나면 정리
    IEnumerator RewardCoroutineWrapper()
    {
        yield return StartCoroutine(RewardCoroutine());
        // 코루틴 완료 시 정리 (보상은 RewardCoroutine 내부 또는 이벤트에서 적용)
        EndInteract();
    }

    // 공통 정리: 코루틴 중지, 상태 리셋
    protected virtual void EndInteract()
    {
        if (_rewardCoroutine != null)
        {
            StopCoroutine(_rewardCoroutine);
            _rewardCoroutine = null;
        }
        
        _player = null;
        _isInteracting = false;
    }

    protected virtual void OnDisable()
    {
        EndInteract();
    }
}