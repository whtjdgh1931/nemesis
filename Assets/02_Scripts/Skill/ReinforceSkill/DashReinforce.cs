using System;
using UnityEngine;


/// <summary>
/// 비브르 강화 대쉬 강화 (약육강식)
/// </summary>
public class Skill_One_Dash : ActiveTech
{
    /// <summary>
    /// 대쉬 시작 독 프리팹
    /// </summary>
    [SerializeField]
    private PoisonDash _poisonDashPrefab;

    private PoisonDashData _poisonDashData;
    /// <summary>
    /// 대쉬 실행시 실행할 액션
    /// </summary>
    public Action _DashTry;


    public override void Activate(SkillManager skillManager, Player player)
    {
        if (_poisonDashPrefab == null)
        {
            _poisonDashPrefab = Resources.Load<PoisonDash>("Prefabs/Skill/SkillObject/Skill_One/PoisonDash");
        }
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);
        _poisonDashData = new PoisonDashData(player,
            _skillData.skillBaseValue_1 + _skillData.skillLevelValue_1 * _skillData.skillLevel, // 데미지
            _skillData.skillBaseValue_2 + _skillData.skillLevelValue_2 * _skillData.skillLevel, // 힐량
            _skillData.skillBaseValue_3 + _skillData.skillLevelValue_3 * _skillData.skillLevel  // 범위
            );
        _DashTry = () => ActiveTry(player);
        player.OnDashStarted += _DashTry;
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        player.OnDashStarted -= _DashTry;
    }

    public override void ActiveTry(Player player)
    {
        Vector3 position = player.transform.position;
        position.y = 0;

        GameManager.Instance.PoolManager.GetFromPool(_poisonDashPrefab, position, _poisonDashPrefab.transform.rotation, null, _poisonDashData).GetComponent<PoisonDash>().Initialize();
        GameManager.Instance.SoundManager.PlaySfxAt("MightMakesRight", position);
    }


    public Skill_One_Dash(SkillData skillData) : base(skillData)
    {
    }
}

/// <summary>
/// 파이로 하트 대쉬 강화 (깜짝선물)
/// </summary>
public class Skill_Two_Dash : ActiveTech
{
    /// <summary>
    /// 대쉬시 남길 폭탄
    /// </summary>
    private DashReinforcePrefab _dashReinforcePrefab;

    private DashReinforceData _dashReinforceData;

    /// <summary>
    /// 대쉬 실행시 실행할 액션
    /// </summary>
    public Action _DashTry;



    public override void Activate(SkillManager skillManager, Player player)
    {
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);
        if (_dashReinforceData == null)
        {
            _dashReinforcePrefab = Resources.Load<DashReinforcePrefab>("Prefabs/Skill/SkillObject/Skill_Two/SkillTwoDash");
        }

        _dashReinforceData = new DashReinforceData(
            skillData.skillBaseValue_1 + skillData.skillLevelValue_1 * skillData.skillLevel // 스킬 범위
            );

        _DashTry = () => ActiveTry(player);
        player.OnDashStarted += _DashTry;
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);

        // 이벤트 해제
        player.OnDashStarted -= _DashTry;

    }

    public override void ActiveTry(Player player)
    {
        Vector3 position = player.transform.position;
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(_dashReinforcePrefab, position, _dashReinforcePrefab.transform.rotation, null, _dashReinforceData).GetComponent<DashReinforcePrefab>().Initialize();
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("SurprisePresent", position);
    }

    public Skill_Two_Dash(SkillData skillData) : base(skillData)
    {
    }
}


public class Skill_Three_Dash : ActiveTech
{
    /// <summary>
    /// 대쉬 시작 넉백 프리팹
    /// </summary>
    [SerializeField]
    private KnockBackDash _knockBackDashPrefab;

    private KnockBackDashData _knockBackDashData;

    /// <summary>
    /// 대쉬 실행시 실행할 액션
    /// </summary>
    public Action _DashTry;


    public override void Activate(SkillManager skillManager, Player player)
    {
        if (_knockBackDashPrefab == null)
        {
            _knockBackDashPrefab = Resources.Load<KnockBackDash>("Prefabs/Skill/SkillObject/Skill_Three/KnockBackDash");
        }
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);

