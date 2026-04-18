using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상호작용 가능한 오브젝튿들을 관리하는 컨트롤러
/// </summary>
public class InteractionController : MonoBehaviour
{
    Dictionary<InteractableType, List<IInteractable>> _interactableMap = new();     // 상호작용 가능한 오브젝트들을 타입별로 맵핑

    public event Action<WeaponType> OnWeaponInteract;   // 무기와 상호작용했을 때 발행되는 이벤트
    public event Action<RoomInfo> OnDoorInteract;       // 문과 상호작용했을 때 발행되는 이벤트

    public void Initialize()
    {
        var manager = GameManager.Instance.InteractableManager;
        manager.OnInteractableRegistered += OnInteractorAdded;
        manager.OnInteractableUnregistered += OnInteractorRemoved;
    }

    void OnInteractorAdded(IInteractable interactable)
    {
        if (!_interactableMap.TryGetValue(interactable.InteractableType, out var list))
        {
            list = new List<IInteractable>();
            _interactableMap[interactable.InteractableType] = list;
        }
        list.Add(interactable);

        // 구독: 상호작용 시작/완료
        interactable.OnInteracted += PublishInteractableEventByType;
        //interactable.OnInteractionCompleted += OnInteractableCompleted;

    }

    void OnInteractorRemoved(IInteractable interactable)
    {
        if (_interactableMap.TryGetValue(interactable.InteractableType, out var list))
        {
            list.Remove(interactable);
            if (list.Count == 0) _interactableMap.Remove(interactable.InteractableType);
        }

        // 구독 해제
        interactable.OnInteracted -= PublishInteractableEventByType;
        //interactable.OnInteractionCompleted -= OnInteractableCompleted;
    }

    /// <summary>
    /// 상호작용 가능한 물체의 상호작용 이벤트를 구독하여
    /// 상호작용 가능한 물체의 타입에 따라 적절한 이벤트를 발행
    /// </summary>
    /// <param name="interactable"></param>
    void PublishInteractableEventByType(IInteractable interactable)
    {
        // 시작 이벤트에 대한 기본 라우팅 (필요 시 더 처리)
        switch (interactable.InteractableType)
        {
            case InteractableType.Weapon:
                if (interactable is WeaponInteractor w)
                {
                    OnWeaponInteract?.Invoke(w.WeaponType);
                }
                break;
            case InteractableType.Door:
                if (interactable is DoorInteractor d)
                {
                    if (d.RoomInfo == null)
                    {
                        Debug.LogError("InteractionController.PublishInteractableEventByType: DoorInteractor의 RoomInfo가 null입니다!");
                        return;
                    }
                    OnDoorInteract?.Invoke(d.RoomInfo);
                }
                break;
            case InteractableType.Reward:
                if(interactable is RewardInteractableObject rewardObj)
                {
                }
                break;

                // 기타 타입 처리
        }
    }

    void OnInteractableCompleted(IInteractable interactable)
    {
        // 완료 이벤트 라우팅: 예를 들어 문이나 아이템 등은 상호작용이 끝나면 파괴해야 한다
        switch (interactable.InteractableType)
        {
            case InteractableType.Door:
                // 실제 Unity 오브젝트로 캐스트해서 파괴
                if (interactable is MonoBehaviour mb)
                {
                    // mb.gameObject 를 파괴하면 컴포넌트와 게임오브젝트 모두 제거
                    Destroy(mb.gameObject);
                }
                else
                {
                    // 드물지만 MonoBehaviour가 아닐 경우(예: ScriptableObject라면) UnityEngine.Object로 캐스트 시도
                    var uo = interactable as UnityEngine.Object;
                    if (uo != null) Destroy(uo);
                    else Debug.LogWarning("Interactable is not a UnityEngine.Object, cannot Destroy it directly.");
                }
                break;
            case InteractableType.Reward:
                break;
            case InteractableType.Weapon:
                break;
            default:
                break;
        }
    }
}
