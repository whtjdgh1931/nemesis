using System;
using UnityEngine;

/// <summary>
/// 특정 지점을 중심으로 범위 내의 IDamageable 객체를 감지하는 클래스
/// </summary>
public class DamageableDetector : MonoBehaviour
{
    [SerializeField] Transform _detectPoint;        // 감지 지점
    [SerializeField] float _radius;                 // 구형 감지 범위의 반지름
    [SerializeField] LayerMask _targetLayerMask;    // 감지할 레이어마스크

    public Transform HitPoint => _detectPoint; // 감지 지점 프로퍼티

    /// <summary>
    /// IDamageable 감지 이벤트
    /// </summary>
    public event Action<IDamageable> OnDetected;

    Collider[] _colliders = new Collider[5];

    /// <summary>
    /// 범위 내의 IDamageable을 감지하는 함수
    /// </summary>
    public void DetectDamageable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i] = null; // 배열 초기화
        }

        // _detectPoint.position을 중심으로
        // _radius 반지름의 구를 그리고
        // 그것과 겹치는 _targetMaskLayer의 콜라이더들을
        // _colliders 배열에 순서대로 저장
        int count =
            Physics.OverlapSphereNonAlloc(
                _detectPoint.position,
                _radius,
                _colliders,
                _targetLayerMask);

        // 감지된 콜라이더의 수가 0보다 크면
        if (count > 0)
        {
            foreach (var collider in _colliders)
            {
                if (collider == null) continue;

                //  GetComponentInParent();
                // 자신이나 부모 게임오브젝트에서 컴포넌트를 찾아 반환해 주는 함수
                IDamageable damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    OnDetected.Invoke(damageable);
                }
            }
        }
    }

    /// <summary>
    /// 감지 지점의 중심을 설정하는 함수
    /// </summary>
    /// <param name="detectPoint"></param>
    public void SetDetectPoint(Transform detectPoint)
    {
        _detectPoint = detectPoint;
    }

    /// <summary>
    /// _detectPoint를 중심으로 _radius 반지름의 구를 그려 감지 범위를 시각화
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (_detectPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_detectPoint.position, _radius);
    }
}
