using System.Collections;
using UnityEngine;

/// <summary>
/// 예시: 레이저형 스페셜(차지 → 단타 발사, Wall에서 끊김, 적 관통)
/// - 차지 타이머는 Player(또는 이 컴포넌트)가 owner.StartCoroutine으로 관리
/// - Initialize(owner) 반드시 호출
/// </summary>
public class PlayerRifleSpecialAttacker : PlayerSpecialAttacker
{
    [Header("Charge")]
    public float maxChargeTime = 1.5f;
    public float minWidth = 0.05f;
    public float maxWidth = 0.6f;

    [Header("Laser")]
    public GameObject laserPrefab; // LineRenderer 포함된 프리팹
    [SerializeField] string _energyCirclePrefabPath = "Prefabs/Bullet/EnergyCircle";
    public float maxDistance = 100f;
    public LayerMask wallMask;
    public LayerMask enemyMask;
    public float visualLifetime = 0.25f;
    [SerializeField] Transform _firePoint;

    GameObject _chargeEffect;
    AudioSource _chargeSFX;

    Coroutine _chargeRoutine;
    float _chargeTimer;

    // 중복 발사 방지 플래그
    bool _hasFired = false;

    public override WeaponType WeaponType => WeaponType.Rifle;

    public void Initialize(Player player, Transform firePoint)
    {
        base.Initialize(player);
        maxChargeTime = 1.5f;
        _firePoint = firePoint;
    }

    public override void StartCharge()
    {
        base.StartCharge();

        // 이미 차지중이면 중복 시작 방지
        if (_chargeRoutine != null)
            return;

        // 발사 플래그 리셋
        _hasFired = false;

        // owner에서 코루틴을 시작하여 차지 업데이트
        _chargeRoutine = _player.StartCoroutine(ChargeRoutine());
        _chargeEffect = GameManager.Instance.PoolManager.GetFromPool("Effect/RifleChargeEffect", _firePoint.position, _firePoint.rotation,_firePoint);
        _chargeSFX = GameManager.Instance.SoundManager.PlaySfxAt("RifleChargeSFX", _firePoint.position, false, 1, 0.8f);
    }

    public override void StopChargeAndFire()
    {
        // 이미 발사한 경우 재발사 방지
        if (_hasFired)
        {
            // 코루틴이 돌고 있으면 중단하고 정리
            if (_chargeRoutine != null) { _player.StopCoroutine(_chargeRoutine); _chargeRoutine = null; }
            _chargeTimer = 0f;
            base.StopChargeAndFire();
            return;
        }

        // 멈추고 발사
        if (_chargeRoutine != null) { _player.StopCoroutine(_chargeRoutine); _chargeRoutine = null; }
        float ratio = Mathf.Clamp01(_chargeTimer / maxChargeTime);
        _chargeTimer = 0f;

        // 실제 발사
        FireWithCharge(ratio);

        base.StopChargeAndFire(); // triggers events & EndSpecial
        GameManager.Instance.PoolManager.ReleaseToPool(_chargeEffect);
        GameManager.Instance.SoundManager.StopSfx(_chargeSFX);
    }

    public override void CancelCharge()
    {
        if (_chargeRoutine != null) { _player.StopCoroutine(_chargeRoutine); _chargeRoutine = null; }
        _chargeTimer = 0f;
        _hasFired = false;
        base.CancelCharge();
    }

    IEnumerator ChargeRoutine()
    {
        _chargeTimer = 0f;
        
        while (_chargeTimer < maxChargeTime)
        {
            _chargeTimer += Time.deltaTime;
            float ratio = Mathf.Clamp01(_chargeTimer / maxChargeTime);
            RaiseChargeUpdated(ratio);
            yield return null;
        }

        // 자동 풀차지 시 자동 발사
        _chargeRoutine = null;
        float fullRatio = 1f;

        // 발사 플래그 세우고 발사
        if (!_hasFired)
        {
            FireWithCharge(fullRatio);
            _hasFired = true;
        }

        // 차지 루틴에서 직접 EndSpecial/Stop 이벤트를 호출할 수 있음.
        base.StopChargeAndFire();
        EndSpecial();
    }

    protected override void Fire()
    {
        // 파생 클래스 외부에서 직접 호출하지 않도록 비워두거나 기본 동작 구현
        FireWithCharge(0f);
    }

    void FireWithCharge(float ratio)
    {
        // 이미 발사된 경우 재진입 방지
        if (_hasFired) return;
        _hasFired = true;

        float width = Mathf.Lerp(minWidth, maxWidth, ratio);

        Vector3 origin = _firePoint.position;
        Vector3 dir = _player.transform.forward;

        if(EventBus.HasMutant4)
        {
            // 돌연변이 4 획득시 에너지 구체 발사
            GameObject energyCirclePrefab = GameManager.Instance.PoolManager.GetFromPool(_energyCirclePrefabPath, origin, Quaternion.identity);
            if (energyCirclePrefab != null)
            {
                Vector3 moveDir = dir;
                var energyCircle = energyCirclePrefab.GetComponent<EnergyCircle>();
                if (energyCircle != null)
                {
                    energyCircle.Initialize(maxChargeTime * ratio, moveDir);
                }
            }
            return;
        }

        if (laserPrefab != null)
        {
            GameObject go = GameManager.Instance.PoolManager.GetFromPool("Prefabs/Bullet/LaserBeam", origin, Quaternion.identity);
            // 선택사항: 부모를 정리된 오브젝트로 붙여서 계층 관리
            go.transform.SetParent(_player.transform, true);

            var laser = go.GetComponent<LaserBeam>();
            if (laser == null)
            {
                Debug.LogError("Laser prefab does not contain LaserBeam component.");
                Destroy(go);
                return;
            }

            laser.Initialize(origin, dir, maxDistance, width, wallMask, enemyMask, _player.gameObject);
            laser.lifeTime = visualLifetime;
            laser.Fire();

        }
        else
        {
            // 프리팹 없으면 직접 판정만
            float endDist = maxDistance;
            if (Physics.Raycast(origin, dir, out RaycastHit wh, maxDistance, wallMask, QueryTriggerInteraction.Ignore))
            {
                endDist = wh.distance;
            }
            RaycastHit[] hits = Physics.RaycastAll(origin, dir, endDist, enemyMask, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                var dmg = h.collider.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    EventBus.MonsterHit(WeaponType.Rifle, ATTACKTYPE.SPECIALATTACK, h.transform, _player.transform);
                }
            }
        }
        GameManager.Instance.PoolManager.ReleaseToPool(_chargeEffect);
        GameManager.Instance.SoundManager.StopSfx(_chargeSFX);
        GameManager.Instance.SoundManager.PlaySfxAt("RifleChargeFireSFX", origin, false, 1, 1f);
    }
}