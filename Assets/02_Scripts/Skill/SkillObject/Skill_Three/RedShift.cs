using UnityEngine;


public class RedShiftData
{
    public Vector3 moveDir;
    public float moveSpeed;
    public float shiftExtend;
    public float knockBackDistance;
    public float collisionDamage;

    public RedShiftData(Vector3 dir, float speed, float shiftExtend,float knockBackDistance ,float collisionDamage)
    {
        moveDir = dir;
        moveSpeed = speed;
        this.shiftExtend = shiftExtend;
        this.knockBackDistance = knockBackDistance;
        this.collisionDamage = collisionDamage;
    }
}

public class RedShift : AreaDamageBase, IInitializePoolable
{
    /// <summary>
    /// 방향
    /// </summary>
    [SerializeField]
    private Vector3 direction;

    /// <summary>
    /// 속도
    /// </summary>
    [SerializeField]
    private float speed = 10f;

    /// <summary>
    /// 스킬 데미지
    /// </summary>
    [SerializeField]
    private float _damage = 20f;

    /// <summary>
    /// 넉백 거리
    /// </summary>
    private float _knockBackDistance;

    private float _currentTime;
    private float _endTime = 5f;
    public void Initialize(object data)
    {
        if(data is RedShiftData redShiftData)
        {
            direction = redShiftData.moveDir;
            speed = redShiftData.moveSpeed;
            _knockBackDistance = redShiftData.knockBackDistance;
            _damage = redShiftData.collisionDamage;
            SetAreaExtent(redShiftData.shiftExtend);
        }
        _currentTime = 0f;

    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        _currentTime += Time.deltaTime;
        if(_currentTime> _endTime)
        {
            _currentTime = 0f;
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_MASK_WALL))
        {
            // 벽에 닿으면 파동 종료
            GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
        }


        else if (other.CompareTag(Constants.TAG_MONSTER))
        {
            MonsterBase monster = other.GetComponent<MonsterBase>();
            if (monster != null)
            {
                Vector3 direction = monster.transform.position - transform.position;
                direction.Normalize();


                monster.KnockBackEnemy(direction, _damage, _knockBackDistance);

            }
        }
    }

}
