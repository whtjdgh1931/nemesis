using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DonutWaveAttack : PoolableObject
{
    [Header("Wave Settings")]
    [SerializeField] private float maxRadius = 50f;      // 최대 크기
    [SerializeField] private float expandSpeed = 5f;     // 확장 속도
    [SerializeField] private float ringThickness = 2f;   // 링 두께
    [SerializeField] private float attackDelay = 1f;    // 공격 간격

    [Header("Visual Settings")]
    [SerializeField] private Color waveColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private Material waveMaterial;
    [SerializeField] private int segments = 50;
    [SerializeField] private bool useFillMesh = true;    // 평면 메쉬로 채우기

    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayer;

    private float currentRadius = 0f;
    private LineRenderer innerRing;
    private LineRenderer outerRing;
    private GameObject donutPlane;
    private MeshFilter planeMeshFilter;
    private MeshRenderer planeMeshRenderer;
    private bool isExpanding = false;

    void Start()
    {
        SetupRings();
        if (useFillMesh)
        {
            SetupDonutPlane();
        }
        TriggerWaveAttack();
    }

    void SetupRings()
    {
        // 안쪽 링 생성
        GameObject innerObj = new GameObject("InnerRing");
        innerObj.transform.SetParent(transform);
        innerObj.transform.localPosition = Vector3.zero;
        innerRing = innerObj.AddComponent<LineRenderer>();

        // 바깥쪽 링 생성
        GameObject outerObj = new GameObject("OuterRing");
        outerObj.transform.SetParent(transform);
        outerObj.transform.localPosition = Vector3.zero;
        outerRing = outerObj.AddComponent<LineRenderer>();

        SetupLineRenderer(innerRing);
        SetupLineRenderer(outerRing);

        innerRing.enabled = false;
        outerRing.enabled = false;
    }

    void SetupLineRenderer(LineRenderer lr)
    {
        lr.positionCount = segments + 1;
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.useWorldSpace = false;
        lr.loop = true;

        if (waveMaterial != null)
            lr.material = waveMaterial;

        lr.startColor = waveColor;
        lr.endColor = waveColor;
    }

    void SetupDonutPlane()
    {
        // 평면 도넛 오브젝트 생성
        donutPlane = new GameObject("DonutPlane");
        donutPlane.transform.SetParent(transform);
        donutPlane.transform.localPosition = Vector3.up * 0.1f; // 약간 위에 띄워서 겹치지 않게

        // Mesh 컴포넌트 추가
        planeMeshFilter = donutPlane.AddComponent<MeshFilter>();
        planeMeshRenderer = donutPlane.AddComponent<MeshRenderer>();

        // Material 설정
        if (waveMaterial != null)
        {
            planeMeshRenderer.material = waveMaterial;
        }

        donutPlane.SetActive(false);
    }

    public void TriggerWaveAttack()
    {
        if (!isExpanding)
        {
            StartCoroutine(ExpandWave());
        }
    }

    IEnumerator ExpandWave()
    {
        while (true)
        {
            isExpanding = true;
            currentRadius = 0f;

            innerRing.enabled = true;
            outerRing.enabled = true;

            if (useFillMesh && donutPlane != null)
            {
                donutPlane.SetActive(true);
            }

            // 이미 타격한 적을 추적
            HashSet<Collider> hitTargets = new HashSet<Collider>();

            while (currentRadius < maxRadius)
            {
                currentRadius += expandSpeed * Time.deltaTime;

                // 링 모양 업데이트
                UpdateRingShape(innerRing, currentRadius);
                UpdateRingShape(outerRing, currentRadius + ringThickness);

                // 평면 도넛 메쉬 업데이트
                if (useFillMesh && donutPlane != null)
                {
                    UpdateDonutPlaneMesh(currentRadius, currentRadius + ringThickness);
                }

                // 충돌 감지 (도넛 영역 내의 적)
                CheckCollisions(hitTargets);
                yield return null;
            }

            innerRing.enabled = false;
            outerRing.enabled = false;

            if (useFillMesh && donutPlane != null)
            {
                donutPlane.SetActive(false);
            }

            isExpanding = false;

            yield return new WaitForSeconds(attackDelay);
        }
    }

    void UpdateRingShape(LineRenderer lr, float radius)
    {
        float angle = 0f;
        float angleStep = (2 * Mathf.PI) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0, z));
            angle += angleStep;
        }
    }

    void UpdateDonutPlaneMesh(float innerRadius, float outerRadius)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // 정점 생성 (XZ 평면)
        for (int i = 0; i <= segments; i++)
        {
            float angle = (2 * Mathf.PI * i) / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // 안쪽 원의 정점
            vertices.Add(new Vector3(cos * innerRadius, 0, sin * innerRadius));
            uvs.Add(new Vector2(0, (float)i / segments));

            // 바깥쪽 원의 정점
            vertices.Add(new Vector3(cos * outerRadius, 0, sin * outerRadius));
            uvs.Add(new Vector2(1, (float)i / segments));
        }

        // 삼각형 생성
        for (int i = 0; i < segments; i++)
        {
            int current = i * 2;
            int next = (i + 1) * 2;

            // 첫 번째 삼각형
            triangles.Add(current);
            triangles.Add(next);
            triangles.Add(current + 1);

            // 두 번째 삼각형
            triangles.Add(current + 1);
            triangles.Add(next);
            triangles.Add(next + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        planeMeshFilter.mesh = mesh;
    }

    void CheckCollisions(HashSet<Collider> hitTargets)
    {
        // 3D Physics 사용
        Collider[] colliders = Physics.OverlapSphere(transform.position, currentRadius + ringThickness, targetLayer);

        foreach (Collider col in colliders)
        {
            // XZ 평면에서 거리 계산 (탑뷰이므로 Y축 제외)
            Vector3 targetPos = col.transform.position;
            Vector3 sourcePos = transform.position;
            float distance = Vector2.Distance(new Vector2(sourcePos.x, sourcePos.z), new Vector2(targetPos.x, targetPos.z));

            // 도넛 영역 안에 있는지 확인
            if (distance >= currentRadius && distance <= currentRadius + ringThickness)
            {
                if (!hitTargets.Contains(col))
                {
                    hitTargets.Add(col);
                    DealDamage(col.gameObject);
                }
            }
        }
    }

    void DealDamage(GameObject target)
    {
        // 적에게 데미지 적용
        var health = target.GetComponent<IDamageable>();
        if (health != null)
        {
            health.TakeDamage(damage, null);
        }
    }

    void DrawCircleGizmo(Vector3 center, float radius, int segments)
    {
        float angle = 0f;
        Vector3 lastPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle = (2 * Mathf.PI * i) / segments;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}