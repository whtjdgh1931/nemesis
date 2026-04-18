using UnityEngine;
using UnityEngine.AI;

public class vortex : PoolableObject,IInitializePoolable
{
    public LayerMask layer;         // 아마 달렸있을 Enemy Layer
    public Collider[] colliders;    // 감지한 Collder배열
    protected Transform point;

    public float radius = 5f;
    public float height = 2f;       // 범위 높이

    protected int ConstHeight = 5;
    public float power = 10f;       // 적용할 힘의 세기



    protected void Update()
    {
        //캡슐의 맨아래 위치
        Vector3 pos1 = new Vector3(transform.position.x, transform.position.y - ConstHeight, transform.position.z);
        //캡슐의 맨위 위치
        Vector3 pos2 = new Vector3(transform.position.x, transform.position.y + ConstHeight, transform.position.z);
        colliders = Physics.OverlapCapsule(pos2, pos1, radius, layer);  //실제 범위



        foreach (var col in colliders)
        {
            MonsterBase monster = col.GetComponent<MonsterBase>();

            if (monster != null)
            {
                if (monster.GetMonsterSize() == MonsterSize.BIG)
                {
                    continue;
                }
            }
            NavMeshAgent agent = col.GetComponent<NavMeshAgent>();

           

            // 이동 방향 계산 (Point의 위치 - 현재 위치)
            Vector3 dir = (point.position - col.transform.position).normalized;

            if (agent != null)
            {

                // 힘 적용
                agent.Move(dir * Time.deltaTime * power);
            }

        }

    }

    protected void SetRadius(float radius)
    {
        this.radius = radius * GameManager.Instance.PlayerStatManager.playerAreaExtent;
        transform.localScale = Vector3.one * this.radius * 2f;
    }


    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
        //Gizmos.DrawWireMesh(); //이거 사용하면 좀더 편한가

    }
    protected void OnDrawGizmosSelected() //실린더 모양 기즈모
    {
        // 기즈모 색상 설정
        Gizmos.color = Color.red;

        // 기즈모의 변환 행렬을 현재 오브젝트에 맞게 설정
        // 이렇게 하면 기즈모가 오브젝트의 위치, 회전, 스케일을 따라갑니다.
        Gizmos.matrix = transform.localToWorldMatrix;

        // 원통의 상단과 하단 원 그리기
        Vector3 top = Vector3.up * height * 0.5f;
        Vector3 bottom = Vector3.down * height * 0.5f;
        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawWireSphere(bottom, radius);

        // 상단과 하단 원을 잇는 4개의 선 그리기
        Vector3 forward = Vector3.forward * radius;
        Vector3 back = Vector3.back * radius;
        Vector3 right = Vector3.right * radius;
        Vector3 left = Vector3.left * radius;

        Gizmos.DrawLine(top + forward, bottom + forward);
        Gizmos.DrawLine(top + back, bottom + back);
        Gizmos.DrawLine(top + right, bottom + right);
        Gizmos.DrawLine(top + left, bottom + left);
    }

    public virtual void Initialize(object data)
    {
        // Point자식
        point = transform.Find("Point");
    }
}
