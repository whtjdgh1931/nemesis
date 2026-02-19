using System;
using System.Collections;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;
using UnityEngine.UIElements;


public class MonsterGrenade : PoolableObject
{

    public bool IsAttacking;


    [SerializeField] private float travelTime = 3.0f;     // 유탄이 도착하는 시간
    [SerializeField] private float explosionRadius = 3f;  // 폭발 반경
    [SerializeField] private float explosionDamage = 30f; // 폭발 데미지

    [SerializeField] private PoolableObject grenadeObject;
    [SerializeField] private PoolableObject explodeCircleObject;

    [Header("Effects")]
    [SerializeField] private PoolableObject bigExplosionEffect;
    [SerializeField] private PoolableObject smallExplosionEffect;

    // --- 새로 추가된 파라미터 (튜닝용) ---
    [Header("Parabola Height Tuning")]
    [SerializeField, Tooltip("짧은 거리(가까움)일 때의 최대 포물선 높이")]
    private float maxParabolaHeight = 15f;
    [SerializeField, Tooltip("먼 거리(멀리)일 때의 최소 포물선 높이")]
    private float minParabolaHeight = 10f;
    [SerializeField, Tooltip("이 거리 이하이면 '가깝다'로 간주")]
    private float closeDistance = 2f;
    [SerializeField, Tooltip("이 거리 이상이면 '멀다'로 간주")]
    private float farDistance = 10f;

    // --- 작은 유탄 관련 파라미터 ---
    [Header("Small Grenade Settings")]
    [SerializeField, Range(0.1f, 1f)] private float smallGrenadeScale = 0.5f;  // 작은 유탄 크기 비율
    [SerializeField] private float smallGrenadeTravelTime = 1.5f;
    [SerializeField] private float smallGrenadeDistance = 3f;  // 작은 유탄이 날아갈 거리
    [SerializeField] private float smallGrenadeHeight = 5f;    // 작은 유탄 포물선 높이
    [SerializeField] private float smallGrenadeExplosionRadius = 2f;
    [SerializeField] private float smallGrenadeExplosionDamage = 15f;

    [Header("Grenade Fire Settings")]
    [SerializeField] private float grenadeFireInterval = 15f;  // 유탄 발사 간격
    private bool isGrenadeActive = false;  // 유탄 발사 활성화 여부
    private Coroutine grenadePatternCoroutine;  // 유탄 패턴 코루틴 참조

    /// <summary>
    /// 유탄 데미지 증가 (아이스 탱크 생존 수에 따라)
    /// </summary>
    public void IncreaseDamage(float multiplier)
    {
        explosionDamage *= multiplier;
        smallGrenadeExplosionDamage *= multiplier;
    }

    /// <summary>
    /// 15초마다 유탄을 발사하는 패턴 시작
    /// </summary>
    public void StartGrenadePattern()
    {
        if (!isGrenadeActive)
        {
            isGrenadeActive = true;
            grenadePatternCoroutine = StartCoroutine(GrenadePatternCoroutine());
        }
    }

    /// <summary>
    /// 유탄 발사 패턴 중지
    /// </summary>
    public void StopGrenadePattern()
    {
        isGrenadeActive = false;
        if (grenadePatternCoroutine != null)
        {
            StopCoroutine(grenadePatternCoroutine);
            grenadePatternCoroutine = null;
        }
    }

    private IEnumerator GrenadePatternCoroutine()
    {
        while (isGrenadeActive)
        {
            FireGrenade();
            yield return new WaitForSeconds(grenadeFireInterval);
        }
    }

    public void FireGrenade()
    {
        // 프리팹 null 체크
        if (grenadeObject == null || explodeCircleObject == null)
        {
            return;
        }

        // 부모 객체(Omega_X7) 위치를 중심으로 4개 구역 설정
        Vector3 centerPos = transform.parent != null ? transform.parent.position : transform.position;

        // 큰 유탄 발사 시 보스 위치에서 큰 폭발 이펙트 재생
        if (bigExplosionEffect != null)
        {
            MonsterBase monster = transform.parent?.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.GetEffectFromPool(bigExplosionEffect, centerPos, Quaternion.identity);
                GameManager.Instance.SoundManager.PlaySfxAt("Monster_Grenade", transform.position);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            Vector3 randomTarget = centerPos;

            switch (i)
            {
                case 0: // 왼쪽 위
                    randomTarget += new Vector3(
                        UnityEngine.Random.Range(-10f, 0f),
                        0f,
                        UnityEngine.Random.Range(0f, 10f)
                    );
                    break;
                case 1: // 오른쪽 위
                    randomTarget += new Vector3(
                        UnityEngine.Random.Range(0f, 10f),
                        0f,
                        UnityEngine.Random.Range(0f, 10f)
                    );
                    break;
                case 2: // 왼쪽 아래
                    randomTarget += new Vector3(
                        UnityEngine.Random.Range(-10f, 0f),
                        0f,
                        UnityEngine.Random.Range(-10f, 0f)
                    );
                    break;
                case 3: // 오른쪽 아래
                    randomTarget += new Vector3(
                        UnityEngine.Random.Range(0f, 10f),
                        0f,
                        UnityEngine.Random.Range(-10f, 0f)
                    );
                    break;
            }

            // 글로벌 좌표 기준 y축을 0으로 설정
            randomTarget.y = 0f;

            StartCoroutine(LaunchGrenade(randomTarget));
        }
    }

