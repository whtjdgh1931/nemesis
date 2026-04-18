using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 문의 보여지는 부분을 담당하는 클래스
/// </summary>
public class DoorView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _rewardText;
    [SerializeField] Image _rewardImage;

    public void SetRewardView(RoomInfo roomInfo)
    {
        // 방의 정보에 따라 보상 결정
        if (roomInfo.TryGetTechSelect(out var tech))
        {
            // TechSelect 우선 처리
            //_rewardText.text = $"Tech: {tech.ToString()}";
            _rewardImage.sprite = GameManager.Instance.DataManager.RoomImageMap[tech.ToString()];
        }
        else if (roomInfo.TryGetNormal(out var normal))
        {
            // Normal 처리
            //_rewardText.text = $"Normal: {normal.ToString()}";
            _rewardImage.sprite = GameManager.Instance.DataManager.RoomImageMap[normal.ToString()];
        }
        else
        {
            // RoomType 처리
            var roomType = roomInfo.RoomType;
            //_rewardText.text = $"Room: {roomType.ToString()}";
            _rewardImage.sprite = GameManager.Instance.DataManager.RoomImageMap[roomType.ToString()];
        }

    }

    public void ToggleReward(bool isOn)
    {
        //_rewardText.gameObject.SetActive(isOn);
        _rewardImage.gameObject.SetActive(isOn);
    }
}