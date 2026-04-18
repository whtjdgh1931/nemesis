using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 몬스터 HP 바 아래에 디버프 아이콘을 표시하는 컴포넌트
/// </summary>
public class DebuffIconUI : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject debuffIconPrefab; // 디버프 아이콘 프리팹

    [Header("Container")]
    [SerializeField] private Transform iconContainer; // 아이콘들이 배치될 부모

    [Header("Settings")]
    [SerializeField] private float iconSize = 20f;
    [SerializeField] private float iconSpacing = 5f;
    [SerializeField] private int maxVisibleIcons = 5; // 최대 표시 개수

    [Header("Stack Text Settings")]
    [SerializeField] private TMPro.TMP_FontAsset stackTextFont; // 스택 텍스트 폰트
    [SerializeField] private TMPro.FontStyles stackTextFontStyle = TMPro.FontStyles.Bold; // 스택 텍스트 폰트 스타일

    [Header("Debuff Sprites")]
    [SerializeField] private Sprite slowIcon;
    [SerializeField] private Sprite poisonIcon;
    [SerializeField] private Sprite overloadIcon;
    [SerializeField] private Sprite stunIcon;
    [SerializeField] private Sprite confusionIcon;
    [SerializeField] private Sprite bindingIcon;
    [SerializeField] private Sprite weakenIcon;

    private Dictionary<string, GameObject> activeIcons = new Dictionary<string, GameObject>();
    private DebuffHandler targetDebuffHandler;

    private void OnEnable()
    {
        // 디버프 이벤트 구독
        DebuffHandler.OnDebuff += OnDebuffChanged;
    }

    private void OnDisable()
    {
        // 디버프 이벤트 구독 해제
        DebuffHandler.OnDebuff -= OnDebuffChanged;
    }

    /// <summary>
    /// 디버프 핸들러 설정
    /// </summary>
    public void SetDebuffHandler(DebuffHandler handler)
    {
        targetDebuffHandler = handler;
        RefreshAllIcons();
    }

    /// <summary>
    /// 디버프 변경 시 호출
    /// </summary>
    private void OnDebuffChanged(DebuffHandler handler)
    {
        // 자신의 DebuffHandler가 아니면 무시
        if (handler != targetDebuffHandler)
            return;

        RefreshAllIcons();
    }

    /// <summary>
    /// 모든 아이콘 새로고침
    /// </summary>
    private void RefreshAllIcons()
    {
        if (targetDebuffHandler == null)
            return;

        // 기존 아이콘 전부 제거
        ClearAllIcons();

        // 현재 걸린 디버프들 확인 및 아이콘 생성
        CheckAndCreateIcon(Constants.DEBUFF_SLOW, slowIcon);
        CheckAndCreateIcon(Constants.DEBUFF_POISON, poisonIcon);
        CheckAndCreateIcon(Constants.DEBUFF_OVERLOAD, overloadIcon);
        CheckAndCreateIcon(Constants.DEBUFF_STUN, stunIcon);
        CheckAndCreateIcon(Constants.DEBUFF_CONFUSION, confusionIcon);
        CheckAndCreateIcon(Constants.DEBUFF_BINDING, bindingIcon);
        CheckAndCreateIcon(Constants.DEBUFF_WEAKEN, weakenIcon);
    }

    /// <summary>
    /// 디버프 확인 및 아이콘 생성
    /// </summary>
    private void CheckAndCreateIcon(string debuffName, Sprite icon)
    {
        if (targetDebuffHandler.HasDebuff(debuffName))
        {
            CreateIcon(debuffName, icon);
        }
    }

    /// <summary>
    /// 아이콘 생성
    /// </summary>
    private void CreateIcon(string debuffName, Sprite icon)
    {
        if (icon == null || debuffIconPrefab == null || iconContainer == null)
            return;

        // 최대 개수 체크
        if (activeIcons.Count >= maxVisibleIcons)
            return;

        GameObject iconObj = Instantiate(debuffIconPrefab, iconContainer);
        Image iconImage = iconObj.GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }

        // 크기 설정
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        }

        activeIcons[debuffName] = iconObj;

        // 스택 표시 (독, 과부하)
        if (debuffName == Constants.DEBUFF_POISON || debuffName == Constants.DEBUFF_OVERLOAD)
        {
            int stackCount = targetDebuffHandler.GetStackCount(debuffName);
            if (stackCount > 1)
            {
                AddStackText(iconObj, stackCount);
            }
        }
    }

    /// <summary>
    /// 스택 수 텍스트 추가
    /// </summary>
    private void AddStackText(GameObject iconObj, int stackCount)
    {
        // 텍스트 오브젝트 생성
        GameObject textObj = new GameObject("StackText");
        textObj.transform.SetParent(iconObj.transform, false);

        TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = stackCount.ToString();
        text.fontSize = iconSize * 0.6f;
        text.color = Color.white;
        text.alignment = TMPro.TextAlignmentOptions.BottomRight;
        text.fontStyle = TMPro.FontStyles.Bold;

        if (stackTextFont != null)
        {
            text.font = stackTextFont;
        }

        // 텍스트 위치 (아이콘 우측 하단)
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 모든 아이콘 제거
    /// </summary>
    private void ClearAllIcons()
    {
        foreach (var icon in activeIcons.Values)
        {
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        activeIcons.Clear();
    }

    /// <summary>
    /// 정리
    /// </summary>
    public void Cleanup()
    {
        ClearAllIcons();
        targetDebuffHandler = null;
        DebuffHandler.OnDebuff -= OnDebuffChanged;
    }
}