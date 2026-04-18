using UnityEngine;

public class PlayerHackingDeviceNormalAttacker : PlayerNormalAttacker
{
    public override WeaponType WeaponType => WeaponType.HackingDevice;
    public  override void Attack()
    {
        throw new System.NotImplementedException();
    }
}