        _knockBackDashData = new KnockBackDashData(
            _skillData.skillBaseValue_1 + _skillData.skillLevelValue_1 * _skillData.skillLevel, // 데미지
            _skillData.skillBaseValue_2 + _skillData.skillLevelValue_2 * _skillData.skillLevel, // 넉백 거리
            _skillData.skillBaseValue_3 + _skillData.skillLevelValue_3 * _skillData.skillLevel  // 스킬 반경
            );
        _DashTry = () => ActiveTry(player);
        player.OnDashStarted += _DashTry;


    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        player.OnDashStarted -= _DashTry;
    }

    public override void ActiveTry(Player player)
    {
        Vector3 position = player.transform.position;
        position.y = 0;


        GameManager.Instance.PoolManager.GetFromPool(_knockBackDashPrefab, position, _knockBackDashPrefab.transform.rotation, null, _knockBackDashData).GetComponent<KnockBackDash>().Initialize();
    }


    public Skill_Three_Dash(SkillData skillData) : base(skillData)
    {
    }
}


/// <summary>
/// GridForge Dash 강화
/// </summary>
public class Skill_Four_Dash : ActiveTech
{

    private float _frontTime;
    private Action _frontAction;

    private SkillEffect _plasmaShieldPrefab;
    private Player _player;

    public override void Activate(SkillManager skillManager, Player player)
    {
        _player = player;

        base.Activate(skillManager, player);
        if (_frontAction != null)
        {
            player.OnDashStarted -= _frontAction;
        }

        _frontTime = skillData.skillBaseValue_1 + skillData.skillLevelValue_1 * skillData.skillLevel;

        _frontAction = () => ActiveTry(player);

        player.OnDashStarted += _frontAction;
    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        player.OnDashStarted -= _frontAction;

    }
    public override void ActiveTry(Player player)
    {
        Vector3 spawnPos = _player.transform.position + _player.transform.forward * 1f+Vector3.up * 1.5f;
        player.playerModel.PlayerFrontInvincibility(_frontTime);

        //로드
        if (_plasmaShieldPrefab == null)
        {
            _plasmaShieldPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/PlasmaShield");
        }

        //생성
        GameManager.Instance.PoolManager.GetFromPool(_plasmaShieldPrefab, spawnPos, _player.transform.rotation, _player.transform);   //위치수정
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("PlasmaShield", spawnPos);
    }

    public Skill_Four_Dash(SkillData skillData) : base(skillData)
    {

    }

}

/// <summary>
/// LUX 제약 Dash 강화
/// </summary>
public class Skill_Five_Dash : ActiveTech
{
    private bool bIsAttackReinForce;

    private float _attackReinForce;
    private Action _dashAction;

    private bool bIsEffect;
    private PoolableObject _skillEffectPrefab;
    private GameObject _skillEffect;

    public override void Activate(SkillManager skillManager, Player player)
    {
        //로드!
        if (_skillEffectPrefab == null)
        {
            _skillEffectPrefab = Resources.Load<PoolableObject>("Prefabs/Effect/Skill/AdrenalineRace");
        }
        bIsEffect = false;

        base.Activate(skillManager, player);
        _dashAction = () => ActiveTry(player);
        _attackReinForce = _skillData.skillLevel * _skillData.skillLevelValue_1 + _skillData.skillBaseValue_1;
        player.OnDashStarted += _dashAction;
        EventBus.OnMonsterHit += HitEnemy;
    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        if (bIsAttackReinForce)
        {
            GameManager.Instance.PlayerStatManager.AddPlayerAttackDamage(-_attackReinForce);
            bIsAttackReinForce = false;
        }
        player.OnDashStarted -= _dashAction;
        EventBus.OnMonsterHit -= HitEnemy;

    }

    public override void ActiveTry(Player player)
    {
        Vector3 spawnPos = player.transform.position + Vector3.up * 1f;

        if (!bIsAttackReinForce)
        {
            //생성!
            if (!bIsEffect)
            {
                _skillEffect = GameManager.Instance.PoolManager.GetFromPool(_skillEffectPrefab, spawnPos, Quaternion.identity, player.transform);

                bIsEffect = true;
            }
            //효과음
            GameManager.Instance.SoundManager.PlaySfxAt("AdrenalineRace", spawnPos);

            GameManager.Instance.PlayerStatManager.AddPlayerAttackDamage(_attackReinForce);

            bIsAttackReinForce = true;
        }
    }

    public override void HitEnemy(WeaponType weaponType, ATTACKTYPE attackType, Transform monster, Transform attacker = null)
    {
        if (bIsAttackReinForce)
        {
            if (attackType == ATTACKTYPE.NORMAL)
            {
                //릴리즈!
                GameManager.Instance.PoolManager.ReleaseToPool(_skillEffect.gameObject);
                _skillEffect = null;
                bIsEffect = false;

                GameManager.Instance.PlayerStatManager.AddPlayerAttackDamage(-_attackReinForce);
                bIsAttackReinForce = false;
            }
        }
    }



    public Skill_Five_Dash(SkillData skillData) : base(skillData)
    {
    }
}
