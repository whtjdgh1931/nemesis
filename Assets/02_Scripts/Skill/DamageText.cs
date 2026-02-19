using TMPro;
using UnityEngine;

public class DamageText : PoolableObject
{
		public TMP_Text text;
		public float floatSpeed = 1f;
		public float duration = 0.5f;
		public float fadeDuration = 0.3f;

		private float timer = 0f;
		private Color originalColor;

		void OnEnable()
		{
				timer = 0f;
				originalColor = text.color;
		}

		public void SetDmg(int damage,bool bIsBillBoard)
		{
				text.text = damage.ToString();
				StartCoroutine(FloatAndFade(bIsBillBoard));
		}

		private System.Collections.IEnumerator FloatAndFade(bool bIsBillBoard)
		{
				Vector3 startPos = transform.position;
				Vector3 endPos = startPos + Vector3.up * 1f;

				while (timer < duration)
				{
						timer += Time.deltaTime;
						float t = timer / duration;

						// 위치 이동
						transform.position = Vector3.Lerp(startPos, endPos, t);

						// 맵 조건 체크 후 Billboard 적용
						if (bIsBillBoard)
						{
								LookAtPlayerOrCamera();
						}

						// 알파 페이드
						if (timer > duration - fadeDuration)
						{
								float fadeT = (timer - (duration - fadeDuration)) / fadeDuration;
								Color c = originalColor;
								c.a = Mathf.Lerp(1f, 0f, fadeT);
								text.color = c;
						}

						yield return null;
				}

				GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
		}


		/// <summary>
		/// 플레이어나 카메라를 바라보게 회전
		/// </summary>
		private void LookAtPlayerOrCamera()
		{
				Transform cam = Camera.main.transform;
				transform.LookAt(transform.position + cam.rotation * Vector3.forward,
												 cam.rotation * Vector3.up);
		}
}
