using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BallSecurityRobot : MonsterBase
{

    [Header("Local Stats"), SerializeField]
    private float _explosionRadius = 3f;      // 실제 폭발 범위

    // attackDamage, attackRange, attackDelay, isDead 등은 MonsterBase에서 상속됨

    [Header("Effects"), SerializeField]
    private PoolableObject circlePrefab;     // AttackDecalEffect 프리팹 (Inspector에서 지정)

    [SerializeField] private PoolableObject explosionEffectPrefab; // 폭발 이펙트 프리팹

    private void Update()
    {
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

            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;

            // 자폭 카운트다운 원 생성 (로봇의 자식으로 붙임)
            if (circlePrefab != null)
            {
                GameObject circle = GameManager.Instance.PoolManager.GetFromPool(circlePrefab, transform.position, circlePrefab.transform.rotation);
                AttackDecalEffect effect = circle.GetComponent<AttackDecalEffect>();
                if (effect != null)
                {
                    effect.Play(attackDelay, _explosionRadius);
                }
            }

            // 자폭까지 딜레이
            GameManager.Instance.SoundManager.PlaySfxAt("Monster_Check", transform.position);
            yield return new WaitForSeconds(attackDelay);

            CheckTarget();
            GetEffectFromPool(explosionEffectPrefab, transform.position, Quaternion.identity);
            GameManager.Instance.SoundManager.PlaySfxAt("Monster_Grenade", transform.position);

            Die();
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

    protected override void Die()
    {
        GetEffectFromPool(explosionEffectPrefab, transform.position, Quaternion.identity);
        GameManager.Instance.SoundManager.PlaySfxAt("Monster_Grenade", transform.position);
        base.Die();
    }
}
