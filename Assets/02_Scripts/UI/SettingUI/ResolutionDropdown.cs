using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Rendering.Universal;

public class ResolutionDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private string tableName;
    [SerializeField] private string[] stringKeys;
    [SerializeField] private Sprite[] optionSprites;

    private void OnEnable()
    {
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        LocalizationSettings.SelectedLocaleChanged += UpdateDropdown;

        UpdateDropdown(LocalizationSettings.SelectedLocale);

        int savedIndex = PlayerPrefs.GetInt(Constants.RESOLUTION_PREF_KEY, 0);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();
        ChangeResolution(savedIndex);
    }

    private void OnDisable()
    {
        resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
        LocalizationSettings.SelectedLocaleChanged -= UpdateDropdown;
    }

    private void UpdateDropdown(Locale locale)
    {
        LocalizationSettings.StringDatabase.GetTableAsync(tableName).Completed += handle =>
        {
            var table = handle.Result;
            resolutionDropdown.options.Clear();

            for (int i = 0; i < stringKeys.Length; i++)
            {
                var key = stringKeys[i];
                var entry = table.GetEntry(key);
                string localizedText = entry?.GetLocalizedString() ?? key;

                Sprite icon = (optionSprites != null && i < optionSprites.Length) ? optionSprites[i] : null;

                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(localizedText, icon, Color.white));
            }

            resolutionDropdown.RefreshShownValue();
        };
    }

    private void ChangeResolution(int index)
    {
        // 저장
        PlayerPrefs.SetInt(Constants.RESOLUTION_PREF_KEY, index);
        PlayerPrefs.Save();

        // 퀄리티 설정 (유효한 인덱스만)
        if (index >= 0 && index <= 3)
        {
            QualitySettings.SetQualityLevel(index);
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

        switch (index)
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
}
