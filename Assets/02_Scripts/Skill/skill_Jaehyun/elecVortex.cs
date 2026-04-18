using UnityEngine;
using UnityEngine.AI;

public class elecVortex : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private LayerMask layer;

    [SerializeField] private float originPullRadius;
    [SerializeField] private float pullRadius;
    [SerializeField] private float originDamageRadius;
    [SerializeField] private float damageRadius;

    [SerializeField] private float height = 2f;
    [SerializeField] private int ConstHeight = 5;
    [SerializeField] private float power = 10f;

    [SerializeField] private float damage;
    [SerializeField] private float damageInterval = 1f;

    private MapController _mapController;
    private float damageTimer = 0f;

    [SerializeField] private Transform point;

    private Collider[] pullColliders;
    private Collider[] damageColliders;

    public float PullRadius { get => pullRadius; set => pullRadius = value; }
    public float DamageRadius { get => damageRadius; set => damageRadius = value; }
    public float Damage { get => damage; set => damage = value; }
    public float Speed { get => speed; set => speed = value; }

    public void Initialize(float damage, float pullRadius, float damageRadius, MapController mapController)
    {
        this.damage = damage;
        originPullRadius = pullRadius;
        originDamageRadius = damageRadius;
        _mapController = mapController;
        damageTimer = 0f;

        GameManager.Instance.PlayerStatManager.OnAreaExtentChange -= SetRadius;
        GameManager.Instance.PlayerStatManager.OnAreaExtentChange += SetRadius;
        SetRadius(GameManager.Instance.PlayerStatManager.playerAreaExtent);
    }

    void Update()
    {
        Vector3 pos1 = transform.position - Vector3.up * ConstHeight;
        Vector3 pos2 = transform.position + Vector3.up * ConstHeight;

        // 끌어당김
        pullColliders = Physics.OverlapCapsule(pos2, pos1, pullRadius, layer);
        foreach (var col in pullColliders)
        {
            MonsterBase monster = col.GetComponent<MonsterBase>();
            if (monster == null || monster.GetMonsterSize() == MonsterSize.BIG) continue;

            NavMeshAgent agent = col.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            Vector3 dir = (point.position - col.transform.position).normalized;
            agent.Move(dir * Time.deltaTime * power);
        }

        // 데미지
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyDamage(pos1, pos2);
            damageTimer = 0f;
        }

        // 가장 가까운 몬스터 추적
        if (_mapController != null)
        {
            GameObject nearest = Constants.GetNearestObject(transform, _mapController.MonsterController.MonsterSpawner.ActiveMonsters);
            if (nearest != null)
            {
                Vector3 targetPos = nearest.transform.position;
                targetPos.y = transform.position.y;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            }
        }
    }

    private void ApplyDamage(Vector3 pos1, Vector3 pos2)
    {
        damageColliders = Physics.OverlapCapsule(pos2, pos1, damageRadius, layer);
        foreach (var col in damageColliders)
        {
            CharacterModelBase target = col.GetComponent<CharacterModelBase>();
            if (target != null)
            {
                target.TakeDamage(damage, null);
            }
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.PlayerStatManager.OnAreaExtentChange -= SetRadius;
    }

    public void SetRadius(float radiusMulti)
    {
        pullRadius = originPullRadius * radiusMulti;
        damageRadius = originDamageRadius * radiusMulti;
        transform.localScale = Vector3.one * pullRadius * 2f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 top = Vector3.up * ConstHeight;
        Vector3 bottom = Vector3.down * ConstHeight;

        // 끌어당김 범위 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(top, pullRadius);
        Gizmos.DrawWireSphere(bottom, pullRadius);
        Gizmos.DrawLine(top + Vector3.forward * pullRadius, bottom + Vector3.forward * pullRadius);
        Gizmos.DrawLine(top + Vector3.back * pullRadius, bottom + Vector3.back * pullRadius);
        Gizmos.DrawLine(top + Vector3.right * pullRadius, bottom + Vector3.right * pullRadius);
        Gizmos.DrawLine(top + Vector3.left * pullRadius, bottom + Vector3.left * pullRadius);

        // 데미지 범위 (노랑)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(top, damageRadius);
        Gizmos.DrawWireSphere(bottom, damageRadius);
        Gizmos.DrawLine(top + Vector3.forward * damageRadius, bottom + Vector3.forward * damageRadius);
        Gizmos.DrawLine(top + Vector3.back * damageRadius, bottom + Vector3.back * damageRadius);
        Gizmos.DrawLine(top + Vector3.right * damageRadius, bottom + Vector3.right * damageRadius);
        Gizmos.DrawLine(top + Vector3.left * damageRadius, bottom + Vector3.left * damageRadius);
    }
}
