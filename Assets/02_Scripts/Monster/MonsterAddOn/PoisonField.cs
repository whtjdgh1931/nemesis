using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static DebuffHandler;

public class PoisonField : PoolableObject
{
    [SerializeField] private float tickDuration = 1f;
    [SerializeField] private Coroutine debuffCoroutine;
    [SerializeField] private float lifeTime = 999f;
    [SerializeField] private float poisonFieldRadius;
    [SerializeField] private string targetTag;
    [SerializeField] private Coroutine lifeTimeCoroutine;
    [SerializeField] private bool isPoison = true;

    [Header("Material Animation")]
    [SerializeField] private Renderer poisonRenderer;  // 장판의 Renderer
    [SerializeField] private float scrollSpeed = 0.2f; // 스크롤 속도
    private Material poisonMaterial;
    private float currentOffsetY = 0f;

    private void Update()
    {
        if (poisonMaterial != null)
        {
            currentOffsetY += Time.deltaTime * scrollSpeed;
            if (currentOffsetY > 1f) currentOffsetY -= 1f; // 0~1 반복
            poisonMaterial.mainTextureOffset = new Vector2(0f, currentOffsetY);
        }
    }


    public void Initialize(string targetTag, float lifeTime, float poisonFieldRadius)
    {
        StopAllCoroutines();
        if (poisonRenderer != null)
        {
            // 인스턴스 머티리얼 생성 (공유 머티리얼 영향 방지)
            poisonMaterial = poisonRenderer.material;
        }
        SetTarget(targetTag);
        SetLifeTime(lifeTime);
        SetRadius(poisonFieldRadius);
        SetScale();
        StartLifeTime();
    }

    public void SetTarget(string targetTag)
    {
        this.targetTag = targetTag;
    }

    public void SetLifeTime(float lifeTime)
    {
        this.lifeTime = lifeTime;
    }

    public void SetRadius(float poisonFieldRadius)
    {
        this.poisonFieldRadius = poisonFieldRadius;
    }

    private void StartLifeTime()
    {
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
        }

        lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && isPoison)
        {
            DebuffHandler debuffHandler = other.GetComponent<DebuffHandler>();
            if (debuffHandler != null && debuffCoroutine == null)
            {
                debuffCoroutine = StartCoroutine(DebuffToTarget(debuffHandler));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (debuffCoroutine != null)
        {
            StopCoroutine(debuffCoroutine);
            debuffCoroutine = null;
        }
    }

    private IEnumerator DebuffToTarget(DebuffHandler debuffHandler)
    {
        while (true)
        {
            DebuffData poison = DebuffData.CreatePoison();
            debuffHandler.ApplyDebuff(poison);
            yield return new WaitForSeconds(tickDuration);
        }
    }

    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }

    private void SetScale()
    {
        gameObject.transform.localScale = new Vector3(poisonFieldRadius, 1f, poisonFieldRadius);
    }
}
