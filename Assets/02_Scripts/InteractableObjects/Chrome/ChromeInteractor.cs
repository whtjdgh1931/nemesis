using System;
using System.Collections;
using UnityEngine;

public class ChromeInteractor : RewardInteractableObject
{
    public override event Action OnRewardGiven;

    public override void TryGetInteracrtionKey(out string title, out string instruction)
    {
        title = "_rewardTitle_Chrome";
        instruction = "_rewardDescription_Chrome";
    }

    protected override IEnumerator RewardCoroutine()
    {
        yield return new WaitForSeconds(0.01f);
        GameManager.Instance.CurrencyManager.AddChrome(10);
        OnRewardGiven?.Invoke();
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}