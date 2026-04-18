using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Damaged : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    private Coroutine fadeRoutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadePanel());
        }
    }

    private IEnumerator FadePanel()
    {
        // 즉시 알파 0.3
        Color c = panelImage.color;
        c.a = 0.3f;
        panelImage.color = c;

        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0.3f, 0f, time / duration);

            c.a = alpha;
            panelImage.color = c;

            yield return null;
        }

        c.a = 0f;
        panelImage.color = c;
    }
}
