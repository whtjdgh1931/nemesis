using UnityEngine;

[CreateAssetMenu(fileName = "RewardDataSO", menuName = "ScriptableObjects/Reward/RewardDataSO")]
public class RewardDataSO : ScriptableObject
{
    [SerializeField] GameObject _rewardPrefab;
    [SerializeField] RewardType _rewardType;

    public GameObject RewardPrefab => _rewardPrefab;
    public RewardType RewardType => _rewardType;
}