using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 플레이씬의 UI를 관리하는 뷰 클래스입니다.
/// </summary>
public class PlaySceneView : MonoBehaviour
{
		[Header("----- UI 컴포넌트 참조 -----")]
		[SerializeField] Slider _hpBarSlider;
		[SerializeField] TextMeshProUGUI _hpText;
		[SerializeField] TextMeshProUGUI _goldText;
		[SerializeField] TextMeshProUGUI _chromeText;
		[SerializeField] Slider _grenadeCooltimeSlider;
		[SerializeField] TextMeshProUGUI _grenadeCountText;
		[SerializeField] RectTransform _playerStat;
		[SerializeField] GameObject _interactionGuideView;
		[SerializeField] TextMeshProUGUI _currentRoomCountText;


		[Header("----- 방 로딩 패널 -----")]
		[SerializeField] GameObject _roomLoadingPanel;
		[SerializeField] TextMeshProUGUI _roomLoadingText;

		[Header("----- 상호작용 패널 -----")]
		[SerializeField] GameObject _interactionPanel;
		[SerializeField] TextMeshProUGUI _interactionTitleText;
		[SerializeField] TextMeshProUGUI _interactionDescriptionText;



		[Header("----- 튜토리얼 패널 -----")]
		[SerializeField] GameObject _tutorialPanel;
		[SerializeField] List<TextMeshProUGUI> _tutorialTexts = new();    // 튜토리얼 텍스트 리스트
		[SerializeField] List<Image> _tutorialCompleteCheckImage = new();        // 튜토리얼 완료 체크 이미지 리스트

		[Header("----- 게임 승리 및 오버 패널 -----")]
		[SerializeField] GameObject _gameOverPanel;

		[SerializeField] GameObject _gameClearPanel;
		[SerializeField] TextMeshProUGUI _gameClearTimeText;

		[Header("----- 게임 타이머 -----")]
		[SerializeField] TextMeshProUGUI _gameTimerText;

		[Header("----- 로컬라이즈 컴포넌트 -----")]
		// 테이블 이름은 Localization Table에서 사용한 이름(또는 Collection 이름)
		const string TABLE_COLLECTION_NAME_INTERACTIONVIEW = "InteractionViewTable";
		const string TABLE_COLLECTION_NAME_ROOMLOADING = "RoomLoadingTable";
		const string TABLE_COLLECTION_NAME_TUTORIAL = "TutorialTable";

		LocalizedString _localInteractionTitle;
		LocalizedString _localInteractionDescription;
		//LocalizedString _localRoomLoadingText;        // 비동기 로딩으로 변경
		List<LocalizedString> _boundTutorialLocalized = new List<LocalizedString>();
		List<LocalizedString.ChangeHandler> _tutorialChangedHandlers = new List<LocalizedString.ChangeHandler>();

		string[] _roomLoadingKeys = new string[]
		{
				"_loadingDescription_1",
				"_loadingDescription_2",
				"_loadingDescription_3",
				"_loadingDescription_4",
				"_loadingDescription_5"
		};

		List<string> _baseTutorialKeys = new List<string>()
		{
				"_tutorial_NormalAttack",
				"_tutorial_GrenadeAttack",
				"_tutorial_SpecialAttack",
				"_tutorial_Dash",
				"_tutorial_Interact"
		};

		Coroutine _roomLoadingRoutine;
		PlayScene _playScene;

		public event Action<DoorInteractor> OnRoomLoadingComplete;

		public void Initialize(PlayScene playScene)
		{
				EventBus.OnBossDead += ShowGameClearPanel;
				_playScene = playScene;

				GameManager.Instance.CurrencyManager.GetCurrentCurrency();
				HideInteractionUI();

				// 처음 시작하면 유탄 슬라이더를 꽉 채우기
				UpdateGrenadeCoolTime(1.0f, 1.0f);

				// 튜토리얼 패널 띄우기
				ShowTutorialPanel();

				// 플레이어 스탯 위치 조정
				SettingPlayerStatPosition();
		}

		public void SettingPlayerStatPosition()
		{
				bool isMobile = Application.isMobilePlatform;
#if UNITY_ANDROID
				isMobile = true;
#endif
				if (isMobile)
				{
						_playerStat.anchorMax = new Vector2(0.5f, 0);
						_playerStat.anchorMin = new Vector2(0.5f, 0);
						_playerStat.pivot = new Vector2(0.5f, 0);
				}
				else
				{
						_playerStat.anchorMax = Vector2.zero;
						_playerStat.anchorMin = Vector2.zero;
						_playerStat.pivot = Vector2.zero;
				}
		}

		public void UpdateHPBar(int currentHp, int maxHp)
		{
				_hpBarSlider.maxValue = maxHp;
				_hpBarSlider.value = currentHp;
				_hpText.text = $"{currentHp} / {maxHp}";
		}

		public void UpdateGoldText(int currentGold)
		{
				_goldText.text = $"{currentGold}";
		}

		public void UpdateChromeText(int currentChrome)
		{
				_chromeText.text = $"{currentChrome}";
		}

