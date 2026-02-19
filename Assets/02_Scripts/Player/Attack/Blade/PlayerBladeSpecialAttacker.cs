using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 검기 관련 로직을 제거하고 "가장 가까운 적에게 순간이동" 기능만 남긴 구현.
/// - 차지 시간과 무관하게 몬스터 전방 2f 위치로 순간이동
/// - 순간이동 시 높이(y)는 0으로 고정
/// - NavMeshAgent 관련 로직 제거하고 CharacterController 사용
/// - 가장 가까운 적이 없으면 아무 동작도 하지 않음
/// </summary>
public class PlayerBladeSpecialAttacker : PlayerSpecialAttacker
{
    [Header("Charge Settings")]
    [SerializeField] float maxChargeTime = 2.0f;        // 완전 차지까지 시간(초)

    [Header("Teleport Tuning")]
    [SerializeField] float teleportForwardDistance = 2.0f; // 몬스터 전방으로부터의 거리 (고정)
    [SerializeField] float teleportHeight = 0f;            // 순간이동 시 높이 (고정 0)

    Coroutine _chargeCoroutine;
    float _currentChargeRatio = 0f; // 남겨두긴 했지만 현재 teleport에는 사용되지 않음

    bool _didTeleportThisUse = false;

    // 이벤트 오버라이드
    public override event Action OnSpecialStarted;
    public override event Action<float> OnSpecialChargeUpdated; // ratio 0..1
    public override event Action OnSpecialFired;
    public override event Action OnSpecialEnded;
    public override event Action OnSpecialCancelled;

    public override WeaponType WeaponType => WeaponType.Blade;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
    }

    public override bool RequestSpecial()
    {
        return base.RequestSpecial();
    }

    public override void StartCharge()
    {
        if (_player == null)
        {
            Debug.LogWarning("PlayerBladeSpecialAttacker: owner is null. Call Initialize first.");
        }

        if (IsCharging || IsActive) return;

        IsCharging = true;
        _didTeleportThisUse = false;
        OnSpecialStarted?.Invoke();

        if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
        _chargeCoroutine = StartCoroutine(ChargeRoutine());
    }

    public override void StopChargeAndFire()
    {
        if (!IsCharging) return;

        IsCharging = false;
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }

        Fire();

        // (사용자가 전에는 항상 호출하도록 변경해둔 상태를 유지)
        OnSpecialFired?.Invoke();

        EndSpecial();
    }

    public override void CancelCharge()
    {
        if (!IsCharging) return;

        IsCharging = false;
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }

        OnSpecialCancelled?.Invoke();
        EndSpecial();
    }

    protected override void Fire()
    {
        if (_player == null)
        {
            Debug.LogWarning("PlayerBladeSpecialAttacker: player is null. Call Initialize first.");
            return;
        }

        Transform nearest = EventBus.GetNearestMonsterFromMe(_player.transform);

        if (nearest == null)
        {
            _didTeleportThisUse = false;
            return;
        }

        // 차지 시간과 무관하게 몬스터 전방 2f, 높이 0으로 고정
        Vector3 teleportPos = nearest.position + nearest.forward * teleportForwardDistance;
        teleportPos.y = teleportHeight;

        // CharacterController 사용(존재하면 enable 토글 후 설정)
        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            _player.transform.position = teleportPos;
            cc.enabled = true;
        }
        else
        {
            // CharacterController가 없으면 transform 직접 세팅
            _player.transform.position = teleportPos;
        }

        // 몬스터를 바라보게 회전 (y 고정)
        Vector3 lookDir = (nearest.position - _player.transform.position);
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
            _player.transform.rotation = Quaternion.LookRotation(lookDir);

        _didTeleportThisUse = true;
    }

    IEnumerator ChargeRoutine()
    {
        float elapsed = 0f;
        _currentChargeRatio = 0f;
        RaiseChargeUpdated(_currentChargeRatio);

        while (IsCharging)
        {
            elapsed += Time.deltaTime;
            _currentChargeRatio = Mathf.Clamp01(elapsed / maxChargeTime);
            RaiseChargeUpdated(_currentChargeRatio);

            if (elapsed >= maxChargeTime)
            {
                IsCharging = false;
                _chargeCoroutine = null;

                Fire();

                if (_didTeleportThisUse)
                {
                    OnSpecialFired?.Invoke();
                }

                EndSpecial();
                yield break;
            }

            yield return null;
        }
    }

    protected override void EndSpecial()
    {
        base.EndSpecial();

        _currentChargeRatio = 0f;
        _didTeleportThisUse = false;
        RaiseChargeUpdated(_currentChargeRatio);
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }

        OnSpecialEnded?.Invoke();
    }
}