using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DetectPoint를 하나 지정하고 그 점 주위에 원을 그려서
/// IInteractable 객체를 감지하는 클래스
/// FixedUpdate 주기로 자동 감지한다.
/// </summary>
public class InteractableDetector : MonoBehaviour
{
    [Header("----- 컴포넌트 참조 -----")]
    [SerializeField] Transform _detectPoint;            // OverlapSphere 감지 지점

    [Header("----- 설정 데이터 -----")]
    [SerializeField] float _radius;                     // OverlapSphere 반지름
    [SerializeField] LayerMask _targetLayerMask;        // 감지할 레이어마스크

    IInteractable _detectedInteractable;                // 현재 감지한 Interactable 객체
    Collider[] _hits = new Collider[10];                // 감지된 콜라이더 배열

    public IInteractable DetectedInteractable => _detectedInteractable; // 현재 감지한 Interactable 객체 공개용 프로퍼티

    /// <summary>
    /// 상호작용 가능한 물체를 감지했을 때 호출되는 이벤트
    /// </summary>
    public event Action<IInteractable> OnDetected;

    /// <summary>
    /// 상호작용 가능한 물체를 놓쳤을 때 호출되는 이벤트
    /// </summary>
    public event Action OnMissed;

    public event Action OnInteractionStarted;

    private void FixedUpdate()
    {
        Detect();
    }

    /// <summary>
    /// 목표로 하는 IInteractable을 감지하고 감지에 성공했을 때 OnDetected 이벤트를 호출한다.
    /// </summary>
    public void Detect()
    {
        // DetectPoint 위치에서 반지름 _radius, 레이어마스크 _targetLayerMask로
        // OverlapSphere를 수행하여 감지된 콜라이더 중 가장 가까운 콜라이더를 찾는다.
        int hitCount = Physics.OverlapSphereNonAlloc(_detectPoint.position, _radius, _hits, _targetLayerMask);
        //Debug.Log(hitCount + "개의 물체 감지됨");

        // 감지된 물체가 있을 경우 IInteractable 인터페이스를 갖고 있는지 확인
        IInteractable nearestInteractable = null;
        float minDistance = float.MaxValue;

        // 가장 가까운 IInteractable 찾기
        for (int i = 0; i < hitCount; i++)
        {
            IInteractable interactable = _hits[i].GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(_detectPoint.position, _hits[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestInteractable = interactable;
                }
            }
        }

        // 가장 가까운 IInteractable이 이전에 감지된 것과 다를 때만 이벤트 발행
        if (nearestInteractable != null && nearestInteractable != _detectedInteractable)
        {
            _detectedInteractable = nearestInteractable;
            OnDetected?.Invoke(_detectedInteractable);
            //Debug.Log("상호작용 가능한 대상 감지됨: " + _detectedInteractable.InteractableType);
        }

        // 감지된 IInteractable이 없어진 경우
        if (nearestInteractable == null && _detectedInteractable != null)
        {
            _detectedInteractable = null;
            OnMissed?.Invoke();
            //Debug.Log("상호작용 가능한 대상에서 멀어짐");
        }
    }

    /// <summary>
    /// 감지한 IInteractable과 상호작용을 수행하는 함수
    /// </summary>
    public void ExecuteInteraction()
    {
        // 감지된 IInteractable이 없으면 함수 종료
        if (_detectedInteractable == null) return;

        _detectedInteractable.TryInteract(transform);
        OnInteractionStarted?.Invoke();
    }

    /// <summary>
    /// _detectPoint를 중심으로 _radius 반지름의 구를 그려서 감지 범위를 시각화
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (_detectPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_detectPoint.position, _radius);
    }
}
