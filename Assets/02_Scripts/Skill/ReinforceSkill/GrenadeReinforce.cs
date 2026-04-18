using UnityEngine;

/// <summary>
/// 비브르 강화 유탄 강화 (포자 퍼뜨리기)
/// </summary>
public class Skill_One_Grenade : ActiveTech
{
    /// <summary>
    /// 착탄 지점 독 프리팹
    /// </summary>
    [SerializeField]
    private GrenadePoison _grenadePoisonPrefab;
    private GrenadePoisonData _grenadePoisonData;

    public override void Activate(SkillManager skillManager, Player player)
    {
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);

        if (_grenadePoisonPrefab == null)
        {
            _grenadePoisonPrefab = Resources.Load<GrenadePoison>("Prefabs/Skill/SkillObject/Skill_One/GrenadePoison");
        }

        _grenadePoisonData = new GrenadePoisonData(skillData.skillBaseValue_1 + skillData.skillLevelValue_1 * skillData.skillLevel,
                skillData.skillBaseValue_2 + skillData.skillLevelValue_2 * skillData.skillLevel);

        EventBus.OnGrenadeBomb -= GrenadeBomb;
        EventBus.OnGrenadeBomb += GrenadeBomb;

    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        EventBus.OnGrenadeBomb -= GrenadeBomb;
    }

    public void GrenadeBomb(Vector3 position)
    {
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(_grenadePoisonPrefab, position, Quaternion.identity, null, _grenadePoisonData).GetComponent<GrenadePoison>().Initialize();
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("SporeBurst", position);
    }


    public Skill_One_Grenade(SkillData choosedSkill) : base(choosedSkill)
    {

    }


}

/// <summary>
/// 파이로 하트 유탄공격 강화
/// </summary>
public class Skill_Two_Grenade : ActiveTech
{

    private float plusDamage;
    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);

        // 스킬 효과 적용 (플레이어 일반 공격력에 접근하여 공격력 추가)
        plusDamage = _skillData.skillBaseValue_1 + _skillData.skillLevelValue_1 * _skillData.skillLevel;
        GameManager.Instance.PlayerStatManager.AddPlayerGrenadeDamageMulti(plusDamage);
    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);

        // 공격력 복귀
        GameManager.Instance.PlayerStatManager.AddPlayerGrenadeDamageMulti(-plusDamage);

    }


    public Skill_Two_Grenade(SkillData skillData) : base(skillData)
    {
    }
}

/// <summary>
/// Gear 유탄 강화
/// </summary>
public class Skill_Three_Grenade : ActiveTech
{
    /// <summary>
    /// 착탄 지점 vortex 프리팹
    /// </summary>
    [SerializeField]
    private GrenadeVortex _grenadeVortexPrefab;
    private GrenadeVortexData _grenadeVortexData;

    public override void Activate(SkillManager skillManager, Player player)
    {
        // 공격 적중 시 이벤트에 추가
        base.Activate(skillManager, player);

        if (_grenadeVortexPrefab == null)
        {
            _grenadeVortexPrefab = Resources.Load<GrenadeVortex>("Prefabs/Skill/SkillObject/Skill_Three/GrenadeVortex");
        }

        _grenadeVortexData = new GrenadeVortexData(skillData.skillBaseValue_2 + skillData.skillLevelValue_2 * skillData.skillLevel,
                skillData.skillBaseValue_1 + skillData.skillLevelValue_1 * skillData.skillLevel);

        EventBus.OnGrenadeBomb -= GrenadeBomb;
        EventBus.OnGrenadeBomb += GrenadeBomb;

    }
    public override void Deactivate(Player player, bool isSameSkill)
    {
        // 리스트 제거
        base.Deactivate(player, isSameSkill);
        // 이벤트 해제
        EventBus.OnGrenadeBomb -= GrenadeBomb;
    }

    public void GrenadeBomb(Vector3 position)
    {
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(_grenadeVortexPrefab, position, Quaternion.identity, null, _grenadeVortexData).GetComponent<GrenadeVortex>().Initialize();
        //효과음
        //GameManager.Instance.SoundManager.PlaySfxAt("Vortex", position);
        GameManager.Instance.SoundManager.PlaySfxForSecondsAt("Vortex", position, 1f);




    }

    public Skill_Three_Grenade(SkillData skillData) : base(skillData)
    {
    }

}

/// <summary>
/// GridForge 유탄 강화
/// </summary>
public class Skill_Four_Grenade : ActiveTech
{
    /// <summary>
    /// 착탄 지점 EMP 프리팹
    /// </summary>
    [SerializeField]
    private GrenadeEMP _grenadeEMPPrefab;
    private GrenadeEMPData _grenadeEMPData;

    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);
        if (_grenadeEMPPrefab == null)
        {
            _grenadeEMPPrefab = Resources.Load<GrenadeEMP>("Prefabs/Skill/SkillObject/Skill_Four/GrenadeEMP");
        }
        _grenadeEMPData = new GrenadeEMPData(skillData.skillBaseValue_1 + skillData.skillLevelValue_1 * skillData.skillLevel,
            skillData.skillBaseValue_2 + skillData.skillLevelValue_2 * skillData.skillLevel);

        EventBus.OnGrenadeBomb -= GrenadeBomb;
        EventBus.OnGrenadeBomb += GrenadeBomb;
    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        EventBus.OnGrenadeBomb -= GrenadeBomb;
    }



    public void GrenadeBomb(Vector3 position)
    {
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(_grenadeEMPPrefab, position, Quaternion.identity, null, _grenadeEMPData).GetComponent<GrenadeEMP>().Initialize();

    }
    public Skill_Four_Grenade(SkillData skillData) : base(skillData)
    {

    }
}

/// <summary>
/// Lux 제약 유탄 강화
/// </summary>
public class Skill_Five_Grenade : ActiveTech
{
    /// <summary>
    /// 착탄 지점 약화 폭발 프리팹
    /// </summary>
    [SerializeField]
    private WeakenArea _grenadeWeakenPrefab;
    private WeakenAreaData _grenadeWeakenData;

    public override void Activate(SkillManager skillManager, Player player)
    {
        base.Activate(skillManager, player);
        if (_grenadeWeakenPrefab == null)
        {
            _grenadeWeakenPrefab = Resources.Load<WeakenArea>("Prefabs/Skill/SkillObject/Skill_Five/GrenadeWeaken");
        }
        _grenadeWeakenData = new WeakenAreaData(skillData.skillBaseValue_1 + skillData.skillLevelValue_1 * skillData.skillLevel);

        EventBus.OnGrenadeBomb -= GrenadeBomb;
        EventBus.OnGrenadeBomb += GrenadeBomb;
    }

    public override void Deactivate(Player player, bool isAnotherSkill)
    {
        base.Deactivate(player, isAnotherSkill);
        EventBus.OnGrenadeBomb -= GrenadeBomb;
    }



    public void GrenadeBomb(Vector3 position)
    {
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(_grenadeWeakenPrefab, position, Quaternion.identity, null, _grenadeWeakenData).GetComponent<WeakenArea>().Initialize();

    }
    public Skill_Five_Grenade(SkillData skillData) : base(skillData)
    {

    }

    
}