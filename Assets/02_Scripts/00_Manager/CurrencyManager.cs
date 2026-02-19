using System;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// 플레이어의 화폐(골드, 크롬 등)를 관리하는 매니저 클래스입니다.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    [Header("----- 데이터(임시) -----")]
    [SerializeField] int _startingCredit = 100;
    [SerializeField] int _creditLimitPerRoom = 50;

    [Header("----- 읽기 전용 -----")]
    [SerializeField] int _currentCredit;
    [SerializeField] int _currentChrome;
    [SerializeField] int _roomCredit;       // 이번 방에서 획득한 골드

    public int CurrentGold => _currentCredit;
    public int CurrentChrome => _currentChrome;

    /// <summary>
    /// 골드가 변경되었을 때 발생하는 이벤트입니다.
    /// 매개변수는 변경된 후 최종적으로 갖고있는 현재 골드 양입니다.
    /// </summary>
    public event Action<int> OnCreditChanged;

    /// <summary>
    /// 크롬이 변경되었을 때 발생하는 이벤트입니다.
    /// 매개변수는 변경된 후 최종적으로 갖고있는 현재 크롬 양입니다.
    /// </summary>
    public event Action<int> OnChromeChanged;

    public void Initialize()
    {
        _currentCredit = _startingCredit;

    }

    public async void SetCreditFromServer()
    {
        // downloadManager가 null일 경우 재시도
        int retryCount = 0;
        int maxRetries = 10;
        int delayMilliseconds = 500;

        while (GameManager.Instance.serverManager.downloadManager == null && retryCount < maxRetries)
        {
            await Task.Delay(delayMilliseconds);
            retryCount++;
        }

        if (GameManager.Instance.serverManager.downloadManager == null)
        {
            Debug.LogError("downloadManager is still null after retries.");
            return;
        }

        _currentChrome = await GameManager.Instance.serverManager.downloadManager.GetChrome();

        OnChromeChanged?.Invoke(_currentChrome);
    }



    /// <summary>
    /// 외부에서 현재 화폐 상태를 가져올 수 있도록 업데이트 이벤트를 강제로 발생시킵니다.
    /// </summary>
    public void GetCurrentCurrency()
    {
        OnChromeChanged?.Invoke(_currentChrome);
        OnCreditChanged?.Invoke(_currentCredit);
    }

    ///// <summary>
    ///// 몬스터 처치로 인한 골드 획득
    ///// </summary>
    ///// <param name="cost"></param>
    //public void AddCreditByMonsterDeath(int cost)
    //{
    //    if (_roomCredit >= _creditLimitPerRoom)
    //    {
    //        return;
    //    }
    //    if (_roomCredit + cost > _creditLimitPerRoom)
    //    {
    //        cost = _creditLimitPerRoom - _roomCredit;
    //    }
    //    _currentCredit += cost;
    //    _roomCredit += cost;

    //    OnCreditChanged?.Invoke(_currentCredit);
    //}

    /// <summary>
    /// 몬스터 처치로 인해 획득하는 크레딧을 방당 제한(_creditLimitPerRoom) 내에서 추가합니다.
    /// 반환값은 실제로 추가된 크레딧 양입니다(0이면 제한으로 더이상 획득 불가).
    /// </summary>
    public int AddCreditByMonsterDeath(int amount)
    {
        if (amount <= 0)
            return 0;

        if (_creditLimitPerRoom <= 0)
            return 0;

        int remaining = Math.Max(0, _creditLimitPerRoom - _roomCredit);
        int toAdd = Math.Min(amount, remaining);

        if (toAdd <= 0)
            return 0;

        // 내부의 공통 AddCredit을 재사용해서 이벤트 호출/로직 일관성 유지
        AddCredit(toAdd);

        _roomCredit += toAdd;

        return toAdd;
    }

    public void ResetRoomCredit()
    {
        _roomCredit = 0;
    }

    public void AddCredit(int amount)
    {
        _currentCredit += amount;
        OnCreditChanged?.Invoke(_currentCredit);
    }

    public void AddChrome(int amount)
    {
        _currentChrome += amount;
        OnChromeChanged?.Invoke(_currentChrome);
    }

    public bool TrySpendCredit(int amount)
    {
        if (_currentCredit < amount)
        {
            return false;
        }

        _currentCredit -= amount;
        OnCreditChanged?.Invoke(_currentCredit);
        return true;
    }
}