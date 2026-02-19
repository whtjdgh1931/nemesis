using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 대시 동작을 관리하는 컴포넌트(대시 상태의 단일 소유자).
/// - IsDashing 플래그 소유
/// - RequestDash(...) 로 대시 요청 (중복 요청 방지)
/// - DashStarted / DashEnded 이벤트로 외부에 상태 변경 통지
/// - Interrupt() 로 외부에서 대시 강제 종료 가능
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerDasher : MonoBehaviour
{
    [SerializeField] bool _isDashing;
    public bool IsDashing => _isDashing;

    CharacterController _cc;
    Coroutine _dashCoroutine;

    public event Action DashStarted;
    public event Action DashEnded;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    public void Initialize(CharacterController cc)
    {
        _cc = cc ?? _cc;
    }

    /// <summary>
    /// 대시를 요청한다. 이미 대시 중이면 false 반환.
    /// 성공하면 내부적으로 대시를 시작하고 DashStarted/DashEnded 이벤트를 발생시킨다.
    /// </summary>
    public bool RequestDash(Vector3 direction, float distance, float duration)
    {
        if (_isDashing) return false;
        if (duration <= 0f || distance <= 0f) return false;

        // 기존 코루틴이 남아있다면 정리
        if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);
        _dashCoroutine = StartCoroutine(DashRoutine(direction.normalized, distance, duration));
        return true;
    }

    /// <summary>
    /// 외부에서 대시를 강제 중단할 때 호출.
    /// </summary>
    public void Interrupt()
    {
        if (!_isDashing) return;
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }

        // 정리
        _isDashing = false;
        DashEnded?.Invoke();
    }

    IEnumerator DashRoutine(Vector3 direction, float distance, float duration)
    {
        float elapsed = 0f;
        float speed = distance / duration;
        _isDashing = true;
        DashStarted?.Invoke();

        while (elapsed < duration)
        {
            float dt = Time.deltaTime;
            float step = speed * dt;

            // 충돌 사전 검사
            float allowed = GetAllowedDistance(direction, step);
            if (allowed <= 0f)
            {
                // 충돌로 중단
                break;
            }

            CollisionFlags flags = _cc.Move(direction * allowed);
            if ((flags & CollisionFlags.Sides) != 0)
            {
                // 벽에 닿음 -> 중단
                break;
            }

            elapsed += dt;
            yield return null;
        }

        // 끝났거나 중단된 경우 정리
        _isDashing = false;
        _dashCoroutine = null;
        DashEnded?.Invoke();
    }

    float GetAllowedDistance(Vector3 dir, float desiredDistance)
    {
        if (_cc == null) return desiredDistance;

        Vector3 center = _cc.transform.position + _cc.center;
        float halfHeight = Mathf.Max(0f, _cc.height * 0.5f - _cc.radius);
        Vector3 top = center + Vector3.up * halfHeight;
        Vector3 bottom = center - Vector3.up * halfHeight;
        float castDistance = desiredDistance + _cc.skinWidth;

        if (Physics.CapsuleCast(bottom, top, _cc.radius, dir, out RaycastHit hit, castDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            float allowed = Mathf.Max(0f, hit.distance - _cc.skinWidth);
            return Mathf.Min(allowed, desiredDistance);
        }
        return desiredDistance;
    }

    void OnDisable()
    {
        // 비활성화 시 안전하게 정리 및 이벤트 통지
        if (_isDashing)
        {
            _isDashing = false;
            if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
            DashEnded?.Invoke();
        }
    }
}