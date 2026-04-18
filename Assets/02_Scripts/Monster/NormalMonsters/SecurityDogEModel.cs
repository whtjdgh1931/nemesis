using System.Collections;
using UnityEngine;

public class SecurityDogEModel : MonsterBase
{
    [Header("Local Stats")]
    [SerializeField] private float jumpPrepareTime = 0.5f;
    [SerializeField] private float jumpForce = 100f;
    [SerializeField] private float jumpCoolTime = 5f;
    [SerializeField] private float currentCoolTime = 0f;

    private float fixedYPosition;
    private Quaternion fixedRotation;
    private Coroutine attackCoroutine; // 공격 코루틴 참조 저장


    private void Update()
    {
        if (isDead || _target == null) return;
        if (isStunned) return;

        currentCoolTime += Time.deltaTime;

        if (CanSeePlayer())
        {
            LookAtPlayer();
        }

        switch (baseState)
        {
            case MonsterState.Idle:
                HandleIdle();
                break;
            case MonsterState.Move:
                HandleMove();
                break;
            case MonsterState.Attack:
                HandleAttack();
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
        if (_target == null || _isAttacking) return;

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

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(_target.position);
        }

        // 애니메이션: 이동 중
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool(IsMove_Hash, true);
        }

        if (distance <= attackRange && CanSeePlayer())
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
            // 애니메이션: 공격 준비를 위해 이동 중지
            if (monsterAnimator != null)
            {
                monsterAnimator.SetBool(IsMove_Hash, false);
            }
            baseState = MonsterState.Attack;
        }
    }

    private void HandleAttack()
    {
        float distance = Vector3.Distance(transform.position, _target.position);

        // 쿨타임 중이거나 공격 중이면
        if (_isAttacking || currentCoolTime <= jumpCoolTime)
        {
            // 범위 밖으로 나가면 Move로 전환
            if (distance > attackRange || !CanSeePlayer())
            {
                baseState = MonsterState.Move;
            }
            return;
        }

        // 공격 가능한 상태
        if (distance <= attackRange && CanSeePlayer())
        {
            _isAttacking = true;
            attackCoroutine = StartCoroutine(PerformAttack());
        }
        else
        {
            baseState = MonsterState.Move;
        }
    }

    private IEnumerator PerformAttack()
    {
        float distance = Vector3.Distance(transform.position, _target.position);

        if (distance < attackRange)
        {
            // 애니메이션: 공격 트리거 발동
            if (monsterAnimator != null)
            {
                monsterAnimator.SetTrigger(Attack_Hash);
            }
            // Y축 위치와 회전값 저장
            fixedYPosition = transform.position.y;
            fixedRotation = transform.rotation;

            // NavMeshAgent 정지 및 비활성화
            agent.isStopped = true;
            agent.ResetPath();

            yield return new WaitForSeconds(0.1f);

            agent.enabled = false;

            // 현재 속도 초기화
            if (monsterRigidbody != null)
            {
                if (!monsterRigidbody.isKinematic)
                {
                    monsterRigidbody.linearVelocity = Vector3.zero;
                    monsterRigidbody.angularVelocity = Vector3.zero;
                }

                // Rigidbody 제약 설정 (Y축 위치와 회전 고정)
                monsterRigidbody.constraints = RigidbodyConstraints.FreezePositionY |
                                               RigidbodyConstraints.FreezeRotation;
            }

            GameManager.Instance.SoundManager.PlaySfxAt("Monster_Bark", transform.position);
            yield return new WaitForSeconds(jumpPrepareTime);

            // 점프 방향 계산
            Vector3 jumpDirection = (_target.position - transform.position).normalized;
            jumpDirection.y = 0; // 수평으로만

            // Rigidbody가 있는지 확인 후 AddForce
            if (monsterRigidbody != null)
            {
                bool wasKinematic = monsterRigidbody.isKinematic;
                if (wasKinematic)
                {
                    monsterRigidbody.isKinematic = false;
                }
                if(baseState == MonsterState.Die)
                {
                    yield break;
                }
                monsterRigidbody.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

                // 돌진 시간
                yield return new WaitForSeconds(1f);

                // 속도 초기화
                monsterRigidbody.linearVelocity = Vector3.zero;
                monsterRigidbody.angularVelocity = Vector3.zero;

                // Y축 위치와 회전값 복원
                Vector3 pos = transform.position;
                pos.y = fixedYPosition;
                transform.position = pos;
                transform.rotation = fixedRotation;

                // 킨마틱 모드 복원
                if (wasKinematic)
                {
                    monsterRigidbody.isKinematic = true;
                }

                // Rigidbody 제약 해제
                monsterRigidbody.constraints = RigidbodyConstraints.None;
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }

            // NavMeshAgent 재활성화
            yield return new WaitForSeconds(0.1f);

            if (!agent.enabled)
            {
                // NavMesh 위에 있는지 확인
                UnityEngine.AI.NavMeshHit hit;
                if (!agent.isOnNavMesh && UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                }

                agent.enabled = true;

                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
            }

            currentCoolTime = 0f;
        }

        _isAttacking = false;
        baseState = MonsterState.Move;
        attackCoroutine = null;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // 공격 중일 때만 충돌 처리
        if (_isAttacking)
        {
            // Y축 위치와 회전값 유지
            Vector3 pos = transform.position;
            pos.y = fixedYPosition;
            transform.position = pos;
            transform.rotation = fixedRotation;

            // 타겟이 맞는지 확인
            if (collision.gameObject.CompareTag(targetTag))
            {
                // 속도 즉시 정지
                if (monsterRigidbody != null)
                {
                    monsterRigidbody.linearVelocity = Vector3.zero;
                    monsterRigidbody.angularVelocity = Vector3.zero;
                }

                // 데미지 적용
                var playerHealth = collision.gameObject.GetComponent<IDamageable>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage, transform);
                }

                // NavMesh 즉시 복원
                StartCoroutine(RestoreNavMeshImmediately());
            }
        }
    }

    private IEnumerator RestoreNavMeshImmediately()
    {
        // 진행 중인 공격 코루틴 중단
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // Rigidbody 정리
        if (monsterRigidbody != null)
        {
            monsterRigidbody.linearVelocity = Vector3.zero;
            monsterRigidbody.angularVelocity = Vector3.zero;

            // 킨마틱 모드로 복원
            if (!monsterRigidbody.isKinematic)
            {
                monsterRigidbody.isKinematic = true;
            }

            // Rigidbody 제약 해제
            monsterRigidbody.constraints = RigidbodyConstraints.None;
        }

        // 약간의 대기 시간 (물리 처리 완료 + 멈춤 연출)
        yield return new WaitForSeconds(0.3f);

        // NavMeshAgent 재활성화
        if (!agent.enabled)
        {
            // NavMesh 위에 있는지 확인
            UnityEngine.AI.NavMeshHit hit;
            if (!agent.isOnNavMesh && UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }

            agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }

        // 쿨타임 리셋 및 상태 전환
        currentCoolTime = 0f;
        _isAttacking = false;
        baseState = MonsterState.Move;
    }

}