using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 무기타입이 총일 때의 일반 공격을 담당하는 클래스
/// </summary>
[DisallowMultipleComponent]
public class PlayerRifleNormalAttacker : PlayerNormalAttacker
{
    public override WeaponType WeaponType => WeaponType.Rifle;
    [Header("Rifle Settings")]
    [SerializeField] float fallbackEnd = 0.2f;

    [Header("Bullet")]
    [SerializeField] Transform _firePoint;

    // 이벤트는 base 클래스의 것을 그대로 사용합니다. (삭제)
    // public override event Action OnAttackStarted; // 제거

    Coroutine _endRoutine;

    // Initialize로 firePoint 주입 가능
    public void Initialize(Player player, Transform firePoint, PoolableObject bulletPrefab = null)
    {
        _player = player;
        if (firePoint != null) _firePoint = firePoint;
    }

    // Attack은 애니메이션 트리거만 담당. 상태/이벤트는 base.StartAttack()이 처리해야 중복 호출을 피할 수 있습니다.
    public override void Attack()
    {
        if (_player == null)
        {
            Debug.LogWarning("PlayerRifleNormalAttacker.Attack: _player is null.");
            return;
        }

        // Animator가 null일 수 있으니 안전 호출
        _player.Animator?.OnNormalAttack();

        // 기존 코루틴 정리
        if (_endRoutine != null)
        {
            // StopCoroutine을 호출할 때는 코루틴을 시작한 객체에서 멈춰야 함.
            _player.StopCoroutine(_endRoutine);
            _endRoutine = null;
        }
        // 코루틴은 활성화가 보장되는 _player에서 시작
        _endRoutine = _player.StartCoroutine(EndAfterDelayCoroutine());
    }

    IEnumerator EndAfterDelayCoroutine()
    {
        yield return new WaitForSeconds(fallbackEnd);
        EndAttack();
    }

    // 애니메이션 이벤트에서 호출: 실제 발사 실행
    public void FireNow()
    {
        if (_firePoint == null)
        {
            Debug.LogWarning("PlayerRifleNormalAttacker.FireNow: _firePoint is null.");
            return; // 반드시 조기 리턴
        }

        const string bulletPoolKey = "Prefabs/Bullet/Bullet";
        const string bulletEffectKey = "Effect/BulletEffect";

        GameObject obj = null;
        try
        {
            obj = GameManager.Instance.PoolManager.GetFromPool(bulletPoolKey, _firePoint.position, _firePoint.rotation);
        }
        catch (Exception e)
        {
            Debug.LogError($"PlayerRifleNormalAttacker.FireNow: PoolManager.GetFromPool threw: {e}");
            return;
        }

        if (obj == null)
        {
            Debug.LogWarning($"PlayerRifleNormalAttacker.FireNow: failed to get bullet from pool '{bulletPoolKey}'.");
            return;
        }

        // 총구 이펙트 (있다면)
        GameObject bulletEffect = null;
        try
        {
            bulletEffect = GameManager.Instance.PoolManager.GetFromPool(bulletEffectKey, _firePoint.position, _firePoint.rotation);
        }
        catch
        {
            // 이펙트 실패는 치명적이지 않으니 경고만
            Debug.LogWarning($"PlayerRifleNormalAttacker.FireNow: failed to get effect from pool '{bulletEffectKey}'.");
        }

        // 사운드 재생(사운드 매니저가 null일 경우 체크)
        GameManager.Instance.SoundManager?.PlaySfxAt("RifleShootSFX", _firePoint.position, false, 1, 1);

        var bullet = obj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetMoveDir(_firePoint.forward);
            // 가능하면 소유자/데미지 등도 설정:
            // if (bullet is IOwnedBullet ob) ob.SetOwner(_player);
        }
        else
        {
            Debug.LogWarning("PlayerRifleNormalAttacker.FireNow: spawned object does not have Bullet component.");
        }
    }

    public override void EndAttack()
    {
        if (_endRoutine != null && _player != null)
        {
            _player.StopCoroutine(_endRoutine);
            _endRoutine = null;
        }
        base.EndAttack();
    }
}