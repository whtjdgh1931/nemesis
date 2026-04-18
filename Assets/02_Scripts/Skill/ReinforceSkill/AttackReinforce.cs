using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 비브르 모션 일반공격 강화
/// </summary>
public class Skill_One_Attack : ActiveTech
{
    public override void Activate(SkillManager skillManager, Player player)
    {
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);
        EventBus.OnMonsterHit += HitEnemy;
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                drone.Attack += HitEnemy;
            }
        }
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        EventBus.OnMonsterHit -= HitEnemy;
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                drone.Attack -= HitEnemy;
            }
        }

    }

    public override void HitEnemy(WeaponType weaponType, ATTACKTYPE attackType, Transform transform, Transform attacker)
    {
        if (attackType != ATTACKTYPE.NORMAL)
        {
            return;
        }
        transform.GetComponent<DebuffHandler>().ApplyDebuff(DebuffHandler.DebuffData.CreatePoison());
    }
    public Skill_One_Attack(SkillData choosedSkill) : base(choosedSkill)
    {

    }
}

/// <summary>
/// 파이로 하트 일반공격 강화
/// </summary>
public class Skill_Two_Attack : ActiveTech
{
    /// <summary>
    /// 파이로 하트 일반공격 강화 스킬 발동시 발행할 이벤트, 스킬 인덱스, 강화 계수
    /// </summary>
    public static event Action<int, float> ActiveSkillEvent;

    /// <summary>
    /// 파이로 하트 일반공격 강화 해제 이벤트, 스킬 인덱스
    /// </summary>
    public static event Action<int> DeactiveSkillEvent;

    private float plusDamage;
    private int originalDroneAttack = 2;
    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);
        #region Test

        // 스킬 효과 적용 (플레이어 일반 공격력에 접근하여 공격력 추가)
        plusDamage = _skillData.skillBaseValue_1 + _skillData.skillLevelValue_1 * _skillData.skillLevel;
        GameManager.Instance.PlayerStatManager.AddPlayerAttackDamage(plusDamage);

        ActiveSkillEvent?.Invoke(_skillData.skillIdx, plusDamage);
        // 공격 적중 시 이벤트에 추가
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            Constants.DRONE_ATTACK = (originalDroneAttack * (1 + plusDamage));
        }
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);

        // 공격력 복귀
        GameManager.Instance.PlayerStatManager.AddPlayerAttackDamage(-plusDamage);
        DeactiveSkillEvent?.Invoke(_skillData.skillIdx);
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            Constants.DRONE_ATTACK = originalDroneAttack;
        }

    }
    #endregion


    public Skill_Two_Attack(SkillData choosedSkill) : base(choosedSkill)
    {

    }
}


/// <summary>
/// GEAR 일반공격 강화, 보류
/// </summary>
public class Skill_Three_Attack : ActiveTech
{
    public override void Activate(SkillManager skillManager, Player player)
    {
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);
        EventBus.OnMonsterHit += KnockBackEnemy;
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                drone.Attack += KnockBackEnemy;
            }
        }
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        EventBus.OnMonsterHit -= KnockBackEnemy;
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        foreach (Drone drone in drones)
        {
            drone.Attack -= KnockBackEnemy;
        }
    }

    public void KnockBackEnemy(WeaponType weapon, ATTACKTYPE attack, Transform enemyTransform, Transform attackerTransform)
    {
        if (attack != ATTACKTYPE.NORMAL)
        {
            return;
        }
        Vector3 direction = enemyTransform.position - attackerTransform.position;
        direction.Normalize();
        MonsterBase monster = enemyTransform.GetComponent<MonsterBase>();
        if (monster != null)
        {
            monster.KnockBackEnemy(direction, 0f, 6f);
        }

    }

    public Skill_Three_Attack(SkillData skillData) : base(skillData)
    {
    }
}


/// <summary>
/// GridForge 일반공격 강화
/// 공격시 스택, 10스택시 공격에 스턴 부여
/// </summary>
public class Skill_Four_Attack : ActiveTech
{
    public int stack;
    private bool bIsEffect;
    private PoolableObject _skillEffectPrefab;
    private GameObject _skillEffect;

    private Player _player;
    private Coroutine _loopRoutine;

    private AudioSource skillSFX;

    /// <summary>
    /// 공격 시도시 실행할 액션
    /// </summary>
    private Action _AttackTry;
    private Action<WeaponType, ATTACKTYPE, Transform, Transform> _DroneAttack;

