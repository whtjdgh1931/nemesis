using System;

using UnityEngine;
using UnityEngine.UIElements;


public class Skill_Collab : SkillBase
{
    /// <summary>
    /// 폭군 데미지
    /// </summary>
    private float _tyrantDamage = 0;

    private SkillEffect _tyrantEffectPrefab;
    /// <summary>
    /// 최상위 포식자 계수
    /// </summary>
    private float _predatorHeal = 0f;
    private float _predatorTime = 0f;
    private WeakenArea _predatorPrefab;
    private WeakenAreaData _predatorData;

    private SkillEffect _apexPredatorPrefab;

    private Player _player;

    /// <summary>
    /// GravityFlare 관련 필드
    /// </summary>
    private GravityFlareRocketData _rocketData;
    private GravityFlareRocketExplosionData _explosionData;
    private GravityFlareRocket _rocketPrefab;
    private GravityFlareRocketExplosion _explosionPrefab;

    /// <summary>
    /// 전자기 폭풍 관련 필드
    /// </summary>
    private elecVortex _elecVortexPrefab;

    /// <summary>
    /// 전기인간 관련 필드
    /// </summary>
    private electricMan _electricManPrefab;


    public override void InitializeSkill(SkillManager skillManager)
    {
        base.InitializeSkill(skillManager);

    }

    /// <summary>
    /// 콜라보 스킬의 경우 해당하는 회사 개수 up
    /// </summary>
    /// <param name="skilldata"></param>
    /// <param name="num"></param>
    public override void SkillNumUp(SkillData skilldata, int num)
    {
        switch (skilldata.skillIdx)
        {
            case Constants.INDEX_FIVE_ONE:
                _skillManager.skill_One.SkillNumUp(skilldata, num);
                _skillManager.skill_Five.SkillNumUp(skilldata, num);
                break;
            case Constants.INDEX_ONE_TWO:
                _skillManager.skill_One.SkillNumUp(skilldata, num);
                _skillManager.skill_Two.SkillNumUp(skilldata, num);
                break;
            case Constants.INDEX_TWO_THREE:
                _skillManager.skill_Two.SkillNumUp(skilldata, num);
                _skillManager.skill_Three.SkillNumUp(skilldata, num);
                break;
            case Constants.INDEX_THREE_FOUR:
                _skillManager.skill_Three.SkillNumUp(skilldata, num);
                _skillManager.skill_Four.SkillNumUp(skilldata, num);
                break;
            case Constants.INDEX_FOUR_FIVE:
                _skillManager.skill_Five.SkillNumUp(skilldata, num);
                _skillManager.skill_Four.SkillNumUp(skilldata, num);
                break;
        }

    }


    public override void ActivateSkill(SkillData choosedSkill)
    {
        _player = _skillManager.playScene.player;
        switch (choosedSkill.skillIdx)
        {
            // 폭군
            case 60:
                ActiveTyrant(choosedSkill);
                break;

            // 최상위 포식자
            case 61:
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnMonsterSpawned -= ConnectWeakneHeal;
                ActivePredator(choosedSkill);
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnMonsterSpawned += ConnectWeakneHeal;
                break;

            // GravityFlare
            case 62:
                ActiveGravityFlare(choosedSkill);
                break;

            // 전자기 폭풍
            case 63:
                ActivateElecVortex(choosedSkill);
                break;

            // 전기 인간
            case 64:
                ActivateElectricMan(choosedSkill);
                break;

            default:
                Debug.LogError("배정되지 않은 idx");
                break;

        }
    }

    #region 폭군
    public void ActiveTyrant(SkillData skill)
    {
        if (_tyrantEffectPrefab == null)
        {
            _tyrantEffectPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/Tyrant");  
        }

        if (skill.skillLevel == 1)
            skillManager.playerStatManager.AddTotalMultiDamage(skill.skillBaseValue_1 + skill.skillLevelValue_1);
        else
            skillManager.playerStatManager.AddTotalMultiDamage(skill.skillLevelValue_1);

        skillManager.playScene.player.playerModel.OnHeal -= DamageNearestMonster;
        _tyrantDamage = skill.skillLevelValue_2 * skill.skillLevel + skill.skillBaseValue_2;
        skillManager.playScene.player.playerModel.OnHeal += DamageNearestMonster;
    }

    public void DamageNearestMonster(int currentHp, int MaxHp)
    {
        GameObject nearestObject = Constants.GetNearestObject(skillManager.playScene.player.transform, skillManager.playScene.MapController.MonsterController.MonsterSpawner.ActiveMonsters);
        if (nearestObject != null)
        {
            MonsterBase nearestMonster = nearestObject.GetComponent<MonsterBase>();
            if (nearestMonster != null)
            {
                nearestMonster.TakeDamage(_tyrantDamage);

                Vector3 spawnPos = nearestMonster.transform.position + nearestMonster.transform.forward * 0.5f;

                if (_tyrantEffectPrefab != null)
                {
                    GameManager.Instance.PoolManager.GetFromPool(_tyrantEffectPrefab,spawnPos,_tyrantEffectPrefab.transform.rotation);
                }
                GameManager.Instance.SoundManager.PlaySfxAt("Tyrant", spawnPos);

            }
            else
            {
            }
        }
        else
        {
            Debug.LogError("게임 오브젝트가 없음");
        }
    }
    #endregion

