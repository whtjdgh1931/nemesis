using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 추후 GEAR 기술 목록
/// </summary>
public class Skill_Three : SkillBase
{
    [SerializeField]
    private RedShift _redshiftPrefab;
    private float _redshiftSpeed;
    private float _redshiftDamage;
    private float _redshiftKnockBackDistance;
    private float _redshiftExtent;

    private horizon _horizonPrefab;
    private GameObject _playerHorizon;

    private SkillEffect _getPrefab; //이펙트

    public override void InitializeSkill(SkillManager skillManager)
    {
        base.InitializeSkill(skillManager);
        if (_redshiftPrefab == null)
        {
            _redshiftPrefab = Resources.Load<RedShift>("Prefabs/Skill/SkillObject/Skill_Three/RedShift");
        }
    }

    public override void ActivateSkill(SkillData choosedSkill)
    {
        if (_getPrefab == null)             //여기서 로드
        {
            _getPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/GetSkill_3");
        }
        PlayAcquireEffect();


        switch (choosedSkill.skillIdx)
        {
            // 중력자 무기
            case 30:
                ActiveTech skillAttack = new Skill_Three_Attack(choosedSkill);
                if (_skillManager.attackTech != null)
                {

                    _skillManager.attackTech.Deactivate(_skillManager.playScene.player, _skillManager.attackTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                skillAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 소용돌이
            case 31:
                ActiveTech SkillGrenade = new Skill_Three_Grenade(choosedSkill);
                if (_skillManager.bombTech != null)
                {

                    _skillManager.bombTech.Deactivate(_skillManager.playScene.player, _skillManager.bombTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                SkillGrenade.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 반동
            case 32:
                ActiveTech SkillSPAttack = new Skill_Three_SPAttack(choosedSkill);
                if (_skillManager.skillTech != null)
                {

                    _skillManager.skillTech.Deactivate(_skillManager.playScene.player, _skillManager.skillTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                SkillSPAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 절대영역
            case 33:
                ActiveTech skillDashAttack = new Skill_Three_Dash(choosedSkill);
                if (_skillManager.dashTech != null)
                {

                    _skillManager.dashTech.Deactivate(_skillManager.playScene.player, _skillManager.dashTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                skillDashAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;


            // 불운
            case 34:
                ActivateMisfortune(choosedSkill);
                break;

            // 적색 편이
            case 35:
                ActivateRedShift(choosedSkill);
                break;

            // 중력 증폭
            case 36:

                ActivateGravity(choosedSkill);

                break;

            // 사건의 지평선
            case 37:
                ActivateHorizon(choosedSkill);
                break;

            default:
                Debug.LogError("에러, 배정되지 않은 idx");
                break;
        }

    }

    #region 불운
    private void ActivateMisfortune(SkillData skill)
    {
        //처음 습득시
        if (skill.skillLevel == 1)
        {
            skillManager.playerStatManager.AddKockBackDamageMulti(skill.skillBaseValue_1 + skill.skillLevelValue_1);
        }
        // 그 이후는 레벨 계수만 추가
        else
        {
            skillManager.playerStatManager.AddKockBackDamageMulti(skill.skillLevelValue_1);

        }
    }
    #endregion
    #region 적색편이
    private void ActivateRedShift(SkillData skill)
    {
        _skillManager.playScene.player.playerModel.PlayerHit -= MakeRedshift;

        _redshiftKnockBackDistance = skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel;
        _redshiftDamage = skill.skillBaseValue_2 + skill.skillLevelValue_2 * skill.skillLevel;
        _redshiftExtent = skill.skillBaseValue_3 + skill.skillLevelValue_3 * skill.skillLevel;
        _redshiftSpeed = 15f;

        _skillManager.playScene.player.playerModel.PlayerHit += MakeRedshift;
    }
    private void MakeRedshift(Transform monsterTransform)
    {

        Vector3 playerPosition = _skillManager.playScene.player.transform.position;
        Vector3 direction = monsterTransform.position - playerPosition;
        direction.y = 0;
        direction.Normalize();
        if (direction == Vector3.zero)
        {
            direction =
            Constants.GetNearestObject(_skillManager.playScene.player.transform, skillManager.playScene.MapController.MonsterController.MonsterSpawner.ActiveMonsters).transform.position
            - playerPosition;
        }
        RedShiftData redShiftData = new RedShiftData(direction, _redshiftSpeed, _redshiftExtent, _redshiftKnockBackDistance, _redshiftDamage);
        playerPosition.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        GameManager.Instance.PoolManager.GetFromPool(_redshiftPrefab, playerPosition, rotation, null, redShiftData);
        GameManager.Instance.SoundManager.PlaySfxAt("RedShift", playerPosition);
    }
    #endregion
    #region 중력 증폭
    public void ActivateGravity(SkillData choosedSkill)
    {
        //처음 습득시
        if (choosedSkill.skillLevel == 1)
        {
            skillManager.playerStatManager.AddKnockBackDistance(choosedSkill.skillLevelValue_1 + choosedSkill.skillBaseValue_1);
        }
        // 그 이후는 레벨 계수만 추가
        else
        {
            skillManager.playerStatManager.AddKnockBackDistance(choosedSkill.skillLevelValue_1);

        }
    }
    #endregion
    #region 사건의 지평선
    public void ActivateHorizon(SkillData skill)
    {
        if (_playerHorizon != null)
        {
            GameManager.Instance.PoolManager.ReleaseToPool(_playerHorizon);
        }

        if (_horizonPrefab == null)
        {
            _horizonPrefab = Resources.Load<horizon>("Prefabs/Skill/SkillObject/Skill_Three/horizon");
        }

        Transform player = skillManager.playScene.player.transform;


        _playerHorizon = GameManager.Instance.PoolManager.GetFromPool(
            _horizonPrefab, player.position, Quaternion.identity,
            player,
            new horizonData(skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel, skillManager.playScene.player));
        

    }
    #endregion

    private void PlayAcquireEffect()
    {
        var player = _skillManager.playScene.player;
        var pos = _skillManager.playScene.player.transform.position;

        GameManager.Instance.PoolManager.GetFromPool(
            _getPrefab,
            pos,
            Quaternion.identity,
            player.transform
        );

        GameManager.Instance.SoundManager.PlaySfxAt("GetSkill", pos);
    }
}



