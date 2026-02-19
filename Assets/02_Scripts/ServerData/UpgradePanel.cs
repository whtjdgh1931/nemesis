using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    private PlayerStatManager _playerStatManager;
    [SerializeField] private GameObject _popUpObject;
    [SerializeField] private TextMeshProUGUI _popUpText;
    [SerializeField] private UpgradeBtn upgradeBtnPrefab;
    [SerializeField] private GameObject ParentPanel;
    [SerializeField] private TextMeshProUGUI _chromeText;
    private void OnEnable()
    {

        EventBus.SetCanGetInput(false);
        if (_playerStatManager == null)
        {
            _playerStatManager = GameManager.Instance.PlayerStatManager;
        }

        if (upgradeBtnPrefab == null)
        {
            upgradeBtnPrefab = Resources.Load<UpgradeBtn>("Prefabs/Skill/UpgradeBtnPrefab");
        }
        foreach (Transform child in ParentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<PlayerStatData> upgradeStats = _playerStatManager.GetUpgradableStats();

        if (upgradeStats.Count == 0)
        {
            Debug.LogWarning("업그레이드 가능 스탯 없음");
        }

        for (int i = 0; i < upgradeStats.Count; i++)
        {
            UpgradeBtn btn = Instantiate(upgradeBtnPrefab, ParentPanel.transform);
            btn.SetPlayerStatData(upgradeStats[i], this);
        }

        _chromeText.text = "Chrome : " + GameManager.Instance.CurrencyManager.CurrentChrome;
    }



    public void OnClick_UpgradeBtn(UpgradeBtn btn)
    {
        string statName = btn.playerStatData.Column;

        if (!_playerStatManager.playerStatDataDic.ContainsKey(statName))
        {
            Debug.LogWarning("해당 스탯 이름이 없습니다.");
            return;
        }

        if (GameManager.Instance.CurrencyManager.CurrentChrome > 100)
        {
            bool isLevelUp = _playerStatManager.playerStatDataDic[statName].LevelUp();
            btn.UpdatePanelData(); // UI 갱신
            PopUp(isLevelUp);
            GameManager.Instance.CurrencyManager.AddChrome(-100);
            GameManager.Instance.serverManager.downloadManager.SetChromeToServer();
        }
        else
        {
            PopUp();
        }

    }

    public void PopUp(bool isUpgrade)
    {
        _popUpObject.SetActive(true);

        string key = isUpgrade ? "UpgradeSuccess" : "UpgradeFail";
        _popUpText.text = GameManager.Instance.languageManager.GetLocalizedText(key);
    }

    public void PopUp()
    {
        _popUpObject.SetActive(true);
        string locale = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code;
        _popUpText.text = locale == "ko" ? "크롬 부족" : "Chrome lack";
    }

    private void OnDisable()
    {
        PlayerModel player = FindAnyObjectByType<PlayerModel>();
        player.Initialize();
        _playerStatManager.UploadToFirebase();

        EventBus.SetCanGetInput(true);
    }
}
