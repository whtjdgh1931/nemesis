using UnityEngine;

/// <summary>
/// 간단한 검기(Blade) 발사체 예제 컴포넌트
/// - Init 으로 대미지/속도/수명/스케일/소유자 설정
/// - 충돌 시 상대에게 IDamageable 형태로 데미지를 전달하거나, Health 컴포넌트를 찾아 호출
/// </summary>
[RequireComponent(typeof(Collider))]
public class BladeProjectile : MonoBehaviour
{
    public float Damage { get; private set; }
    public float Speed { get; private set; }
    public float Lifetime { get; private set; }
    public GameObject Owner { get; private set; }

    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // 콜라이더는 트리거로 설정하는 것을 권장합니다 (물리력 영향 없이 충돌 판정만)
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public void Initialize(float damage, float speed, float lifetime, float scale = 1f, GameObject owner = null)
    {
        Damage = damage;
        Speed = speed;
        Lifetime = lifetime;
        Owner = owner;

        transform.localScale = Vector3.one * scale;

        if (_rb != null)
        {
            _rb.linearVelocity = transform.forward * Speed;
        }

        if (Lifetime > 0f)
            Destroy(gameObject, Lifetime);
    }

    void Update()
    {
        // Rigidbody로 움직이지 않는 경우(또는 없는 경우), Transform으로 전진
        if (_rb == null)
        {
            transform.position += transform.forward * Speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 소유자에는 데미지 적용하지 않음
        if (Owner != null && other.gameObject == Owner) return;

        // IDamageable 인터페이스 사용을 권장 (아래는 예시)
        var dmgable = other.GetComponent<IDamageable>();
        if (dmgable != null)
        {
            dmgable.TakeDamage(Damage);
        }

        // 충돌하면 즉시 파괴 (필요에 따라 관통/반사 등 로직 추가 가능)
        Destroy(gameObject);
    }
}