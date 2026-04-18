using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SkillTooltipUI : PoolableObject
{
    private SkillTooltipData _skillTooltipData;

    [SerializeField]
    private TextMeshProUGUI _keywordText;
    [SerializeField]
    private TextMeshProUGUI _scriptText;

    public void SetTooltipData(SkillTooltipData data)
    {
				_skillTooltipData = data;

				RefreshLanguage();
		}

		public void RefreshLanguage()
    {
						string locale = LocalizationSettings.SelectedLocale.Identifier.Code;
        if (_keywordText != null && _skillTooltipData != null)
				{
						_keywordText.text = locale == "ko" ? _skillTooltipData.keyword : _skillTooltipData.keywordEN;
				}

#if UNITY_STANDALONE_WIN
				if (_scriptText != null && _skillTooltipData != null)
				{
						_scriptText.text = locale == "ko" ? _skillTooltipData.PCScript : _skillTooltipData.PCScriptEN;
				}
#elif UNITY_ANDROID

				if (_scriptText != null && _skillTooltipData != null)
				{
						_scriptText.text = locale == "ko" ? _skillTooltipData.mobileScript : _skillTooltipData.mobileScriptEN;
				}
#endif
		}

		public void Release()
    {

        _keywordText.text = null;
        _scriptText.text = null;

        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }    

}
