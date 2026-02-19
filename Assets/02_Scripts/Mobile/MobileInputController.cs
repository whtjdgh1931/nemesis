using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileInputController : MonoBehaviour
{
		[Header("조이스틱 및 버튼")]
		[SerializeField] private Joystick joystick;
		[SerializeField] private Button dashButton;
		[SerializeField] private Button normalAttackButton;
		[SerializeField] private Button specialAttackButton;
		[SerializeField] private Button grenadeButton;
		[SerializeField] private GameObject mobilePanel;
		[SerializeField] private Button interActionBtn;

		[Header("연결 대상")]
		[SerializeField] private Player player;
		[SerializeField] private PlayerInputHandler inputHandler;

		private bool isHoldingNormalAttack = false;
		private bool isInit = false;
		private bool isTouchOverButton = false;
		private bool isGrenadeAiming = false;
		private int grenadeFingerId = -1;
		private Vector3 grenadeTarget;

		private RectTransform grenadeRect;

		public void Initialize(Player player, PlayerInputHandler input)
		{
				mobilePanel.SetActive(true);
				this.player = player;
				inputHandler = input;

				dashButton.onClick.AddListener(OnDashPressed);
				grenadeButton.onClick.AddListener(OnGrenadePressed);

				AddEventTrigger(normalAttackButton.gameObject, EventTriggerType.PointerDown, OnNormalAttackDown);
				AddEventTrigger(normalAttackButton.gameObject, EventTriggerType.PointerUp, OnNormalAttackUp);
				AddEventTrigger(specialAttackButton.gameObject, EventTriggerType.PointerDown, OnSpecialAttackDown);
				AddEventTrigger(specialAttackButton.gameObject, EventTriggerType.PointerUp, OnSpecialAttackUp);
				AddEventTrigger(grenadeButton.gameObject, EventTriggerType.PointerDown, OnGrenadeDown);
				AddEventTrigger(grenadeButton.gameObject, EventTriggerType.PointerUp, OnGrenadeUp);
				AddEventTrigger(grenadeButton.gameObject, EventTriggerType.PointerEnter, OnGrenadeEnter);
				AddEventTrigger(grenadeButton.gameObject, EventTriggerType.PointerExit, OnGrenadeExit);

				grenadeRect = grenadeButton.GetComponent<RectTransform>();

				player.OnInteractableDetected += SetBtnOn;
				player.OnInteractableMissed += SetBtnOff;
				interActionBtn.onClick.AddListener(player.ExecuteInteraction);
				SetBtnOff();

				isInit = true;
		}

		public void SetBtnOn(IInteractable i)
		{
				interActionBtn.gameObject.SetActive(true);
		}

		public void SetBtnOff()
		{
				interActionBtn.gameObject.SetActive(false);

		}

		private void Update()
		{
				if (!isInit || player == null || inputHandler == null || !EventBus.CanGetInput)
						return;

				// 이동 처리
				Vector2 dir = joystick.Direction;
				Vector3 moveDir = new Vector3(dir.x, 0f, dir.y);
				if (!isHoldingNormalAttack)
				{
						inputHandler.TriggerMoveInput(moveDir);
				}

				// 일반 공격 처리
				if (isHoldingNormalAttack)
				{
						inputHandler.TriggerNormalAttackInput();
				}

				// 유탄 조준 위치 갱신
				bool aimingUpdated = false;




				if (isGrenadeAiming)
				{
						foreach (Touch touch in Input.touches)
						{
								if (touch.fingerId == grenadeFingerId &&
										(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
								{
										Vector3? target = GetTouchGroundPoint(touch.position);
										if (target.HasValue)
										{
												grenadeTarget = target.Value;
												player.GrenadeAttacker.UpdateAiming(grenadeTarget);
												aimingUpdated = true;
										}
								}

								if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) &&
										touch.fingerId == grenadeFingerId)
								{
										isGrenadeAiming = false;
										grenadeFingerId = -1;
										player.GrenadeAttacker.ConfirmAiming();
								}
						}

						if (!aimingUpdated)
						{
								player.GrenadeAttacker.UpdateAiming(grenadeTarget);
						}
				}

		}


		private void OnDashPressed()
		{
				inputHandler?.TriggerDashInput();
		}

		private void OnGrenadePressed()
		{
				if (Input.touchCount == 0) return;
				if(EventBus.IsColosseumRoom)
				{
						return;
				}

				Vector3? target = GetTouchGroundPoint(Input.GetTouch(0).position);
				if (target.HasValue)
				{
						inputHandler?.TriggerGrenadeAttackInput(target.Value);
				}
		}

		private void OnNormalAttackDown(BaseEventData data)
		{
				isHoldingNormalAttack = true;
		}

		private void OnNormalAttackUp(BaseEventData data)
		{
				isHoldingNormalAttack = false;
		}

		private void OnSpecialAttackDown(BaseEventData data)
		{
				inputHandler?.TriggerSpecialAttackInput();
		}

		private void OnSpecialAttackUp(BaseEventData data)
		{
				inputHandler?.TriggerSpecialAttackCanceled();
		}

		private Vector3? GetTouchGroundPoint(Vector2 touchPos)
		{
				Ray ray = Camera.main.ScreenPointToRay(touchPos);
				if (Physics.Raycast(ray, out RaycastHit hit, 200f, LayerMask.GetMask("Ground")))
				{
						return hit.point;
				}
				return null;
		}

		private bool IsTouchOnScreen(Vector2 touchPos)
		{
				return touchPos.x >= 0 && touchPos.x <= Screen.width &&
							 touchPos.y >= 0 && touchPos.y <= Screen.height;
		}
		private void OnGrenadeDown(BaseEventData data)
		{
				if (EventBus.IsColosseumRoom)
				{
						if (player.GrenadeAttacker.isAiming)
						{
								return;

						}
						Vector3 monsterPos = EventBus.EliteBoss.transform.position;
						monsterPos.y = 0;
						Debug.Log(monsterPos);
						player.GrenadeAttacker.SetAiming(true);
						player.GrenadeAttacker.UpdateAiming(monsterPos);
						player.GrenadeAttacker.ConfirmAiming();


						return;

				}
				isGrenadeAiming = true;
				isTouchOverButton = true;


				// 유탄 버튼 영역에 해당하는 터치를 찾아 fingerId 저장
				foreach (Touch touch in Input.touches)
				{
						if (RectTransformUtility.RectangleContainsScreenPoint(grenadeRect, touch.position, null))
						{
								grenadeFingerId = touch.fingerId;
								break;
						}
				}

				player.GrenadeAttacker.StartAiming();
		}

		private void OnGrenadeUp(BaseEventData data)
		{
				if (!isGrenadeAiming) return;


				isGrenadeAiming = false;
				grenadeFingerId = -1;

				if (isTouchOverButton)
						player.GrenadeAttacker.CancelAiming();
				else
						player.GrenadeAttacker.ConfirmAiming();
		}

		private void AddEventTrigger(GameObject obj, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
		{
				EventTrigger trigger = obj.GetComponent<EventTrigger>();
				if (trigger == null)
						trigger = obj.AddComponent<EventTrigger>();

				EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
				entry.callback.AddListener(action);
				trigger.triggers.Add(entry);
		}

		private void OnGrenadeEnter(BaseEventData data) => isTouchOverButton = true;
		private void OnGrenadeExit(BaseEventData data) => isTouchOverButton = false;

		private void OnDisable()
		{
				isInit = false;
		}

		public void OnSetting()
		{
				GameManager.Instance.UIManager.OpenSettingPanel();
		}
}
