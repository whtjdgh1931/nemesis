using System.Collections;
using UnityEngine;

public class AttackDecalEffect : PoolableObject
{
    [Header("Components")]
    public GameObject countCircle;  // 자식 CountCircle 오브젝트

    private Vector3 targetScale;    // 목표 스케일
    private Coroutine growRoutine;  // 코루틴 중복 방지용


    public void Initialize()
    {
        if (countCircle == null)
        {
            // 자식에서 CountCircle 자동 탐색
            Transform child = transform.Find("CountCircle");
            if (child != null)
                countCircle = child.gameObject;
        }
    }

    /// <summary>
    /// 외부에서 호출: 원을 duration 동안 0 → targetScale 로 커지게 하고, 끝나면 파괴됨
    /// </summary>
    public void Play(float duration, float radius)
    {
        Initialize();
        if (countCircle == null) return;

        // 최종 스케일을 반지름 기준으로 설정 (x,y,z 동일)
        targetScale = Vector3.one * radius * 2f;
        gameObject.transform.localScale = targetScale; // BaseCircle도 맞춰줌

        StopAllCoroutines();

        countCircle.transform.localScale = Vector3.zero;
        countCircle.SetActive(true);

        growRoutine = StartCoroutine(GrowAndRelease(duration));
    }

    private IEnumerator GrowAndRelease(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            countCircle.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            yield return null;
        }

        countCircle.transform.localScale = Vector3.one;

        // BaseCircle 프리팹 반환
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}
