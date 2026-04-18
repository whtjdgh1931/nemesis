using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 추후 비브르 기술로 변경
/// </summary>
public class Skill_One : SkillBase
{
    private Coroutine _autoHeal;

    /// <summary>
    /// 피격시 생성할 독 프리팹
    /// </summary>
    [SerializeField]
    private PoisonSpread _hitPoisonSpreadPrefab;
    private PoisonSpreadData _hitPoisonSpreadData;
    private Action<Transform> PoisonSpreadAction;

    

    /// <summary>
    /// 진화에 필요한 스택
    /// </summary>
    private int levelupStack;

    private SkillEffect _getPrefab; //이펙트


    public override void ActivateSkill(SkillData choosedSkill)
    {
        if (_getPrefab == null)             //여기서 로드
        {
            _getPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/GetSkill_1");
        }
        PlayAcquireEffect();

        switch (choosedSkill.skillIdx)
        {
            // 독사의 앞니
            case 10:
                ActiveTech skillAttack = new Skill_One_Attack(choosedSkill);
                if (_skillManager.attackTech != null)
                {
                    _skillManager.attackTech.Deactivate(_skillManager.playScene.player, _skillManager.attackTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 포자 퍼뜨리기
            case 11:
                ActiveTech skillGrenade = new Skill_One_Grenade(choosedSkill);
                if (_skillManager.bombTech != null)
                {
                    if (_skillManager.bombTech.skillData.skillIdx != choosedSkill.skillIdx)
                    {
                        _skillManager.bombTech.Deactivate(_skillManager.playScene.player, _skillManager.bombTech.skillData.skillIdx != choosedSkill.skillIdx);
                    }
                }
                skillGrenade.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 피의 갈증
            case 12:
                ActiveTech skillSPAttack = new Skill_One_SPAttack(choosedSkill);
                if (_skillManager.skillTech != null)
                {
                    if (_skillManager.skillTech.skillData.skillIdx != choosedSkill.skillIdx)
                    {
                        _skillManager.skillTech.Deactivate(_skillManager.playScene.player, _skillManager.skillTech.skillData.skillIdx != choosedSkill.skillIdx);
                    }
                }
                skillSPAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 약육강식
            case 13:
                ActiveTech skillDash = new Skill_One_Dash(choosedSkill);
                if (_skillManager.dashTech != null)
                {
                    if (_skillManager.dashTech.skillData.skillIdx != choosedSkill.skillIdx)
                    {
                        _skillManager.dashTech.Deactivate(_skillManager.playScene.player, _skillManager.dashTech.skillData.skillIdx != choosedSkill.skillIdx);
                    }
                }
                skillDash.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 넘치는 활력
            case 14:
                _skillManager.playScene.player.playerModel.SetMaxHp((int)choosedSkill.skillLevelValue_1);
                break;

            // 초재생
            case 15:
                skillManager.playScene.MapController.OnRoomStart += StartAutoHeal;
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnAllWavesCompleted += StopAutoHeal;
                break;

            // 독성혈액
            case 16:
                if (choosedSkill.skillLevel == 1)
                {
                    // 플레이어 모델에 받는데미지 감소 계수를 추가하여 10퍼센트 
                    _skillManager.playerStatManager.AddReduceDamagePercent(choosedSkill.skillBaseValue_1 + choosedSkill.skillLevelValue_1);
                    // 데이터 제작
                    _hitPoisonSpreadData = new PoisonSpreadData(choosedSkill.skillBaseValue_2 + choosedSkill.skillLevelValue_2);
                }
                else
                {
                    _skillManager.playScene.player.playerModel.PlayerHit -= PoisonSpreadAction;
                    // 플레이어 모델에 받는데미지 감소 계수를 추가하여 10퍼센트 
                    _skillManager.playerStatManager.AddReduceDamagePercent(choosedSkill.skillLevelValue_1);
                }

                // 데이터 제작
                _hitPoisonSpreadData = new PoisonSpreadData(choosedSkill.skillBaseValue_2 + choosedSkill.skillLevelValue_2);
                PoisonSpreadAction = (transform) => SpreadPoison(_skillManager.playScene.player);
                _skillManager.playScene.player.playerModel.PlayerHit += PoisonSpreadAction;
                break;

            // 진화
            case 17:
                // 연결시 SkillLevelUp이 완료되면 다음 과정으로 넘어갈 수 있게
                if(choosedSkill.skillLevel == 1)
                {
                    levelupStack = 0;
                }
                EventBus.OnEvolution += SkillLevelUp;
                break;

            default:
                Debug.LogError("에러, 배정되지 않은 idx");
                break;
        }

    }

    #region 초재생

    //초재생 코루틴
    public IEnumerator StartAutoHealRoutine()
    {
        int stack = 0;
        while (stack < 20)
        {
            yield return new WaitForSeconds(Constants.HEAL_SECOND);
            _skillManager.playScene.player.playerModel.Heal(Constants.HEAL_AMOUNT);
            stack++;
        }
        StopAutoHeal();
    }


    /// <summary>
    /// 초재생 실행
    /// </summary>
    public void StartAutoHeal()
    {
        _autoHeal = StartCoroutine(StartAutoHealRoutine());
    }

    /// <summary>
    /// 초재생 종료
    /// </summary>
    public void StopAutoHeal()
    {
        if(_autoHeal == null)
        {
            return;
        }
        StopCoroutine(_autoHeal);
        _autoHeal = null;
    }
    #endregion

    #region 독성혈액

    public void SpreadPoison(Player player)
    {
        Vector3 position = player.transform.position;
        position.y = 0;
        PoisonSpread poisonSpread = GameManager.Instance.PoolManager.GetFromPool(_hitPoisonSpreadPrefab, position,_hitPoisonSpreadPrefab.transform.rotation,player.transform,_hitPoisonSpreadData).GetComponent<PoisonSpread>();
        poisonSpread.Initialize();
    }

    #endregion

    
    

    

    #region 진화
    public void SkillLevelUp()
    {
        ++levelupStack;
        if (levelupStack >= 2)
        {
            // 업그레이드 가능 스킬이 없다면 리턴
            if (!(_skillManager.upgradeSkillList.Count > 0))
            {
                return;
            }
            // 랜덤한 스킬 레벨 업
            SkillData choosedskill = _skillManager.upgradeSkillList[UnityEngine.Random.Range(0, _skillManager.upgradeSkillList.Count)];
            choosedskill.ChooseSkill();
            choosedskill.skillCompany.ActivateSkill(choosedskill);

            // 스택 초기화
            levelupStack = 0;
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


