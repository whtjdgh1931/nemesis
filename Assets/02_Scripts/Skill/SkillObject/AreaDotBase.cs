using UnityEngine;

public class AreaDotBase : AreaDamageBase
{
    /// <summary>
    /// 영역에 들어오면 스킬 발동
    /// </summary>
    /// <param name="other"></param>
    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_checkTargetTag))
        {
            ActiveSkill(other.transform);
        }
    }

    /// <summary>
    /// 영역에서 나가면 스킬 적용 종료
    /// </summary>
    /// <param name="other"></param>
    public virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_checkTargetTag))
        {
            EndSkill(other.transform);
        }
    }


    /// <summary>
    /// 스킬 종료
    /// </summary>
    /// <param name="target"></param>
    public virtual void EndSkill(Transform target)
    {

    }

}