    #region 최상위 포식자
    public void ActivePredator(SkillData skill)
    {
        _predatorHeal = (skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel);
        _predatorTime = (skill.skillBaseValue_3 + skill.skillLevelValue_3 * skill.skillLevel);
        _predatorData = new WeakenAreaData(skill.skillBaseValue_2 + skill.skillLevelValue_2 * skill.skillLevel);
  
        if (_predatorPrefab == null)
        {
            _predatorPrefab = Resources.Load<WeakenArea>("Prefabs/Skill/SkillObject/Skill_Collab/PredatorPrefab");
        }
        //로드
        if (_apexPredatorPrefab == null)
        {
						_apexPredatorPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/ApexPredator_Player");
				}


    }

    public void ConnectWeakneHeal(MonsterBase monster)
    {
        Action handler = null;
        handler = () =>
        {
            monster.OnDieEvent -= handler;
            WeakenMonsterKill(monster);
        };

        monster.OnDieEvent += handler;
    }

    public void WeakenMonsterKill(MonsterBase monster)
    {
        if (!monster.isWeaken) return;
        skillManager.playScene.player.playerModel.Heal((int)_predatorHeal);
        MakeWeakenArea(monster);
        skillManager.playScene.player.playerModel.PlayerInvincibility(_predatorTime);

        GameManager.Instance.PoolManager.GetFromPool(_apexPredatorPrefab, _player.transform.position, Quaternion.identity);  //생성
    }

    public void MakeWeakenArea(MonsterBase monster)
    {
        Vector3 position = monster.transform.position;
        position.y = 0;
				if (_predatorPrefab == null || _apexPredatorPrefab == null)
				{
						Debug.LogError("PoolableObject 또는 prefab이 null입니다.");
				}
        GameManager.Instance.PoolManager.GetFromPool(_predatorPrefab, position, Quaternion.identity, null, _predatorData).GetComponent<WeakenArea>().Initialize();

		}
		#endregion

		#region GravityFlare
		public void ActiveGravityFlare(SkillData skill)
    {
        if(_explosionPrefab == null)
        {
            _explosionPrefab = Resources.Load<GravityFlareRocketExplosion>("Prefabs/Skill/SkillObject/Skill_Collab/GravityFlareRocketExplosion");
        }
        if(_rocketPrefab == null)
        {
            _rocketPrefab = Resources.Load<GravityFlareRocket>("Prefabs/Skill/SkillObject/Skill_Collab/GravityFlareRocket");
        }

        _explosionData = new GravityFlareRocketExplosionData(skill.skillBaseValue_2 + skill.skillLevelValue_2 * skill.skillLevel,
            skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel);
        EventBus.OnMonsterKnockBack -= ShotRocketToKnockBackMonster;
        EventBus.OnMonsterKnockBack += ShotRocketToKnockBackMonster;
    }

    public void ShotRocketToKnockBackMonster(Vector3 position)
    {

        Vector3 playerPosition = skillManager.playScene.player.transform.position;
        position.y = 0;
        playerPosition.y = 0;
        Vector3 direction = position - playerPosition;
        direction.Normalize();
        playerPosition.y += Constants.MISSILIE_HEIGHT;

        _rocketData = new GravityFlareRocketData( _explosionData, _explosionPrefab);
        GameManager.Instance.PoolManager.GetFromPool(_rocketPrefab, playerPosition, Quaternion.LookRotation(direction), null, _rocketData);
        
    }
    #endregion

    #region 전자기 폭풍
    public void ActivateElecVortex(SkillData skill)
    {
        if(_elecVortexPrefab ==null)
        {
            _elecVortexPrefab = Resources.Load<elecVortex>("Prefabs/Skill/SkillObject/Skill_Collab/ElecVortex");
        }
        Instantiate(_elecVortexPrefab, skillManager.playScene.player.transform.position,Quaternion.identity).Initialize(
           skill.skillBaseValue_3 + skill.skillLevelValue_3 * skill.skillLevel,
            skill.skillBaseValue_2 + skill.skillLevelValue_2 * skill.skillLevel,
            skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel,
            skillManager.playScene.MapController);

        //
        GameManager.Instance.SoundManager.PlaySfxAt("MagneticStorm", skillManager.playScene.player.transform.position, true);

    }
    #endregion

    #region 전기 인간
    public void ActivateElectricMan(SkillData skill)
    {
        if(_electricManPrefab == null)
        {
            _electricManPrefab = Resources.Load<electricMan>("Prefabs/Skill/SkillObject/Skill_Collab/ElectricMan");
        }

        electricMan electric =  Instantiate(_electricManPrefab, skillManager.playScene.player.transform);
        skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnAllWavesCompleted += electric.ClearDictionary;
    }
    #endregion
}
