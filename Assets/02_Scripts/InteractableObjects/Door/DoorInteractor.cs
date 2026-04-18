using System;
using UnityEngine;

/// <summary>
/// Door의 상호작용을 담당하는 클래스
/// </summary>
public class DoorInteractor : InteractableObject
{
    [SerializeField] Collider _interactionCollider;

    [SerializeField] InteractableType _interactableType = InteractableType.Door;

    [SerializeField] RoomInfo _roomInfo;
    [SerializeField] bool _canInteract = true;

    // 재진입 방지 (선택)
    bool _isInteracting = false;

    public RoomInfo RoomInfo => _roomInfo;
    public override InteractableType InteractableType => _interactableType;

    // 파생에서 event를 다시 선언하지 마세요; base.OnInteracted를 그대로 사용합니다.
    public override event Action<IInteractable> OnInteracted;

    private void OnEnable()
    {
        if(_interactionCollider == null)
            _interactionCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// 상호작용 시도. 성공하면 true 반환(상호작용 시작), 실패하면 false 반환.
    /// </summary>
    public override bool TryInteract(Transform subject)
    {
        // 이미 상호작용 중이면 거부
        if (_isInteracting)
        {
            return false;
        }

        if (_roomInfo == null)
        {
            Debug.LogError($"DoorInteractor.TryInteract: RoomInfo가 null입니다. Door가 올바르게 Initialize 되었는지 확인하세요. (object={name})");
            return false;
        }

        if (!_canInteract)
        {
            return false;
        }

        // 필요하면 subject 체크
        if (subject == null)
        {
            Debug.LogWarning("TryInteract 호출 시 subject가 null입니다.");
        }

        // 상호작용 시작
        _isInteracting = true;

        // 플레이어 입력 끄기
        EventBus.SetCanGetInput(false);

        OnInteracted?.Invoke(this);

        // 즉시 처리형이면 바로 끝내고 _isInteracting 리셋 (혹은 연출/비동기라면 End 시 리셋)
        _isInteracting = false;
        return true;
    }

    // RoomInfo를 세팅하는 안전한 공개 메서드
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        if (roomInfo == null)
        {
            Debug.LogError("DoorInteractor.SetRoomInfo 호출 시 roomInfo가 null입니다! 호출자 스택을 확인하세요.");
            return;
        }
        _roomInfo = roomInfo;
    }

    public void ToggleInteraction(bool canInteract)
    {
        // 안전 검사
        if (_interactionCollider == null) return;

        // 필요하면 try-catch로 한번 더 방어 (보통 null 검사로 충분)
        try
        {
            _interactionCollider.enabled = canInteract;
        }
        catch (MissingReferenceException)
        {
            // 이미 파괴된 경우 레퍼런스 초기화
            _interactionCollider = null;
        }
    }

    public override void TryGetInteracrtionKey(out string title, out string description)
    {
        title = _roomInfo != null ? _roomInfo.GetTitleKey() : "알 수 없는 방";
        description = _roomInfo != null ? _roomInfo.GetDescriptionKey() : "설정되지 않은 방입니다.";
    }
}