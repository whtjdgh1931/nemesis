using System.Collections;
using UnityEngine;

public class PlayerGrenadeBullet : MonoBehaviour
{
    [SerializeField] private float travelTime = 1.0f;
    [SerializeField] private float travelSpeed = 30.0f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] float _mutatntSpeedMultiplier = 1.5f;

    [Header("Parabola Height Tuning")]
    [SerializeField] private float maxParabolaHeight = 15f;
    [SerializeField] private float minParabolaHeight = 10f;
    [SerializeField] private float closeDistance = 2f;
    [SerializeField] private float farDistance = 10f;

    [Header("Missile Visual")]
    [SerializeField] private Transform missileVisual; // 미사일 모델 (자식 오브젝트)

    private bool isExplode;

    public void Initialize(Transform firePoint, Vector3 target)
    {
        isExplode = false;
        if (EventBus.HasMutant2)
        {
            StartCoroutine(DirectMoveRoutine(firePoint, target));
            return;
        }
        StartCoroutine(ParabolaMove(firePoint, target));
    }

    public IEnumerator ParabolaMove(Transform firePoint, Vector3 target)
    {
        Vector3 start = firePoint.position + Vector3.up * 1.5f;
        float elapsed = 0f;

        float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(target.x, 0f, target.z));
        float tDist = Mathf.InverseLerp(closeDistance, farDistance, distance);
        float smooth = Mathf.SmoothStep(0f, 1f, tDist);
        float parabolaHeight = Mathf.Lerp(maxParabolaHeight, minParabolaHeight, smooth);

        if (distance < 0.2f)
            parabolaHeight = Mathf.Min(parabolaHeight, maxParabolaHeight * 0.8f);

        Vector3 previousPos = start;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector3 flatPos = Vector3.Lerp(start, target, t);
            float parabola = 4f * parabolaHeight * (t - t * t);
            flatPos.y += parabola;

            transform.position = flatPos;

            // 회전 적용
            Vector3 moveDir = (flatPos - previousPos).normalized;
            if (moveDir != Vector3.zero)
            {
                missileVisual.forward = moveDir;
            }

            previousPos = flatPos;
            yield return null;
        }

        // 도착 후 폭발 처리
        Explode(transform.position, transform);
        isExplode = true;
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
    IEnumerator DirectMoveRoutine(Transform firePoint, Vector3 target)
    {
        while (true)
        {
            Vector3 direction = (target - firePoint.position).normalized;
            transform.position += direction * travelSpeed * _mutatntSpeedMultiplier * Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isExplode)
        {
            return;
        }
        if (EventBus.HasMutant2)
        {
            if (other.CompareTag(Constants.TAG_GROUND) || other.CompareTag(Constants.TAG_WALL) || other.CompareTag(Constants.TAG_MONSTER))
            {
                Explode(transform.position, transform);
                isExplode = true;
                GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
                return;
            }
        }
        if (other.CompareTag(Constants.TAG_GROUND) || other.CompareTag(Constants.TAG_WALL))
        {
            Explode(transform.position, transform);
            isExplode = true;

            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }

    private void Explode(Vector3 position, Transform grenadeTransform)
    {
        // non-alloc array
        Collider[] hits = new Collider[10];
        int hitCount = Physics.OverlapSphereNonAlloc(position, explosionRadius, hits, enemyLayer);

        GameManager.Instance.SoundManager.PlaySfxAt("GrenadeSFX", position);
        GameManager.Instance.PoolManager.GetFromPool("Effect/GrenadeExplosionEffect", position, Quaternion.identity);
        EventBus.GrenadeBomb(position);
        if (hitCount <= 0)
        {
            return;
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hits[i];
            if (hit == null)
                continue;

            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                Transform monster = hit.transform;
                EventBus.MonsterHit(WeaponType.None, ATTACKTYPE.GRENADE, monster, grenadeTransform);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
