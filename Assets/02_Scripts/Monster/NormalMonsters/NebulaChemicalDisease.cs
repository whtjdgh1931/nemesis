using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaChemicalDisease : MonsterBase
{
    [Header("Local Stats")]
    [SerializeField] private float _poisonFieldDuration = 5f; // 독성 구름 지속 시간
    [SerializeField] private float _poisonFieldRadius = 3f;   // 독성 구름 반경
    [SerializeField] private float _poisonFieldDelay = 2f;

    [Header("PoisonFieldPrefab"), SerializeField]
    private PoolableObject poisonFieldPrefab; // 독성 구름 프리팹
    [SerializeField] private PoolableObject grenadeObject; // 독성 유탄 프리팹

    [Header("AttackDecalPrefab"), SerializeField]
    private PoolableObject attackDecalPrefab; // 공격 장판 프리팹

    // 유탄 포물선 높이 조절 변수
    private float maxParabolaHeight = 10f;
    private float minParabolaHeight = 5f;
    private float closeDistance = 5f;
    private float farDistance = 10f;


    private void Update()
    {
        if (isDead || _target == null) return;
        if (isStunned) return;

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

            // 애니메이션: 공격 트리거 발동
            if (monsterAnimator != null)
            {
                monsterAnimator.SetTrigger(Attack_Hash);
            }

            poisonFieldPrefab.GetComponent<PoisonField>().SetLifeTime(_poisonFieldDuration); // 독성 구름 지속 시간 설정

            Vector3 attackPos = _target.position;

            GameObject decalObj = GameManager.Instance.PoolManager.GetFromPool(attackDecalPrefab, attackPos, attackDecalPrefab.transform.rotation);
            decalObj.GetComponent<AttackDecalEffect>().Play(_poisonFieldDelay, _poisonFieldRadius / 2);

            StartCoroutine(GrenadeVisualEffect(transform.position + Vector3.up, attackPos, _poisonFieldDelay));

            // 장판 생성 후 딜레이동안 대기
            yield return new WaitForSeconds(_poisonFieldDelay);

            GameObject poisonObj = GameManager.Instance.PoolManager.GetFromPool(poisonFieldPrefab, attackPos, poisonFieldPrefab.transform.rotation);// 플레이어에게 독성 구름 발사
            PoisonField poisonField = poisonObj.GetComponent<PoisonField>();
            poisonField.Initialize(targetTag, _poisonFieldDuration, _poisonFieldRadius);

            // 독성 구름을 발사한 후 일정 시간 대기
            yield return new WaitForSeconds(attackDelay);
        }
        _isAttacking = false;
        baseState = MonsterBase.MonsterState.Move; // 공격 후 다시 추격 상태로 전환
    }

    private IEnumerator GrenadeVisualEffect(Vector3 startPos, Vector3 targetPos, float duration)
    {
        if (grenadeObject == null)
            yield break;

        GameObject grenade = GameManager.Instance.PoolManager.GetFromPool(
            grenadeObject,
            startPos,
            Quaternion.identity
        );

        if (grenade == null)
            yield break;

        float elapsed = 0f;
        float distance = Vector3.Distance(startPos, targetPos);

        // 거리 비례 포물선 높이 계산
        float tDist = Mathf.InverseLerp(closeDistance, farDistance, distance);
        float smooth = Mathf.SmoothStep(0f, 1f, tDist);
        float parabolaHeight = Mathf.Lerp(maxParabolaHeight, minParabolaHeight, smooth);

        Vector3 prevPos = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float parabola = 4f * parabolaHeight * (t - t * t);
            flatPos.y += parabola;

            // 위치 갱신
            grenade.transform.position = flatPos;

            // 진행 방향 계산 후 회전 적용
            Vector3 direction = flatPos - prevPos;
            if (direction.sqrMagnitude > 0.0001f)
            {
                grenade.transform.rotation = Quaternion.LookRotation(direction);
            }

            prevPos = flatPos;

            yield return null;
        }

        GameManager.Instance.PoolManager.ReleaseToPool(grenade);
    }

}
