using System;
using UnityEngine;

/// <summary>
/// 플레이어가 획득하는 액티브형 기술 기본 클래스
/// Use()는 실행 시
/// Use(Transform)은 적중시
/// </summary>
public abstract class ActiveTech
{
    [Header("기술 정보")]
    [SerializeField] protected SkillData _skillData; // 기술의 이름
    public SkillData skillData { get {  return _skillData; } }

    public string TechIdx => _skillData.skillIdx.ToString(); // 기술의 번호를 반환하는 속성
    public string TechDescription => _skillData.skillScript; // 기술의 설명을 반환하는 속성
    // public Sprite TechIcon => skillData.skillImagePath; // 기술의 아이콘 이미지를 반환하는 속성
    public int TechLevel => _skillData.skillLevel; // 기술의 레벨을 반환하는 속성


    /// <summary>
    /// 기술이 추가될 때 호출되는 메서드
    /// </summary>
    public virtual void Activate(SkillManager skillManager, Player player)
    {
       
        switch (_skillData.skillTag)
        {
            case Constants.SKILL_TAG_ATTACK:
                skillManager.SetAttackTech(this);
                break;
            case Constants.SKILL_TAG_GEN:
								skillManager.SetBombTech(this);
                break;
            case Constants.SKILL_TAG_SP:
								skillManager.SetSkillTech(this);
                break;
            case Constants.SKILL_TAG_DASH:
								skillManager.SetDashTech(this);
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// 기술이 제거될 때 호출되는 메서드
    /// </summary>
    public virtual void Deactivate(Player player,bool isAnotherSkill)
    {
        _skillData.RemoveList(isAnotherSkill);
    }

    /// <summary>
    /// 기술이 적중될 때 호출되는 메서드
    /// </summary>  
    public virtual void HitEnemy(WeaponType weaponType, ATTACKTYPE attackType, Transform monster,Transform attacker = null)
    {
    }

    /// <summary>
    /// 공격 시도 시 호출되는 메서드
    /// </summary>
    public virtual void ActiveTry(Player player)
    {

    }

    public ActiveTech(SkillData skillData)
    {
        _skillData = skillData;
    }
}
