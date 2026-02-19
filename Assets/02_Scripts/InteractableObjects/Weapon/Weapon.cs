using UnityEngine;

/// <summary>
/// 플레이어의 무기 타입을 정의하는 열거형
/// </summary>
public enum WeaponType
{
    None,
    Blade,
    Rifle,
    HackingDevice,
}

public class Weapon : MonoBehaviour
{
    [SerializeField] WeaponType _weaponType;
    public WeaponType WeaponType => _weaponType;

    public void Initialize(WeaponType weaponType)
    {
        _weaponType = weaponType;
    }
}