		#region 상호작용 UI 바인딩 (로컬라이즈된 문자열)
		// IInteractable에서 키를 받아 바인딩합니다.
		public void ShowInteractionUI(IInteractable interactable)
		{
				if (interactable == null) return;

				// 인터랙터로부터 로컬라이제이션 키를 받는다
				interactable.TryGetInteracrtionKey(out string titleKey, out string instructionKey);

				BindInteractionLocalizedStrings(titleKey, instructionKey);
				_interactionPanel?.SetActive(true);

				_interactionGuideView.transform.position = interactable.GuidePoint; // UI 위치 설정
				_interactionGuideView.SetActive(true); // UI 활성화
		}

		public void HideInteractionUI()
		{
				_interactionPanel?.SetActive(false);
				_interactionGuideView.SetActive(false);
				UnbindInteractionLocalizedStrings();
		}

		// 바인딩: 기존 구독 해제 후 새 LocalizedString 생성 및 구독
		void BindInteractionLocalizedStrings(string titleKey, string descriptionKey)
		{
				UnbindInteractionLocalizedStrings();

				if (!string.IsNullOrEmpty(titleKey))
				{
						_localInteractionTitle = new LocalizedString { TableReference = TABLE_COLLECTION_NAME_INTERACTIONVIEW, TableEntryReference = titleKey };
						_localInteractionTitle.StringChanged += OnTitleChanged;
						_localInteractionTitle.RefreshString();
				}

				if (!string.IsNullOrEmpty(descriptionKey))
				{
						_localInteractionDescription = new LocalizedString { TableReference = TABLE_COLLECTION_NAME_INTERACTIONVIEW, TableEntryReference = descriptionKey };
						_localInteractionDescription.StringChanged += OnDescChanged;
						_localInteractionDescription.RefreshString();
				}
		}

		void UnbindInteractionLocalizedStrings()
		{
				if (_localInteractionTitle != null) _localInteractionTitle.StringChanged -= OnTitleChanged;
				if (_localInteractionDescription != null) _localInteractionDescription.StringChanged -= OnDescChanged;
				_localInteractionTitle = null;
				_localInteractionDescription = null;
		}

		void OnTitleChanged(string localized)
		{
				if (_interactionTitleText != null) _interactionTitleText.text = localized;
		}

		void OnDescChanged(string localized)
		{
				if (_interactionDescriptionText != null) _interactionDescriptionText.text = localized;
		}
		#endregion

		public void UpdateGrenadeCoolTime(float currentCooltime, float maxCooltime)
		{
				_grenadeCooltimeSlider.maxValue = maxCooltime;
				_grenadeCooltimeSlider.value = currentCooltime;
		}

		public void UpdateGrenadeCount(int currentCount, int maxCount)
		{
				_grenadeCountText.text = $"{currentCount} / {maxCount}";
		}

		#region 룸 로딩 시 로딩 텍스트(LocalizedString) 바인딩
		public void ShowRoomLoadingPanel(DoorInteractor doorInteractor)
		{
				EventBus.bIsRoomReady = false;
				_roomLoadingRoutine = StartCoroutine(RoomLoadingRoutine(doorInteractor));
		}

		/// <summary>
		/// 룸 로딩 시 실행할 코루틴
		/// 랜덤으로 로딩 텍스트를 보여줌
		/// </summary>
		/// <returns></returns>
		IEnumerator RoomLoadingRoutine(DoorInteractor doorInteractor)
		{
				EventBus.SetCanTimeRun(false);

				int rand = UnityEngine.Random.Range(0, _roomLoadingKeys.Length);
				string key = _roomLoadingKeys[rand];

				var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TABLE_COLLECTION_NAME_ROOMLOADING, key);
				handle.Completed += (AsyncOperationHandle<string> op) =>
				{
						_roomLoadingText.text = (op.Status == AsyncOperationStatus.Succeeded) ? op.Result : "Loading...";
				};

				_roomLoadingPanel.SetActive(true);
				yield return new WaitForSeconds(0.5f);

				OnRoomLoadingComplete?.Invoke(doorInteractor);
				yield return new WaitUntil(() => EventBus.bIsRoomReady);
				_roomLoadingPanel.SetActive(false);

