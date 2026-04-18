using System.Collections;
using UnityEngine;

public class GravityFlareRocketExplosionData
{
    public float damage;
    public float extent;

    public GravityFlareRocketExplosionData(float damage, float extent)
    {
        this.damage = damage;
        this.extent = extent;
    }
}

public class GravityFlareRocketExplosion : AreaDamageBase, IInitializePoolable
{
    private float _damage;


    public void Initialize()
    {
        CheckTarget();
        StartCoroutine(RemoveCoroutine());
    }
    public override void ActiveSkill(Transform target)
    {
        target.GetComponent<MonsterBase>().TakeDamage(_damage);
    }
    public void Initialize(object data)
    {
       if(data is GravityFlareRocketExplosionData skillData)
        {
            _damage = skillData.damage;
            SetAreaExtent(skillData.extent);
        }
    }

    public IEnumerator RemoveCoroutine()
    {
        yield return new WaitForSeconds(Constants.SKILL_REMAIN);
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);


    }
}
