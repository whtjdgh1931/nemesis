using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite1 : MonsterBase
{
    private string EnglishName = "Nebula's Guard Captain";
    private string KoreanName = "네뷸라사 경비대장";

    [Header("Local Stats")]
    [SerializeField] private float _box_Length = 3;
    [SerializeField] private float _box_Height = 3;
    [SerializeField] private float _box_Width = 3;

    [Header("Elite Stats")]
    [SerializeField] private float teleportAttackRange = 4.5f; // 텔포 공격 범위   
    [SerializeField] private float teleportAttackDelay = 1f; // 텔포 공격 딜레이
    [SerializeField] private float bladeAttackkDelay = 1f;
    [SerializeField] int stopDistance = 1;

    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private PoolableObject attackDecal;
    [SerializeField] private PoolableObject bladeWavePrefab;
    [SerializeField] private PoolableObject BulletPrefab;

    private int attackCount = 2;
    private float attackDist = 2f; // 공격 시 전진 거리

    [Header("CoolTimes")]
    [SerializeField] private float Pattern1CoolTime = 7f;
    [SerializeField] private float Pattern2CoolTime = 7f;

    [Header("Phase")]
    [SerializeField] private bool isPhase2 = false;
    private float lastHealthCheckThreshold = 1f;


    public override void Initialize(object data = null)
    {
        SetMonsterName(EnglishName, KoreanName);
        base.Initialize(data);
    }

    private void Update()
    {
        CoolTimeController();
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
                    TryUseSkill();
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
            baseState = MonsterState.Idle;
            return;
        }

        if (!_isAttacking) //패턴 하나 끝나면
        {
            agent.ResetPath();
            baseState = MonsterState.Attack;
        }
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

    #region 패턴1 코루틴
    private IEnumerator Pattern1Routine()
    {

        Pattern1CoolTime = 7f;
        _isAttacking = true;

        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _meshRenderer.enabled = false;

        // 생성 범위 크기 변경
        yield return StartCoroutine(ScaleTime());

        //보이기
        _meshRenderer.enabled = true;


        //페이즈마다 파동 횟수 변경
        if (currentHealth <= 50)
        {
            yield return StartCoroutine(BladeWave(5, bladeAttackkDelay));
        }
        else 

        {
            yield return StartCoroutine(BladeWave(3, bladeAttackkDelay));
        }
        _isAttacking = false;
    }
    


    private IEnumerator ScaleTime()
    {
        Vector3[] spawnPoints =
        {
            new Vector3( 2f, 0,  0f), // +X 방향
            new Vector3(-2f, 0,  0f), // -X 방향
            new Vector3( 0f, 0,  2f), // +Z 방향
            new Vector3( 0f, 0, -2f)  // -Z 방향
        };

        // 랜덤 인덱스 선택
        int randomIndex = Random.Range(0, spawnPoints.Length);

        // 최종 위치 계산
        Vector3 spawnPos = _target.transform.position + spawnPoints[randomIndex];    //플레이어위치+동서남북

        GameObject decal = GameManager.Instance.PoolManager.GetFromPool(attackDecal, spawnPos, attackDecal.transform.rotation);

        decal.GetComponent<AttackDecalEffect>().Play(teleportAttackDelay, teleportAttackRange);
        yield return new WaitForSeconds(teleportAttackDelay); // 공격 대기시간
        // 콜라이더 탐색
        Collider[] hitColliders = Physics.OverlapSphere(spawnPos, teleportAttackRange);

        foreach (Collider collider in hitColliders)
        {
            if (collider.tag == targetTag)
            {
                collider.GetComponent<IDamageable>().TakeDamage(attackDamage, transform);
            }
        }

        transform.position = new Vector3(spawnPos.x, 2f, spawnPos.z);   //range로 위치이동
    }


    private IEnumerator BladeWave(int attackCount, float cooltime)
    {
        for (int i = 0; i < attackCount; i++)
        {
            // 프리팹 발사
            ShootBlade();

            // 쿨타임
            yield return new WaitForSeconds(cooltime);
        }
    }
    

    void ShootBlade()
    {
        // 플레이어의 위치를 가져오되, 높이는 몬스터 높이와 동일하게 맞추기
        Vector3 targetPos = new Vector3(_target.position.x, transform.position.y, _target.position.z);

        // 수평 방향으로만 LookAt
        transform.LookAt(targetPos);

        // 생성
        Vector3 spawnPos = new Vector3(transform.position.x, 1f, transform.position.z) + transform.forward * 1f;
        GameObject blade = GameManager.Instance.PoolManager.GetFromPool(bladeWavePrefab, spawnPos, transform.rotation);
        EliteBlade bladeset = blade.GetComponent<EliteBlade>();
        bladeset.Initialize(targetTag, attackDamage, 5f, gameObject);
    }
    #endregion

    #region 패턴2 코루틴
    private IEnumerator Pattern2Routine()   // 패턴2 모든 루틴
    {

        Pattern2CoolTime = 7f;
        _isAttacking = true;
        // 플레이어 방향 계산
        Vector3 dirToPlayer = (_target.position - transform.position).normalized;

        // 플레이어에서 일정 거리 떨어진 곳 까지 대쉬
        Vector3 dashTarget = _target.position - dirToPlayer * stopDistance;

        // 대쉬 실행
        yield return StartCoroutine(Dash(dashTarget));

        // 짧은 대기(이거 없으니까 좀 어색함)
        yield return new WaitForSeconds(0.5f);
        monsterAnimator.SetTrigger(Attack_Hash);

        // 공격 2회 반복
        for (int i = 0; i < attackCount; i++)
        {
            yield return StartCoroutine(AttackOnce());
        }

        // 페이즈마다 총알 발사 횟수 변경
        if (currentHealth <= 50)
            yield return StartCoroutine(Shoot(5, 0.5f));
        else
        {
            yield return StartCoroutine(Shoot(3, 0.5f));
        }
        _isAttacking = false;
    }


    private IEnumerator AttackOnce()
    {
        // 공격 시작: 플레이어를 바라보고 앞으로 전진
        transform.LookAt(_target);
        transform.position += transform.forward * attackDist;

        // 공격 모션 타이밍 (애니메이션 타이밍 맞추는 용도)
        yield return new WaitForSeconds(0.3f);

        // 대미지 판정 (박스 안에 있을 때만)
        Vector3 center = transform.position + transform.forward * (_box_Length / 2f);
        Vector3 halfExtents = new Vector3(_box_Width / 2f, _box_Height / 2f, _box_Length / 2f);
        Quaternion orientation = Quaternion.LookRotation(transform.forward);

        Collider[] hitTargets = Physics.OverlapBox(center, halfExtents, orientation);

        foreach (Collider target in hitTargets)
        {
            if (target.CompareTag(targetTag) && target.TryGetComponent(out IDamageable player))
            {
                player.TakeDamage(attackDamage, transform);
            }
        }

        // TakeDamage 쿨타임
        yield return new WaitForSeconds(attackDelay);
    }


    private IEnumerator Dash(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);

        // 경로 계산 중일 때 기다림
        while (agent.pathPending)
            yield return null;


        // 도착할 때까지 대기
        while (agent.remainingDistance > agent.stoppingDistance) //플레이어와의 거리가 멈춤거리보다 클 때까지
            yield return null;


        // 도착 후 멈춤
        agent.isStopped = true;
        agent.ResetPath();
        yield return null;
    }


    private IEnumerator Shoot(int count, float cool)
    {
        for (int i = 0; i < count; i++)
        {
            // 프리팹 발사
            ShootBullet();

            // 1초 대기
            yield return new WaitForSeconds(cool);
        }
    }

    void ShootBullet()
    {
        // 플레이어의 위치를 가져오되, 높이는 몬스터 높이와 동일하게 맞추기
        Vector3 targetPos = new Vector3(_target.position.x, transform.position.y, _target.position.z);

        // 수평 방향으로만 LookAt
        transform.LookAt(targetPos);

        // 생성
        Vector3 spawnPos = new Vector3(transform.position.x, 1f, transform.position.z) + transform.forward * 1f;
        GameObject bullet = GameManager.Instance.PoolManager.GetFromPool(BulletPrefab, spawnPos, transform.rotation);
        TurretBullet BulletSet = bullet.GetComponent<TurretBullet>();
        BulletSet.Initialize(targetTag, attackDamage, 5f, gameObject);
    }
    #endregion

    private void CoolTimeController()
    {
        if (Pattern1CoolTime > 0)
        {
            Pattern1CoolTime -= Time.deltaTime;
        }

        if (Pattern2CoolTime > 0)
        {
            Pattern2CoolTime -= Time.deltaTime;
        }
    }

    private void TryUseSkill()
    {
        // 사용 가능한 스킬 코루틴 리스트
        List<IEnumerator> availableSkills = new List<IEnumerator>();


        if (Pattern1CoolTime <= 0)
        {
            availableSkills.Add(Pattern1Routine());

        }
        if (Pattern2CoolTime <= 0)
        {
            availableSkills.Add(Pattern2Routine());
        }

        // 사용 가능한 스킬이 있으면 랜덤으로 선택
        if (availableSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSkills.Count);
            StartCoroutine(availableSkills[randomIndex]);
        }
        else
        {
            // 모든 스킬이 쿨타임이면 다시 추격
            baseState = MonsterState.Move;
        }
    }

    public override void TakeDamage(float damage, Transform attacker)
    {
        base.TakeDamage(damage, attacker);

        float healthRatio = currentHealth / maxHealth;

        // 체력 임계값을 넘었을 때만 체크
        if (healthRatio <= 0.5f && lastHealthCheckThreshold > 0.5f)
        {
            EnterPhase2();
        }

        lastHealthCheckThreshold = healthRatio;
    }

    /// <summary>
    /// 2 페이즈 진입
    /// </summary>
    private void EnterPhase2()
    {
        isPhase2 = true;
        _box_Length = 4f;
        _box_Width = 4f;
    }
}
