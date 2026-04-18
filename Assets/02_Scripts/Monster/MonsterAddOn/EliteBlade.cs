using System.Collections;
using UnityEngine;

public class EliteBlade : PoolableObject
{
    private float speed = 13f;
    private float lifeTime;
    private float damage;
    [SerializeField] private string targetTag;
    private GameObject owner;
    private Coroutine lifeTimeCoroutine;
    private Vector3 moveDir = Vector3.forward;

    // 추가: 혼란 상태의 몬스터가 쏜 총알인지 구별
    private bool isConfusedShot = false;

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
    public void SetTarget(string targetTag)
    {
        this.targetTag = targetTag;
    }
    public void SetLifeTime(float lifeTime)
    {
        this.lifeTime = lifeTime;
    }

    public void Initialize(string targetTag, float damage, float lifeTime, GameObject owner, bool isConfusedShot = false)
    {
        SetTarget(targetTag);
        SetDamage(damage);
        SetLifeTime(lifeTime);
        this.owner = owner;
        this.isConfusedShot = isConfusedShot;
        StartLifeTime();
    }

    private void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime);
    }

    private void StartLifeTime()
    {
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
        }
        lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
    }

    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner)
        {
            return;
        }

        // 몬스터 태그 처리
        if (other.CompareTag(Constants.TAG_MONSTER))
        {
            // 혼란 상태의 총알이 아니면 몬스터 통과
            if (!isConfusedShot)
            {
                return;
            }

            // 혼란 상태의 총알이면 데미지 처리
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform);
            }

            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
                lifeTimeCoroutine = null;
            }
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
            return;
        }

        // 타겟 태그 처리
        if (other.CompareTag(targetTag))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            reflect reflectable = other.GetComponent<reflect>();

            if (reflectable != null && reflectable.isReflecting == true)
            {
                Reflect(other);
                return;
            }

            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform);
            }

            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
                lifeTimeCoroutine = null;
            }
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
        // Electric 태그는 통과
        else if (!other.CompareTag(Constants.TAG_ELECTIC))
        {
            // 그 외 오브젝트(벽 등)에 부딪히면 제거
            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
                lifeTimeCoroutine = null;
            }
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }

    private void Reflect(Collider reflector)
    {
        moveDir = -moveDir;
        targetTag = Constants.TAG_MONSTER;
        owner = reflector.gameObject;
        isConfusedShot = true; // 반사된 총알은 몬스터에게 데미지
    }

    private void OnDisable()
    {
        moveDir = Vector3.forward;
        targetTag = Constants.TAG_PLAYER;
        owner = null;
        isConfusedShot = false;
    }
}
