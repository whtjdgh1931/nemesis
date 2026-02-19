using System;
using UnityEngine;

/// <summary>
/// 기록판 상호작용을 위한 클래스
/// </summary>
public class RecordInteractor : InteractableObject
{
    public override InteractableType InteractableType => InteractableType.Record;

    public override event Action<IInteractable> OnInteracted;

    public override void TryGetInteracrtionKey(out string title, out string description)
    {
        title = "_recordTitle";
        description = "_recordDescription";
    }

    public override bool TryInteract(Transform subject)
    {
        GameManager.Instance.UIManager.OpenRecord();
        OnInteracted?.Invoke(this);
        return true;
    }
}
