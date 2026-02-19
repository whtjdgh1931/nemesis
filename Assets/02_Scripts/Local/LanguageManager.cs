using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LanguageManager : MonoBehaviour
{
		public event Action OnLanguageChanged;

		public void ChangeLanguage(string localeCode)
		{
				StartCoroutine(SetLocale(localeCode));
		}

		private IEnumerator SetLocale(string localeCode)
		{
				yield return LocalizationSettings.InitializationOperation;

				var selectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
				if (selectedLocale != null)
				{
						LocalizationSettings.SelectedLocale = selectedLocale;

						GameManager.Instance.UIManager?.RefreshCurrentSkillUI();
						GameManager.Instance.UIManager?.RefreshAllChooseButtons();

						OnLanguageChanged?.Invoke();
				}
				else
				{
						Debug.LogError($"❌ '{localeCode}'에 해당하는 Locale을 찾을 수 없습니다.");
				}
		}

		public string GetLocalizedText(string key)
		{
				var table = LocalizationSettings.StringDatabase.GetTable(Constants.LOCAL_TABLE);
				if (table == null)
				{
						Debug.LogWarning($"❌ 테이블 '{Constants.LOCAL_TABLE}'을 찾을 수 없습니다.");
						return $"[MissingTable:{Constants.LOCAL_TABLE}]";
				}

				var entry = table.GetEntry(key);
				if (entry == null)
				{
						Debug.LogWarning($"❌ 키 '{key}'를 테이블에서 찾을 수 없습니다.");
						return $"[MissingKey:{key}]";
				}

				if (string.IsNullOrEmpty(entry.LocalizedValue))
				{
						Debug.LogWarning($"⚠️ 키 '{key}'의 번역 값이 비어 있습니다.");
						return $"[Empty:{key}]";
				}

				return entry.LocalizedValue;
		}
}
