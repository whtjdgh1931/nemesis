using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class CyberpunkButtonEffect : MonoBehaviour
{
		private Button button;
		private UnityEngine.Events.UnityEvent inspectorEvents;
		private Image buttonImage;
		private Color originalColor;

		private Sequence currentSequence; // 실행 중인 시퀀스를 저장




		void Start()
		{
				DOTween.Init();
				if (button == null)
				{
						button = GetComponent<Button>();
				}
				if (buttonImage == null)
				{
						buttonImage = GetComponent<Image>();
				}
				else
				{
						originalColor = buttonImage.color;
				}

				inspectorEvents = button.onClick;

				button.onClick = new Button.ButtonClickedEvent();
				button.onClick.AddListener(PlayEffect);
		}

		void PlayEffect()
		{
				// 이전 시퀀스가 살아있으면 먼저 Kill
				currentSequence?.Kill();

				currentSequence = DOTween.Sequence();
				currentSequence.Append(button.transform.DOScale(1.2f, 0.1f));
				currentSequence.Append(button.transform.DOScale(1f, 0.1f));
				currentSequence.Join(button.transform.DOShakeScale(
						0.25f,
						strength: new Vector3(0.2f, 0.2f, 0),
						vibrato: 30,
						randomness: 90,
						fadeOut: true
				));
				currentSequence.Append(button.transform.DOScale(1f, 0.05f));

				currentSequence.OnComplete(() =>
				{
						inspectorEvents?.Invoke();
				});
		}

		private void OnDisable()
		{
				// 버튼 비활성화 시 애니메이션 종료
				currentSequence?.Kill();
				currentSequence = null;
		}
}
