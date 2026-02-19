using UnityEngine;

public class PlayerHackingDeviceSpecialAttacker : PlayerSpecialAttacker
{
    public override WeaponType WeaponType => WeaponType.HackingDevice;

    protected override void Fire()
    {
        throw new System.NotImplementedException();
    }
}