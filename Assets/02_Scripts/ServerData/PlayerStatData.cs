using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

/// <summary>
/// 게임 내에서 사용되는 플레이어 능력치 데이터를 표현하는 클래스
/// JSON으로부터 파싱된 PlayerStatJsonData를 기반으로 초기화됨
/// </summary>
public class PlayerStatData
{
    /// <summary>
    /// 능력치 키 이름 (예: bladeAttackDamage)
    /// </summary>
    private string _column;
    public string Column => _column;

    /// <summary>
    /// 데이터 타입 (예: int, float, bool, string)
    /// </summary>
    private string _type;
    public string Type => _type;

    /// <summary>
    /// 능력치 설명 (예: "검 일반 공격력 기본값")
    /// </summary>
    private string _description;
    public string Description => _description;

    /// <summary>
    /// 기본값 (타입에 따라 변환된 실제 값)
    /// </summary>
    private object _defaultValue;
    public object DefaultValue => _defaultValue;

    /// <summary>
    /// 업그레이드 수치 (타입에 따라 변환된 실제 값)
    /// </summary>
    private object _upgradeValue;
    public object UpgradeValue => _upgradeValue;

    /// <summary>
    /// 최대 업그레이드 레벨
    /// </summary>
    private int _maxLevel;
    public int MaxLevel => _maxLevel;

    /// <summary>
    /// 현재 적용된 업그레이드 레벨
    /// </summary>
    private int _currentLevel;
    public int CurrentLevel => _currentLevel;

    /// <summary>
    /// 현재 레벨에 따른 실제 적용값 반환
    /// int/float 타입은 기본값 + (업그레이드값 * 현재레벨)
    /// bool/string 타입은 기본값 그대로 반환
    /// </summary>
    public object GetEffectiveValue()
    {
        switch (_type)
        {
            case "int":
                return (int)_defaultValue + (int)_upgradeValue * _currentLevel;
            case "float":
                return (float)_defaultValue + (float)_upgradeValue * _currentLevel;
            case "bool":
                return _currentLevel==1;
            case "string":
                return _defaultValue;
            default:
                return _defaultValue;
        }
    }

    /// <summary>
    /// 생성자: JSON 데이터를 기반으로 능력치 정보 초기화
    /// </summary>
    /// <param name="jsonData">PlayerStatJsonData 객체</param>
    public PlayerStatData(PlayerStatJsonData jsonData)
    {
        _column = jsonData.column;
        _type = jsonData.type;
        _description = jsonData.description;
        _defaultValue = jsonData.GetTypedDefaultValue();
        _upgradeValue = jsonData.GetTypedUpgradeValue();
        _maxLevel = jsonData.maxLevel;
        _currentLevel = jsonData.currentLevel;
    }

    /// <summary>
    /// 현재 레벨을 설정 (0 이상 최대레벨 이하로 제한)
    /// </summary>
    /// <param name="level">설정할 레벨</param>
    public void SetLevel(int level)
    {
        _currentLevel = Mathf.Clamp(level, 0, _maxLevel);
    }

    public bool LevelUp()
    {
        if(_currentLevel < _maxLevel)
        {
            _currentLevel++;
            GameManager.Instance.PlayerStatManager.InitializeStatByReflection(this);
            return true;
        }
        else
        {
            return false;
        }
    }
}



/// <summary>
/// JSON으로부터 파싱된 플레이어 능력치 항목 하나를 표현하는 클래스
/// 각 항목은 이름, 타입, 설명, 기본값, 업그레이드 수치, 최대/현재 레벨 정보를 포함함
/// </summary>
public class PlayerStatJsonData
{
    /// <summary>
    /// 능력치 키 이름 (예: bladeAttackDamage)
    /// </summary>
    [JsonProperty("컬럼명")]
    public string column;

    /// <summary>
    /// 데이터 타입 (예: int, float, bool, string)
    /// </summary>
    [JsonProperty("타입")]
    public string type;

    /// <summary>
    /// 능력치 설명 (예: "검 일반 공격력 기본값")
    /// </summary>
    [JsonProperty("설명")]
    public string description;

    /// <summary>
    /// 기본값 (문자열로 저장되며 타입에 따라 변환 가능)
    /// </summary>
    [JsonProperty("기본값")]
    public string defaultValue;

    /// <summary>
    /// 업그레이드 수치 (문자열로 저장되며 타입에 따라 변환 가능)
    /// </summary>
    [JsonProperty("업그레이드")]
    public string upgradeValue;

    /// <summary>
    /// 해당 능력치의 최대 업그레이드 레벨
    /// </summary>
    [JsonProperty("최대레벨")]
    public int maxLevel;

    /// <summary>
    /// 현재 적용된 업그레이드 레벨
    /// </summary>
    [JsonProperty("현재레벨")]
    public int currentLevel;

    /// <summary>
    /// 타입에 따라 변환된 기본값 반환
    /// </summary>
    public object GetTypedDefaultValue()
    {
        return ParseValue(defaultValue, type);
    }

    /// <summary>
    /// 타입에 따라 변환된 업그레이드값 반환
    /// </summary>
    public object GetTypedUpgradeValue()
    {
        return ParseValue(upgradeValue, type);
    }

    /// <summary>
    /// 문자열 값을 타입에 따라 적절한 C# 타입으로 변환
    /// </summary>
    /// <param name="value">변환할 값</param>
    /// <param name="type">타입 정보</param>
    /// <returns>변환된 값 (int, float, bool, string 중 하나)</returns>
    private object ParseValue(string value, string type)
    {
        switch (type)
        {
            case "int":
                return int.TryParse(value, out var i) ? i : 0;
            case "float":
                return float.TryParse(value, out var f) ? f : 0f;
            case "bool":
                return value == "1" || value.ToLower() == "True";
            case "string":
                return value;
            default:
                return value;
        }
    }
}
