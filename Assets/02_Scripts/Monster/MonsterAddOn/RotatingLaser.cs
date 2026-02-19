using UnityEngine;
using System.Collections.Generic;

public class RotatingLaser : MonoBehaviour
{
    [Header("회전 설정")]
    public float rotationSpeed = 30f; // 회전 속도 (도/초)
    public float maxDistance = 1000f; // 레이저 최대 거리
    public LayerMask wallLayer; // 벽 레이어

    [Header("레이저 비주얼")]
    public float laserWidth = 0.5f; // 레이저 두께 (0.1 → 0.5로 증가)
    public Color laserColor = Color.red; // 레이저 색상
    public Material laserMaterial; // 커스텀 머티리얼 (선택사항)
    public Gradient colorGradient; // 그라디언트 (선택사항)

    [Header("데미지 설정")]
    public float damageAmount = 10f; // 데미지 양
    public float damageInterval = 1f; // 데미지 간격 (초)
    public LayerMask playerLayer; // 플레이어 레이어

    [Header("활성화 설정")]
    public float activationDelay = 2f; // 활성화 지연 시간 (초)

    private LineRenderer[] lineRenderers = new LineRenderer[4];
    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();
    private bool isActive = false; // 레이저 활성화 상태
    private bool isInitialized = false; // 초기화 완료 여부

    void OnEnable()
    {
        // 풀에서 꺼내질 때마다 초기화
        if (!isInitialized)
        {
            InitializeLasers();
            isInitialized = true;
        }

        // 상태 리셋
        ResetLaser();

        // 활성화 타이머 시작
        CancelInvoke(); // 기존 Invoke 취소
        Invoke("ActivateLaser", activationDelay);
    }

    void OnDisable()
    {
        // 비활성화될 때 정리
        CancelInvoke();
        isActive = false;
        lastDamageTime.Clear();
    }

    void InitializeLasers()
    {
        // 4개의 LineRenderer 생성 (최초 1회만)
        for (int i = 0; i < 4; i++)
        {
            GameObject laserObj = new GameObject("Laser_" + i);
            laserObj.transform.parent = transform;
            laserObj.transform.localPosition = Vector3.zero;

            lineRenderers[i] = laserObj.AddComponent<LineRenderer>();
            lineRenderers[i].positionCount = 2;

            // 레이저 비주얼 설정
            UpdateLaserVisuals(lineRenderers[i]);
        }
    }

    void ResetLaser()
    {
        // 레이저 상태 리셋
        isActive = false;
        lastDamageTime.Clear();

        // 회전 초기화
        transform.localRotation = Quaternion.identity;

        // 모든 LineRenderer 비활성화
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
            {
                lr.enabled = false;
            }
        }
    }

    void ActivateLaser()
    {
        isActive = true;

        // 모든 LineRenderer 활성화
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
            {
                lr.enabled = true;
            }
        }
    }

    void UpdateLaserVisuals(LineRenderer lr)
    {
        lr.startWidth = laserWidth;
        lr.endWidth = laserWidth;

        // 커스텀 머티리얼이 있으면 사용, 없으면 기본 머티리얼
        if (laserMaterial != null)
        {
            lr.material = laserMaterial;
        }
        else
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }

        // 그라디언트가 설정되어 있으면 사용, 없으면 단색
        if (colorGradient != null && colorGradient.colorKeys.Length > 0)
        {
            lr.colorGradient = colorGradient;
        }
        else
        {
            lr.startColor = laserColor;
            lr.endColor = laserColor;
        }
    }

    void Update()
    {
        // 활성화되지 않았으면 실행하지 않음
        if (!isActive) return;

        // y축 기준으로 빙글빙글 회전
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // 4방향 설정 (상하좌우)
        Vector3[] directions = new Vector3[4]
        {
            transform.forward,  // 앞
            -transform.forward, // 뒤
            transform.right,    // 오른쪽
            -transform.right    // 왼쪽
        };

        // 각 방향으로 레이저 발사
        for (int i = 0; i < 4; i++)
        {
            if (lineRenderers[i] == null) continue;

            Vector3 direction = directions[i];

            // Raycast로 벽 감지
            RaycastHit hit;

            // 시작점
            lineRenderers[i].SetPosition(0, transform.position);

            // 끝점 설정
            Vector3 endPoint;
            if (Physics.Raycast(transform.position, direction, out hit, maxDistance, wallLayer))
            {
                // 벽에 닿으면 그 지점까지만
                endPoint = hit.point;
            }
            else
            {
                // 벽이 없으면 최대 거리까지
                endPoint = transform.position + direction * maxDistance;
            }

            lineRenderers[i].SetPosition(1, endPoint);

            // 플레이어 감지 및 데미지 처리
            CheckPlayerHit(transform.position, endPoint, direction);
        }
    }

    void CheckPlayerHit(Vector3 start, Vector3 end, Vector3 direction)
    {
        float distance = Vector3.Distance(start, end);
        RaycastHit[] hits = Physics.RaycastAll(start, direction, distance, playerLayer);

        foreach (RaycastHit hit in hits)
        {
            GameObject player = hit.collider.gameObject;

            // 데미지 간격 체크
            if (!lastDamageTime.ContainsKey(player) || Time.time - lastDamageTime[player] >= damageInterval)
            {
                // IDamageable 인터페이스 호출
                IDamageable damageable = player.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damageAmount, null);
                    lastDamageTime[player] = Time.time;
                }
            }
        }
    }

    // Inspector에서 값 변경 시 실시간 반영
    void OnValidate()
    {
        if (lineRenderers != null)
        {
            foreach (var lr in lineRenderers)
            {
                if (lr != null)
                {
                    UpdateLaserVisuals(lr);
                }
            }
        }
    }
}