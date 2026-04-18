using System;
using System.Collections;
using UnityEngine;

public class TechSelectPackInteractor : RewardInteractableObject
{
    [SerializeField] TechItem _techItem;

    [Header("----- 설정값 -----")]
    [SerializeField] TechSelectPackType _packType;

    public TechSelectPackType PackType => _packType;

    public override event Action OnRewardGiven;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override IEnumerator RewardCoroutine()
    {
        GameManager.Instance.UIManager.onRewardSelect += RaiseRewardGivenEvent;
        GameManager.Instance.SoundManager.PlaySfx("UIPopUp");
        yield return new WaitForSeconds(0.5f); // 보상 선택 UI 열리는 시간 대기
        _techItem.GetSkill(_packType);
    }

    protected void RaiseRewardGivenEvent()
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
        title = $"_rewardTitle_TechSelectPack_{_packType.ToString()}";
        instruction = $"_rewardDescription_TechSelectPack_{_packType.ToString()}";
    }
}
