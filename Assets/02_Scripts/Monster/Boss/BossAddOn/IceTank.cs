using UnityEngine;

public class IceTank : MonsterBase
{
    public void SetStateDie()
    {
        baseState = MonsterState.Die;
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}
