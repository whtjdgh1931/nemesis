using System.Collections;
using UnityEngine;


public class PoisonSpreadData
{
    public float extent;

    public PoisonSpreadData(float extent)
    {
        this.extent = extent;
    }
}
public class PoisonSpread : AreaDamageBase,IInitializePoolable
{


    /// <summary>
    /// 스킬에 맞는 효과 발동
    /// </summary>
    /// <param name="target"></param>
    public override void ActiveSkill(Transform target)
    {
        // 독 적용
        target.GetComponent<DebuffHandler>().ApplyDebuff(DebuffHandler.DebuffData.CreatePoison());

    }


    public IEnumerator DestroyPoisonSpreadCoroutine()
    {
        yield return new WaitForSeconds(Constants.SKILL_REMAIN);
        GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
    }


    public void Initialize()
    {
        CheckTarget();
        StartCoroutine(DestroyPoisonSpreadCoroutine());
    }

    public void Initialize(object data)
    {
        if(data is PoisonSpreadData skillData)
        {
            SetAreaExtent(skillData.extent);
        }
    }
}
