using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 상태, 속성, 데이터(체력, 경험치, 레벨 등)를 관리하는 클래스입니다.
/// 게임 내에서 플레이어의 다양한 정보와 상태값을 저장하고 제공합니다.
/// </summary>
public class PlayerModel : CharacterModelBase
{

    /// <summary>
    /// 플레이어 피격시
    /// </summary>
    public event Action<Transform> PlayerHit;

    /// <summary>
    /// 회피 가능한지
    /// </summary>
    private bool bIsAvoid;

    #region 부활
    private bool bCanRevive;
    #endregion

    #region 무적
    /// <summary>
    /// 무적 상태인지
    /// </summary>
    [SerializeField]
    private bool _bIsInvincibility;

    public bool bIsInvincibility { get { return _bIsInvincibility; } }
    public void SetIsInvincibility(bool bIsInvincibility)
    {
        _bIsInvincibility = bIsInvincibility;
    }

    // 남은 무적 시간
    private float _invincibilityTimeRemaining;

    /// <summary>
    /// 무적 활성에 대한 코루틴
    /// </summary>
    private Coroutine _invincibilityCoroutine;

    #endregion

    #region 전방무적
    
    [SerializeField]
    /// <summary>
    /// 전방 무적
    /// </summary>
    private bool _bIsFrontInvincibility;

    /// <summary>
    /// 전방 무적 시간
    /// </summary>
    private float _frontInvincibilityTimeRemaining;

    /// <summary>
    /// 전방 무적 코루틴
    /// </summary>
    private Coroutine _FrontInvincibilityCoroutine;


    #endregion


    /// <summary>
    /// 회피율
    /// </summary>
    private float _avoidNum;
    public float avoidNum { get { return _avoidNum; } }
    public void SetAvoidNum(float avoidNum)
    {
        _avoidNum = avoidNum;
    }

    /// <summary>
    /// 데미지 감소 효과 적용
    /// </summary>
    private bool bIsReduceDamage;
    private float _damageReducePercent;

    public override void TakeDamage(float damage, Transform attacker)
    {
        // 무적 상태라면 데미지X
        if (_bIsInvincibility)
        {
            return;
        }

        // 전방 무적이라면 데미지 x
        if (_bIsFrontInvincibility)
        {
            if (IsAttackFront(attacker))
            {
                return;
            }
        }

        // 회피가능하다면 확률 계산 후 회피
        if (bIsAvoid)
        {
            int tempNum = UnityEngine.Random.Range(0, 100);
            if (tempNum < 100 * _avoidNum)
            {
                return;
            }
        }

        // 받는 피해 감소가 있다면 데미지 감소
        if (bIsReduceDamage)
        {
            damage *= (1f - _damageReducePercent);
        }

        if (attacker != null)
        {
            OnPlayerHit(attacker);
        }

        base.TakeDamage(damage, attacker);
    }

    protected override void Die()
    {
        if(bCanRevive)
        {
            SettingMaxHp((int)GameManager.Instance.PlayerStatManager.playerStatDataDic["playerHP"].GetEffectiveValue());
            bCanRevive = false;
            return;
        }
        base.Die();
        EventBus.SetCanTimeRun(false);
    }
    public override void Initialize()
    {
        // 플레이어 json파일 데이터에서 최대체력 받아옴
        base.Initialize();

        

        SettingMaxHp((int)GameManager.Instance.PlayerStatManager.playerStatDataDic["playerHP"].GetEffectiveValue());
        SetCurrentHp(maxHealth); // 초기화 시 현재 체력을 최대 체력으로 설정

        bCanRevive = (bool)GameManager.Instance.PlayerStatManager.playerStatDataDic["playerRevive"].GetEffectiveValue();

        // 필드값 초기화
        _bIsInvincibility = false;
        _invincibilityCoroutine = null;
        bIsAvoid = false;
        _avoidNum = 0f;
        bIsReduceDamage = false;
        _damageReducePercent = 0f;
        _invincibilityTimeRemaining = 0f;
        debuffHandler.InitializePlayer();

        GameManager.Instance.PlayerStatManager.OnplayerAvoidanceChange += OnPlayerAvoidanceChange;
        GameManager.Instance.PlayerStatManager.OnplayerHitPercentChange += OnPlayerHitReducePercentChange;
		}

    

    public void AutoHeal()
    {
				if (GameManager.Instance.PlayerStatManager.playerStatDataDic["playerRestore"].CurrentLevel != 0)
				{
            
					Heal((int)GameManager.Instance.PlayerStatManager.playerStatDataDic["playerRestore"].GetEffectiveValue());
				}
		}



    private void OnPlayerHitReducePercentChange(float reducePercent)
    {
        if (reducePercent > 0)
        {
            bIsReduceDamage = true;
        }
        else
        {
            bIsReduceDamage = false;
        }
        _damageReducePercent = reducePercent;
    }

    private void OnPlayerAvoidanceChange(float avoidance)
    {
        if (avoidance > 0)
        {
            bIsAvoid = true;
        }
        else
        {
            bIsAvoid = false;
        }
        _avoidNum = avoidance;
    }

    public void OnPlayerHit(Transform monsterTransform)
    {
        PlayerHit?.Invoke(monsterTransform);
    }

    #region 무적

    public void PlayerInvincibility(float time)
    {
        // 무적 상태가 이미 활성화되어 있고, 남은 시간이 더 길면 무시
        if (_bIsInvincibility && _invincibilityTimeRemaining >= time)
        {
            return;
        }

        // 기존 코루틴 중단
        if (_invincibilityCoroutine != null)
        {
            StopCoroutine(_invincibilityCoroutine);
            _invincibilityCoroutine = null;
        }

        // 새 시간으로 무적 재시작
        _bIsInvincibility = true;
        _invincibilityTimeRemaining = time;
        _invincibilityCoroutine = StartCoroutine(PlayerInvincibilityCoroutine());
    }

    private IEnumerator PlayerInvincibilityCoroutine()
    {
        while (_invincibilityTimeRemaining > 0f)
        {
            _invincibilityTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        _bIsInvincibility = false;
        _invincibilityCoroutine = null;
    }
    #endregion

    public bool IsAttackFront(Transform attackerTransform)
    {
        // 플레이어 기준 정면 방향
        Vector3 forward = transform.forward;

        // 공격자 방향 벡터
        Vector3 toAttacker = attackerTransform.position - transform.position;

        // 두 벡터 사이의 각도 계산
        float angle = Vector3.Angle(forward, toAttacker);

        // ±90도 이내면 정면으로 간주
        return angle <= 90f;
    }

    public void PlayerFrontInvincibility(float time)
    {
        // 무적 상태가 이미 활성화되어 있고, 남은 시간이 더 길면 무시
        if (_bIsFrontInvincibility && _frontInvincibilityTimeRemaining >= time)
        {
            return;
        }

        // 기존 코루틴 중단
        if (_FrontInvincibilityCoroutine != null)
        {
            StopCoroutine(_FrontInvincibilityCoroutine);
            _FrontInvincibilityCoroutine = null;
        }

        // 새 시간으로 무적 재시작
        _bIsFrontInvincibility = true;
        _frontInvincibilityTimeRemaining = time;
        _FrontInvincibilityCoroutine = StartCoroutine(PlayerFrontInvincibilityCoroutine());
    }

    private IEnumerator PlayerFrontInvincibilityCoroutine()
    {
        while (_frontInvincibilityTimeRemaining > 0f)
        {
            _frontInvincibilityTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        _bIsFrontInvincibility = false;
        _FrontInvincibilityCoroutine = null;
    }

}
