using System;
using System.Collections;
using UnityEngine;

public class Drone : PoolableObject
{
    [SerializeField]
    private enum State
    {
        Idle, // 몬스터 감지 못함
        Attack // 공격
    }
    private PlayerModel _playerModel;

    /// <summary>
    /// 공격 적중시 이벤트 실행
    /// </summary>
    public event Action<WeaponType, ATTACKTYPE, Transform, Transform> Attack;

    /// <summary>
    /// 공격 실행시 이벤트 실행
    /// </summary>
    public event Action attackTry;


    /// <summary>
    /// 현재 상태
    /// </summary>
    [SerializeField]
    private State _currentState = State.Idle;

    /// <summary>
    /// 공격 쿨타임
    /// </summary>
    [SerializeField]
    private float _attackCoolTime = Constants.DRONE_ATTACKDELAY;

    /// <summary>
    /// 사정거리
    /// </summary>
    [SerializeField]
    private float _attackRange = Constants.DRONE_ATTACKRANGE;

    /// <summary>
    /// 탐색 콜라이더 리스트
    /// </summary>
    [SerializeField]
    private Collider[] _results = new Collider[Constants.DRONE_SEARCHNUM];


    /// <summary>
    /// 공격 중인지
    /// </summary>
    [SerializeField]
    private bool _bIsAttacking;

    /// <summary>
    /// 현재 공격 목표
    /// </summary>
    [SerializeField]
    private Transform _currentTarget;
    public Transform currentTarget { get { return _currentTarget; } }

    /// <summary>
    /// 타겟 피격 이펙트
    /// </summary>
    public GameObject hitEffectPrefab;

    private void Update()
    {
        switch (_currentState)
        {
            case State.Idle:
                SearchEnemy();
                break;
            case State.Attack:
                if (currentTarget == null)
                {
                    return;
                }
                LookTarget();
                if (!_bIsAttacking)
                {
                    attackTry?.Invoke();
                    StartCoroutine(AttackTarget());
                }
                break;
            default:
                break;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Attack?.Invoke(WeaponType.None, ATTACKTYPE.NORMAL, _currentTarget.transform, transform);
        }
    }

    public void InitializeDrone(Player player)
    {
        _playerModel = player.playerModel;
        player.playerModel.OnDieEvent += ReleaseToPool;

        if (GameManager.Instance.skillManager.attackTech != null)
        {
            if(GameManager.Instance.skillManager.attackTech is Skill_Three_Attack skillThree)
            {
                Attack += skillThree.KnockBackEnemy;
            }
            else if (GameManager.Instance.skillManager.attackTech is Skill_Four_Attack skillFour)
            {
                skillFour.DroneEventConnect(this);
            }
            else
            {
                Attack += GameManager.Instance.skillManager.attackTech.HitEnemy;

            }
        }
    }

    /// <summary>
    /// 타겟 설정 
    /// </summary>
    public void SearchEnemy()
    {
        // 콜라이더 탐색 (필요하다면 레이어마스크 설정)
        int hitColliders = Physics.OverlapSphereNonAlloc(transform.position, _attackRange, _results);

        // 임시 저장할 변수
        MonsterBase targetMonster = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < hitColliders; i++)
        {
            // Monster인지 판단
            MonsterBase currentMonster = _results[i].GetComponent<MonsterBase>();
            if (currentMonster != null)
            {
                // Monster라면 거리 비교
                float distacne = Vector3.Distance(_results[i].transform.position, transform.position);
                if (distacne < minDistance)
                {
                    minDistance = distacne;
                    targetMonster = currentMonster;

                }
            }

        }

        // 몬스터가 탐색되었다면 타겟 설정
        if (targetMonster != null)
        {
            _currentTarget = targetMonster.transform;
            _currentState = State.Attack;
        }

    }

    /// <summary>
    /// 드론이 몬스터를 바라보게
    /// </summary>
    public void LookTarget()
    {
        Vector3 direction = (_currentTarget.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * Constants.DRONE_ROTATION_SPEED);
        }
    }

    /// <summary>
    /// 몬스터 공격
    /// </summary>
    /// <returns></returns>
    public IEnumerator AttackTarget()
    {
        _bIsAttacking = true;
        // 타겟이 존재하고 사정거리 안이라면
        if (_currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < _attackRange)
        {
            // 이부분에서 IDamageable 인터페이스를 삭제하고 동훈이 만든 IDamageAble 인터페이스로 바꿈
            IDamageable target = _currentTarget.GetComponent<IDamageable>();
            if (target != null)
            {
                Attack?.Invoke(WeaponType.None, ATTACKTYPE.NORMAL, currentTarget,transform);
                // 이부분에서 IDamageAble 인터페이스로 바꾸면서 TakeHit 함수대신 TakeDamage 함수로 바꿈
                target.TakeDamage(Constants.DRONE_ATTACK, null);
                yield return new WaitForSeconds(_attackCoolTime);
            }
        }
        _bIsAttacking = false;
        _currentState = State.Idle;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void ReleaseObject()
    {
        Attack = null;
        attackTry = null;
    }

    public void ReleaseToPool()
    {
        ReleaseObject();
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }

    public void OnDisable()
    {
        if(!_playerModel.isDead)
        {
            return;
        }
        ReleaseObject();
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}
