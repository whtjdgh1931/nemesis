using System.Collections;
using UnityEngine;

public class SquareDecalEffect : PoolableObject
{
    [Header("Components")]
    public GameObject countSquare;  // 자식 Square 오브젝트

    private Vector3 targetScale;    // 목표 스케일
    private Coroutine growRoutine;  // 코루틴 중복 방지용


    /// <summary>
    /// 외부에서 호출: 사각형을 duration 동안 targetScale 로 커지게 하고, 끝나면 반환됨
    /// </summary>
    public void Play(float duration, Transform rotationTarget, Vector3 rotationOffset, float distance = 40f)
    {
        if (countSquare == null ) return;

        targetScale = Vector3.one;

        if (growRoutine != null)
        {
            StopCoroutine(growRoutine);
        }

        countSquare.transform.localScale = Vector3.zero;
        countSquare.SetActive(true);

        growRoutine = StartCoroutine(GrowAndRelease(duration, rotationTarget, rotationOffset, distance));
    }

    private IEnumerator GrowAndRelease(float duration, Transform rotationTarget, Vector3 rotationOffset, float distanceFromTarget)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 localscale = new Vector3(0, 1, 1);
            countSquare.transform.localScale = Vector3.Lerp(localscale, targetScale, t);

            if (rotationTarget != null)
            {
                Vector3 offset = rotationTarget.forward * distanceFromTarget;
                Vector3 newPos = rotationTarget.position + offset;
                newPos.y = 1.5f;
                transform.position = newPos;

                transform.rotation = Quaternion.Euler(rotationOffset.x, rotationTarget.eulerAngles.y + rotationOffset.y,rotationOffset.z);
            }

            yield return null;
        }

        countSquare.transform.localScale = Vector3.one;

        // 프리팹 반환
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}
