using TMPro;
using UnityEngine;

/// <summary>
/// 상호작용 가능한 상태일 때 UI를 표시하는 뷰 클래스
/// </summary>
public class InteractionGuideView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _interactionGuideText; // 상호작용 안내 텍스트 UI

    public void Initialize()
    {
        HideUI(); // 초기에는 UI를 숨김
    }

    public void ShowUI(IInteractable interactable)
    {
        // _interactionGuideText.text = interactable.InteractionPrompt; // 상호작용 안내 문구 설정
        gameObject.transform.position = interactable.GuidePoint; // UI 위치 설정
        gameObject.SetActive(true); // UI 활성화
    }

    public void HideUI()
    {
        gameObject.SetActive(false); // UI 비활성화
    }
}
