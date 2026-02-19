using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class SkillBtn : PoolableObject, IPointerEnterHandler, IPointerExitHandler
{
		private SkillData _skillData;
		public SkillData skillData => _skillData;

		[SerializeField] private Image _skillImage;
		[SerializeField] private TextMeshProUGUI _skillScirpt;
		[SerializeField] private TextMeshProUGUI _skillIDX;
		[SerializeField] private TextMeshProUGUI _skillLevel;
		
		[SerializeField] 
		private float _currentTime;
		[SerializeField]
		private float _tooltipTime;
		[SerializeField]
		private bool _bIsPointerEnter;

		public void SetSkillData(SkillData setSkillData)
		{
				_skillData = setSkillData;
		}

		public void SetSkillInfo(SkillData choosedSkill)
		{
				_skillData = choosedSkill;

				// 언어 번역
				RefreshLanguage();

				if (_skillImage != null)
						_skillImage.sprite = choosedSkill.skillImagePath;

				if (_skillIDX != null)
						_skillIDX.text = choosedSkill.skillIdx.ToString();

				if (_skillLevel != null)
						_skillLevel.text = $"{choosedSkill.skillLevel} / {choosedSkill.skillMaxLevel}";
		}

		/// <summary>
		/// 언어 변경 시 텍스트만 갱신
		/// </summary>
		public void RefreshLanguage()
		{
				if (_skillScirpt != null && _skillData != null)
				{
						string locale = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code;
						_skillScirpt.text = locale == "ko" ? _skillData.skillScript : _skillData.skillScriptEn;
				}
		}

		public GameObject GetGameObject() => gameObject;

		public void Update()
		{
				if (_bIsPointerEnter)
				{
						_currentTime += Time.deltaTime;
						if (_currentTime > _tooltipTime)
						{
								GameManager.Instance.UIManager.skillTooltip.ShowTooltip(skillData);
								_bIsPointerEnter = false;
						}
				}
		}

		public void ReleaseObject()
		{
				if (_skillScirpt != null) _skillScirpt.text = null;
				if (_skillImage != null) _skillImage.sprite = null;
				if (_skillIDX != null) _skillIDX.text = null;
				if (_skillLevel != null) _skillLevel.text = null;
				_currentTime = 0f;
				_bIsPointerEnter = false;

				_skillData = null;
				GameManager.Instance.UIManager.skillTooltip.ReleaseCurrentTooltip();
				GetComponent<Button>().onClick.RemoveAllListeners();
    }

		public void OnPointerEnter(PointerEventData eventData)
		{
				_bIsPointerEnter = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{

				_currentTime = 0f;
				_bIsPointerEnter = false;
				GameManager.Instance.UIManager.skillTooltip.ReleaseCurrentTooltip();
		}
}
