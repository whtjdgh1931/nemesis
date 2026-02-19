using System.Collections;
using UnityEngine;

public class NebulaPhantom : MonsterBase
{
    [Header("Local Stats")]
    [SerializeField] private float aimingDelay; // 조준 시간(공격 전 대기 시간)

    [Header("Laser")]
    [SerializeField] private PoolableObject SquareDecalPrefab;

    private void Update()
    {
        if (isDead || _target == null) return;
        if (isStunned) return;

        LookAtPlayer();
        StartCoroutine(HidingFunction());

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
                {
                    StartCoroutine(PerformAttack());
                }
                break;
            case MonsterState.Die:
                Die();
                break;
        }
    }


    private void HandleIdle()
    {
        // 플레이어와 거리
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance <= detectionRange && CanSeePlayer())
        {
            baseState = MonsterState.Move;
        }
    }

    private void HandleMove()
    {
        if (_target == null) return;

        float distance = Vector3.Distance(transform.position, _target.position);

        if (distance > detectionRange || !CanSeePlayer())
        {
            agent.ResetPath();

            // 애니메이션: 이동 중지
            if (monsterAnimator != null)
            {
                monsterAnimator.SetBool(IsMove_Hash, false);
            }

            baseState = MonsterState.Idle;
            return;
        }

        agent.SetDestination(_target.position);

        // 애니메이션: 이동 중
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool(IsMove_Hash, true);
        }

        if (distance <= attackRange && CanSeePlayer())
        {
            agent.ResetPath();

            // 애니메이션: 공격 준비를 위해 이동 중지
            if (monsterAnimator != null)
            {
                monsterAnimator.SetBool(IsMove_Hash, false);
            }

            baseState = MonsterState.Attack;
        }
    }

    private IEnumerator PerformAttack()
    {
        _isAttacking = true;

        if (_target != null && Vector3.Distance(transform.position, _target.position) <= attackRange)
        {
            Vector3 spawnPos = transform.position + transform.forward * 40;
            spawnPos.y = 0;
            GameObject decalobj = GameManager.Instance.PoolManager.GetFromPool(SquareDecalPrefab, spawnPos, SquareDecalPrefab.transform.rotation);

            decalobj.GetComponent<SquareDecalEffect>().Play(attackDelay, transform, new Vector3(90, 0, 0));
            yield return new WaitForSeconds(attackDelay);
            monsterAnimator.SetTrigger(Attack_Hash);
            GameManager.Instance.SoundManager.PlaySfxAt("Monster_Laser", transform.position);

            float laserLength = 40f; // 사거리

            // 레이저 시작 위치를 플레이어 높이에 맞춤
            Vector3 startPos = transform.position + transform.forward * 0.5f;
            startPos.y = _target.position.y + 0.5f;

            // 벽에 막히는 Raycast
            if (Physics.Raycast(startPos, transform.forward, out RaycastHit hit, laserLength, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.CompareTag(targetTag) && !isDead)
                {
                    var damageable = hit.collider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(attackDamage, transform);
                    }
                }
            }
            yield return new WaitForSeconds(attackDelay / 2);
        }

        _isAttacking = false;
        baseState = MonsterState.Move; // 공격 후 다시 추격 상태로 전환
    }

    private IEnumerator HidingFunction()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        float hidingTimer = 0f;
        hidingTimer += Time.deltaTime;
        if (hidingTimer >= 10f)
        {
            renderer.enabled = false;
            yield return new WaitForSeconds(1f);
            renderer.enabled = true;
        }
    }
}
