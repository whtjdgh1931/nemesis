using UnityEngine;

[CreateAssetMenu(fileName = "TechSelectPack", menuName = "ScriptableObjects/Reward/RewardDataSO_TechPack")]
public class RewardDataSO_TechPack : RewardDataSO
{
    [SerializeField] TechSelectPackType _techPackType;

    public TechSelectPackType TechPackType => _techPackType;
}