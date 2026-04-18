using System.Collections;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterSize
{
    SMALL,
    MIDDLE,
    BIG
}



public class MonsterBase : CharacterModelBase, IInitializePoolable
{
    public enum MonsterState
    {
        Idle,
        Move,
        Attack,
        Die
    }

    public class MonsterNameData
    {
        public string englishName;
        public string koreanName;

        public MonsterNameData(string eng, string kor)
        {
            englishName = eng;
            koreanName = kor;
        }
    }

    [SerializeField] protected MonsterNameData monsterNameData = new MonsterNameData("Monster", "몬스터");

    // 기존 string _monsterName 제거 (대체됨)
    public string GetEnglishName() => monsterNameData.englishName;
    public string GetKoreanName() => monsterNameData.koreanName;

    public virtual void SetMonsterName(string english, string korean)
    {
        monsterNameData.englishName = english;
        monsterNameData.koreanName = korean;
    }


    [Header("Base Stats")]
    [SerializeField] private float maxEliteHealth;
    [SerializeField] protected float attackDamage = 10;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float attackDelay = 0.5f;
    [SerializeField] protected float originalSpeed = 10f;
    [SerializeField] public string targetTag = Constants.TAG_PLAYER;
    [SerializeField] protected MonsterSize monsterSize = MonsterSize.SMALL;
    [SerializeField] protected int cost;
    [SerializeField] protected Rigidbody monsterRigidbody;
    [SerializeField] protected Collider monsterCollider;
    [SerializeField] protected bool _isAttacking = false;

    [Header("Base State")]
    [SerializeField] protected MonsterState baseState = MonsterState.Idle;


    #region 넉백
    [SerializeField] private float _knockBackDamage;
    [SerializeField] private Coroutine _knockBackCoroutine;
    #endregion
    [SerializeField] protected Transform player;


