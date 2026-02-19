using System;
using UnityEngine;

/// <summary>
/// 상호작용 가능한 무기(훈련장의 무기)가 상호작용 하기 위해 구현하는 클래스
/// </summary>
public class WeaponInteractor : InteractableObject
{
    [SerializeField] string _title;
    [SerializeField] string _instruction;

    [SerializeField] WeaponType _weaponType;

    bool _isInteracting = false;

    public override InteractableType InteractableType => InteractableType.Weapon;
    public WeaponType WeaponType => _weaponType;

    public override event Action<IInteractable> OnInteracted;

    public override void TryGetInteracrtionKey(out string title, out string instruction)
    {
        title = _title;
        instruction = _instruction;
    }

    // TryInteract 패턴: 상호작용 시도 결과를 즉시 반환 (true: 시작, false: 거부)
    public override bool TryInteract(Transform subject)
    {
        // 이미 상호작용 중이면 거부
        if (_isInteracting)
            return false;

        // 예: 플레이어 인벤토리/장착 상태 검사 (프로젝트에 맞게 확장)
        // if (PlayerInventory.Instance.IsFull()) { GameManager.Instance.UIManager.ShowMessage("인벤토리가 가득합니다."); return false; }

        _isInteracting = true;

        // 상호작용 매니저에 등록 (기존 로직 유지)
        GameManager.Instance.InteractableManager.Register(this);

        // 상호작용 이벤트 발생
        OnInteracted?.Invoke(this);

        // 무기 줍기는 대부분 즉시 완료형이므로 바로 해제
        _isInteracting = false;

        return true;
    }
}