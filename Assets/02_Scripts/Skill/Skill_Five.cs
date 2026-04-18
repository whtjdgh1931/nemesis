using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// LUX 강화 목록
/// </summary>
public class Skill_Five : SkillBase
{
    // 캐터민 드래프트
    private Coroutine _draftCoroutine;
    private float _draftMoveSpeed;
    private float _draftTime;

    private SkillEffect _ketamineDriftPrefab;
    private Player _player;
    // 정제
    private float _addTotalDamage;

    private SkillEffect _getPrefab; //이펙트
    public override void ActivateSkill(SkillData choosedSkill)
    {
        _player = _skillManager.playScene.player;

        if (_getPrefab == null)             //여기서 로드
        {
            _getPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/GetSkill_5");
        }
        PlayAcquireEffect();

        switch (choosedSkill.skillIdx)
        {
            // 약화 약물
            case 50:
                ActiveTech skillAttack = new Skill_Five_Attack(choosedSkill);
                if (_skillManager.attackTech != null)
                {
                    _skillManager.attackTech.Deactivate(_skillManager.playScene.player, _skillManager.attackTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 환각 구름
            case 51:
                ActiveTech skillGrenade = new Skill_Five_Grenade(choosedSkill);
                if (_skillManager.bombTech != null)
                {
                    _skillManager.bombTech.Deactivate(_skillManager.playScene.player, _skillManager.bombTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillGrenade.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 섬망
            case 52:
                ActiveTech skillSPAttack = new Skill_Five_SPAttack(choosedSkill);
                if (_skillManager.skillTech != null)
                {
                    _skillManager.skillTech.Deactivate(_skillManager.playScene.player, _skillManager.skillTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillSPAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 아드레날린 레이스
            case 53:
                ActiveTech skillDash = new Skill_Five_Dash(choosedSkill);
                if (_skillManager.dashTech != null)
                {
                    _skillManager.dashTech.Deactivate(_skillManager.playScene.player, _skillManager.dashTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillDash.Activate(_skillManager, _skillManager.playScene.player);
                break;


            // 선택편향
            case 54:
                ActivateSelective(choosedSkill);
                break;

            // 불릿 타임
            case 55:
                ActiveBulletTime(choosedSkill);
                break;

            // 케타민 드리프트
            case 56:
                if(_draftCoroutine!=null)
                {
                    StopCoroutine(_draftCoroutine );
                    skillManager.playerStatManager.AddPlayerMoveSpeed(_draftMoveSpeed);
                }
                _draftCoroutine = null;
                skillManager.playScene.player.playerModel.PlayerHit -= Draft;
                _draftMoveSpeed = choosedSkill.skillBaseValue_1 + choosedSkill.skillLevelValue_1 * choosedSkill.skillLevel;
                _draftTime = choosedSkill.skillBaseValue_2+ choosedSkill.skillLevelValue_2*choosedSkill.skillLevel;
                skillManager.playScene.player.playerModel.PlayerHit += Draft;
                break;

            // 정제
            case 57:

                _addTotalDamage = choosedSkill.skillBaseValue_1+ choosedSkill.skillLevelValue_1*choosedSkill.skillLevel;
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnMonsterSpawned -= ConnectRefinement;
                skillManager.playScene.MapController.MonsterController.MonsterSpawner.OnMonsterSpawned += ConnectRefinement;
                break;



            default:
                Debug.LogError("에러, 배정되지 않은 idx");
                break;

        }
    }

    #region 불릿타임
    public void ActiveBulletTime(SkillData skill)
    {
        if (skill.skillLevel == 1)
        {
            _skillManager.playerStatManager.AddPlayerAvoidance(skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel);
        }
        else
        {
            _skillManager.playerStatManager.AddPlayerAvoidance(skill.skillLevelValue_1);
        }

    }
    #endregion

    #region 선택편향
    public void ActivateSelective(SkillData skill)
    {
        if (skill.skillLevel == 1)
        {
            _skillManager.playerStatManager.AddWeakenPlusDamage(skill.skillBaseValue_1 + skill.skillLevelValue_1 * skill.skillLevel);
        }
        else
        {
            _skillManager.playerStatManager.AddWeakenPlusDamage(skill.skillLevelValue_1);
        }
    }
    #endregion

    #region 캐타민 드래프트
    public void Draft(Transform monster)
    {
        // 이동속도 증가/재갱신 처리
        if (_draftCoroutine != null)
        {
            StopCoroutine(_draftCoroutine);
        }
        else
        {
            skillManager.playerStatManager.AddPlayerMoveSpeed(_draftMoveSpeed);
        }

        _draftCoroutine = StartCoroutine(StartDraftCoroutine(_draftMoveSpeed, _draftTime));

        //  맞을 때마다 항상 이펙트를 생성하도록 이동
        Quaternion spawnRot = _player.transform.rotation * Quaternion.Euler(0, 180f, 0);

        if (_ketamineDriftPrefab == null)
        {
            _ketamineDriftPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/KetamineDrift");
        }

        GameManager.Instance.PoolManager.GetFromPool(_ketamineDriftPrefab, _player.transform.position, spawnRot, _player.transform);
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("KetamineDrift", _player.transform.position);
    }


    public IEnumerator StartDraftCoroutine(float moveSpeed,float time)
    {
        yield return new WaitForSeconds(time);
        skillManager.playerStatManager.AddPlayerMoveSpeed(-moveSpeed);
        _draftCoroutine = null;
    }
    #endregion

    #region 정제
    public void ConnectRefinement(MonsterBase monster)
    {
        Action handler = null;
        handler = () =>
        {
            monster.OnDieEvent -= handler;
            AddTotalDamageWhenMonsterDie();
        };

        monster.OnDieEvent += handler;
    }

    public void AddTotalDamageWhenMonsterDie()
    {
        skillManager.playerStatManager.AddTotalMultiDamage(_addTotalDamage);
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
