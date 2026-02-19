using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// GridForge 기술 목록
/// </summary>

public class Skill_Four : SkillBase
{
    private SkillEffect _enhancedBoostPrefab;
    private Player _player;

    private SkillEffect _getPrefab; //이펙트

    #region 드론
    [SerializeField]
    private Drone dronePrefab;
    private List<Drone> droneList;
    public override void InitializeSkill(SkillManager skillManager)
    {
        base.InitializeSkill(skillManager);
        if (dronePrefab == null)
        {
            dronePrefab = Resources.Load<Drone>("Prefabs/Drone/DronePrefab");
        }
        if(droneList == null)
        {
            droneList = new List<Drone>();
        }
        else
        {
            droneList.Clear();
        }
    }
    #endregion

    #region 강화된 추진력
    private float plusMoveSpeed;
    private float plusMoveSpeedTime;
    #endregion
    public override void ActivateSkill(SkillData choosedSkill)
    {
        _player = _skillManager.playScene.player;

        if (_getPrefab == null)             //여기서 로드
        {
            _getPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/GetSkill_4");
        }
        PlayAcquireEffect();

        switch (choosedSkill.skillIdx)
        {
            // 찌릿찌릿
            case 40:
                ActiveTech skillAttack = new Skill_Four_Attack(choosedSkill);
                if (_skillManager.attackTech != null)
                {
                    _skillManager.attackTech.Deactivate(_skillManager.playScene.player, _skillManager.attackTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // EMP
            case 41:
                ActiveTech skillGrenade = new Skill_Four_Grenade(choosedSkill);
                if (_skillManager.bombTech != null)
                {
                    _skillManager.bombTech.Deactivate(_skillManager.playScene.player, _skillManager.bombTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillGrenade.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 급속냉동
            case 42:
                ActiveTech skillSPAttack = new Skill_Four_SPAttack(choosedSkill);
                if (_skillManager.skillTech != null)
                {
                    _skillManager.skillTech.Deactivate(_skillManager.playScene.player, _skillManager.skillTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillSPAttack.Activate(_skillManager, _skillManager.playScene.player);
                break;

            // 플라즈마 쉴드
            case 43:
                ActiveTech skillDash = new Skill_Four_Dash(choosedSkill);
                if (_skillManager.dashTech != null)
                {
                    _skillManager.dashTech.Deactivate(_skillManager.playScene.player, _skillManager.dashTech.skillData.skillIdx != choosedSkill.skillIdx);
                }
                skillDash.Activate(_skillManager, _skillManager.playScene.player);
                break;
            // 취약
            case 44:
                ActivateWeaken(choosedSkill);
                break;

            // 점진되는 고통
            case 45:
                _skillManager.playScene.player.playerModel.GetDebuffHandler().ConnectIncreasePain();
                break;

            // 드론무리
            case 46:
                ActivateSpawnDrone(choosedSkill.skillLevel);
                break;


            // 강화된 추진력
            case 47:
                ActivateThrust(choosedSkill);
                break;



            default:
                Debug.LogError("에러, 배정되지 않은 idx");
                break;

        }

    }

    #region 취약
    public void ActivateWeaken(SkillData choosedSkill)
    {
        if (choosedSkill.skillLevel == 1)
        {
            _skillManager.playerStatManager.AddWeakenPlusDamage(choosedSkill.skillBaseValue_1 + choosedSkill.skillLevelValue_1);
        }
        else
        {
            _skillManager.playerStatManager.AddWeakenPlusDamage(choosedSkill.skillLevelValue_1);
        }
    }
    #endregion

    #region 드론무리
    /// <summary>
    /// 드론 소환
    /// </summary>
    /// <param name="skillLevel"></param>
    public void ActivateSpawnDrone(int skillLevel)
    {
        Transform playerTransform = _skillManager.playScene.player.transform;
        Drone drone = GameManager.Instance.PoolManager.GetFromPool(dronePrefab,Vector3.zero , playerTransform.rotation,playerTransform).GetComponent<Drone>();
        drone.InitializeDrone(_skillManager.playScene.player);
        droneList.Add(drone);
        float x = 0 - (float)(skillLevel - 1) / 2;

        for (int i = 0; i < skillLevel; i++)
        {

            if (skillLevel % 2 == 1)
            {
                droneList[i].transform.localPosition = new Vector3((x + i), 1, -1 + Mathf.Abs(skillLevel / 2 - i) * Constants.DRONE_Z_POSITION);

            }
            else if (skillLevel == 4)
            {
                if (i == 0 || i == 3)
                {
                    droneList[i].transform.localPosition = new Vector3((x + i), 1, -1 + Constants.DRONE_Z_POSITION);

                }
                else
                {
                    droneList[i].transform.localPosition = new Vector3((x + i), 1, -1);

                }
            }
            else
            {
                droneList[i].transform.localPosition = new Vector3((x + i), 1, -1);
            }
        }

    }
    #endregion

    #region 강화된 추진력
    private void ActivateThrust(SkillData choosedSkill)
    {
        if (choosedSkill.skillLevel == 1)
        {
            // 대쉬 이동거리 value1 만큼 증가
            _skillManager.playerStatManager.AddPlayerDashDistanceMulti(choosedSkill.skillBaseValue_1 + choosedSkill.skillLevelValue_1);
        }
        else
        {
            skillManager.playerStatManager.AddPlayerDashDistanceMulti(choosedSkill.skillLevelValue_1);
        }
        // 대쉬 후 value3초간 value2 만큼 이동속도 증가 이벤트 연결
        plusMoveSpeed = choosedSkill.skillBaseValue_2 + choosedSkill.skillLevelValue_2 * choosedSkill.skillLevel;
        plusMoveSpeedTime = choosedSkill.skillBaseValue_3 + choosedSkill.skillLevelValue_3 * choosedSkill.skillLevel;
        
        _skillManager.playScene.player.OnDashStarted += plusMoveSpeedAfterDash;
    }

    private void plusMoveSpeedAfterDash()
    {
        skillManager.playerStatManager.AddPlayerMoveSpeedMulti(plusMoveSpeed);
        Quaternion spawnRot =_player.transform.rotation * Quaternion.Euler(0, 180f, 0);
        //로드
        if (_enhancedBoostPrefab == null)
        {
            _enhancedBoostPrefab = Resources.Load<SkillEffect>("Prefabs/Effect/Skill/Enhancedboost");
        }
        //생성
        GameManager.Instance.PoolManager.GetFromPool(_enhancedBoostPrefab, _player.transform.position, spawnRot, _player.transform);
        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("Enhancedboost", _player.transform.position);

        StartCoroutine(MinusMoveSpeedAfterDash());
    }
    IEnumerator  MinusMoveSpeedAfterDash()
    {
        yield return new WaitForSeconds(plusMoveSpeedTime);
        skillManager.playerStatManager.AddPlayerMoveSpeedMulti(-plusMoveSpeed);
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
