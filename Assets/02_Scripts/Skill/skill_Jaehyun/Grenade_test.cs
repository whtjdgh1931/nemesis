using System.Collections;
using UnityEngine;

public class Grenade_test : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private GameObject explodeCircle;
    [SerializeField] private float travelTime = 1.0f;     // 유탄이 도착하는 시간
    [SerializeField] private float travelSpeed = 30.0f;
    [SerializeField] private float explosionRadius = 3f;  // 폭발 반경
    [SerializeField] private float explosionDamage = 30f; // 폭발 데미지
    [SerializeField] private LayerMask groundLayer;       // Ground 레이어만 감지
    [SerializeField] private LayerMask enemyLayer;        // 적 탐지용

    // --- 새로 추가된 파라미터 (튜닝용) ---
    [Header("Parabola Height Tuning")]
    [SerializeField, Tooltip("짧은 거리(가까움)일 때의 최대 포물선 높이")]
    private float maxParabolaHeight = 15f;
    [SerializeField, Tooltip("먼 거리(멀리)일 때의 최소 포물선 높이")]
    private float minParabolaHeight = 10f;
    [SerializeField, Tooltip("이 거리 이하이면 '가깝다'로 간주")]
    private float closeDistance = 2f;
    [SerializeField, Tooltip("이 거리 이상이면 '멀다'로 간주")]
    private float farDistance = 10f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 targetPos = hit.point;
                LaunchGrenade(targetPos);
            }

        }
    }

    private void LaunchGrenade(Vector3 targetPos)
    {
        GameObject grenade = Instantiate(grenadePrefab, transform.position, Quaternion.identity);
        GameObject explodeRange = Instantiate(explodeCircle, targetPos + Vector3.up * 0.1f, explodeCircle.transform.rotation);
        StartCoroutine(ParabolaMove(grenade, targetPos));
        Destroy(explodeRange, travelTime);
    }

    

    private IEnumerator ParabolaMove(GameObject grenade, Vector3 target)
    {
        Vector3 start = grenade.transform.position;
        float elapsed = 0f;

        // 수평(또는 전체) 거리
        float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(target.x, 0f, target.z));

        // 거리 기반으로 높이 결정 (부드러운 보간)
        float tDist = Mathf.InverseLerp(closeDistance, farDistance, distance); // 0 -> close, 1 -> far
        // 부드럽게 전환
        float smooth = Mathf.SmoothStep(0f, 1f, tDist);
        // 가까울수록 max, 멀수록 min (따라서 lerp는 max->min)
        float parabolaHeight = Mathf.Lerp(maxParabolaHeight, minParabolaHeight, smooth);

        // 추가 안전 처리: 매우 가까울 때 과도한 높이 방지 (옵션)
        // 예: 거리가 0.1 이내이면 높이를 더 낮춤
        if (distance < 0.2f)
        {
            parabolaHeight = Mathf.Min(parabolaHeight, maxParabolaHeight * 0.8f);
        }

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            // 등속으로 시작->도착 (XZ 평면 포함)
            Vector3 flatPos = Vector3.Lerp(start, target, t);

            // 포물선: (t - t^2) 형태가 가운데에서 최고점, 끝에서는 0
            float parabola = 4f * parabolaHeight * (t - t * t);

            flatPos.y += parabola;

            grenade.transform.position = flatPos;

            yield return null;
        }

        Explode(grenade.transform.position);
        Destroy(grenade);
    }

    private void Explode(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, explosionRadius, enemyLayer);
        foreach (Collider hit in hits)
        {
            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage, transform);
            }
        }

    }
}
