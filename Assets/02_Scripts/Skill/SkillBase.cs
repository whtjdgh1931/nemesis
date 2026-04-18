using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{


    protected SkillManager _skillManager;
    public SkillManager skillManager { get { return _skillManager; } }


    /// <summary>
    /// 현재 가지고 있는 스킬 개수
    /// </summary>
    protected int _skillNum;
    public int skillNum { get { return _skillNum; } }
    public virtual void SkillNumUp(SkillData skilldata, int num)
    {
        _skillNum+= num;
    }

    /// <summary>
    /// Json파일로 부터 데이터를 저장할 리스트
    /// </summary>
    private List<skillJsonData> _skillJsonDataList = new List<skillJsonData>();
    public List<skillJsonData> skillJsonDataList { get { return _skillJsonDataList; } }

    /// <summary>
    /// 아직 고르지 않은 회사 스킬 리스트
    /// </summary>
    private List<SkillData> _skillList = new List<SkillData>();
    public List<SkillData> skillList { get { return _skillList; } }


    /// <summary>
    /// 플레이어가 가지고 있는 스킬 종류
    /// </summary>
    private List<SkillData> _currentSkillData = new List<SkillData>();
    public List<SkillData> currentSkillData { get { return _currentSkillData; } }


    /// <summary>
    /// 고른 스킬의 개수 반환
    /// </summary>
    /// <returns></returns>
    public int GetNumberSkillList()
    {
        return currentSkillData.Count;
    }


    /// <summary>
    /// 스킬 데이터 경로
    /// </summary>
    [SerializeField]
    private string _skillDataPath;

    public virtual void InitializeSkill(SkillManager skillManager)
    {
        ReadJsonFile();
        _skillManager = skillManager;
        _skillNum = 0;
    }

    public void ReadJsonFile()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(_skillDataPath);
        if (jsonFile == null)
        {
            Debug.LogError("오류 : 파일 없음 " + _skillDataPath);
            return;
        }
        else
        {
            string jsonText = jsonFile.text;
            _skillJsonDataList = JsonConvert.DeserializeObject<List<skillJsonData>>(jsonText);
            InitSkillDictionary();
        }

    }

    /// <summary>
    /// 회사 스킬 레벨 초기화
    /// </summary>
    public void InitSkillDictionary()
    {
        _skillList.Clear();
        _currentSkillData.Clear();
        for (int i = 0; i < skillJsonDataList.Count; i++)
        {
            _skillList.Add(new SkillData(skillJsonDataList[i],this));
        }
    }

    /// <summary>
    /// 스킬 처음 선택시
    /// </summary>
    /// <param name="skillData"></param>
    public void ChooseSkill(SkillData skillData)
    {
        if (_skillList.Remove(skillData))
        {
            _currentSkillData.Add(skillData);
            SkillNumUp(skillData,1);
            // MaxLevel이 1이 아니라면(업그레이드 가능하다면)
            if (skillData.skillMaxLevel != 1)
            {
                //업그레이드 가능 리스트에 추가
                _skillManager.upgradeSkillList.Add(skillData);
            }
        }
        else
        {
            Debug.LogError($"{skillData}가 없음");
        }

    }

    /// <summary>
    /// 스킬 선택시 발동
    /// </summary>
    /// <param name="choosedSkill"></param>
    public abstract void ActivateSkill(SkillData choosedSkill);


}






