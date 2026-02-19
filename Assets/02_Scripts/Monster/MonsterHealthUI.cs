using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class MonsterHealthUI : PoolableObject, IInitializePoolable, IReleasePoolable
{
    [Header("References")]
    [SerializeField] private MonsterBase monsterBase;
    [SerializeField] private Transform monsterTransform;

    [Header("Normal UI Components")]
    [SerializeField] private GameObject normalHealthUI;
    [SerializeField] private Slider normalHealthSlider;
    [SerializeField] private DebuffIconUI normalDebuffIconUI;

    [Header("Boss UI Components")]
    [SerializeField] private GameObject bossHealthUI;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI bossHealthText;
    [SerializeField] private DebuffIconUI bossDebuffIconUI;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private bool hideWhenDead = true;

    [Header("Boss UI Position")]
    [SerializeField] private Vector2 bossUIPosition = new Vector2(0, 350); // Inspector에서 조정 가능

    [Header("Color Settings")]
    [SerializeField] private bool useColorGradient = true;
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private Image normalFillImage;
    private Image bossFillImage;

    // 전역 Canvas
    public static Canvas monsterHealthUIRoot;

    // 현재 사용 중인 UI 타입
    private bool isBoss = false;
    private Slider currentSlider;
    private Image currentFillImage;

    private void LateUpdate()
    {
        if (monsterBase == null || monsterTransform == null)
            return;

        // 몬스터가 죽었을 때 처리
        if (hideWhenDead && monsterBase.GetMonsterState() == MonsterBase.MonsterState.Die)
        {
            GameManager.Instance.PoolManager.ReleaseToPoolByInterface(this);
            return;
        }

        // 보스는 화면 상단 고정, 일반 몬스터는 머리 위 따라다니기
        if (!isBoss)
        {
            Vector3 worldPosition = monsterTransform.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // 카메라 뒤에 있으면 숨김
            if (screenPosition.z < 0)
            {
                if (normalHealthUI.activeSelf)
                    normalHealthUI.SetActive(false);
                return;
            }

            if (!normalHealthUI.activeSelf)
                normalHealthUI.SetActive(true);

            rectTransform.position = screenPosition;
        }

        DebuffHandler debuffHandler = monsterBase.GetComponent<DebuffHandler>();

        if (debuffHandler != null)
        {
            if (isBoss && bossDebuffIconUI != null)
            {
                bossDebuffIconUI.SetDebuffHandler(debuffHandler);
            }
            else if (!isBoss && normalDebuffIconUI != null)
            {
                normalDebuffIconUI.SetDebuffHandler(debuffHandler);
            }
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (monsterBase == null || currentSlider == null)
            return;

        float currentHealth = monsterBase.GetCurrentHealth();
        float maxHealth = monsterBase.GetMaxHealth();
        float healthPercentage = currentHealth / maxHealth;

        currentSlider.value = healthPercentage;

        // 색상 변경
        if (useColorGradient && currentFillImage != null)
        {
            if (healthPercentage > 0.5f)
                currentFillImage.color = highHealthColor;
            else if (healthPercentage > 0.25f)
                currentFillImage.color = midHealthColor;
            else
                currentFillImage.color = lowHealthColor;
        }

        // 보스 체력 텍스트 업데이트
        if (isBoss && bossHealthText != null)
        {
            bossHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }

        // 체력이 꽉 찼을 때 숨김 옵션
        if (hideWhenFull && healthPercentage >= 0.99f)
        {
            if (isBoss && bossHealthUI.activeSelf)
                bossHealthUI.SetActive(false);
            else if (!isBoss && normalHealthUI.activeSelf)
                normalHealthUI.SetActive(false);
        }
    }

    public void SetMonster(MonsterBase monster)
    {
        monsterBase = monster;
        monsterTransform = monster.transform;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 보스인지 확인
        isBoss = (monster.GetMonsterSize() == MonsterSize.BIG);

        // UI 전환
        if (isBoss)
        {
            SetupBossUI();
        }
        else
        {
            SetupNormalUI();
        }

        UpdateHealthBar();
    }

    /// <summary>
    /// 일반 몬스터 UI 설정
    /// </summary>
    private void SetupNormalUI()
    {
        // 보스 UI 비활성화
        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);

        // 일반 UI 활성화
        if (normalHealthUI != null)
            normalHealthUI.SetActive(true);

        // 현재 사용 중인 UI 설정
        currentSlider = normalHealthSlider;
        currentFillImage = normalFillImage;
    }

    /// <summary>
    /// 보스 UI 설정
    /// </summary>
    private void SetupBossUI()
    {
        // 일반 UI 비활성화
        if (normalHealthUI != null)
            normalHealthUI.SetActive(false);

        // 보스 UI 활성화
        if (bossHealthUI != null)
            bossHealthUI.SetActive(true);

        // 현재 사용 중인 UI 설정
        currentSlider = bossHealthSlider;
        currentFillImage = bossFillImage;

        // 보스 이름 설정
        if (bossNameText != null)
        {
            string locale = LocalizationSettings.SelectedLocale.Identifier.Code;
            bossNameText.text = locale == "ko" ? monsterBase.GetKoreanName() : monsterBase.GetEnglishName();
        }

        // 보스 UI 위치 강제 설정
        if (bossHealthUI != null)
        {
            RectTransform bossRect = bossHealthUI.GetComponent<RectTransform>();
            if (bossRect != null)
            {
                // 앵커를 상단 중앙으로
                bossRect.anchorMin = new Vector2(0.5f, 1f);
                bossRect.anchorMax = new Vector2(0.5f, 1f);
                bossRect.pivot = new Vector2(0.5f, 1f);

                // 위치를 Inspector에서 설정한 값으로
                bossRect.anchoredPosition = bossUIPosition;
            }
        }
    }

    public void OnHealthChanged()
    {
        UpdateHealthBar();
    }

    #region IInitializePoolable
    public void Initialize(object data = null)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (monsterHealthUIRoot == null)
        {
            GameObject rootObj = new GameObject("MonsterHealthUIRoot");
            monsterHealthUIRoot = rootObj.AddComponent<Canvas>();
            monsterHealthUIRoot.renderMode = RenderMode.ScreenSpaceOverlay;
            monsterHealthUIRoot.sortingOrder = 100;

            CanvasScaler scaler = rootObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            rootObj.AddComponent<GraphicRaycaster>();

            RectTransform rootRect = rootObj.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = Vector2.zero;
                rootRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        transform.SetParent(monsterHealthUIRoot.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (normalHealthSlider != null && normalFillImage == null)
            normalFillImage = normalHealthSlider.fillRect?.GetComponent<Image>();

        if (bossHealthSlider != null && bossFillImage == null)
            bossFillImage = bossHealthSlider.fillRect?.GetComponent<Image>();

        if (normalHealthUI != null)
            normalHealthUI.SetActive(false);
        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);

        if (data is MonsterBase monster)
            SetMonster(monster);

        gameObject.SetActive(true);

        if (!isBoss && monsterTransform != null && mainCamera != null)
        {
            Vector3 worldPosition = monsterTransform.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;
        }

        UpdateHealthBar();
    }
    #endregion

    #region IReleasePoolable
    public void ReleaseObjectPool()
    {
        monsterBase = null;
        monsterTransform = null;
        isBoss = false;

        if (normalHealthUI != null)
            normalHealthUI.SetActive(false);
        if (bossHealthUI != null)
            bossHealthUI.SetActive(false);

        // Slider 초기화
        if (normalHealthSlider != null)
            normalHealthSlider.value = 1f;
        if (bossHealthSlider != null)
            bossHealthSlider.value = 1f;

        // 색상 초기화
        if (normalFillImage != null)
            normalFillImage.color = highHealthColor;
        if (bossFillImage != null)
            bossFillImage.color = highHealthColor;

        // 디버프 초기화
        if (normalDebuffIconUI != null)
            normalDebuffIconUI.Cleanup();
        if (bossDebuffIconUI != null)
            bossDebuffIconUI.Cleanup();

        currentSlider = null;
        currentFillImage = null;

        gameObject.SetActive(false);
    }
    #endregion
}