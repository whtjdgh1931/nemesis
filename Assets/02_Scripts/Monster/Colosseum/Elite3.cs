using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Elite3 : MonsterBase
{
    private string EnglishName = "Giant Mech ProtoType";
    private string KoreanName = "거대로봇 실험체";

    [Header("Missile")]
    [SerializeField] private int missileAttackCounter = 0;
    [SerializeField] private int maxMissileAttackCount = 5;

    [Header("Bullet")]
    [SerializeField] float bulletLifeTime = 8f;

    [Header("Prefabs")]
    [SerializeField] private PoolableObject missilePrefab;
    [SerializeField] private PoolableObject eliteBulletPrefab;
    [SerializeField] private PoolableObject waveAttackPrefab;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    [Header("CoolTimes")]
    [SerializeField] private float bulletAttackCoolTime = 5f;
    [SerializeField] private float missileAttackCoolTime = 7f;

    private float lastHealthCheckThreshold = 1f;
    private bool _isPhase2 = false;

    public override void Initialize(object data = null)
    {
        SetMonsterName(EnglishName, KoreanName);
        base.Initialize(data);
        Invoke(nameof(DisableAgent), 0.1f);
    }


    private void Update()
    {
        CoolTimeController();
        if (isDead || _target == null) return;
        if (isStunned) return;

        switch (baseState)
        {
            case MonsterState.Idle:
                HandleIdle();
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
        if (distance <= attackRange && CanSeePlayer())
        {
            baseState = MonsterState.Attack;
        }
    }

    #region 총알 패턴 코루틴
    private IEnumerator BulletAttack()
    {
        _isAttacking = true;
        bulletAttackCoolTime = 5f; // 총알 공격 쿨타임 초기화
        if (_target != null && Vector3.Distance(transform.position, _target.position) <= attackRange)
        {
            float angleOffset = 0f; // 회전 각도 오프셋
            int maxRotations = 20; // 총 회전 수

            for (int rotation = 0; rotation < maxRotations; rotation++)
            {
                // 10방향으로 총알 발사
                for (int i = 0; i < 10; i++)
                {
                    float angle = i * 36f + angleOffset;
                    Quaternion bulletRotation = Quaternion.Euler(0, angle, 0);
                    Vector3 spawnPos = transform.position;
                    spawnPos.y = 1f;
                    GameObject bullet = GameManager.Instance.PoolManager.GetFromPool(eliteBulletPrefab, spawnPos, bulletRotation);
                    EliteBullet elitetBullet = bullet.GetComponent<EliteBullet>();
                    if (elitetBullet != null)
                    {
                        elitetBullet.Initialize(targetTag, attackDamage, bulletLifeTime, gameObject);
                        GameManager.Instance.SoundManager.PlaySfxAt("Monster_Machinegun", transform.position);
                    }
                }

                angleOffset += 10f; // 매 회전마다 10도씩 회전

                // 360도를 넘으면 초기화 (한 바퀴 완성)
                if (angleOffset >= 360f)
                {
                    angleOffset -= 360f;
                }

                yield return new WaitForSeconds(attackDelay);
            }
        }
        _isAttacking = false;
        baseState = MonsterState.Attack;
    }
    #endregion

    #region 미사일 패턴 코루틴
    private IEnumerator MissileAttack()
    {
        _isAttacking = true;
        missileAttackCoolTime = 5f;
        while (missileAttackCounter < maxMissileAttackCount)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0, -transform.localScale.z);
            spawnPos.y = 0;
            GameObject missileObj = GameManager.Instance.PoolManager.GetFromPool(missilePrefab, spawnPos, Quaternion.identity);
            spawnedObjects.Add(missileObj);

            if (missileObj != null)
            {
                Missile missile = missileObj.GetComponent<Missile>();
                missile?.Initialize(_isPhase2);
            }

            yield return new WaitForSeconds(1f);
            missileAttackCounter++;
        }

        _isAttacking = false;
        missileAttackCounter = 0;
        baseState = MonsterState.Attack;
    }
    #endregion

    #region 쿨타임 컨트롤러
    private void CoolTimeController()
    {
        if (bulletAttackCoolTime > 0)
            bulletAttackCoolTime -= Time.deltaTime;
        if (missileAttackCoolTime > 0)
            missileAttackCoolTime -= Time.deltaTime;
    }
    #endregion

    #region 랜덤 스킬 사용
    private void TryUseSkill()
    {
        // 사용 가능한 스킬 코루틴 리스트
        List<IEnumerator> availableSkills = new List<IEnumerator>();

        if (bulletAttackCoolTime <= 0)
        {
            availableSkills.Add(BulletAttack());
        }
        if (missileAttackCoolTime <= 0)
        {
            availableSkills.Add(MissileAttack()); availableSkills.Add(MissileAttack());
        }

        // 사용 가능한 스킬이 있으면 랜덤으로 선택
        if (availableSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSkills.Count);
            StartCoroutine(availableSkills[randomIndex]);
        }
        else
        {
            baseState = MonsterState.Attack;
        }
    }
    #endregion

    public override void TakeDamage(float damage, Transform attacker)
    {
        base.TakeDamage(damage, attacker);

        float healthRatio = (float)currentHealth / (float)maxHealth;

        // 체력 임계값을 넘었을 때만 체크
        if (!_isPhase2 && healthRatio <= 0.7f && lastHealthCheckThreshold > 0.7f)
        {
            _isPhase2 = true;
            Vector3 pos = transform.position;
            pos.y = 0.1f;
            GameObject wave = GameManager.Instance.PoolManager.GetFromPool(waveAttackPrefab, pos, Quaternion.identity);
            spawnedObjects.Add(wave);
        }

        lastHealthCheckThreshold = healthRatio;
    }

		public override void ShowDamage(float damageAmount, Vector3 position, int damage)
		{
				if (damageTextPrefab == null)
				{
						damageTextPrefab = Resources.Load<PoolableObject>("Prefabs/Skill/DamageCanvas");
				}
				bool bIsBillBoard = false;

				if (EventBus.IsColosseumRoom)
				{

						// 카메라 기준 왼쪽 방향
						Transform cam = Camera.main.transform;
						position += -cam.right * 8f;

						bIsBillBoard = true;
				}

				GameManager.Instance.PoolManager.GetFromPool(damageTextPrefab, position, Quaternion.identity).GetComponentInChildren<DamageText>().SetDmg(damage, bIsBillBoard);
		}
    private void DisableAgent()
    {
        if (agent != null && agent.isOnNavMesh) // ← NavMesh 확인
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        else if (agent != null)
        {
            // 아직 NavMesh 안 올라갔으면 재시도
            Invoke(nameof(DisableAgent), 0.1f);
        }
    }

    protected override void Die()
    {
        base.Die();

        //소환된 waveAttackPrefab 반환 처리
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Missile missile = obj.GetComponent<Missile>();
                missile?.SetDie();
                if (obj != null)
                {
                    GameManager.Instance.PoolManager.ReleaseToPool(obj);
                }
            }
        }
        spawnedObjects.Clear();
    }
}

