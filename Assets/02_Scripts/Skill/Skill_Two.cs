using System;
using UnityEngine;

/// <summary>
/// 추후 파이로 하트 모션
/// </summary>
public class Skill_Two : SkillBase
{
    /// <summary>
    /// 폭사 프리팹
    /// </summary>
    [SerializeField]
    private ExplosionDeath _explosionDeathPrefab;
    private ExplosionDeathData _explosionDeathData;


    private SkillEffect _getPrefab; //이펙트

    public override void ActivateSkill(SkillData choosedSkill)
    {
        if (_getPrefab == null)             //여기서 로드
        {
            _getPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/GetSkill_2");
        }
        PlayAcquireEffect();


        switch (choosedSkill.skillIdx)
        {
            // 베고, 찌르고, 부수고
            case 20:
                ActiveTech skillAttack = new Skill_Two_Attack(choosedSkill);
                if (_skillManager.attackTech != null)
                {
                    _skillManager.attackTech.Deactivate(_skillManager.playScene.player, _skillManager.attackTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                skillAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 폭발!
            case 21:
                ActiveTech skillGrenade = new Skill_Two_Grenade(choosedSkill);
                if (_skillManager.bombTech != null)
                {

                    _skillManager.bombTech.Deactivate(_skillManager.playScene.player, _skillManager.bombTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                skillGrenade.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 비밀무기
            case 22:
                ActiveTech skillSPAttack = new Skill_Two_SPAttack(choosedSkill);
                if (_skillManager.skillTech != null)
                {

                    _skillManager.skillTech.Deactivate(_skillManager.playScene.player, _skillManager.skillTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                skillSPAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 깜짝선물
            case 23:
                ActiveTech skillDashAttack = new Skill_Two_Dash(choosedSkill);
                if (_skillManager.dashTech != null)
                {

                    _skillManager.dashTech.Deactivate(_skillManager.playScene.player, _skillManager.dashTech.skillData.skillIdx != choosedSkill.skillIdx);

                }
                skillDashAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 전투장비 강화
            case 24:
                OnCombatEquipment(choosedSkill);
                break;

            // 개선된 폭격
            case 25:
                ActiveImprovedBomb(choosedSkill);
                break;

            // 폭사
            case 26:
                _explosionDeathData = new ExplosionDeathData(
                    choosedSkill.skillBaseValue_1 + choosedSkill.skillLevelValue_1 * choosedSkill.skillLevel,
                    choosedSkill.skillBaseValue_2 + choosedSkill.skillLevelValue_2 * choosedSkill.skillLevel
                    );

                // 몬스터 스포너에 몬스터 생성시 이벤트에 연결
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnMonsterSpawned -= ConnectMakeExpolsion;
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnMonsterSpawned += ConnectMakeExpolsion;
                break;

            // 기폭제
            case 27:
                // 범위 증가
                ActivePriming(choosedSkill);
                break;

            default:
                Debug.LogError("에러, 배정되지 않은 idx");
                break;

        }
    }

    #region 폭사
    public void ConnectMakeExpolsion(MonsterBase monster)
    {
        Action handler = null;
        handler = () =>
        {
            monster.OnDieEvent -= handler; 
            MakeExplosion(monster.transform);
        };

        monster.OnDieEvent += handler;
    }

    public void MakeExplosion(Transform monsterTransform)
    {
        Vector3 position = monsterTransform.position;
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(_explosionDeathPrefab, position, _explosionDeathPrefab.transform.rotation,null, _explosionDeathData).GetComponent<ExplosionDeath>().Initialize();
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("ExplosiveDeath", position);
    }
    #endregion

    #region 전투장비 강화

    /// <summary>
    /// 전투장비 이벤트 발행
    /// </summary>
    /// <param name="skillData"></param>
    public void OnCombatEquipment(SkillData choosedSkill)
    {
        if (choosedSkill.skillLevel == 1)
        {
            skillManager.playerStatManager.AddTotalMultiDamage(choosedSkill.skillLevelValue_1+choosedSkill.skillBaseValue_1);
        }
        else
        {
            skillManager.playerStatManager.AddTotalMultiDamage(choosedSkill.skillLevelValue_1);

        }
    }

    #endregion

    #region 개선된 폭격
    public void ActiveImprovedBomb(SkillData choosedSkill)
    {

        if (choosedSkill.skillLevel == 1)
        {
            _skillManager.playerStatManager.AddGrenadeCoolTimeMulti(choosedSkill.skillBaseValue_1+choosedSkill.skillLevelValue_1);
        }
        else
        {
            _skillManager.playerStatManager.AddGrenadeCoolTimeMulti(choosedSkill.skillLevelValue_1);
        }
    }
    #endregion

    #region 기폭제
    public void ActivePriming(SkillData choosedSkill)
    {
        if (choosedSkill.skillLevel == 1)
        {
            _skillManager.playerStatManager.AddPlayerAreaExtent(choosedSkill.skillBaseValue_1+choosedSkill.skillLevelValue_1*choosedSkill.skillLevel);

        }
        else
        {
            _skillManager.playerStatManager.AddPlayerAreaExtent(choosedSkill.skillLevelValue_1);

        }
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


