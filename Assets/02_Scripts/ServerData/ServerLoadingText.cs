using TMPro;
using UnityEngine;
using System.Collections;

public class LoadingTextAnimator : MonoBehaviour
{
		[SerializeField] private TMP_Text loadingText;
		[SerializeField] private string baseText = "Loading...";
		[SerializeField] private float interval = 0.5f;
		[SerializeField] private int maxDots = 3;

		private Coroutine animationCoroutine;

		private void OnEnable()
		{
				animationCoroutine = StartCoroutine(AnimateLoadingText());
		}

		private void OnDisable()
		{
				if (animationCoroutine != null)
				{
						StopCoroutine(animationCoroutine);
				}
		}

		private IEnumerator AnimateLoadingText()
		{
				int dotCount = 0;
				bool increasing = true;

				while (true)
				{
						loadingText.text = baseText + new string('.', dotCount);

						yield return new WaitForSeconds(interval);

						if (increasing)
						{
								dotCount++;
								if (dotCount >= maxDots)
										increasing = false;
						}
						else
						{
								dotCount--;
								if (dotCount <= 0)
										increasing = true;
						}
				}
		}
}
