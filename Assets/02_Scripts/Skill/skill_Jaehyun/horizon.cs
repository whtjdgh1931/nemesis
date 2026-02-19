using Unity.VisualScripting;
using UnityEngine;

public class horizonData
{
    public float damage;
    public Player player;

    public horizonData(float damage, Player player  )
    {
        this.damage = damage;
        this.player = player;
    }
}


public class horizon : PoolableObject,IInitializePoolable
{
    [SerializeField] private LayerMask layer;    // Enemy Layer
    [SerializeField] private Collider[] colliders;
    [SerializeField] private Collider[] colliders_range;
    [SerializeField] private float radius;
    [SerializeField] private float inRadius;
    [SerializeField] private float height = 2f;
    [SerializeField] private int ConstHeight = 5;

    [SerializeField] private GameObject player;
    [SerializeField] private float damage;           // 초당 데미지
    [SerializeField] private float damageInterval = 1f; // 1초 간격으로 데미지
    private float damageTimer = 0f;



    void Update()
    {
        // 캡슐 범위 계산
        Vector3 pos1 = new Vector3(transform.position.x, transform.position.y - ConstHeight, transform.position.z);
        Vector3 pos2 = new Vector3(transform.position.x, transform.position.y + ConstHeight, transform.position.z);

        colliders = Physics.OverlapCapsule(pos2, pos1, radius, layer);

        //데미지 타이머
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyDamage();
            damageTimer = 0f;
        }
    }
    private void ApplyDamage()
    {
        foreach (var col in colliders)
        {
            if (col == null) continue;

            float distance = Vector3.Distance(player.transform.position, col.transform.position);

            // 플레이어와의 거리가 3 이상일 때만
            if (distance >= inRadius)
            {
                CharacterModelBase target = col.GetComponent<CharacterModelBase>();
                if (target != null)
                {
                    target.TakeDamage(damage, null);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 top = Vector3.up * height * 0.5f;
        Vector3 bottom = Vector3.down * height * 0.5f;
        Gizmos.DrawWireSphere(top, radius);
        Gizmos.DrawWireSphere(bottom, radius);

        Vector3 forward = Vector3.forward * radius;
        Vector3 back = Vector3.back * radius;
        Vector3 right = Vector3.right * radius;
        Vector3 left = Vector3.left * radius;

        Gizmos.DrawLine(top + forward, bottom + forward);
        Gizmos.DrawLine(top + back, bottom + back);
        Gizmos.DrawLine(top + right, bottom + right);
        Gizmos.DrawLine(top + left, bottom + left);
    }

    public void Initialize(object data)
    {
        if(data is horizonData skillData)
        {
            damage = skillData.damage;
            player = skillData.player.gameObject;
            radius = 4f * GameManager.Instance.PlayerStatManager.playerAreaExtent;
            inRadius = 3f * GameManager.Instance.PlayerStatManager.playerAreaExtent;
            transform.localScale = Vector3.one * radius * 2f;

            GameManager.Instance.PlayerStatManager.OnAreaExtentChange -= SetRadius;
            GameManager.Instance.PlayerStatManager.OnAreaExtentChange += SetRadius;
        }
    }

    public void SetRadius(float radiusMulti)
    {
        radius = 4f * radiusMulti;
        inRadius = 3f * radiusMulti;

        transform.localScale = Vector3.one * radius * 2f;

    }
}