    [Header("Components")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Transform _target;

    [Header("Animator")]
    [SerializeField] protected Animator monsterAnimator;
    [SerializeField] protected Vector3 originalSkinTransform;
    [SerializeField] protected bool hasDieAnimation = false;
    [SerializeField] protected bool checkOnce = false;
    protected readonly int IsMove_Hash = Animator.StringToHash("IsMove");
    protected readonly int Attack_Hash = Animator.StringToHash("Attack");
    protected readonly int Die_Hash = Animator.StringToHash("Die");

    [Header("UI")]
    [SerializeField] private MonsterHealthUI healthUI;
    [SerializeField] private string healthUIPrefabPath = "Prefabs/UI/MonsterHealthUI";

    /// <summary>
    /// 공격력 반환
    /// </summary>
    /// <returns></returns>
    public float GetAttackDamage()
    {
        return attackDamage;
    }
    public MonsterSize GetMonsterSize()
    {
        return monsterSize;
    }
    public MonsterState GetMonsterState()
    {
        return baseState;
    }

    /// <summary>
    /// 공격력 설정
    /// </summary>
    /// <param name="attackDamage"></param>
    public void SetAttackDamage(float attackDamage)
    {
        this.attackDamage = attackDamage;
    }

    public Transform GetTarget()
    {
        return _target;
    }
    public int GetCost()
    {
        return cost;
    }
    public void SetTarget(Transform target)
    {
        _target = target;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public virtual void Initialize(object data = null)
    {
        base.Initialize();

        // === 상태 초기화 ===
        isDead = false;
        isPushed = false;
        isStunned = false;
        isBindned = false;
        isWeaken = false;
        _isAttacking = false;

        // === 상태 머신 초기화 ===
        baseState = MonsterState.Idle;

        // === 컴포넌트 초기화 ===
        agent = GetComponentInChildren<NavMeshAgent>();
        agent.enabled = true;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = originalSpeed;
        }
        if (monsterAnimator == null)
        {
            monsterAnimator = GetComponentInChildren<Animator>();
        }

        // === 애니메이터 초기화 ===
        if (monsterAnimator != null)
        {
            Transform skinTransform = monsterAnimator.transform;

            if (!checkOnce)
            {
                originalSkinTransform = skinTransform.localPosition;
                checkOnce = true;
            }
            skinTransform.localPosition = originalSkinTransform;
            skinTransform.localRotation = Quaternion.identity;

            monsterAnimator.Rebind();
            monsterAnimator.Update(0f);
        }

        // === 타겟 설정 ===
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
            _target = targetObj.transform;

        // === 체력 초기화 ===
        SetCurrentHp(maxHealth);

        // === 디버프 초기화 ===
        debuffHandler.InitializeMonster(agent);
        debuffHandler.ClearAllDebuffs();

        // === 물리 초기화 (넉백 관련) ===
        monsterRigidbody = GetComponentInChildren<Rigidbody>();
        if (monsterRigidbody != null)
        {
            monsterRigidbody.isKinematic = true;
        }

        monsterCollider = GetComponentInChildren<Collider>();

        // === 콜라이더 활성화 ===
        if (monsterCollider != null)
        {
            monsterCollider.enabled = true;
        }

        // === 코루틴 정리 ===
        if (_knockBackCoroutine != null)
        {
            StopCoroutine(_knockBackCoroutine);
            _knockBackCoroutine = null;
        }

        // === UI 초기화 (풀에서 가져오기) ===
        if (healthUI == null && GameManager.Instance != null && GameManager.Instance.PoolManager != null)
        {
            GameObject uiObj = GameManager.Instance.PoolManager.GetFromPool(
                healthUIPrefabPath,
                Vector3.zero,
                Quaternion.identity,
                MonsterHealthUI.monsterHealthUIRoot != null ? MonsterHealthUI.monsterHealthUIRoot.transform : null,
                this
            );

            // 지역 변수 제거, 멤버 healthUI 사용
            healthUI = uiObj.GetComponent<MonsterHealthUI>();

            if (healthUI != null)
            {
                
                healthUI.SetMonster(this);
            }
            else if (healthUI != null)
            {
                //디버프 아이콘만 초기화
                if (healthUI.GetComponent<MonsterHealthUI>() != null)
                {
                    // DebuffIconUI 강제 새로고침
                    var debuffIconUI = healthUI.GetComponentInChildren<DebuffIconUI>();
                    if (debuffIconUI != null)
                    {
                        debuffIconUI.Cleanup();
                        debuffIconUI.SetDebuffHandler(debuffHandler);
                    }
                }
            }
        }

        // === 모든 코루틴 정리 (사망 애니메이션 코루틴 포함) ===
        StopAllCoroutines();
    }

    protected bool CanSeePlayer()
    {
        if (_target == null)
        {
            return false;
        }

        Vector3 dir = (_target.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, _target.position);

        // 혼란 상태면 거리만 체크 (시야 무시)
        DebuffHandler debuffHandler = GetComponent<DebuffHandler>();
        if (debuffHandler != null && debuffHandler.HasDebuff(Constants.DEBUFF_CONFUSION))
        {
            return true;  // 혼란 상태면 항상 true
        }

        int mask = LayerMask.GetMask(targetTag, Constants.LAYER_MASK_WALL);
        if (Physics.Raycast(transform.position + Vector3.up * 2f, dir, out RaycastHit hit, dist, mask))
        {
            if (hit.transform == _target)
            {
                return true;
            }
            return false;
        }
        return false;
    }


    protected void LookAtPlayer()
    {
        Vector3 dir = new Vector3(_target.position.x - transform.position.x, 0, _target.position.z - transform.position.z).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    public void SetEliteMaxHealth(int roomCount)
    {
        maxEliteHealth = (float)_maxHealth * 1 + (0.1f * roomCount);
        _maxHealth = (int)maxEliteHealth;
    }

    #region 이펙트 관리
    public void GetEffectFromPool(PoolableObject effectPrefab, Vector3 position, Quaternion rotation, float? customDuration = null)
    {
        if (effectPrefab == null)
        {
            return;
        }

        GameObject effectObj = GameManager.Instance.PoolManager.GetFromPool(
            effectPrefab,
            position,
            rotation
        );

        if (effectObj == null)
        {
            return;
        }

        // ParticleSystem 찾기 및 재생
        ParticleSystem ps = effectObj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();

            // 지속시간 계산
            float duration = customDuration ?? (ps.main.duration + ps.main.startLifetime.constantMax);
            StartCoroutine(ReturnEffectToPool(effectObj, duration));
        }
        else
        {
            // ParticleSystem이 없으면 기본 지속시간 사용
            float duration = customDuration ?? 2f;
            StartCoroutine(ReturnEffectToPool(effectObj, duration));
        }
    }

    public void GetEffectFromPool(PoolableObject effectPrefab, Transform spawnTransform, float? customDuration = null)
    {
        if (spawnTransform == null)
        {
            return;
        }

        GetEffectFromPool(effectPrefab, spawnTransform.position, spawnTransform.rotation, customDuration);
    }

    protected IEnumerator ReturnEffectToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (effect != null)
        {
            GameManager.Instance.PoolManager.ReleaseToPool(effect);
        }
    }

    /// <summary>
    /// 데미지 이펙트
    /// </summary>
		[SerializeField] protected PoolableObject damageTextPrefab;

