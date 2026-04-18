using System.Collections;
using UnityEngine;

public class NebulaVanguard : MonsterBase
{
    [Header("Local Stats")]
    [SerializeField] private float _box_Length = 4;
    [SerializeField] private float _box_Height = 3;
    [SerializeField] private float _box_Width = 3;

    private void Update()
    {
        if (isDead || _target == null || baseState == MonsterState.Die) return;
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
        // 애니메이션: Idle 상태에서는 이동 중지
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool(IsMove_Hash, false);
        }

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
        float distance = Vector3.Distance(transform.position, _target.position);
        _isAttacking = true;

        if (_target != null && distance <= attackRange)
        {
            // 애니메이션: 공격 트리거 발동
            if (monsterAnimator != null)
            {
                monsterAnimator.SetTrigger(Attack_Hash);
            }
            GameManager.Instance.SoundManager.PlaySfxAt("Monster_Slash", transform.position);

            // 몬스터 기준 중심 위치 설정
            Vector3 center = transform.position + transform.forward * (_box_Length / 2f);
            // 박스의 반 크기
            Vector3 halfExtents = new Vector3(_box_Width / 2f, _box_Height / 2f, _box_Length / 2f);
            // 박스의 회전 (몬스터 정면을 기준으로 정렬)
            Quaternion orientation = Quaternion.LookRotation(transform.forward);
            // 박스 영역 안의 적 탐색
            Collider[] hitTarget = Physics.OverlapBox(center, halfExtents, orientation);

            foreach (Collider target in hitTarget)
            {
                if (target.TryGetComponent(out IDamageable playerHealth) && target.tag == targetTag)
                {
                    yield return new WaitForSeconds(0.7f);
                    float finalPlayerDistance = Vector3.Distance(transform.position, base._target.position);
                    if (finalPlayerDistance <= attackRange)
                    {
                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage(attackDamage, transform);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(attackDelay);
        }

        _isAttacking = false;
        baseState = MonsterState.Move; // 공격 후 다시 추격 상태로 전환
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + transform.forward * (_box_Length / 2f);
        Vector3 halfExtents = new Vector3(_box_Width / 2f, _box_Height / 2f, _box_Length / 2f);
        Quaternion orientation = Quaternion.LookRotation(transform.forward);

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(center, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }
}