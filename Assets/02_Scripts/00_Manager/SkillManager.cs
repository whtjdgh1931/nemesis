using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    #region skill
    private Skill_One _skill_One;
    public Skill_One skill_One { get { return _skill_One; } }

    private Skill_Two _skill_Two;
    public Skill_Two skill_Two { get { return _skill_Two; } }

    private Skill_Three _skill_Three;
    public Skill_Three skill_Three { get { return _skill_Three; } }

    private Skill_Four _skill_Four;
    public Skill_Four skill_Four { get { return _skill_Four; } }

    private Skill_Five _skill_Five;
    public Skill_Five skill_Five { get { return _skill_Five; } }

    private Skill_Collab _skill_Collab;
    public Skill_Collab skill_Collab { get { return _skill_Collab; } }

    private Skill_Mutant _skill_Mutant;
    public Skill_Mutant skill_Mutant { get { return _skill_Mutant; } }
    #endregion

    private PlayScene _playerScenes;
    public PlayScene playScene => _playerScenes;

    private PlayerStatManager _playerStatManager;
    public PlayerStatManager playerStatManager { get { return _playerStatManager; } }



    /// <summary>
    /// 플레이어 할당
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayScene(PlayScene playScene)
    {
        _playerScenes = playScene;
    }



    #region reinforce
    /// <summary>
    /// 일반공격 강화 기술
    /// </summary>
    [SerializeField]
    private ActiveTech _attackTech;
    public ActiveTech attackTech { get { return _attackTech; } }
    public void SetAttackTech(ActiveTech attackTech)
    {
        _attackTech = attackTech;
    }

    /// <summary>
    /// 유탄 강화 기술
    /// </summary>
    private ActiveTech _bombTech;
    public ActiveTech bombTech { get { return _bombTech; } }
    public void SetBombTech(ActiveTech bombTech)
    {
        _bombTech = bombTech;
    }

    /// <summary>
    /// 특수 공격 강화 기술
    /// </summary>
    private ActiveTech _skillTech;
    public ActiveTech skillTech { get { return _skillTech; } }
    public void SetSkillTech(ActiveTech skillTech)
    {
        _skillTech = skillTech;
    }

    /// <summary>
    /// 대쉬 강화 기술
    /// </summary>
    private ActiveTech _dashTech;
    public ActiveTech dashTech { get { return _dashTech; } }
    public void SetDashTech(ActiveTech dashTech)
    {
        _dashTech = dashTech;
    }
    #endregion

    /// <summary>
    /// 업그레이드 가능 스킬들 목록
    /// </summary>
    private List<SkillData> _upgradeSkillList = new List<SkillData>();
    public List<SkillData> upgradeSkillList { get { return _upgradeSkillList; } }

    public void InitializeSkillManager()
    {

        _skill_One = GetComponent<Skill_One>();
        _skill_Two = GetComponent<Skill_Two>();
        _skill_Three = GetComponent<Skill_Three>();
        _skill_Four = GetComponent<Skill_Four>();
        _skill_Five = GetComponent<Skill_Five>();
        _skill_Collab = GetComponent<Skill_Collab>();
        _skill_Mutant = GetComponent<Skill_Mutant>();

        _skill_One.InitializeSkill(this);
        _skill_Two.InitializeSkill(this);
        _skill_Three.InitializeSkill(this);
        _skill_Four.InitializeSkill(this);
        _skill_Five.InitializeSkill(this);
        _skill_Collab.InitializeSkill(this);
        _skill_Mutant.InitializeSkill(this);

        _playerStatManager = GameManager.Instance.PlayerStatManager;

    }
        
    public void Reset()
    {
        if (_attackTech != null && _playerScenes.player != null)
        {
            _attackTech.Deactivate(_playerScenes.player, true);
        }
        if (_dashTech != null && _playerScenes.player != null)
        {
            _dashTech.Deactivate(_playerScenes.player, true);
        }
        if(_bombTech!= null && _playerScenes.player != null)
        {
            _bombTech.Deactivate(_playerScenes.player, true);
        }
        if (_skillTech!= null && _playerScenes.player!=null)
        {
            _skillTech.Deactivate(_playerScenes.player, true);
        }
        _attackTech = null;
        _dashTech = null;
        _bombTech = null;
        _skillTech = null;
        _skill_One.InitializeSkill(this);
        _skill_Two.InitializeSkill(this);
        _skill_Three.InitializeSkill(this);
        _skill_Four.InitializeSkill(this);
        _skill_Five.InitializeSkill(this);
        _skill_Collab.InitializeSkill(this);
        _skill_Mutant.InitializeSkill(this);
        _upgradeSkillList.Clear();
    }

    /// <summary>
    /// 뽑은 스킬 리스트 순회하여 리스트 제작
    /// </summary>
    public List<SkillData> GetChooseSkillList()
    {
        List<SkillData> currentSkillData = new List<SkillData>();


        if (_skill_Mutant.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_Mutant.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }
        }

        if (_skill_Collab.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_Collab.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }
        }

        if (_skill_One.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_One.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }
        }

        if (_skill_Two.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_Two.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }

        }

        if (_skill_Three.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_Three.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }
        }

        if (_skill_Four.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_Four.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }
        }

        if (_skill_Five.GetNumberSkillList() != 0)
        {
            foreach (SkillData skillData in _skill_Five.currentSkillData)
            {
                currentSkillData.Add(skillData);
            }
        }



        if (currentSkillData.Count > 0)
            return currentSkillData;
        else return null;

    }




    /// <summary>
    /// 입력받은 개수만큼 가중치에 따른 스킬 회사 반환
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public TechSelectPackType[] GetSkillPackTypes(int count)
    {
        int skillOneNum = _skill_One.skillNum + 1;
        int skillTwoNum = _skill_Two.skillNum + 1;
        int skillThreeNum = _skill_Three.skillNum + 1;
        int skillFourNum = _skill_Four.skillNum + 1;
        int skillFiveNum = _skill_Five.skillNum + 1;

        // 스킬팩을 다 뽑았고, 콜라보 스킬 조건이 안되면 가중치 0
        if (skill_One.skillList.Count == 0 && CheckCollabo(skill_One, out _) == false)
        {
            skillOneNum = 0;
        }
        if (skill_Two.skillList.Count == 0 && CheckCollabo(skill_Two, out _) == false)
        {
            skillTwoNum = 0;
        }
        if (skill_Three.skillList.Count == 0 && CheckCollabo(skill_Three, out _) == false)
        {
            skillThreeNum = 0;
        }
        if (skill_Four.skillList.Count == 0 && CheckCollabo(skill_Four, out _) == false)
        {
            skillFourNum = 0;
        }
        if (skill_Five.skillList.Count == 0 && CheckCollabo(skill_Five, out _) == false)
        {
            skillFiveNum = 0;
        }


        int totalNum = skillOneNum + skillTwoNum + skillThreeNum + skillFourNum + skillFiveNum;

        TechSelectPackType[] packTypes = new TechSelectPackType[count];
        TechSelectPackType packType;
        for (int i = 0; i < count; i++)
        {
            int totalChance = UnityEngine.Random.Range(0, totalNum);
            if (totalChance < skillOneNum)
            {
                packType = TechSelectPackType.Company1;
            }
            else if (totalChance < skillOneNum + skillTwoNum)
            {
                packType = TechSelectPackType.Company2;
            }
            else if (totalChance < skillOneNum + skillTwoNum + skillThreeNum)
            {
                packType = TechSelectPackType.Company3;
            }
            else if (totalChance < skillOneNum + skillTwoNum + skillThreeNum + skillFourNum)
            {
                packType = TechSelectPackType.Company4;
            }
            else
            {
                packType = TechSelectPackType.Company5;
            }
            if (Array.Exists(packTypes, x => x == packType))
            {
                i--;
                continue;
            }
            else
            {
                packTypes[i] = packType;
            }


        }
        return packTypes;
    }

    /// <summary>
    /// 현재 가지고 있는 총 스킬 개수
    /// </summary>
    /// <returns></returns>
    public int GetTotalSkillNumber()
    {
        int skillOneNum = _skill_One.GetNumberSkillList();
        int skillTwoNum = _skill_Two.GetNumberSkillList();
        int skillThreeNum = _skill_Three.GetNumberSkillList();
        int skillFourNum = _skill_Four.GetNumberSkillList();
        int skillFiveNum = _skill_Five.GetNumberSkillList();

        return skillOneNum + skillTwoNum + skillThreeNum + skillFourNum + skillFiveNum;
    }



    /// <summary>
    /// 콜라보 스킬 조건 검사
    /// </summary>
    /// <param name="skillCompany"></param>
    public bool CheckCollabo(SkillBase skillCompany, out List<int> indexList)
    {
        indexList = new List<int>();
        bool bCheck = false;
        // 현재 고른 기술팩의 조건 검사
        if (skillCompany.currentSkillData.Count < Constants.COLLABCNT)
        {
            indexList = null;
            return bCheck;
        }

        if (skillCompany == _skill_One)
        {
            // 연관된 기술 회사 조건 검사
            if (_skill_Two.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_ONE_TWO);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }

            if (_skill_Five.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_FIVE_ONE);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }
        }
        else if (skillCompany == _skill_Two)
        {
            // 연관된 기술 회사 조건 검사
            if (_skill_Three.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_TWO_THREE);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }

            if (_skill_One.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_ONE_TWO);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }
        }
        else if (skillCompany == _skill_Three)
        {
            // 연관된 기술 회사 조건 검사
            if (_skill_Four.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_THREE_FOUR);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }

            if (_skill_Two.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_TWO_THREE);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }
        }
        else if (skillCompany == _skill_Four)
        {
            // 연관된 기술 회사 조건 검사
            if (_skill_Five.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_FOUR_FIVE);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }

            if (_skill_Three.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_THREE_FOUR);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }
        }
        else if (skillCompany == _skill_Five)
        {
            // 연관된 기술 회사 조건 검사
            if (_skill_Four.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_FOUR_FIVE);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }

            if (_skill_One.currentSkillData.Count >= Constants.COLLABCNT)
            {
                int index = _skill_Collab.skillList.FindIndex(skillData => skillData.skillIdx == Constants.INDEX_FIVE_ONE);
                if (index != Constants.NOCONTAININDEX)
                {
                    indexList.Add(index);
                }

                bCheck = true;
            }
        }

        // 콜라보 스킬 조건 만족하는지 반환
        return bCheck;
    }


#if UNITY_EDITOR
    /// <summary>
    /// 스킬 인덱스로 해당 스킬을 찾아 ActivateSkill 실행
    /// </summary>
    public void ActivateSkillByIndex(int skillIdx)
    {
        SkillData targetSkill = null;
        SkillBase owner = null;

        // 모든 회사 스킬 리스트를 순회하며 해당 인덱스를 가진 스킬 찾기
        List<SkillBase> allCompanies = new List<SkillBase>
        {
                _skill_One, _skill_Two, _skill_Three, _skill_Four, _skill_Five,
                _skill_Collab, _skill_Mutant
        };

        foreach (var company in allCompanies)
        {
            foreach (var skill in company.skillList)
            {
                if (skill.skillIdx == skillIdx)
                {
                    targetSkill = skill;
                    owner = company;
                    break;
                }
            }
            if (targetSkill != null) break;
        }

        if (targetSkill != null && owner != null)
        {
            targetSkill.ChooseSkill();
            owner.ChooseSkill(targetSkill);
            owner.ActivateSkill(targetSkill);
        }
        else
        {
            Debug.LogWarning($"⚠️ 스킬 인덱스 {skillIdx}에 해당하는 스킬을 찾을 수 없습니다.");
        }
    }

#endif

}
