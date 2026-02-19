using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상호작용 가능한 오브젝튿들을 관리하는 매니저
/// </summary>
public class InteractableManager : MonoBehaviour
{
    public event Action<IInteractable> OnInteractableRegistered;
    public event Action<IInteractable> OnInteractableUnregistered;

    public void Register(IInteractable interactable)
    {
        OnInteractableRegistered?.Invoke(interactable);
    }

    public void Unregister(IInteractable interactable)
    {
        OnInteractableUnregistered?.Invoke(interactable);
    }
}