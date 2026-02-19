using System.Collections;
using UnityEngine;

public class KnockBackDashData
{
    public float skillDamage;
    public float skillKnockBackDistance;
    public float skillExtent;

    public KnockBackDashData(float damage, float knockBackDistance,float extent)
    {
        skillDamage = damage;
        skillKnockBackDistance = knockBackDistance;
        skillExtent = extent;
    }
}


public class KnockBackDash : AreaDamageBase,IInitializePoolable
{
    private float _damage;
    private float _knockBackDistance;

    public override void ActiveSkill(Transform target)
    {
        MonsterBase monster = target.GetComponent<MonsterBase>();
        if(monster!=null)
        {
            Vector3 dir = target.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            monster.KnockBackEnemy(dir,_damage,_knockBackDistance);
        }
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }



    public void Initialize()
    {
        
        CheckTarget();
        StartCoroutine(ReleaseObjectCoroutine());
    }

    public void Initialize(object data)
    {
        if(data is  KnockBackDashData skillData)
        {
            _damage = skillData.skillDamage;
            _knockBackDistance = skillData.skillKnockBackDistance * GameManager.Instance.PlayerStatManager.knockBackDistance;
            SetAreaExtent(skillData.skillExtent);
        }
    }

    IEnumerator ReleaseObjectCoroutine()
    {
        yield return new WaitForSeconds(Constants.SKILL_REMAIN);
        GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
    }
}
