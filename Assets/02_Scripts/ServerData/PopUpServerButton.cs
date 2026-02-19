using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Threading.Tasks;

public class CyberpunkButtonAnimator
{
		private readonly Button button;
		private readonly Image buttonImage;
		private readonly Color originalColor;

		public CyberpunkButtonAnimator(Button button)
		{
				this.button = button;
				this.buttonImage = button.GetComponent<Image>();
				if (buttonImage != null)
						originalColor = buttonImage.color;
		}

		/// <summary>
		/// 버튼 클릭 시 애니메이션 실행 후 지정된 로직 실행
		/// </summary>
		public void Bind(Action onCompleteAction)
		{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
						Sequence seq = DOTween.Sequence();

						// 네온 펄스
						if (buttonImage != null)
						{
								seq.Append(buttonImage.DOColor(Color.cyan, 0.1f));
								seq.Append(buttonImage.DOColor(originalColor, 0.1f));
						}

						// 글리치 흔들림
						seq.Join(button.transform.DOShakePosition(
								0.25f,
								strength: new Vector3(10, 0, 0),
								vibrato: 30,
								randomness: 90,
								snapping: false,
								fadeOut: true
						));

						// 애니메이션 끝난 뒤 로직 실행
						seq.OnComplete(() =>
						{
								onCompleteAction?.Invoke();
						});
				});
		}

		/// <summary>
		/// async 로직도 지원
		/// </summary>
		public void BindAsync(Func<Task> onCompleteAsync)
		{
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
						Sequence seq = DOTween.Sequence();

						if (buttonImage != null)
						{
								seq.Append(buttonImage.DOColor(Color.cyan, 0.1f));
								seq.Append(buttonImage.DOColor(originalColor, 0.1f));
						}

						seq.Join(button.transform.DOShakePosition(
								0.25f,
								strength: new Vector3(10, 0, 0),
								vibrato: 30,
								randomness: 90,
								snapping: false,
								fadeOut: true
						));

						seq.OnComplete(async () =>
						{
								if (onCompleteAsync != null)
										await onCompleteAsync();
						});
				});
		}
}
