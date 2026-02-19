using System.Collections;
using UnityEngine;

public class ShotgunDecalEffect : PoolableObject
{
    private MeshRenderer meshRenderer;
    private Material materialInstance;
    private Coroutine fadeRoutine;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;
    public float startAlpha = 0.5f;
    public float endAlpha = 1.0f;
    public int blinkCount = 2; // 깜빡이는 횟수

    public void Play()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(BlinkAndReturn());
    }

    private IEnumerator BlinkAndReturn()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            materialInstance = meshRenderer.material;
        }
        if (materialInstance == null) yield break;

        Color color = materialInstance.color;
        float blinkDuration = fadeDuration / blinkCount; // 각 깜빡임당 시간

        // 깜빡이기
        for (int i = 0; i < blinkCount; i++)
        {
            // 페이드 인 (startAlpha -> endAlpha)
            float elapsed = 0f;
            while (elapsed < blinkDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (blinkDuration / 2f);
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                materialInstance.color = color;
                yield return null;
            }

            // 페이드 아웃 (endAlpha -> startAlpha)
            elapsed = 0f;
            while (elapsed < blinkDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (blinkDuration / 2f);
                color.a = Mathf.Lerp(endAlpha, startAlpha, t);
                materialInstance.color = color;
                yield return null;
            }
        }

        // 최종적으로 완전히 불투명하게
        color.a = endAlpha;
        materialInstance.color = color;

        // 풀로 반환
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}