using System.Collections;
using UnityEngine;

public class EliteBullet : PoolableObject
{
    private float speed = 7f;
    private float lifeTime;
    private float damage;
    [SerializeField] private string targetTag;

    private GameObject owner;
    private Coroutine lifeTimeCoroutine;


    //변경된 부분
    private Vector3 moveDir = Vector3.forward;


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

    public void Initialize(string targetTag, float damage, float lifeTime, GameObject owner)
    {
        SetTarget(targetTag);
        SetDamage(damage);
        SetLifeTime(lifeTime);
        this.owner = owner;
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
        if (other.gameObject == owner || other.CompareTag(Constants.TAG_MONSTER))
        {
            return;
        }
        if (other.CompareTag(targetTag))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            // 변경된 부분
            reflect reflectable = other.GetComponent<reflect>();
            if (reflectable != null && reflectable.isReflecting == true)
            {
                Reflect(other);
                return;
            }
            //
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

        else
        {
            if (other.CompareTag(Constants.TAG_ELECTIC))
            {
                return;
            }
            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
                lifeTimeCoroutine = null;
            }
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }

    //변경 부분
    private void Reflect(Collider reflector)
    {
        moveDir = -moveDir; // 방향 전환

        targetTag = Constants.TAG_MONSTER; // 타겟태그 변경
        owner = reflector.gameObject; // 주인 변경

    }

    //초기화
    private void OnDisable()
    {
        moveDir = Vector3.forward;
        targetTag = Constants.TAG_PLAYER;
        owner = null;
    }
}
