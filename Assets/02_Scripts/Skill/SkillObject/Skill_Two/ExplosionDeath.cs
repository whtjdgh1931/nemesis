using System.Collections;
using UnityEngine;

public class ExplosionDeathData
{
    public float damage;
    public float extent;
    
    public ExplosionDeathData(float damage, float extent)
    {
        this.damage = damage;
        this.extent = extent;
    }
}
public class ExplosionDeath : AreaDamageBase,IInitializePoolable
{
    private float _damage;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Initialize()
    {
        _areaExtent = Constants.EXPLOSIONDEATH_EXTENT;
        CheckTarget();
        StartCoroutine(DestroyExplosionCoroutine(0.5f, this));
    }

    public override void ActiveSkill(Transform target)
    {
        target.GetComponent<IDamageable>().TakeDamage(Constants.EXPLOSIONDEATH_DAMAGE, null);
    }

    public IEnumerator DestroyExplosionCoroutine(float time, PoolableObject gameObject)
    {
        yield return new WaitForSeconds(time);
        GameManager.Instance.PoolManager.ReleaseToPoolByInterface(gameObject);
    }

    public void Initialize(object data)
    {
        if(data is ExplosionDeathData skillData)
        {
            _damage = skillData.damage;
            SetAreaExtent(skillData.extent);
        }
    }
}
