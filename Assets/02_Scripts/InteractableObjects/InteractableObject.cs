using System;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] protected Transform _guidePoint;
    public abstract InteractableType InteractableType { get; }

    public Vector3 GuidePoint => _guidePoint.position;

    public abstract event Action<IInteractable> OnInteracted;

    public abstract void TryGetInteracrtionKey(out string title, out string description);

    public abstract bool TryInteract(Transform subject);

   
}
