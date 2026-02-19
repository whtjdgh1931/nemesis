using System.Collections;
using UnityEngine;

public class SkillEffect : PoolableObject
{
    [SerializeField] float remainTime;

    public void OnEnable()
    {
        StartCoroutine(ReturnToPool());
    }

    public IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(remainTime);
        GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
    }


}
