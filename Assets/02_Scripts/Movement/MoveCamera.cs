using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 25,-25);
    private float cameraSpeed = 5f;

    [Header("카메라 제한 영역")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minZ;
    [SerializeField] private float maxZ;

    //
    private float transparentAlpha = 0.3f;
    public LayerMask obstacleMask; // 투명화 대상 레이어 설정

    private MeshRenderer lastRenderer;
    //

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {
        //transform.position = target.position + offset;

        // 기본 카메라 위치
        Vector3 desiredPos = target.position + offset;

        // X, Z 좌표를 제한 (카메라가 맵 밖을 보지 않게)
        float clampX = Mathf.Clamp(desiredPos.x, minX, maxX);
        float clampZ = Mathf.Clamp(desiredPos.z, minZ, maxZ);

        //transform.position = new Vector3(clampX, desiredPos.y, clampZ);
        transform.position = Vector3.Lerp(transform.position, new Vector3(clampX, desiredPos.y, clampZ), cameraSpeed * Time.deltaTime);//Lerp사용

        //복원
        if (lastRenderer != null)
        {
            SetAlpha(lastRenderer, 1f);
            lastRenderer = null;
        }

        // 카메라에서 플레이어 방향으로 레이 쏘기
        Vector3 dir = target.position - transform.position;
        float dist = Vector3.Distance(target.position, transform.position);
        Ray ray = new Ray(transform.position, dir);
        RaycastHit hit;
        
        // 장애물 레이어만 체크
        if (Physics.Raycast(ray, out hit, dist))//obstacle추가
        {
            MeshRenderer rend = hit.collider.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                
                SetAlpha(rend, transparentAlpha);
                lastRenderer = rend; // 이번 프레임의 투명화 대상 저장
            }
        }
    }
    void SetAlpha(MeshRenderer rend, float alpha)
    {
        Color c = rend.material.color;
        c.a = alpha;
        rend.material.color = c;
    }

}
