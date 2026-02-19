using System.Collections;
using UnityEngine;

public class Missile : MonsterBase
{

    [Header("Local Stats"), SerializeField]
    private float _explosionRadius = 2f;      // 실제 폭발 범위
    private float lifeTime = 15f;               // 미사일 생존 시간
    private float elapsedTime;

    private bool onPhase2 = false;

    // attackDamage, attackRange, attackDelay, isDead 등은 MonsterBase에서 상속됨

    [Header("Effects"), SerializeField]
    private PoolableObject circlePrefab;     // AttackDecalEffect 프리팹 (Inspector에서 지정)

    [Header("Bullet"), SerializeField]
    private PoolableObject bulletPrefab;      // 총알 프리팹
    private float bulletDamage = 10f;         // 총알 데미지
    private float bulletLifeTime = 5f;        // 총알 생명 시간

    [SerializeField] private PoolableObject explosionEffect;

    public override void Initialize(object data = null)
    {
        base.Initialize(data); // 부모 클래스의 Initialize 호출

        // 미사일 전용 초기화
        if (data is bool phase2)
        {
            onPhase2 = phase2;
        }
        elapsedTime = 0f;
        baseState = MonsterState.Idle;
        _isAttacking = false;
        StopAllCoroutines();
    }


    private void Update()
    {
        if (!isDead)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= lifeTime)
            {
                baseState = MonsterState.Die;
                Die();
                return;
            }
        }
        if (isDead || _target == null) return;
        if (isStunned) return;

        switch (baseState)
        {
            case MonsterState.Idle:
                HandleIdle();
                break;

            case MonsterState.Move:
                HandleMove();
                break;

            case MonsterState.Attack:
                if (!_isAttacking)
                    StartCoroutine(SelfDestructionAttack());
                break;

            case MonsterState.Die:
                Die();
                break;
        }
    }

    private void HandleIdle()
    {
        float distance = Vector3.Distance(transform.position, _target.position);

        if (distance <= detectionRange)
        {
            baseState = MonsterState.Move;
        }
    }

    private void HandleMove()
    {
        float distance = Vector3.Distance(transform.position, _target.position);

        if (distance > detectionRange)
        {
            agent.ResetPath();
            baseState = MonsterState.Idle;
            return;
        }

        agent.SetDestination(_target.position);

        if (distance <= attackRange && !_isAttacking)
        {
            baseState = MonsterState.Attack;
        }
    }

    private IEnumerator SelfDestructionAttack()
    {
        if (_target != null && !_isAttacking)
        {
            _isAttacking = true;

            // 자폭 카운트다운 원 생성
            if (circlePrefab != null)
            {
                GameObject circle = GameManager.Instance.PoolManager.GetFromPool(circlePrefab, transform.position, circlePrefab.transform.rotation);
                AttackDecalEffect effect = circle.GetComponent<AttackDecalEffect>();
                if (effect != null)
                {
                    effect.Play(0.1f, _explosionRadius);
                }
            }

            yield return new WaitForSeconds(0.1f);

            // 폭발 이펙트 재생 (자기 위치에서)
            if (explosionEffect != null)
            {
                GetEffectFromPool(explosionEffect, transform.position, Quaternion.identity);
            }

            CheckTarget();

            baseState = MonsterState.Die;
        }
    }

    public void CheckTarget()
    {
        // 콜라이더 탐색
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.tag == targetTag)
            {
                collider.GetComponent<IDamageable>().TakeDamage(attackDamage, transform);
            }
        }
    }

    private void DieAttack()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnBulletAtAngle(i * 120f);
        }
    }

    private void SpawnBulletAtAngle(float angle)
    {
        Quaternion bulletRotation = Quaternion.Euler(0, angle, 0);
        GameObject bullet = GameManager.Instance.PoolManager.GetFromPool(bulletPrefab, transform.position, bulletRotation);
        TurretBullet turretBullet = bullet.GetComponent<TurretBullet>();
        if (turretBullet != null)
        {
            turretBullet.Initialize(targetTag, bulletDamage, bulletLifeTime, gameObject); // 생명 시간 5초로 설정
        }
    }

    /// <summary>
    /// 외부에서 호출하여 몬스터를 즉시 죽이는 메서드
    /// </summary>
    public void SetDie()
    {
        baseState = MonsterState.Die;
        Die();
    }



    protected override void Die()
    {
        if (onPhase2)
        {
            DieAttack();
        }
        GameManager.Instance.SoundManager.PlaySfxAt("Monster_Grenade", transform.position);
        base.Die();
    }
}