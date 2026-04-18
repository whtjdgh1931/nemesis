using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [SerializeField] private SkillBtn _skillBtnPrefab;
    [SerializeField] private GameObject _listPanel;

    [SerializeField] private Image _skillImage;
    [SerializeField] private TextMeshProUGUI _skillScriptText;
    [SerializeField] private TextMeshProUGUI _skillValueScriptText;
    [SerializeField] private TextMeshProUGUI _skillLevelText;
    [SerializeField] private TextMeshProUGUI _skillNameText;
    [SerializeField] private Transform _parentContent;
    [SerializeField] private UpgradePanel _upgradePanel;
    [SerializeField] private RecordPanel _recordPanel;
    [SerializeField] private SettingPanel _settingPanel;

    [SerializeField] private GameObject _skillBtnPanel;
    [SerializeField] private GameObject _parentPanel;
    [SerializeField] private SkillBtn _skillChooseBtnPrefab;
    public event Action onRewardSelect;
    #region 스킬 리스트
    /// <summary>
    /// 현재 보유 스킬 리스트에서 선택한 버튼 정보
    /// </summary>
    private SkillBtn _currentSelectedSkillBtn;

    /// <summary>
    /// 현재 활성화 되어있는 스킬 버튼 리스트
    /// </summary>
    private List<SkillBtn> _activeChooseButtons = new List<SkillBtn>();

    [SerializeField]
    private SkillTooltip _skillTooltip;
    public SkillTooltip skillTooltip { get { return _skillTooltip; } }



    private void ApplySavedResolution()
    {
        int savedIndex = PlayerPrefs.HasKey(Constants.RESOLUTION_PREF_KEY) ? PlayerPrefs.GetInt(Constants.RESOLUTION_PREF_KEY) : 0;


        // 퀄리티 설정 (유효한 인덱스만)
        if (savedIndex >= 0 && savedIndex <= 3)
        {
            QualitySettings.SetQualityLevel(savedIndex);
        }
        else
        {
            Debug.LogError("해당 품질 인덱스 없음");
            return;
        }

        // 카메라 자동 탐색
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("MainCamera 태그가 지정된 카메라를 찾을 수 없습니다.");
            return;
        }

        var cameraData = cam.GetUniversalAdditionalCameraData();

        switch (savedIndex)
        {
            case 0: // 최고
            case 1: // 높음
                cameraData.renderPostProcessing = true;
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                break;

            case 2: // 중간
            case 3: // 낮음
                cameraData.renderPostProcessing = false;
                cameraData.antialiasing = AntialiasingMode.None;
                break;
        }
    }


    private void ApplySavedLanguage()
    {
        if (PlayerPrefs.HasKey(Constants.LOCAL_PREF_KEY))
        {
            int savedIndex = PlayerPrefs.GetInt(Constants.LOCAL_PREF_KEY);

            if (savedIndex >= 0 && savedIndex < LocalizationSettings.AvailableLocales.Locales.Count)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedIndex];
            }
        }
        else
        {
            // 기본값 설정 (예: 첫 번째 로케일)
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
    }



    public IEnumerator InitializeManager()
    {

        if (_skillBtnPrefab == null)
            _skillBtnPrefab = Resources.Load<SkillBtn>("Prefabs/Skill/SkillBtnPrefab");

        if (_skillChooseBtnPrefab == null)
            _skillChooseBtnPrefab = Resources.Load<SkillBtn>("Prefabs/Skill/SkillChoosePrefab");

        _skillTooltip.Initialize();

        // Localization 시스템 초기화 완료까지 대기
        yield return LocalizationSettings.InitializationOperation;

        // 초기화 완료 후 언어 설정
        ApplySavedLanguage();

        // 해상도 설정
        ApplySavedResolution();
    }


    public void MakeCurrentSkillList()
    {
        _listPanel.SetActive(true);
        EventBus.SetCanGetInput(false);
        List<SkillData> list = GameManager.Instance.skillManager.GetChooseSkillList();
        if (list == null) return;

        foreach (SkillData skill in list)
        {
            MakeSkillBtn(skill, _parentContent);
        }
    }

    public void MakeSkillBtn(SkillData skillData, Transform parentContent)
    {
        SkillBtn skillBtn = GameManager.Instance.PoolManager
                .GetFromPool(_skillBtnPrefab, _skillBtnPrefab.transform.position, _skillBtnPrefab.transform.rotation, parentContent)
                .GetComponent<SkillBtn>();

        skillBtn.SetSkillInfo(skillData);
        skillBtn.GetComponent<Button>().onClick.AddListener(() => OnClick_SkillListBtn(skillBtn));
    }

    public void OnClick_SkillListBtn(SkillBtn skillBtn)
    {
        // 선택된 버튼 저장
        _currentSelectedSkillBtn = skillBtn;

        SkillData data = skillBtn.skillData;
        _skillImage.sprite = data.skillImagePath;


        string locale = LocalizationSettings.SelectedLocale.Identifier.Code;
        _skillScriptText.text = $"{data.skillIdx}\n" + (locale == "ko" ? data.skillScript : data.skillScriptEn);
        _skillValueScriptText.text = locale == "ko" ? data.skillValueScript : data.skillValueScriptEn;
        _skillLevelText.text = $"{data.skillLevel} / {data.skillMaxLevel}";
        _skillNameText.text = $"{(locale == "ko" ? data.skillName : data.skillNameEn)}";
    }
    /// <summary>
    /// 언어 변경 시 UI 갱신
    /// </summary>
    public void RefreshCurrentSkillUI()
    {
        if (_currentSelectedSkillBtn != null)
        {
            OnClick_SkillListBtn(_currentSelectedSkillBtn);
        }
    }

    public void OnClick_ListExitBtn()
    {
        _currentSelectedSkillBtn = null;
        _skillImage.sprite = null;
        _skillScriptText.text = null;
        _skillValueScriptText.text = null;
        _skillLevelText.text = null;
        _skillNameText.text = null;

        foreach (Transform child in _parentContent)
        {
            PoolableObject childPool = child.GetComponent<PoolableObject>();
            if (childPool != null)
            {
                SkillBtn skillBtn = childPool.GetComponent<SkillBtn>();
                if (skillBtn != null) skillBtn.ReleaseObject();
                GameManager.Instance.PoolManager.ReleaseToPoolByInterface(childPool);
            }
        }
    }
		#endregion

		public void SetActiveSkillBtnPanel(bool isActive)
		{
				SetInput(!isActive);

				if (isActive)
				{
				_skillBtnPanel.SetActive(true);
					
				}
				else
				{
						// 닫을 때는 그냥 비활성화
						_skillBtnPanel.transform.localScale = Vector3.one;
						_skillBtnPanel.SetActive(false);
						onRewardSelect?.Invoke();
				}

				EventBus.SetCanGetInput(!isActive);
		}

    public void StartSkillBtnPanelAnim()
    {
				// 처음엔 작게 시작
				_skillBtnPanel.transform.localScale = Vector3.one * 0.5f;

				// DOTween 애니메이션: 0.5 → 1
				_skillBtnPanel.transform
						.DOScale(1f, 0.3f) // 0.3초 동안
						.SetEase(Ease.OutBack); // 튀어나오는 듯한 탄성 효과
		}

		#region skill choose
		public SkillBtn MakeSkillBtn()
    {
        SkillBtn skillBtn = GameManager.Instance.PoolManager
                .GetFromPool(_skillChooseBtnPrefab, Vector3.zero, _skillChooseBtnPrefab.transform.rotation, _parentPanel.transform)
                .GetComponent<SkillBtn>();

        _activeChooseButtons.Add(skillBtn);
        return skillBtn;
    }

    /// <summary>
    /// 버튼 텍스트 갱신
    /// </summary>
    public void RefreshAllChooseButtons()
    {
        foreach (SkillBtn button in _activeChooseButtons)
        {
            button.RefreshLanguage();
        }
    }


    public void DestroyChildObject(Transform parentObject)
    {
        Transform[] children = new Transform[parentObject.childCount];
        for (int i = 0; i < parentObject.childCount; i++)
            children[i] = parentObject.GetChild(i);

        foreach (Transform child in children)
        {
            PoolableObject childPool = child.GetComponent<PoolableObject>();
            if (childPool != null)
            {
                SkillBtn skillBtn = childPool.GetComponent<SkillBtn>();
                if (skillBtn != null) skillBtn.ReleaseObject();
                GameManager.Instance.PoolManager.ReleaseToPoolByInterface(childPool);
            }
        }
    }

    public void OnClickListExitBtn(Transform content)
    {
        _currentSelectedSkillBtn = null;
        _skillImage.sprite = null;
        _skillScriptText.text = null;
        _skillValueScriptText.text = null;
        _skillLevelText.text = null;
        _skillNameText.text = null;

        Transform[] children = new Transform[content.childCount];
        for (int i = 0; i < content.childCount; i++)
            children[i] = content.GetChild(i);

        foreach (Transform child in children)
        {
            PoolableObject childPool = child.GetComponent<PoolableObject>();
            if (childPool != null)
            {
                SkillBtn skillBtn = childPool.GetComponent<SkillBtn>();
                if (skillBtn != null) skillBtn.ReleaseObject();
                GameManager.Instance.PoolManager.ReleaseToPoolByInterface(childPool);
            }
        }

        _listPanel.SetActive(false);
    }
    #endregion

    public void SetInput(bool isLock)
    {
        EventBus.SetCanGetInput(isLock);
        if (EventBus.IsColosseumRoom)
        {
            Cursor.lockState = !isLock ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !isLock; // 커서 보이게/숨기기
        }
        Time.timeScale = 1.0f;
        EventBus.SetCanTimeRun(isLock);
    }
    public void SetInputAtIntroSettingPanel()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            EventBus.SetCanGetInput(true);
        }
    }

    public void OnUpgradePanel()
    {
        _upgradePanel.gameObject.SetActive(true);
    }

    public void OpenRecord()
    {
        _recordPanel.gameObject.SetActive(true);
    }

    public void OpenSettingPanel()
    {
        _settingPanel.OnOpenSetting();
    }

    public void ResetUIManager()
    {
        _upgradePanel.gameObject.SetActive(false);
        _recordPanel.gameObject.SetActive(false);
        _listPanel.gameObject.SetActive(false);
        _skillBtnPanel.gameObject.SetActive(false);
		}
}
