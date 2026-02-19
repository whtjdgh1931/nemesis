using System.Collections;
using UnityEngine;


public class WeakenAreaData
{
    public float extent;

    public WeakenAreaData(float extent)
    {
        this.extent = extent;
    }
}


public class WeakenArea : AreaDamageBase, IInitializePoolable
{
    public void Initialize(object data)
    {
        if (data is WeakenAreaData skillData)
        {
            SetAreaExtent(skillData.extent);
        }
    }

    public void Initialize()
    {
        CheckTarget();
        StartCoroutine(DestroyWeakenAreaCoroutine());

    }

    public override void ActiveSkill(Transform target)
    {
        MonsterBase monster = target.GetComponent<MonsterBase>();
        if (monster != null)
        {
            if (monster.GetMonsterSize() == MonsterSize.BIG)
            {
                monster.GetDebuffHandler().ApplyDebuff(DebuffHandler.DebuffData.CreateWeaken(5f, 10f));
            }
            else
            {
                monster.GetDebuffHandler().ApplyDebuff(DebuffHandler.DebuffData.CreateWeaken());
            }
        }

    }

    public IEnumerator DestroyWeakenAreaCoroutine()
    {
        yield return new WaitForSeconds(Constants.SKILL_REMAIN);
        GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
    }

}
