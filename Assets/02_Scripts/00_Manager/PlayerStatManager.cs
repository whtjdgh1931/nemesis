using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerStatManager : MonoBehaviour
{
    private Dictionary<string,PlayerStatData> _playerStatDataDic = new Dictionary<string,PlayerStatData>();
    public Dictionary<string, PlayerStatData> playerStatDataDic { get { return _playerStatDataDic; } }

    public List<PlayerStatData> GetUpgradableStats()
    {
        return playerStatDataDic
            .Values
            .Where(stat => stat.MaxLevel > 0)
            .ToList();
    }

    private bool bIsInit = false;

    private List<PlayerStatJsonData> _playerjsonData = new List<PlayerStatJsonData>();

    #region 공격력 

    #region 검
    /// <summary>
    /// 검 일반 공격력
    /// </summary>
    private float _bladeAttackDamage;
    public float bladeAttackDamage { get { return _bladeAttackDamage; } }
    public void AddBladeAttackDamage(float plusBladeAttack)
    {
        _bladeAttackDamage += plusBladeAttack;
    }


    /// <summary>
    /// 검 특수 공격력
    /// </summary>
    private float _bladeSPAttackDamage;
    public float bladeSPAttackDamage { get { return _bladeAttackDamage; } }
    public void AddBladeSPAttackDamage(float plusBladeSPAttack)
    {
        _bladeSPAttackDamage += plusBladeSPAttack;
    }

    #endregion
    #region 라이플
    /// <summary>
    /// 원거리 일반 공격력
    /// </summary>
    private float _playerRifleAttackDamage;
    public float playerRifleAttackDamage { get { return _playerRifleAttackDamage; } }
    public void AddPlayerRifleAttackDamage(float plusPlayerRifleAttack)
    {
        _playerRifleAttackDamage += plusPlayerRifleAttack;
    }

    /// <summary>
    /// 원거리 특수 최소 공격력
    /// </summary>
    private float _playerRifleSPAttackMinDamage;
    public float playerRifleSPAttackMinDamage { get { return _playerRifleSPAttackMinDamage; } }
    public void AddPlayerRifleSPAttackMinDamage(float plusPlayerRifleSPAttackDamage)
    {
        _playerRifleSPAttackMinDamage += plusPlayerRifleSPAttackDamage;
    }

    /// <summary>
    /// 원거리 특수 공격 차지 비율
    /// </summary>
    private float _playerRifleChargeRatio;
    public float playerRifleChargeRatio { get { return _playerRifleChargeRatio; } }   
    public void SetPlayerRifleChargeRatio(float chargeRatio)
    {
        _playerRifleChargeRatio = chargeRatio;
    }

    /// <summary>
    /// 원거리 특수 공격 최대 차지 데미지
    /// </summary>
    private float _playerRifleMaxChargeDamage;
    public float playerRifleMaxChargeDamage { get { return _playerRifleMaxChargeDamage; } }
    public void SetPlayerRifleMaxChargeDamage(float maxChargeDamage)
    {
        _playerRifleMaxChargeDamage = maxChargeDamage;
        OnPlayerRifleMaxChargeDamage?.Invoke(_playerRifleMaxChargeDamage);
    }
    public event Action<float> OnPlayerRifleMaxChargeDamage;

    /// <summary>
    /// 플레이어 블렛 이동 속도
    /// </summary>
    private float _playerBulletMoveSpeed;
    public float playerBulletMoveSpeed { get { return _playerBulletMoveSpeed; } }
    public void AddPlayerBulletMoveSpeed(float plusPlayerBulletMoveSpeed)
    {
        _playerBulletMoveSpeed += plusPlayerBulletMoveSpeed;
    }

    /// <summary>
    /// 플레이어 블렛 지속 시간
    /// </summary>
    private float _playerBulletLifeTime;
    public float playerBulletLifeTime { get { return _playerBulletLifeTime; } }
    public void AddPlayerBulletLifeTime(float plusPlayerBulletLifeTime)
    {
        _playerBulletLifeTime += plusPlayerBulletLifeTime;
    }

    #endregion
    #region 특수장비
    /// <summary>
    /// 특수장비 일반 공격력
    /// </summary>
    private float _playerHackAttackDamage;
    public float playerHackAttackDamage { get { return _playerHackAttackDamage; } }
    public void AddPlayerHackAttackDamage(float plusPlayerHackAttackDamage)
    {
        _playerHackAttackDamage += plusPlayerHackAttackDamage;
    }

    /// <summary>
    /// 특수장비 특수 공격력
    /// </summary>
    private float _playerHackSPAttackDamage;
    public float playerHackSPAttackDamage { get { return _playerHackSPAttackDamage; } }
    public void AddPlayerHackSPAttackDamage(float plusPlayerHackSPAttackDamage)
    {
        _playerHackSPAttackDamage += plusPlayerHackSPAttackDamage;
    }
    #endregion
    /// <summary>
    /// 플레이어 일반 공격 데미지 계수
    /// </summary>
    private float _playerAttackDamage;
    public float playerAttackDamage { get { return _playerAttackDamage; } }
    public void AddPlayerAttackDamage(float plusDamage)
    {
        _playerAttackDamage += plusDamage;
        OnPlayerAttackDamageChange?.Invoke(_playerAttackDamage);
    }
    public event Action<float> OnPlayerAttackDamageChange;



    /// <summary>
    /// 플레이어 유탄 공격 데미지 계수
    /// </summary>
    private float _playerGrenadeDamageMulti;
    public float playerGrenadeDamageMulti { get { return _playerGrenadeDamageMulti; } }
    public void AddPlayerGrenadeDamageMulti(float plusGrenadeDamage)
    {
        _playerGrenadeDamageMulti += plusGrenadeDamage;
    }
    

    /// <summary>
    /// 플레이어 특수 공격 데미지 계수
    /// </summary>
    private float _playerSPAttackDamage;
    public float playerSPAttackDamage { get { return _playerSPAttackDamage; } }
    public void AddPlayerSPAttackDamage(float plusSPAttackDamage)
    {
        _playerSPAttackDamage += plusSPAttackDamage;
    }

    /// <summary>
    /// 플레이어 특수 효과 계수
    /// </summary>
    private float _playerSPAttackValue;
    public float playerSPAttackValue { get { return _playerSPAttackValue; } }
    public void AddPlayerSPAttackValue(float plusSPValue)
    {
        _playerSPAttackValue += plusSPValue;
        OnPlayerSPAttackValueChange?.Invoke(_playerSPAttackValue);
    }
    public event Action<float> OnPlayerSPAttackValueChange;

    /// <summary>
    /// 플레이어 대쉬 공격력
    /// </summary>
    private float _playerDashDamage;
    public float playerDashDamage { get { return _playerDashDamage; } }
    public void AddPlayerDashDamage(float plusDashDamage)
    {
        _playerDashDamage += plusDashDamage;
    }

    /// <summary>
    /// 플레이어 대쉬 공격력계수
    /// </summary>
    private float _playerDashDamageMulti;
    public float playerDashDamageMulti { get { return _playerDashDamageMulti; } }
    public void AddPlayerDashDamageMulti(float plusDashDamage)
    {
        _playerDashDamageMulti += plusDashDamage;
    }

    #endregion

    #region 모든 데미지, 이동속도, 애니메이션 재생속도

    /// <summary>
    /// 모든 데미지 증가 계수
    /// </summary>
    private float _totalMultiDamage;
    public float totalMultiDamage { get { return _totalMultiDamage; } }

    /// <summary>
    /// 모든 데미지 계수 증가
    /// </summary>
    /// <param name="addDamage">ex)0.5배</param>
    public void AddTotalMultiDamage(float addDamage)
    {
        _totalMultiDamage += addDamage;
    }


    /// <summary>
    /// 플레이어 이동 속도
    /// </summary>
    private float _playerMoveSpeed;
    public float playerMoveSpeed { get { return _playerMoveSpeed; } }
    public void AddPlayerMoveSpeed(float plusMoveSpeed)
    {
        _playerMoveSpeed += plusMoveSpeed;
        OnPlayerMoveSpeedChange?.Invoke(_playerMoveSpeed* _playerMoveSpeedMulti);
    }
    public event Action<float> OnPlayerMoveSpeedChange;

    
    private float _playerMoveSpeedMulti;
    public float playerMoveSpeedMulti { get { return _playerMoveSpeedMulti; } }
    public void AddPlayerMoveSpeedMulti(float plusMoveSpeed)
    {
        _playerMoveSpeedMulti += plusMoveSpeed;
        OnPlayerMoveSpeedChange?.Invoke(_playerMoveSpeed * _playerMoveSpeedMulti);
    }



    /// <summary>
    /// 플레이어 대쉬 거리
    /// </summary>
    private float _playerDashDistance;
    public float playerDashDistance { get { return _playerDashDistance; } }
    public void AddPlayerDashDistance(float plusDashDistance)
    {
        _playerDashDistance += plusDashDistance;
    }

    /// <summary>
    /// 플레이어 대쉬거리 곱 계수
    /// </summary>
    private float _playerDashDistanceMulti;
    public float playerDashDistanceMulti { get { return _playerDashDistanceMulti; } }
    public void AddPlayerDashDistanceMulti(float plusDashDistanceMulti)
    {
        _playerDashDistanceMulti += plusDashDistanceMulti;
        OnPlayerDashDistanceMultiChange?.Invoke();
    }
    public event Action OnPlayerDashDistanceMultiChange;
    /// <summary>
    /// 플레이어 애니메이션 일반 공격 재생 속도
    /// </summary>
    private float _playerAttackAnimSpeed;
    public float playerAttackAnimSpeed { get { return _playerAttackAnimSpeed; } }
    public void AddPlayerAttackAnimSpeed(float plusAttackAnimSpeed)
    {
        _playerAttackAnimSpeed += plusAttackAnimSpeed;
    }


    #endregion

    #region 유탄, 범위공격

    /// <summary>
    /// 유탄 공격력
    /// </summary>
    private float _playerGrenadeDamage;
    public float playerGrenadeDamage { get { return _playerGrenadeDamage; } }
    public void AddPlayerGreneadeDamage(float plusGreneadeDamage)
    {
        _playerGrenadeDamage += plusGreneadeDamage;
    }

    /// <summary>
    /// 플레이어 범위 공격 범위 계수
    /// </summary>
    private float _playerAreaExtent;
    public float playerAreaExtent { get { return _playerAreaExtent; } }
    public void AddPlayerAreaExtent(float plusAreaExtent)
    {
        _playerAreaExtent += plusAreaExtent;
        OnAreaExtentChange?.Invoke(_playerAreaExtent);
    }
    public event Action<float> OnAreaExtentChange;

    /// <summary>
    /// 유탄 쿨타임 
    /// </summary>
    private float _grenadeCoolTime;
    public float grenadeCoolTime { get { return _grenadeCoolTime; } }
    public void SetGrenadeCoolTime(float grenadeCoolTime)
    {
        _grenadeCoolTime = grenadeCoolTime;
        OnGrenadeCoolTimeChange?.Invoke(_grenadeCoolTime);
    }
    public event Action<float> OnGrenadeCoolTimeChange;



    /// <summary>
    /// 유탄 쿨타임 계수
    /// </summary>
    private float _grenadeCoolTimeMulti;
    public float grenadeCoolTimeMulti { get { return _grenadeCoolTimeMulti; } }
    public void AddGrenadeCoolTimeMulti(float grenadeCoolTime)
    {
        _grenadeCoolTimeMulti += grenadeCoolTime;
        OnGrenadeCoolTimeMultiChange?.Invoke();
    }
    public event Action OnGrenadeCoolTimeMultiChange;

    #endregion

    #region 받는 피해
    /// <summary>
    /// 플레이어 받는 피해 계수
    /// </summary>
    private float _playerReduceDamagePercent;
    public float playerReduceDamagePercent { get { return _playerReduceDamagePercent; } }
    public void AddReduceDamagePercent(float plusReduceDamagePercent)
    {
        _playerReduceDamagePercent += plusReduceDamagePercent;
        OnplayerHitPercentChange?.Invoke(_playerReduceDamagePercent);
    }
    public event Action<float> OnplayerHitPercentChange;

    /// <summary>
    /// 플레이어 회피율
    /// </summary>
    private float _playerAvoidance;
    public float playerAvoidance { get { return _playerAvoidance; } }
    public void AddPlayerAvoidance(float plusPlayerAvoidance)
    {
        _playerAvoidance += plusPlayerAvoidance;
        OnplayerAvoidanceChange?.Invoke(_playerAvoidance);
    }
    public event Action<float> OnplayerAvoidanceChange;
    #endregion

    #region 넉백

    /// <summary>
    /// 넉백 충돌 데미지
    /// </summary>
    private float _knockBackDamage;
    public float knockBackDamage { get { return _knockBackDamage; } }
    public void AddKockBackDamage(float plusKnockBackDamage)
    {
        _knockBackDamage += plusKnockBackDamage;
    }

    /// <summary>
    /// 넉백 충돌 데미지 계수 
    /// </summary>
    private float _knockBackDamageMulti;
    public float knockBackDamageMulti { get { return _knockBackDamageMulti; } }
    public void AddKockBackDamageMulti(float plusKnockBackDamageMulti)
    {
        _knockBackDamageMulti += plusKnockBackDamageMulti;
        OnKnockBackDamgeMultiChange?.Invoke(_knockBackDamageMulti);
    }
    public event Action<float> OnKnockBackDamgeMultiChange;

    /// <summary>
    /// 넉백 거리 계수
    /// </summary>
    private float _knockBackDistance;
    public float knockBackDistance { get { return _knockBackDistance; } }
    public void AddKnockBackDistance(float plusKnockBackDistance)
    {
        _knockBackDistance += plusKnockBackDistance;
        OnKnockBackDistanceChange?.Invoke(_knockBackDistance);
    }
    public event Action<float> OnKnockBackDistanceChange;

    /// <summary>
    /// 넉백 미는 힘
    /// </summary>
    private float _knockBackPower;
    public float knockBackPower { get { return _knockBackPower; } }
    public void AddKnockBackPower(float plusKnockBackPower)
    {
        _knockBackPower += plusKnockBackPower;
    }
    #endregion

    #region 추가데미지
    /// <summary>
    /// 취약, 디버프 있으면 추가 데미지 계수
    /// </summary>
    private float _debuffPlusDamage;
    public float debuffPlusDamage { get { return _debuffPlusDamage; } }
    public void AddDebuffPlusDamage(float plusDebuffPlusDamage)
    {
        _debuffPlusDamage += plusDebuffPlusDamage;
    }

    /// <summary>
    /// 약화 추가 데미지 계수
    /// </summary>
    private float _weakenPlusDamage;
    public float weakenPlusDamage { get { return _weakenPlusDamage; } }
    public void AddWeakenPlusDamage(float PlusWeakenDamage)
    {
        _weakenPlusDamage += PlusWeakenDamage;
    }
    #endregion

    /// <summary>
    /// 플레이어 부활 여부
    /// </summary>
    private bool _playerRevive;
    public bool playerRevive { get { return _playerRevive; } }
    public void SetPlayerRevive(bool playerRevive)
    {
        _playerRevive = playerRevive;
    }

    /// <summary>
    /// 플레이어 자동 회복
    /// </summary>
    private int _playerRestore;
    public int playerRestore { get { return _playerRestore; } } 
    public void SetPlayerRestore(int playerRestore)
    {
        _playerRestore += playerRestore;
    }



    public void TakeDamage(WeaponType weaponType,ATTACKTYPE attackType,Transform monster, Transform attackerTransform = null)
    {
     
        float damage = 0f;
        switch (attackType)
        {
            case ATTACKTYPE.NORMAL:
                switch (weaponType)
                {
                    case WeaponType.Blade:
                        damage = _bladeAttackDamage * playerAttackDamage;
                        break;
                    case WeaponType.Rifle:
                        damage = _playerRifleAttackDamage * playerAttackDamage;
                        break;
                    case WeaponType.HackingDevice:
                        damage = _playerHackAttackDamage * playerAttackDamage;
                        break;
                    default:
                        break;
                }
                break;
            case ATTACKTYPE.GRENADE:
                damage = playerGrenadeDamage * playerGrenadeDamageMulti;
                break;
            case ATTACKTYPE.SPECIALATTACK:
                switch (weaponType)
                {
                    case WeaponType.Blade:
                        damage = _bladeSPAttackDamage * playerSPAttackDamage;
                        break;
                    case WeaponType.Rifle:
                        damage = Mathf.Lerp(playerRifleSPAttackMinDamage,playerRifleMaxChargeDamage,playerRifleChargeRatio) * playerSPAttackDamage * playerSPAttackValue;
                        if(Mathf.Approximately(playerRifleChargeRatio,1f))
                        {
                            damage *= 2f;
                        }

                        break;
                    case WeaponType.HackingDevice:
                        damage = _playerHackSPAttackDamage * playerSPAttackDamage;
                        break;
                    default:
                        break;
                }
                break;
            case ATTACKTYPE.DASH:
                damage = _playerDashDamage * playerDashDamageMulti;
                break;
            
            default:
                break;
        }
        
        monster.GetComponent<MonsterBase>().TakeDamage(damage, attackerTransform);
    }


    public async Task Initialize()
    {
        bIsInit = false;
       await InitPlayerStat();

        foreach (var stat in _playerStatDataDic.Values)
        {
            InitializeStatByReflection(stat);
        }

        bIsInit = true;
    }

    public void TakeDamageToEventBus()
    {
				EventBus.OnMonsterHit += TakeDamage;

		}

		public async Task WaitForInitAsync()
    {
        while (!bIsInit)
        { 
            await Task.Delay(100); // 100ms마다 확인
        }
    }

    public async Task InitPlayerStat()
    {
       await ReadJsonFile();
    }

    public async Task ReadJsonFile()
    {
        if (!File.Exists(Constants.FILE_PATH_PLAYERSTAT))
        {
            int retryCount = 0;
            int maxRetries = 30;
            int delayMilliseconds = 500;
             GameManager.Instance.serverManager.downloadManager.DownloadPlayerStatToLocal();

            while (!File.Exists(Constants.FILE_PATH_PLAYERSTAT) && retryCount < maxRetries)
            {
                await Task.Delay(delayMilliseconds);
                retryCount++;
            }

            if (!File.Exists(Constants.FILE_PATH_PLAYERSTAT))
            {
                Debug.LogError("downloadManager is still null after retries.");
                return;
            }
        }

        try
        {
            string jsonText = File.ReadAllText(Constants.FILE_PATH_PLAYERSTAT);
            _playerjsonData = JsonConvert.DeserializeObject<List<PlayerStatJsonData>>(jsonText);
            InitSkillDictionary();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("JSON 파싱 오류: " + ex.Message);
        }
    }

    public void InitSkillDictionary()
    {
        _playerStatDataDic.Clear();
        
        foreach (var data in _playerjsonData)
        {
            _playerStatDataDic.Add(data.column, new PlayerStatData(data));
        }
    }

    public void InitializeStatByReflection(PlayerStatData statData)
    {
        var field = this.GetType().GetField($"_{statData.Column}", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null && field.FieldType == typeof(float))
        {
            var value = statData.GetEffectiveValue();
            if (value is float floatValue)
            {
                field.SetValue(this, floatValue);
            }
        }
    }

    public void SaveCurrentLevelsToJson()
    {
        List<PlayerStatJsonData> updatedList = new();

        foreach (var pair in _playerStatDataDic)
        {
            var statData = pair.Value;
            updatedList.Add(new PlayerStatJsonData
            {
                column = statData.Column,
                type = statData.Type,
                description = statData.Description,
                defaultValue = statData.DefaultValue.ToString(),
                upgradeValue = statData.UpgradeValue.ToString(),
                maxLevel = statData.MaxLevel,
                currentLevel = statData.CurrentLevel
            });
        }

        string json = JsonConvert.SerializeObject(updatedList, Formatting.Indented);
        File.WriteAllText(Constants.FILE_PATH_PLAYERSTAT, json);
    }


    public void UploadToFirebase()
    {
        SaveCurrentLevelsToJson();
        GameManager.Instance.serverManager.downloadManager.UploadPlayerStatFromLocal();
    }

   
}

