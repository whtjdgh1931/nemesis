using System;
using UnityEngine;

/// <summary>
/// 몬스터와 플레이어 부모 클래스
/// </summary>
[RequireComponent(typeof(DebuffHandler))]
public abstract class CharacterModelBase : PoolableObject, IDamageable
{
    [SerializeField]
    protected int _maxHealth = 100;
    public int maxHealth { get { return _maxHealth; } } // 최대 체력을 반환하는 속성
    public void SetMaxHp(int plusMaxHp)
    {
        _maxHealth += plusMaxHp;
        _currentHealth += plusMaxHp;
        OnHpChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void SettingMaxHp(int MaxHp)
    {
        _maxHealth = MaxHp;
        _currentHealth = MaxHp;
        OnHpChanged?.Invoke(_currentHealth, _maxHealth);
    }


    [SerializeField]
    protected int _currentHealth;
    public int currentHealth { get { return _currentHealth; } } // 현재 체력을 반환하는 속성

    /// <summary>
    /// Initialize용 현제 체력 세팅
    /// </summary>
    /// <param name="currentHp"></param>
    public void SetCurrentHp(int currentHp)
    {
        _currentHealth = currentHp;
        OnHpChanged?.Invoke(_currentHealth, _maxHealth);
    }

    [SerializeField]
    protected float _moveSpeed; // 이동 속도
    public float moveSpeed { get { return _moveSpeed; } }
    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
        OnMoveSpeedChanged?.Invoke(_moveSpeed);
    }



    [Header("상태이상")]
    [SerializeField] public bool isStunned = false;    // 스턴
    [SerializeField] public bool isPushed = false;     // 밀림
    [SerializeField] public bool isBindned = false;    // 속박
    [SerializeField] public bool isWeaken = false;     // 약화
    [SerializeField] public bool isDead = false;       // 죽음


    /// <summary>
    /// 체력 변경 시 발생하는 이벤트
    /// currentHp / maxHp
    /// </summary>
    public event Action<int, int> OnHpChanged;
    public event Action<int, int> OnHeal;
    public event Action<float> OnMoveSpeedChanged; // 이동 속도 변경 시 발생하는 이벤트

    public event Action OnDieEvent;

    [SerializeField] protected DebuffHandler debuffHandler;
    public DebuffHandler GetDebuffHandler()
    {
        return debuffHandler;
    }


    /// <summary>
    /// 캐릭터 생성 시 초기화 함수
    /// </summary>
    public virtual void Initialize()
    {
        debuffHandler = GetComponent<DebuffHandler>();
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    /// <param name="plusHp"></param>
    public void Heal(int plusHp)
    {
        if(_currentHealth == maxHealth)
        {
            return;
        }
        _currentHealth += plusHp;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        OnHpChanged?.Invoke(_currentHealth, _maxHealth);
        OnHeal?.Invoke(_currentHealth, _maxHealth);
    }

    public virtual void TakeDamage(float damage, Transform attacker = null)
    {
        if (isDead) return;

        #region 추가데미지
        float addDamage = 1f;

        if(debuffHandler.GetActiveDebuffCount()>0&&!CompareTag(Constants.TAG_PLAYER))
        {
            addDamage += GameManager.Instance.PlayerStatManager.debuffPlusDamage;
        }
        
        

        // 약화
        if (debuffHandler != null)
        {
            if (debuffHandler.HasDebuff(Constants.DEBUFF_WEAKEN))
            {
                addDamage += GameManager.Instance.PlayerStatManager.weakenPlusDamage;
            }
        }

        damage*=addDamage;


        #endregion

        #region 모든 데미지
        float totalDamage = 1f;
        // 과부하 디버프
        if (debuffHandler != null && debuffHandler.HasDebuff(Constants.DEBUFF_OVERLOAD))
        {
            int stacks = debuffHandler.GetStackCount(Constants.DEBUFF_OVERLOAD);
            float bonus =  (0.05f * stacks);
            totalDamage += bonus;
        }

  

        // 화상 디버프
        if (debuffHandler != null && debuffHandler.HasDebuff(Constants.DEBUFF_BURN))
        {
            totalDamage += 2f;
            debuffHandler.RemoveDebuff(Constants.DEBUFF_BURN);
        }

        if (this is MonsterBase monster)
        {

            totalDamage += GameManager.Instance.PlayerStatManager.totalMultiDamage;
        }
        #endregion
        damage *= totalDamage;

        _currentHealth -= (int)damage;
        if (currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        else
        {
            OnHpChanged?.Invoke(_currentHealth, _maxHealth); // 체력 변경 이벤트 발행
        }
    }



    protected virtual void Die()
    {
        OnDieEvent?.Invoke();
        isDead = true;
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        OnDieEvent = null;
    }
}
