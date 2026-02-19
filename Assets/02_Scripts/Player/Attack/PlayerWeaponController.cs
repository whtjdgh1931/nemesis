using System;
using UnityEditor;
using UnityEngine;



/// <summary>
/// 플레이어가 장착한 무기를 관리하는 클래스
/// </summary>
public class PlayerWeaponController : MonoBehaviour
{
    [Header("----- 컴포넌트 참조 -----")]
    [SerializeField] Transform _bladeHoldParent;            // 칼 무기 장착 위치
    [SerializeField] Transform _rifleHoldParent;            // 총 무기 장착 위치
    [SerializeField] Transform _hackingDeviceHoldParent;    // 해킹 장비 무기 장착 위치

    [Header("----- 확인용 -----")]
    [SerializeField] Weapon _currentWeapon;

    public Weapon CurrentWeapon => _currentWeapon;

    public event Action<Weapon> OnWeaponChanged;

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize()
    {
        //Debug.Log("초기 무기 장착");
        EquipWeapon(WeaponType.Rifle);
        OnWeaponChanged?.Invoke(_currentWeapon);
    }

    /// <summary>
    /// 무기와 상호작용 시 호출되는 함수
    /// </summary>
    /// <param name="newWeapon"></param>
    public void ChangeWeapon(WeaponType newWeaponType)
    {
        // 장착중인 무기가 없을 때
        if (_currentWeapon == null)
        {
            EquipWeapon(newWeaponType);
            return;
        }

        // 장착중인 무기가 있을 때
        else
        {
            // 같은 무기 타입이면 교체하지 않음
            if (newWeaponType == _currentWeapon.WeaponType)
            {
                return;
            }

            // 다른 무기 타입이면 기존 무기 제거 후
            Destroy(_currentWeapon.gameObject);
            // 새로운 무기 장착
            EquipWeapon(newWeaponType);
        }
    }

    /// <summary>
    /// 무기 장착 시 호출되는 함수
    /// </summary>
    public Weapon EquipWeapon(WeaponType newWeaponType)
    {
        //Debug.Log("무기 장착");
        // ResourceManager에서 무기 프리팹을 가져와서
        GameObject weaponPrefab = GameManager.Instance.DataManager.WeaponSetMap[newWeaponType].WeaponPrefab;
        // 해당 무기 타입에 맞는 무기 장착 위치에 생성
        GameObject weaponObj = Instantiate(weaponPrefab, GetParentTransform(newWeaponType));
        // 현재 장착된 무기로 설정
        _currentWeapon = weaponObj.GetComponent<Weapon>();
        // 무기 초기화
        _currentWeapon.Initialize(newWeaponType);
        // 무기 변경 이벤트 호출
        OnWeaponChanged?.Invoke(_currentWeapon);
        // 장착된 무기 반환
        return _currentWeapon;
    }

    /// <summary>
    /// WeaponType에 맞는 부모 Transform을 반환하는 함수
    /// </summary>
    /// <param name="weaponType"></param>
    /// <returns></returns>
    Transform GetParentTransform(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Blade => _bladeHoldParent,
            WeaponType.Rifle => _rifleHoldParent,
            WeaponType.HackingDevice => _hackingDeviceHoldParent,
            _ => null
        };
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Player))]
public class WeaponControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Player player = (Player)target;
        if (GUILayout.Button("라이플 장착"))
        {
            player.WeaponController.ChangeWeapon(WeaponType.Rifle);
        }
        if (GUILayout.Button("블레이드 장착"))
        {
            player.WeaponController.ChangeWeapon(WeaponType.Blade);
        }
        
    }
}
#endif