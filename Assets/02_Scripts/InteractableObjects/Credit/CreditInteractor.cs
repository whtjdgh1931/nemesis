using System;
using System.Collections;
using UnityEngine;

public class CreditInteractor : RewardInteractableObject
{
    public override event Action OnRewardGiven;

    public override void TryGetInteracrtionKey(out string title, out string instruction)
    {
        title = "_rewardTitle_Credit";
        instruction = "_rewardDescription_Credit";
    }

    protected override IEnumerator RewardCoroutine()
    {
        yield return new WaitForSeconds(0.01f);
        GameManager.Instance.CurrencyManager.AddCredit(100);
        OnRewardGiven?.Invoke();
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}