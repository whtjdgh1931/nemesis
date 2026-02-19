using System;
using UnityEngine;

/// <summary>
/// 플레이어의 특수공격을 담당하는 추상 클래스
/// - 무기타입별로 상속하여 구현
/// - Initialize(owner)을 통해 owner를 주입받아 안전하게 코루틴을 실행하도록 한다.
/// - 기본적으로 차지형/즉시형 모두를 지원할 수 있는 API 제공
/// </summary>
public abstract class PlayerSpecialAttacker : MonoBehaviour
{
    public abstract WeaponType WeaponType { get; }

    // 생명주기/상태 이벤트
    public virtual event Action OnSpecialStarted;
    public virtual event Action<float> OnSpecialChargeUpdated; // ratio 0..1
    public virtual event Action OnSpecialFired;
    public virtual event Action OnSpecialEnded;
    public virtual event Action OnSpecialCancelled;

    // 이벤트 레이즈 함수
    protected virtual void RaiseChargeUpdated(float ratio)
    {
        OnSpecialChargeUpdated?.Invoke(ratio);
    }

    protected virtual void OnSpecialFiredInvoke()
    {
        OnSpecialFired?.Invoke();
    }

    protected Player _player;

    // 상태 플래그
    public bool IsCharging { get; protected set; }
    public bool IsActive { get; protected set; } // 공격 활성(발사중 등)

    // owner 주입 (반드시 호출)
    public virtual void Initialize(Player player)
    {
        _player = player;
        OnSpecialChargeUpdated += GameManager.Instance.PlayerStatManager.SetPlayerRifleChargeRatio;
    }

    // 외부에서 호출하는 진입점: 큐잉/쿨타임 로직을 파생클래스에서 구현 가능
    public virtual bool RequestSpecial()
    {
        if (IsActive || IsCharging) return false;
        // 기본 동작: 바로 시작 가능
        return true;
    }

    // 차지 시작 (Input started에서 호출)
    public virtual void StartCharge()
    {
        if (_player == null) Debug.LogWarning("PlayerSpecialAttacker: owner is null. Call Initialize first.");
        if (IsCharging || IsActive) return;
        IsCharging = true;
        OnSpecialStarted?.Invoke();
    }

    // 차지 중지(버튼 해제)하고 발사
    public virtual void StopChargeAndFire()
    {
        if (!IsCharging) return;
        IsCharging = false;
        Fire();
        OnSpecialFired?.Invoke();
        EndSpecial(); // 기본적으로 발사 후 종료
    }

    // 강제 취소(피격 등)
    public virtual void CancelCharge()
    {
        if (!IsCharging) return;
        IsCharging = false;
        OnSpecialCancelled?.Invoke();
        EndSpecial();
    }

    // 실제 발사 로직은 파생 클래스에서 구현
    protected abstract void Fire();

    // 특수 동작 종료(정리)
    protected virtual void EndSpecial()
    {
        IsActive = false;
        OnSpecialEnded?.Invoke();
    }
}