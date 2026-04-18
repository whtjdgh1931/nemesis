using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 데미지를 받을 수 있는 오브젝트에서 공통적으로 구현해야 하는 메서드.
    /// </summary>
    void TakeDamage(float damage, Transform attacker = null);
}
