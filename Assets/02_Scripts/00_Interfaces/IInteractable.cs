using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상호작용 가능한 인터페이스
/// 이 인터페이스를 구현하는 오브젝트는 플레이어와 상호작용할 수 있습니다.
/// Interactable 오브젝트는 플레이어가 접근했을 때 상호작용 가이드를 표시하고,
/// Player게임오브젝트에 있는 InteractableDetector 스크립트를 통해 상호작용을 처리합니다.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 상호작용 가이드를 표시할 월드 좌표
    /// </summary>
    Vector3 GuidePoint { get; }

    /// <summary>
    /// Interactor의 타입을 반드시 명시
    /// </summary>
    InteractableType InteractableType { get; }

    event Action<IInteractable> OnInteracted;

   /// <summary>
   /// 상호작용을 시도하는 함수
   /// </summary>
   /// <param name="subject">상호작용 하는 객체의 Transform</param>
   /// <returns></returns>
    bool TryInteract(Transform subject);

    void TryGetInteracrtionKey(out string title, out string description);
}