    private IEnumerator LaunchGrenade(Vector3 targetPos)
    {
        if (grenadeObject == null)
        {
            yield break;
        }

        Vector3 startPos = transform.parent != null ? transform.parent.position : transform.position;
        startPos += Vector3.up * 1.0f;

        GameObject grenade = GameManager.Instance.PoolManager.GetFromPool(grenadeObject, startPos, Quaternion.identity);

        if (grenade == null)
        {
            yield break;
        }

        grenade.transform.localScale = Vector3.one;

        if (explodeCircleObject != null)
        {
            GameObject explodeRange = GameManager.Instance.PoolManager.GetFromPool(explodeCircleObject, targetPos + Vector3.up * 0.1f, Quaternion.Euler(90f, 0f, 0f));
            AttackDecalEffect effect = explodeRange?.GetComponent<AttackDecalEffect>();
            if (effect != null)
            {
                effect.Play(travelTime, explosionRadius);
            }
        }

        StartCoroutine(ParabolaMove(grenade, targetPos));
        yield return new WaitForSeconds(travelTime);
    }

    private IEnumerator ParabolaMove(GameObject grenade, Vector3 target)
    {
        Vector3 start = grenade.transform.position;
        float elapsed = 0f;

        float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(target.x, 0f, target.z));

        float tDist = Mathf.InverseLerp(closeDistance, farDistance, distance);
        float smooth = Mathf.SmoothStep(0f, 1f, tDist);
        float parabolaHeight = Mathf.Lerp(maxParabolaHeight, minParabolaHeight, smooth);

        if (distance < 0.2f)
            parabolaHeight = Mathf.Min(parabolaHeight, maxParabolaHeight * 0.8f);

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector3 flatPos = Vector3.Lerp(start, target, t);
            float parabola = 4f * parabolaHeight * (t - t * t);
            flatPos.y += parabola;

            grenade.transform.position = flatPos;
            yield return null;
        }

        Vector3 explosionPos = grenade.transform.position;
        Explode(explosionPos);

        // 작은 유탄 5개 발사
        SpawnSmallGrenades(explosionPos);

        GameManager.Instance.PoolManager.ReleaseToPool(grenade);
    }

    private void SpawnSmallGrenades(Vector3 centerPos)
    {
        int grenadeCount = 5;
        float angleStep = 360f / grenadeCount; // 72도

        for (int i = 0; i < grenadeCount; i++)
        {
            float angle = angleStep * i;
            float radian = angle * Mathf.Deg2Rad;

            // 목표 위치 계산 (Y축 회전)
            Vector3 direction = new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
            Vector3 targetPos = centerPos + direction * smallGrenadeDistance;

            // 작은 유탄 생성 및 발사
            StartCoroutine(LaunchSmallGrenade(centerPos, targetPos));
        }
    }

    private IEnumerator LaunchSmallGrenade(Vector3 startPos, Vector3 targetPos)
    {
        GameObject smallGrenade = GameManager.Instance.PoolManager.GetFromPool(
            grenadeObject,
            startPos,
            Quaternion.identity
        );

        if (smallGrenade == null) yield break;

        smallGrenade.transform.localScale = Vector3.one * smallGrenadeScale;

        GameObject smallExplodeRange = GameManager.Instance.PoolManager.GetFromPool(
            explodeCircleObject,
            targetPos + Vector3.up * 0.1f,
            Quaternion.Euler(90f, 0f, 0f)
        );

        if (smallExplodeRange != null)
        {
            AttackDecalEffect effect = smallExplodeRange.GetComponent<AttackDecalEffect>();
            if (effect != null)
            {
                effect.Play(smallGrenadeTravelTime, smallGrenadeExplosionRadius);
            }
        }

        float elapsed = 0f;

        while (elapsed < smallGrenadeTravelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / smallGrenadeTravelTime);

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            float parabola = 4f * smallGrenadeHeight * (t - t * t);
            flatPos.y += parabola;

            smallGrenade.transform.position = flatPos;
            yield return null;
        }

        SmallGrenadeExplode(smallGrenade.transform.position);

        smallGrenade.transform.localScale = Vector3.one;
        GameManager.Instance.PoolManager.ReleaseToPool(smallGrenade);
    }

    private void Explode(Vector3 position)
    {
        // 큰 유탄 폭발 시에도 작은 폭발 이펙트 재생
        if (smallExplosionEffect != null)
        {
            MonsterBase monster = transform.parent?.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.GetEffectFromPool(smallExplosionEffect, position, Quaternion.identity);
                GameManager.Instance.SoundManager.PlaySfxAt("Monster_Grenade", transform.position);
            }
        }

        Collider[] hits = Physics.OverlapSphere(position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(Constants.TAG_PLAYER))
            {
                IDamageable player = hit.GetComponent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(explosionDamage);
                }
            }
        }
    }

    private void SmallGrenadeExplode(Vector3 position)
    {
        // 작은 유탄 폭발 시 작은 폭발 이펙트 재생
        if (smallExplosionEffect != null)
        {
            MonsterBase monster = transform.parent?.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.GetEffectFromPool(smallExplosionEffect, position, Quaternion.identity);
                GameManager.Instance.SoundManager.PlaySfxAt("Monster_Grenade", transform.position);
            }
        }

        Collider[] hits = Physics.OverlapSphere(position, smallGrenadeExplosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(Constants.TAG_PLAYER))
            {
                IDamageable player = hit.GetComponent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(smallGrenadeExplosionDamage);
                }
            }
        }
    }
}