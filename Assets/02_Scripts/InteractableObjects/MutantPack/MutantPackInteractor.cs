using System;
using System.Collections;
using UnityEngine;

// TechItem과 SkillChoose Componenet 강제
[RequireComponent(typeof(TechItem))]
[RequireComponent(typeof(SkillChoose))]
public class MutantPackInteractor : RewardInteractableObject
{
    [SerializeField] TechItem _techItem;

    public override event Action OnRewardGiven;

    public override void Initialize()
    {
        base.Initialize();
    }

    // 보상 플로우(예: UI열고 플레이어가 선택하면 확정되는 경우)
    protected override IEnumerator RewardCoroutine()
    {
        GameManager.Instance.UIManager.onRewardSelect += RaiseRewardGivenEvent;
        // UI가 없으면 바로 적용 (폴백)
        yield return new WaitForSeconds(0.2f);

        // 실제 보상 적용
        _techItem.Mutantupgrade();
    }

    void RaiseRewardGivenEvent()
    {
        OnRewardGiven?.Invoke();
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameManager.Instance.UIManager.onRewardSelect -= RaiseRewardGivenEvent;
    }

    public override void TryGetInteracrtionKey(out string title, out string instruction)
    {
        title = "_rewardTitle_MutantPack";
        instruction = "_rewardDescription_MutantPack";
    }
}