using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;



/// <summary>
/// 비브르 모션 특수 공격 강화 - 피의 갈증
/// </summary>
public class Skill_One_SPAttack : ActiveTech
{
    private Player _player;

    private Action _AttackTry;

    private SkillEffect _skillEffectPrefab; //이펙트

    
    /// <summary>   
    /// 공격 한 번 당 한 번만 적용하도록 하기 위한 필드값
    /// </summary>
    public bool isHit;

    public override void Activate(SkillManager skillManager, Player player)
    {
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);
        _player = player;
        _AttackTry = () => ActiveTry(player);

        if(_skillEffectPrefab == null )             //여기서 로드
        {
            _skillEffectPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/Bloodlust");
        }
        player.OnSpecialAttackStarted += _AttackTry;
        EventBus.OnMonsterHit += HitEnemy;

    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        player.OnSpecialAttackStarted -= _AttackTry;
        EventBus.OnMonsterHit -= HitEnemy;



    }

    /// <summary>
    /// 공격 시도 시 변수 초기화
    /// </summary>
    public override void ActiveTry(Player player)
    {
        isHit = false;

        
    }

    public override void HitEnemy(WeaponType weapon, ATTACKTYPE attack, Transform transform,Transform attackerTransform)
    {
        if(attack!=ATTACKTYPE.SPECIALATTACK)
        {
            return;
        }    

        // 이미 효과가 발동했었다면 return;
        if (isHit) return;

        isHit = true;

        Vector3 spawnPos = _player.transform.position+ Vector3.up * 1f;

        //이펙트
        GameManager.Instance.PoolManager.GetFromPool(_skillEffectPrefab, spawnPos, Quaternion.identity);
        //효과음 
        GameManager.Instance.SoundManager.PlaySfxAt("Bloodlust", _player.transform.position);
        _player.playerModel.Heal(Constants.SKILL_ONE_SPATTACKHEAL);
    }


    public Skill_One_SPAttack(SkillData choosedSkill) : base(choosedSkill)
    {

    }
}


/// <summary>
/// 파이로 하트 특수 공격 강화 - 비밀무기
/// </summary>
public class Skill_Two_SPAttack : ActiveTech
{
    private float plusValue;
    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);

        // 스킬 효과 적용 (플레이어 일반 공격력에 접근하여 공격력 추가)
        plusValue = _skillData.skillBaseValue_1 + _skillData.skillLevelValue_1 * _skillData.skillLevel;
        GameManager.Instance.PlayerStatManager.AddPlayerSPAttackValue(plusValue);
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);

        // 공격력 복귀
        GameManager.Instance.PlayerStatManager.AddPlayerSPAttackValue(-plusValue);

    }


    public Skill_Two_SPAttack(SkillData choosedSkill) : base(choosedSkill)
    {

    }
}

/// <summary>
/// Gear 특수공격 강화(반동)
/// </summary>
public class Skill_Three_SPAttack: ActiveTech
{
    private float _reflectionTime;
    private reflect _playerReflect;

    private SkillEffect _backlashPrefab;    //반동 이펙트
    private Player _player;                 //플레이어 위치에 생성


    public override void Activate(SkillManager skillManager, Player player)
    {
        //추가
        _player = player;

        base.Activate(skillManager, player);
        _reflectionTime = _skillData.skillBaseValue_1 + _skillData.skillLevelValue_1*_skillData.skillLevel;
        
        _playerReflect = player.GetComponent<reflect>();
        
        if (_playerReflect == null)
        {
            _playerReflect = player.AddComponent<reflect>();
            
        }
        player.OnSpecialAttackStarted -= ActiveTry;
        player.OnSpecialAttackStarted += ActiveTry;
        
    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        player.OnSpecialAttackStarted -= ActiveTry;

    }

    public void ActiveTry()
    {
        
        if (_playerReflect == null)
        {
            return;
        }

        //로드
        if (_backlashPrefab == null)
        {
            _backlashPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/Backlash_Player");
        }
        //생성
        GameManager.Instance.PoolManager.GetFromPool(_backlashPrefab, _player.transform.position, Quaternion.identity);
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("Backlash", _player.transform.position);

        _playerReflect.StartReflectCoroutine(_reflectionTime);


    }

    

    public Skill_Three_SPAttack(SkillData skillData) : base(skillData)
    {
    }
}

/// <summary>
/// GridForge 특수공격 강화
/// </summary>
public class Skill_Four_SPAttack : ActiveTech
{
    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);
        EventBus.OnMonsterHit += HitEnemy;

    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        EventBus.OnMonsterHit -= HitEnemy;

    }

    public override void HitEnemy(WeaponType weapon, ATTACKTYPE attack, Transform transform, Transform attackerTransform)
    {
        if (attack != ATTACKTYPE.SPECIALATTACK)
        {
            return;
        }


        MonsterBase monster = transform.GetComponent<MonsterBase>();

        if (monster.GetMonsterSize() == MonsterSize.BIG)
        {
            return;
        }
        else
        {
            monster.GetDebuffHandler().ApplyDebuff(DebuffHandler.DebuffData.CreateBinding(2f));
        }
    }

    public Skill_Four_SPAttack(SkillData skillData) : base(skillData)
    {
    }
}

/// <summary>
/// Lux 제약 특수공격 강화
/// </summary>
public class Skill_Five_SPAttack : ActiveTech
{
    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);
        EventBus.OnMonsterHit += HitEnemy;

    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        EventBus.OnMonsterHit -= HitEnemy;

    }

    public override void HitEnemy(WeaponType weapon, ATTACKTYPE attack, Transform transform, Transform attackerTransform)
    {
        if (attack != ATTACKTYPE.SPECIALATTACK)
        {
            return;
        }


        MonsterBase monster = transform.GetComponent<MonsterBase>();

        if (monster.GetMonsterSize() == MonsterSize.BIG)
        {
            return;
        }
        else
        {
            monster.GetDebuffHandler().ApplyDebuff(DebuffHandler.DebuffData.CreateConfusion());
        }
    }

    public Skill_Five_SPAttack(SkillData skillData) : base(skillData)
    {
    }
}