using UnityEngine;

[CreateAssetMenu(fileName = "PlayerWeaponSet", menuName = "ScriptableObjects/Player/PlayerWeaponSet")]
public class PlayerWeaponSet : ScriptableObject
{
    [Header("----- 무기가 바뀌면 같이 바뀌는 컴포넌트 -----")]
    [SerializeField] GameObject _weaponPrefab;                 // 무기 프리팹
    [SerializeField] WeaponType _weaponType;                   // 무기 타입
    [SerializeField] RuntimeAnimatorController _animController;// 플레이어 애니메이터 컨트롤러

    public GameObject WeaponPrefab => _weaponPrefab;
    public WeaponType WeaponType => _weaponType;
    public RuntimeAnimatorController AnimController => _animController;
}