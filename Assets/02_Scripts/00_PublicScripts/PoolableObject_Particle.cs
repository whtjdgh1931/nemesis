using UnityEngine;

/// <summary>
/// 파티클 시스템이 있는 오브젝트의 풀링을 담당하는 클래스
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PoolableObject_Particle : PoolableObject
{
    [SerializeField] ParticleSystem _particleSystem;

    private void Awake()
    {
        if (_particleSystem == null)
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        // 파티클의 재생이 끝나면 풀로 되돌림
        if (_particleSystem != null && !_particleSystem.isPlaying)
        {
            GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
        }
    }
}
