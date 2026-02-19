using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Omega_X7 : MonsterBase
{
    private string EnglishName = "OMEGA_X7";
    private string KoreanName = "오메가_X7";

    [Header("Missile")]
    [SerializeField] private float misileAttackDamage = 20f;

    [Header("Shotgun Attack")]
    [SerializeField] private float shotgunDamage = 30f;
    [SerializeField] private float shotgunRange = 1000f;
    [SerializeField] private float shotgunAngle = 120f;
    [SerializeField] private float shotgunWarningDuration = 2f;

    [Header("Prefabs")]
    [SerializeField] private PoolableObject missilePrefab;
    [SerializeField] private PoolableObject laserTurretPrefab;
    [SerializeField] private PoolableObject shotgunDecalPrefab; // 샷건 범위 표시 프리팹
    [SerializeField] private PoolableObject iceTankPrefab;

    [Header("CoolTimes")]
    [SerializeField] private float shotgunttackCoolTime = 5f;
    [SerializeField] private float missileAttackCoolTime = 7f;
    [SerializeField] private float laserTurretSpawnCoolTime = 30f;

    [Header("Local Stats")]
    private bool _isShotgunAttacking = false;
    private bool _isMissileAttacking = false;
    private bool _isIceTankSpawning = false;

    [Header("Effect Positions")]
    [SerializeField] Transform shotgunEffectPos1;
    [SerializeField] Transform shotgunEffectPos2;

    [Header("Effects")]
    [SerializeField] private PoolableObject shotgunEffectPrefab;

    private float lastHealthCheckThreshold = 1f;
    private bool _isPhase2 = false;

    [SerializeField] private MonsterGrenade monsterGrenade;

    public override void Initialize(object data = null)
    {
        SetMonsterName(EnglishName, KoreanName);

        base.Initialize(data);

        monsterGrenade = GetComponentInChildren<MonsterGrenade>();

        Invoke(nameof(DisableAgent), 0.1f);

    }

    private void Update()
    {
        CoolTimeController();
        if (isDead || _target == null) return;
        if (isStunned) return;
        LookAtPlayer();

        switch (baseState)
        {
            case MonsterState.Idle:
                HandleIdle();
                break;
            case MonsterState.Attack:
                TryUseSkill();
                break;
            case MonsterState.Die:
                Die();
                break;
        }
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


    private void HandleIdle()
    {
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance <= attackRange && CanSeePlayer())
        {
            baseState = MonsterState.Attack;
        }
    }

    #region 레이저 터렛 스폰
    private IEnumerator SpawnLaserTurret()
    {
        _isAttacking = true;

        Vector3 leftBottomSpawnPos = transform.position + new Vector3(-15, 0, -15);
        Vector3 rightBottomSpawnPos = transform.position + new Vector3(15, 0, -15);
        Vector3 upSpawnPos = transform.position + new Vector3(0, 0, 15);

        GameManager.Instance.PoolManager.GetFromPool(laserTurretPrefab, leftBottomSpawnPos, Quaternion.identity);
        GameManager.Instance.PoolManager.GetFromPool(laserTurretPrefab, rightBottomSpawnPos, Quaternion.identity);
        GameManager.Instance.PoolManager.GetFromPool(laserTurretPrefab, upSpawnPos, Quaternion.identity);

        laserTurretSpawnCoolTime = 30f;
        _isAttacking = false;

        yield return null;
    }
    #endregion

    #region 아이스 탱크 스폰
    private IEnumerator SpawnIceTank()
    {
        _isIceTankSpawning = true;
        bool previousAttackingState = _isAttacking;
        _isAttacking = true;

        Vector3 leftTopSpawnPos = transform.position + new Vector3(-15, 0, 15);
        Vector3 rightTopSpawnPos = transform.position + new Vector3(15, 0, 15);
        Vector3 bottomSpawnPos = transform.position + new Vector3(0, 0, -15);

        List<GameObject> spawnedIceTanks = new List<GameObject>();
        spawnedIceTanks.Add(GameManager.Instance.PoolManager.GetFromPool(iceTankPrefab, leftTopSpawnPos, Quaternion.identity));
        spawnedIceTanks.Add(GameManager.Instance.PoolManager.GetFromPool(iceTankPrefab, rightTopSpawnPos, Quaternion.identity));
        spawnedIceTanks.Add(GameManager.Instance.PoolManager.GetFromPool(iceTankPrefab, bottomSpawnPos, Quaternion.identity));

        yield return new WaitForSeconds(10f);

        int survivingIceTankCount = 0;
        foreach (GameObject iceTank in spawnedIceTanks)
        {
            if (iceTank != null && iceTank.activeInHierarchy)
            {
                IceTank icetankState  = iceTank.GetComponent<IceTank>();
                icetankState.SetStateDie();
                survivingIceTankCount++;
            }
        }

        if (survivingIceTankCount > 0)
        {
            float damageMultiplier = 1f + (survivingIceTankCount * 0.3f);
            shotgunDamage *= damageMultiplier;

            if (monsterGrenade != null)
            {
                monsterGrenade.IncreaseDamage(damageMultiplier);
            }
        }

        _isAttacking = previousAttackingState;
        _isIceTankSpawning = false;

        if (monsterGrenade != null)
        {
            monsterGrenade.StartGrenadePattern();
        }
    }
    #endregion

    #region 샷건 패턴 코루틴
    private IEnumerator ShotgunAttack()
    {
        _isShotgunAttacking = true;

        if (_target == null)
        {
            _isShotgunAttacking = false;
            baseState = MonsterState.Idle;
            yield break;
        }

        Vector3 directionToPlayer = new Vector3(
            _target.position.x - transform.position.x,
            0,
            _target.position.z - transform.position.z
        ).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);

        float forwardOffset = transform.localScale.z / 2f;
        Vector3 spawnPos = transform.position + directionToPlayer * forwardOffset;
        spawnPos.y = 0.1f;

        GameObject decalObj = GameManager.Instance.PoolManager.GetFromPool(shotgunDecalPrefab, spawnPos, lookRotation);
        ShotgunDecalEffect decalEffect = decalObj.GetComponent<ShotgunDecalEffect>();
        if (decalEffect != null)
        {
            decalEffect.Play();
        }

        yield return new WaitForSeconds(shotgunWarningDuration);

        PlayShotgunEffect();
        PerformShotgunHitscan(spawnPos, directionToPlayer);
        GameManager.Instance.SoundManager.PlaySfxAt("GravityFlare", transform.position);

        shotgunttackCoolTime = 5f;
        _isShotgunAttacking = false;
    }

    private void PlayShotgunEffect()
    {
        if (shotgunEffectPos1 != null) GetEffectFromPool(shotgunEffectPrefab, shotgunEffectPos1);
        if (shotgunEffectPos2 != null) GetEffectFromPool(shotgunEffectPrefab, shotgunEffectPos2);
    }

    private void PerformShotgunHitscan(Vector3 attackOrigin, Vector3 attackDirection)
    {
        if (_target == null) return;

        // Y축을 같은 높이로 맞춰서 거리 계산 (수평 거리만 체크)
        Vector3 targetPosFlat = new Vector3(_target.position.x, attackOrigin.y, _target.position.z);
        Vector3 attackOriginFlat = new Vector3(attackOrigin.x, attackOrigin.y, attackOrigin.z);

        Vector3 directionToTarget = (targetPosFlat - attackOriginFlat).normalized;
        float angleToTarget = Vector3.Angle(attackDirection, directionToTarget);
        float distanceToTarget = Vector3.Distance(attackOriginFlat, targetPosFlat);

        if (angleToTarget <= shotgunAngle / 2f && distanceToTarget <= shotgunRange)
        {
            IDamageable damageable = _target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(shotgunDamage);
            }
        }
    }
    #endregion

    #region 미사일 패턴 코루틴
    private IEnumerator MissileAttack()
    {
        _isMissileAttacking = true;

        int attackCount = 0;
        int maxAttacks = 3;

        while (!isDead && attackCount < maxAttacks)
        {
            Vector3 backwardDirection = -transform.forward;
            float sideOffset = Random.Range(-5f, 5f);
            Vector3 rightDirection = transform.right * sideOffset;
            Vector3 spawnPos = transform.position + backwardDirection * transform.localScale.z + rightDirection;
            spawnPos.y = 0;

            GameObject missileObj = GameManager.Instance.PoolManager.GetFromPool(missilePrefab, spawnPos, Quaternion.identity, null, _isPhase2);
            Missile missile = missileObj.GetComponent<Missile>();
            if (missile != null && _target != null)
            {
                missile.SetAttackDamage(misileAttackDamage);
            }

            attackCount++;
            yield return new WaitForSeconds(3f);
        }

        missileAttackCoolTime = 7f;
        _isMissileAttacking = false;
    }
    #endregion

    #region 쿨타임 컨트롤러
    private void CoolTimeController()
    {
        if (shotgunttackCoolTime > 0) shotgunttackCoolTime -= Time.deltaTime;
        if (missileAttackCoolTime > 0) missileAttackCoolTime -= Time.deltaTime;
        if (laserTurretSpawnCoolTime > 0) laserTurretSpawnCoolTime -= Time.deltaTime;
    }
    #endregion

    #region 랜덤 스킬 사용
    private void TryUseSkill()
    {
        List<IEnumerator> availableSkills = new List<IEnumerator>();

        if (shotgunttackCoolTime <= 0 && !_isShotgunAttacking && !_isIceTankSpawning && _isPhase2)
        {
            availableSkills.Add(ShotgunAttack());
        }

        if (missileAttackCoolTime <= 0 && !_isMissileAttacking)
        {
            availableSkills.Add(MissileAttack());
        }

        if (laserTurretSpawnCoolTime <= 0 && !_isAttacking)
        {
            availableSkills.Add(SpawnLaserTurret());
        }

        if (availableSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSkills.Count);
            StartCoroutine(availableSkills[randomIndex]);
        }
        else if (!_isShotgunAttacking && !_isMissileAttacking)
        {
            baseState = MonsterState.Idle;
        }
    }
    #endregion

    public override void TakeDamage(float damage, Transform attacker)
    {
        base.TakeDamage(damage, attacker);

        float healthRatio = (float)currentHealth / (float)maxHealth;

        if (!_isPhase2 && healthRatio <= 0.7f && lastHealthCheckThreshold > 0.7f)
        {
            _isPhase2 = true;
            StartCoroutine(SpawnIceTank());
        }

        lastHealthCheckThreshold = healthRatio;
    }

    protected override void Die()
    {
        if (monsterGrenade != null)
        {
            monsterGrenade.StopGrenadePattern();
        }

        base.Die();
    }
}