		public virtual void ShowDamage(float damageAmount, Vector3 position,int damage)
		{
        if(damageTextPrefab == null)
        {
            damageTextPrefab = Resources.Load<PoolableObject>("Prefabs/Skill/DamageCanvas");
        }
        bool bIsBillBoard = false;

        if(EventBus.IsColosseumRoom)
        {

						// 카메라 기준 왼쪽 방향
						Transform cam = Camera.main.transform;
						position += -cam.right * 3f;

						bIsBillBoard = true;
        }

        GameManager.Instance.PoolManager.GetFromPool(damageTextPrefab, position, Quaternion.identity).GetComponentInChildren<DamageText>().SetDmg(damage,bIsBillBoard);
		}

		#endregion

		#region 사망 처리 및 애니메이션 처리
		protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        baseState = MonsterState.Die;

        if (monsterCollider != null)
        {
            monsterCollider.enabled = false;
        }

        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool(IsMove_Hash, false);
            monsterAnimator.ResetTrigger(Attack_Hash);
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (hasDieAnimation && monsterAnimator != null)
        {
            monsterAnimator.SetTrigger(Die_Hash);
            StartCoroutine(WaitForDieAnimation());
        }
        else
        {
            CompleteDeath();
        }
    }

    private IEnumerator WaitForDieAnimation()
    {
        yield return null;

        AnimatorStateInfo stateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);

        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        CompleteDeath();
    }

    private void CompleteDeath()
    {
        // UI 풀로 반환
        if (healthUI != null && GameManager.Instance != null && GameManager.Instance.PoolManager != null)
        {
            GameManager.Instance.PoolManager.ReleaseToPoolByInterface(healthUI);
            healthUI = null;
        }

        GameManager.Instance.CurrencyManager.AddCreditByMonsterDeath(cost);
        base.Die();
    }

    #endregion


    #region 넉백

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isPushed)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_MASK_WALL))
            {
                TakeDamage(GameManager.Instance.PlayerStatManager.knockBackDamage * GameManager.Instance.PlayerStatManager.knockBackDamageMulti, null);
                EndKnockBack();
            }

        }
    }

    public override void TakeDamage(float damage, Transform attacker = null)
    {
        base.TakeDamage(damage, attacker);

        // 남은 체력이 0보다 크면 데미지를 보여줌
				if (currentHealth > 0)
				{
				ShowDamage(damage,transform.position + Vector3.up*2f,(int)damage);
				}


        if (healthUI != null)
        {
            healthUI.OnHealthChanged();
        }
    }


    /// <summary>
    /// 넉백 실행
    /// </summary>
    public void KnockBackEnemy(Vector3 pushDirection, float damage, float knockBackDistance)
    {
        TakeDamage(damage, null);

        if (isDead)
        {
            return;
        }
        if (_knockBackCoroutine != null)
        {
            return;
        }
        if (monsterSize == MonsterSize.BIG)
        {
            return;
        }
        monsterCollider.isTrigger = false;
        isPushed = true;
        //debuffHandler.ApplyDebuff(DebuffHandler.DebuffData.CreateStun(0.5f));
        agent.isStopped = true;
        monsterRigidbody.isKinematic = false;
        monsterRigidbody.linearVelocity = Vector3.zero;
        Vector3 push = pushDirection * Constants.KNOCKBACK_POWER;
        monsterRigidbody.AddForce(push, ForceMode.VelocityChange);

        EventBus.MonsterKnockBack(transform.position);
        _knockBackCoroutine = StartCoroutine(KnockBackCoroutine(knockBackDistance));
        StartCoroutine(KnockBackCoolTime());
    }


    /// <summary>
    /// 넉백 코루틴
    /// </summary>
    /// <param name="pushDirection"></param>
    /// <param name="damage"></param>
    /// <returns></returns>
    public IEnumerator KnockBackCoroutine(float knockBackDistance)
    {
        float time = 0;
        float maxTime = 1f;
        Vector3 startPosition = transform.position;
        while (Vector3.Distance(transform.position, startPosition) < knockBackDistance)
        {
            time += Time.deltaTime;
            if (time > maxTime)
            {
                break;
            }
            yield return null;
        }
        EndKnockBack();
    }

    public void EndKnockBack()
    {
        isPushed = false;
        monsterRigidbody.linearVelocity = Vector3.zero;
        monsterRigidbody.isKinematic = true;
        if (!isBindned)
        {
            agent.isStopped = false;
        }
    }

    public IEnumerator KnockBackCoolTime(float knockBackCoolTime = Constants.KNOCKBACK_COOLTIME)
    {
        yield return new WaitForSeconds(knockBackCoolTime);
        _knockBackCoroutine = null;
    }
    #endregion

#if UNITY_EDITOR
    public void SetDebugStats(int hp, float speed)
    {
        _maxHealth = hp;
        _currentHealth = hp;
        _moveSpeed = speed;
    }
#endif
}