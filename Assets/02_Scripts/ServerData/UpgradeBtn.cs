using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBtn : MonoBehaviour
{
    private PlayerStatData _playerStatData;
    public PlayerStatData playerStatData { get { return _playerStatData; } }
    [SerializeField] private TextMeshProUGUI _statNameText;
    [SerializeField] public Button upgradeBtn;
    [SerializeField] private TextMeshProUGUI levelText;

    public void SetPlayerStatData(PlayerStatData statData, UpgradePanel panel)
    {
        _playerStatData = statData;
        CyberpunkButtonAnimator btnAnim = new CyberpunkButtonAnimator(upgradeBtn);
        btnAnim.Bind(() => panel.OnClick_UpgradeBtn(this));
        UpdatePanelData();
    }

    public void UpdatePanelData()
    {
        _statNameText.text = GameManager.Instance.languageManager.GetLocalizedText(_playerStatData.Column);

        if (_playerStatData.CurrentLevel == _playerStatData.MaxLevel)
        {
            upgradeBtn.interactable = false;
        }

        levelText.text = $"{_playerStatData.CurrentLevel} / {_playerStatData.MaxLevel}";
    }



}
