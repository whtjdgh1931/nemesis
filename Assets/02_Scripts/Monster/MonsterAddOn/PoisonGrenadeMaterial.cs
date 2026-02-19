using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PoisonGrenadeMaterial : PoolableObject, IInitializePoolable
{

    private float lifeTime = 2f;

    private Coroutine lifeTimeCoroutine;

    public void Initialize(object data = null)
    {
        // Initialize가 호출되면 즉시 코루틴 시작 준비
        if (gameObject.activeInHierarchy)
        {
            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
            }
            StartLifeTime();
        }
    }
    private void OnEnable()
    {
        // OnEnable 시 코루틴 시작
        StartLifeTime();
    }

    private void StartLifeTime()
    {
        // 기존 코루틴이 있으면 정지
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
        }

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
    }
    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }

}
