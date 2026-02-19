using UnityEngine;

public class RifleWeapon : Weapon
{
    [SerializeField] Transform _firePos;

    public Transform FirePos => _firePos;
}
