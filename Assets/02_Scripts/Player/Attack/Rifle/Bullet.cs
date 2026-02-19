using System;
using UnityEngine;

public class BulletData
{

}

/// <summary>
/// 총기류의 탄환을 관리하는 클래스
/// </summary>
public class Bullet : PoolableObject
{
    [Header("----- 데이터로 빼기 전 임시 -----")]
    [SerializeField] float _moveSpeed; // 탄환의 이동 속도
    [SerializeField] float _lifeTime;   // 탄환의 생존 시간

    [Header("----- 읽기 전용 -----")]
    [SerializeField] float _lifeTimer;              // 생존 시간 타이머
    [SerializeField] Vector3 _moveDir;

    public event Action OnLifeTimeExpired; // 생존 시간 만료 이벤트

    private void Update()
    {
        // 안전하게 이동 처리 (moveDir가 0이면 이동/회전 하지 않음)
        if (_moveDir.sqrMagnitude > 0.0001f)
            Move(_moveDir);

        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _lifeTime)
        {
            OnLifeTimeExpired?.Invoke();
            _lifeTimer = 0f;
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }

    public void Initialize()
    {
        // 여기서 데이터 초기화 작업 수행 가능
        _moveSpeed = GameManager.Instance.PlayerStatManager.playerBulletMoveSpeed;
        _lifeTime = GameManager.Instance.PlayerStatManager.playerBulletLifeTime;
    }

    // <summary>
    /// 발사 방향 설정
    /// - 돌연변이(HasMutant3) 활성 시 가장 가까운 적을 찾고, 없으면 원래 방향 사용
    /// - GetNearestMonsterFromMe가 null을 반환할 수 있으므로 방어 처리 필요
    /// </summary>
    public void SetMoveDir(Vector3 moveDir)
    {
        // 기본: 인자로 받은 방향이 유효하면 정규화해서 사용
        Vector3 initialDir = moveDir.sqrMagnitude > 0.0001f ? moveDir.normalized : Vector3.zero;

        if (EventBus.HasMutant3)
        {
            // EventBus.GetNearestMonsterFromMe may return null — 방어 처리
            var nearest = EventBus.GetNearestMonsterFromMe(transform); // Transform 또는 null 예상
            if (nearest != null)
            {
                Vector3 dirToMonster = (nearest.position - transform.position);
                // y 축 차이가 크면 평면으로 제한하려면 아래처럼 y = 0 처리 가능 (선택)
                // dirToMonster.y = 0f;
                if (dirToMonster.sqrMagnitude > 0.0001f)
                {
                    _moveDir = dirToMonster.normalized;
                    return;
                }
            }
        }

        // 돌연변이 미적용 시 일반 동작
        _moveDir = initialDir;
    }

    public void Move(Vector3 moveDir)
    {
        // 안전하게 회전: moveDir이 거의 0이면 LookRotation 호출 금지
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
            transform.Translate(moveDir * _moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(Constants.TAG_MONSTER))
        {
            EventBus.MonsterHit(WeaponType.Rifle, ATTACKTYPE.NORMAL, other.transform,transform);
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
            GameManager.Instance.PoolManager.GetFromPool("Effect/BulletHitEffect", other.transform.position + Vector3.up, Quaternion.identity);
        }
        else if(other.CompareTag(Constants.TAG_WALL))
        {
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }
}