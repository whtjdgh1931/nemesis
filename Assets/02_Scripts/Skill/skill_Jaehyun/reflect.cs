using System.Collections;
using UnityEngine;

public class reflect : MonoBehaviour
{
    public bool isReflecting = false;
    [SerializeField] private float _remainTime;
    private Coroutine _currentReflect;



    public void StartReflectCoroutine(float time)
    {
        // 남은 시간이 더 길다면 리턴
        if(time < _remainTime)
        {
            return;
        }
        _remainTime = time;
        if(_currentReflect != null)
        {
            StopCoroutine(_currentReflect);
            _currentReflect = null;
        }

        _currentReflect = StartCoroutine(ReflectingCoroutine());
    }


    public IEnumerator ReflectingCoroutine()
    {
        isReflecting = true;
        while(_remainTime > 0)
        {
            _remainTime -= Time.deltaTime;
            yield return null;
        }
        _remainTime = 0;
        isReflecting = false;
        _currentReflect = null;
    }
    
}
