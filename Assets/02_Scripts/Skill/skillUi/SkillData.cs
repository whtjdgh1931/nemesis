using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬의 정보를 가지고 있는 클래스
/// </summary>
public class SkillData
{
    // JsonProperty("INDEX")
    private int _skillIdx;
    public int skillIdx { get { return _skillIdx; } }

    // JsonProperty("기술이름")
    private string _skillName;
    public string skillName { get { return _skillName; } }

    // JsonProperty("영문 기술이름")
    private string _skillNameEn;
    public string skillNameEn { get { return _skillNameEn; } }

    // JsonProperty("설명")
    private string _skillScript;
    public string skillScript { get { return _skillScript; } }

    // JsonProperty("영문 설명")
    private string _skillScriptEn;
    public string skillScriptEn { get { return _skillScriptEn; } }

    // JsonProperty("적용수치 설명 텍스트")
    private string _skillValueScript;
    public string skillValueScript { get { return _skillValueScript; } }

    // JsonProperty("영문 적용수치 설명 텍스트")
    private string _skillValueScriptEn;
    public string skillValueScriptEn { get { return _skillValueScriptEn; } }

    // JsonProperty("이미지경로")
    private Sprite _skillImage;
    public Sprite skillImagePath { get { return _skillImage; } }

    // JsonProperty("기업분류")
    private string _skillCompanyName;
    public string skillCompanyName { get { return _skillCompanyName; } }

    // JsonProperty("태그")
    private string _skillTag;
    public string skillTag { get { return _skillTag; } }

    // JsonProperty("최대레벨")
    private int _skillMaxLevel;
    public int skillMaxLevel { get { return _skillMaxLevel; } }

    // JsonProperty("레벨당성장수치1")
    private float _skillLevelValue_1;
    public float skillLevelValue_1 { get { return _skillLevelValue_1; } }

    // JsonProperty("기본밸류1")
    private float _skillBaseValue_1;
    public float skillBaseValue_1 { get { return _skillBaseValue_1; } }

    // JsonProperty("레벨당성장수치2")
    private float _skillLevelValue_2;
    public float skillLevelValue_2 { get { return _skillLevelValue_2; } }

    // JsonProperty("기본밸류2")
    private float _skillBaseValue_2;
    public float skillBaseValue_2 { get { return _skillBaseValue_2; } }

    // JsonProperty("레벨당성징수치3")
    private float _skillLevelValue_3;
    public float skillLevelValue_3 { get { return _skillLevelValue_3; } }

    // JsonProperty("기본밸류3")
    private float _skillBaseValue_3;
    public float skillBaseValue_3 { get { return _skillBaseValue_3; } }

    // JsonProperty("포함키워드")
    private List<string> _keywords;
    public List<string> keywords { get { return _keywords; } }

    private int _skillLevel;
    public int skillLevel { get { return _skillLevel; } }

    private SkillBase _skillCompany;
    public SkillBase skillCompany { get { return _skillCompany; } }

    public SkillData(skillJsonData data, SkillBase skillCompany)
    {
        _skillIdx = data.index;
        _skillName = data.skillName;
        _skillNameEn = data.skillNameEn;
        _skillScript = data.skillScript;
        _skillScriptEn = data.skillScriptEn;
        _skillValueScript = data.skillValueScript;
        _skillValueScriptEn = data.skillValueScriptEn;
        _skillImage = Resources.Load<Sprite>($"SkillImage/TechImage_{skillIdx}");
        _skillCompanyName = data.skillCompany;
        _skillTag = data.skillTag;
        _skillMaxLevel = data.skillMaxLevel ?? 0;
        _skillLevelValue_1 = data.skillLevelValue_1 ?? 0;
        _skillBaseValue_1 = data.skillBaseValue_1 ?? 0;
        _skillLevelValue_2 = data.skillLevelValue_2 ?? 0;
        _skillBaseValue_2 = data.skillBaseValue_2 ?? 0;
        _skillLevelValue_3 = data.skillLevelValue_3 ?? 0;
        _skillBaseValue_3 = data.skillBaseValue_3 ?? 0;
        _keywords = string.IsNullOrEmpty(data.Keywords) ? new List<string>() : new List<string>(data.Keywords.Split(';'));

        _skillLevel = 0;
        _skillCompany = skillCompany;
    }

    /// <summary>
    /// 스킬 선택
    /// </summary>
    /// <returns></returns>
    public bool ChooseSkill()
    {
        _skillLevel++;
        if (_skillMaxLevel == _skillLevel)
        {
            GameManager.Instance.skillManager.upgradeSkillList.Remove(this);
        }
        return _skillLevel == 1;
    }

    /// <summary>
    /// 스킬을 현재 가지고 있는 스킬 리스트에서 삭제
    /// </summary>
    public void RemoveList(bool isAnotherSkill)
    {
        if (!isAnotherSkill)
        {
            // 같은 스킬이면 리턴
            return;
        }
        _skillLevel = 0;
        _skillCompany.currentSkillData.Remove(this);
        _skillCompany.SkillNumUp(this, -1);
        // 업그레이드 가능 리스트에 있다면
        if (GameManager.Instance.skillManager.upgradeSkillList.Contains(this))
        {
            GameManager.Instance.skillManager.upgradeSkillList.Remove(this);
        }
        _skillCompany.skillList.Add(this);
    }
}



/// <summary>
/// SkillData와 Json파일 연결용 클래스
/// </summary>
public class skillJsonData
{
    [JsonProperty("INDEX")]
    public int index;

    [JsonProperty("기술이름")]
    public string skillName;

    [JsonProperty("영문 기술이름")]
    public string skillNameEn;

    [JsonProperty("설명")]
    public string skillScript;

    [JsonProperty("영문 설명")]
    public string skillScriptEn;

    [JsonProperty("적용수치 설명텍스트")]
    public string skillValueScript;

    [JsonProperty("영문 적용수치 설명텍스트")]
    public string skillValueScriptEn;

    [JsonProperty("기업분류")]
    public string skillCompany;

    [JsonProperty("태그")]
    public string skillTag;

    [JsonProperty("최대레벨")]
    public int? skillMaxLevel;

    [JsonProperty("레벨당성장수치1")]
    public float? skillLevelValue_1;

    [JsonProperty("기본밸류1")]
    public float? skillBaseValue_1;

    [JsonProperty("레벨당성장수치2")]
    public float? skillLevelValue_2;

    [JsonProperty("기본밸류2")]
    public float? skillBaseValue_2;

    [JsonProperty("레벨당성징수치3")]
    public float? skillLevelValue_3;

    [JsonProperty("기본밸류3")]
    public float? skillBaseValue_3;

    [JsonProperty("포함키워드")]
    public string Keywords;

}