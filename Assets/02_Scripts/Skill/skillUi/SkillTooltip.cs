using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 툴팁과 관련된 클래스
/// </summary>
public class SkillTooltip : MonoBehaviour
{
    /// <summary>
    /// 스킬 툴팁 키워드에 따른 스킬 툴팁 데이터 보관 딕셔너리
    /// </summary>
    private Dictionary<string, SkillTooltipData> _skillTooltipKeywords = new Dictionary<string, SkillTooltipData>();
    public Dictionary<string, SkillTooltipData> skillTooltipKeywords { get { return _skillTooltipKeywords; } }

    private SkillTooltipUI _skillTooltipUIPrefab;


    private List<SkillTooltipUI> _currentUIList = new List<SkillTooltipUI>();

    /// <summary>
    /// Json 파일로 부터 스킬 툴팁 리스트 임시 보관
    /// </summary>
    private List<SkillTooltipJsonData> _skillTooltipJsonDataList = new List<SkillTooltipJsonData>();

    public void Initialize()
    {

        if (_skillTooltipUIPrefab == null)
        {
            _skillTooltipUIPrefab = Resources.Load<SkillTooltipUI>(Constants.RESOURCES_PATH_SKILLTOOLTIPUI);
        }
        _currentUIList.Clear();
        ReadJsonFile();

    }

    public void ReadJsonFile()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(Constants.RESOURCES_PATH_SKILLTOOLTIP);
        if (jsonFile != null)
        {
            string jsonText = jsonFile.text;
            _skillTooltipJsonDataList = JsonConvert.DeserializeObject<List<SkillTooltipJsonData>>(jsonText);
            InitDictionary();
        }
        else
        {
            Debug.LogError("오류 : 스킬 툴팁 파일 없음 : " + Constants.RESOURCES_PATH_SKILLTOOLTIP);
            return;
        }
    }


    /// <summary>
    /// 툴팁 딕셔너리 저장
    /// </summary>
    public void InitDictionary()
    {
        for (int i = 0; i < _skillTooltipJsonDataList.Count; i++)
        {
            _skillTooltipKeywords.Add(_skillTooltipJsonDataList[i].Keyword, new SkillTooltipData(_skillTooltipJsonDataList[i]));
        }

    }

    /// <summary>
    /// 스킬 툴팁 UI 생성
    /// </summary>
    /// <param name="skillData"></param>
    public void ShowTooltip(SkillData skillData)
    {
        if (skillData.keywords.Count == 0)
        {
            return;
        }


        for (int i = 0; i < skillData.keywords.Count; i++)
        {
            string tag = skillData.keywords[i].Trim();

            // 빈 태그 무시
            if (string.IsNullOrEmpty(tag)) continue;

            // 키 존재 여부 확인
            if (_skillTooltipKeywords.TryGetValue(tag, out var tooltipData))
            {
                SkillTooltipUI skillTooltipObj = GameManager.Instance.PoolManager
                        .GetFromPool(_skillTooltipUIPrefab, Vector3.zero, Quaternion.identity, transform)
                        .GetComponent<SkillTooltipUI>();

                skillTooltipObj.SetTooltipData(tooltipData);
                _currentUIList.Add(skillTooltipObj);

            }
            else
            {
            }
        }
    }


    public void ReleaseCurrentTooltip()
    {
        foreach (SkillTooltipUI tooltipUI in _currentUIList)
        {
            tooltipUI.Release();
        }
        _currentUIList.Clear();
    }

}