    public override void Activate(SkillManager skillManager, Player player)
    {

        // 공격 시도 시 이벤트에 추가
        base.Activate(skillManager, player);

        _player = player;

        if (_skillEffectPrefab == null)
        {
            _skillEffectPrefab = Resources.Load<PoolableObject>("Prefabs/Effect/Skill/Zzap_Player");
        }
        bIsEffect = false;
        _AttackTry = () => ActiveTry(player);
        _DroneAttack = (WeaponType weapon, ATTACKTYPE attack, Transform transform, Transform attackerTransform) => ActiveTry(player);
        player.OnNormalAttackStarted += _AttackTry;
        EventBus.OnMonsterHit += HitEnemy;
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                DroneEventConnect(drone);
            }
        }
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        player.OnNormalAttackStarted -= _AttackTry;
        EventBus.OnMonsterHit -= HitEnemy;


        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                drone.Attack -= _DroneAttack;
            }
        }

    }

    public void DroneEventConnect(Drone drone)
    {
        // ActiveTry 연결
        drone.Attack += _DroneAttack;
    }



    public override void ActiveTry(Player player)
    {
        stack++;
        // 최대 스택 체한
        if (stack >= 10)
        {
            if (!bIsEffect)
            {
                _skillEffect = GameManager.Instance.PoolManager.GetFromPool(_skillEffectPrefab, player.transform.position, Quaternion.identity, player.transform);
                //지속되는 효과음(루프)
                skillSFX = GameManager.Instance.SoundManager.PlaySfxAt("Zzap", player.transform.position, true);
                

                bIsEffect = true;

            }
            stack = 10;
        }
    }

    public override void HitEnemy(WeaponType weapon, ATTACKTYPE attack, Transform transform, Transform attackerTransform)
    {
        if (stack >= 10)
        {
            // 스턴 적용
            transform.GetComponent<DebuffHandler>().ApplyDebuff(DebuffHandler.DebuffData.CreateStun(1f));
            GameManager.Instance.PoolManager.ReleaseToPool(_skillEffect.gameObject);

            //효과음 끄기
            GameManager.Instance.SoundManager.StopSfx(skillSFX);

            _skillEffect = null;
            stack = 0;
            bIsEffect = false;
        }
    }


    public Skill_Four_Attack(SkillData skillData) : base(skillData)
    {
        stack = 0;
        if(_skillEffect!=null)
        {
            GameManager.Instance.PoolManager.ReleaseToPool(_skillEffect.gameObject);

            //효과음 끄기
            GameManager.Instance.SoundManager.StopSfx(skillSFX);

            _skillEffect = null;
        }
        bIsEffect = false;
    }
    IEnumerator LoopSoundCoroutine()
    {
        while (bIsEffect)
        {
            GameManager.Instance.SoundManager.PlaySfxAt("Zzap", _player.transform.position);
            yield return new WaitForSeconds(0.3f);
        }
    }
}

/// <summary>
/// lux 제약 일반공격 강화
/// </summary>
public class Skill_Five_Attack : ActiveTech
{
    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);

        EventBus.OnMonsterHit += HitEnemy;
        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                drone.Attack += HitEnemy;
            }
        }
    }

    public override void Deactivate(Player player, bool isSameSkill)
    {
        base.Deactivate(player, isSameSkill);

        EventBus.OnMonsterHit -= HitEnemy;

        Drone[] drones = player.transform.GetComponentsInChildren<Drone>();
        if (drones.Length > 0)
        {
            foreach (Drone drone in drones)
            {
                drone.Attack -= HitEnemy;
            }
        }
    }

    public override void HitEnemy(WeaponType weapon, ATTACKTYPE attack, Transform transform, Transform attackerTransform)
    {
        MonsterBase monster = transform.GetComponent<MonsterBase>();

        if (monster.GetMonsterSize() == MonsterSize.BIG)
        {
            monster.GetDebuffHandler().ApplyDebuff(DebuffHandler.DebuffData.CreateWeaken(5f, 10f));
        }
        else
        {
            monster.GetDebuffHandler().ApplyDebuff(DebuffHandler.DebuffData.CreateWeaken(5f, 30f));
        }
    }

    public Skill_Five_Attack(SkillData skillData) : base(skillData)
    {

    }
}



