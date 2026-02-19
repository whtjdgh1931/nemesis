using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이 씬에서 플레이어의 클리어타임 기록을 위해 시간을 체크하는 클래스
/// </summary>
public class TimeChecker : MonoBehaviour
{
    [SerializeField] float _currentTime = 0f;
    [SerializeField] float _timeScale = 1f;

    Coroutine _timeCheckRoutine;
    
    public float CurrentTime => _currentTime;   

    public event Action<float> OnTimeUpdated;

    public void Initialize()
    {
        _currentTime = 0f;
        OnTimeUpdated?.Invoke(_currentTime);
        EventBus.OnBossDead += StopTimeCheck;
    }

    public void StartTimeCheck()
    {
        if (_timeCheckRoutine != null)
            StopCoroutine(_timeCheckRoutine);
        _currentTime = 0f;
        OnTimeUpdated?.Invoke(_currentTime);
        _timeCheckRoutine = StartCoroutine(TimeCheckRoutine());
    }

    public void StopTimeCheck()
    {
        GameManager.Instance.serverManager.downloadManager.UploadClearTime(_currentTime);
        if (_timeCheckRoutine != null)
            StopCoroutine(_timeCheckRoutine);
    }

    /// <summary>
    /// 클리어 타임 측정을 위한 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator TimeCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.1f); // timeScale 무시
            if (EventBus.CanTimeRun)
            {
                _currentTime += 0.1f * _timeScale;
                OnTimeUpdated?.Invoke(_currentTime);
            }
        }
    }

    private void OnDisable()
    {
        if( _timeCheckRoutine != null)
            StopCoroutine(_timeCheckRoutine);
        EventBus.OnBossDead -= StopTimeCheck;
    }
}