				EventBus.SetCanTimeRun(true);
				_roomLoadingRoutine = null;
		}
		#endregion

		#region 튜토리얼 패널 텍스트(LocalizedString) 바인딩
		public void ShowTutorialPanel()
		{
				// 먼저 기존 바인딩 해제
				UnbindAllTutorialBindings();

				// 키 목록 구성 (플랫폼별)
				List<string> tutorialKeys = new List<string>();
				if (Application.isMobilePlatform)
				{
						foreach (var baseKey in _baseTutorialKeys) tutorialKeys.Add(baseKey + "_Mobile");
				}
				else
				{
						foreach (var baseKey in _baseTutorialKeys) tutorialKeys.Add(baseKey + "_PC");
				}

				// 필요한 크기로 리스트 초기화
				int count = _tutorialTexts.Count;
				_boundTutorialLocalized = new List<LocalizedString>(new LocalizedString[count]);

				// 바인딩
				for (int i = 0; i < count; i++)
				{
						string key = (i < tutorialKeys.Count) ? tutorialKeys[i] : null;
						BindTutorialLocalizedString(i, key);
				}

				_tutorialPanel.SetActive(true);
		}

		void BindTutorialLocalizedString(int index, string tutorialKey)
		{
				if (index < 0 || index >= _tutorialTexts.Count) return;

				// 이전 바인딩 있으면 해제
				if (index < _boundTutorialLocalized.Count && _boundTutorialLocalized[index] != null)
				{
						var prev = _boundTutorialLocalized[index];
						var prevHandler = _tutorialChangedHandlers[index];
						if (prevHandler != null) prev.StringChanged -= prevHandler;
						_boundTutorialLocalized[index] = null;
						_tutorialChangedHandlers[index] = null;
				}

				if (string.IsNullOrEmpty(tutorialKey))
				{
						_tutorialTexts[index].text = "";
						return;
				}

				var ls = new LocalizedString { TableReference = TABLE_COLLECTION_NAME_TUTORIAL, TableEntryReference = tutorialKey };

				// LocalizedString.ChangeHandler 타입으로 생성 (Action<string>이 아님)
				LocalizedString.ChangeHandler handler = (string localized) =>
				{
						if (_tutorialTexts != null && index >= 0 && index < _tutorialTexts.Count && _tutorialTexts[index] != null)
								_tutorialTexts[index].text = localized;
				};

				ls.StringChanged += handler;
				ls.RefreshString();

				// 리스트에 보관 (인덱스가 부족하면 확장)
				while (_boundTutorialLocalized.Count <= index) _boundTutorialLocalized.Add(null);
				while (_tutorialChangedHandlers.Count <= index) _tutorialChangedHandlers.Add(null);

				_boundTutorialLocalized[index] = ls;
				_tutorialChangedHandlers[index] = handler;
		}

		void UnbindAllTutorialBindings()
		{
				for (int i = 0; i < _boundTutorialLocalized.Count; i++)
				{
						var ls = _boundTutorialLocalized[i];
						var handler = (i < _tutorialChangedHandlers.Count) ? _tutorialChangedHandlers[i] : null;
						if (ls != null && handler != null)
								ls.StringChanged -= handler;
				}
				_boundTutorialLocalized.Clear();
				_tutorialChangedHandlers.Clear();
		}

		public void HideTutorialPanel()
		{
				_tutorialPanel.SetActive(false);
				UnbindAllTutorialBindings();
		}
		#endregion

		public void ShowGameOverPanel()
		{
				_gameOverPanel.SetActive(true);
		}

		public void ShowGameClearPanel()
		{
				TimeSpan timeSpan = TimeSpan.FromSeconds(_playScene.TimeChecker.CurrentTime);
				// 현재 시간을 분 : 초 형식으로 변환하여 표시
				_gameClearTimeText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
				_gameClearPanel.SetActive(true);
		}

		public void UpdateTimer(float timeInSeconds)
		{
				TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
				_gameTimerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
		}

		public void CheckNormalAttackTutorialComplete()
		{
				if (_tutorialCompleteCheckImage.Count > 0)
				{
						_tutorialCompleteCheckImage[0].gameObject.SetActive(true);
				}
		}
		public void CheckGrenadeAttackTutorialComplete()
		{
				if (_tutorialCompleteCheckImage.Count > 1)
				{
						_tutorialCompleteCheckImage[1].gameObject.SetActive(true);
				}
		}
		public void CheckSpecialAttackTutorialComplete()
		{
				if (_tutorialCompleteCheckImage.Count > 2)
				{
						_tutorialCompleteCheckImage[2].gameObject.SetActive(true);
				}
		}
		public void CheckDashTutorialComplete()
		{
				if (_tutorialCompleteCheckImage.Count > 3)
				{
						_tutorialCompleteCheckImage[3].gameObject.SetActive(true);
				}
		}
		public void CheckInteractTutorialComplete()
		{
				if (_tutorialCompleteCheckImage.Count > 4)
				{
						_tutorialCompleteCheckImage[4].gameObject.SetActive(true);
				}
		}

		public void OnGoToMainClicked()
		{
				GameManager.Instance.serverManager.downloadManager.SetChromeToServer();
				GameManager.Instance.SceneLoadManager.LoadIntroScene();
		}

		public async void UpdateCurrentRoomCountText(int roomCount)
		{
				_currentRoomCountText.text = $"Room {roomCount}";

				if (roomCount == 1)
				{
				GameManager.Instance.SceneLoadManager.SetCurrentState(LOADSTATE.Finish);

						await System.Threading.Tasks.Task.Delay(500);

						GameManager.Instance.SceneLoadManager.UnloadScene();
				}
		}

		private void OnDisable()
		{
				EventBus.OnBossDead -= ShowGameClearPanel;
				UnbindInteractionLocalizedStrings();
				UnbindAllTutorialBindings(); // 튜토리얼 바인딩도 안전하게 해제
		}
}