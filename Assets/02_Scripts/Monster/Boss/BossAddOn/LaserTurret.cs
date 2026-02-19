using System.Collections;
using UnityEngine;

public class LaserTurret : MonsterBase
{
    [Header("Turret Settings")]
    [SerializeField] private float lifeTime = 12f;

    private Coroutine lifeTimeCoroutine;


    private void Update()
    {
        if (isDead || _target == null) return;
        if (isStunned) return;

        switch (baseState)
        {
            case MonsterState.Idle:
                HandleIdle();
                break;
            case MonsterState.Die:
                Die();
                break;
        }
    }

    private void HandleIdle()
    {

    }


    public override void Initialize(object data = null)
    {
        base.Initialize(data);

        // Initialize가 호출되면 즉시 코루틴 시작 준비
        if (gameObject.activeInHierarchy)
        {
            StartLifeTime();
        }
    }

    private void OnEnable()
    {
        // OnEnable 시 코루틴 시작
        StartLifeTime();
    }

    private void OnDisable()
    {
        // 비활성화 시 코루틴 정리
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }
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

        if (!isDead)
        {
            Die();
        }
    }
}