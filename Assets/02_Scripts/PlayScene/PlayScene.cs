using Unity.Cinemachine;
using UnityEngine;

public class PlayScene : MonoBehaviour
{
    [Header("----- 컴포넌트 참조 -----")]
    [SerializeField] Player _player;                               // 플레이어
    [SerializeField] PlayerInputHandler _inputHandler;             // 플레이어 입력 핸들러
    [SerializeField] MapController _mapController;                 // 맵 컨트롤러
    [SerializeField] PlaySceneView _playSceneView;                 // 플레이 씬 뷰
    [SerializeField] CameraMover _cameraMover;                     // 카메라 무버
    [SerializeField] CinemachineBrain _cinemachineBrain;         // 시네머신 브레인
    [SerializeField] TimeChecker _timeChecker;                     // 클리어 타임 체크 컴포넌트

    [Header("---- 모바일 용 참조 ----")]
    [SerializeField] MobileInputController _mobileInputController; // 모바일용 inputHandler

    [Header("----- 읽기 전용 속성 -----")]
    [SerializeField] bool _isColosseumRoom = false;               // 콜로세움 방 여부

    public MapController MapController => _mapController;
    public TimeChecker TimeChecker => _timeChecker;
    public Player player => _player;

    private void Awake()
    {
        EventBus.IsColosseumChanged += IsColosseum;
        IsColosseum(false);


        // PlayerInputHandler의 이벤트와 Player 메서드 연결
        _inputHandler.OnMoveInput += OnMoveInput;
        _inputHandler.OnDashInput += () => _player.SetDashPressed(true);
        _inputHandler.OnNormalAttackInput += () => _player.SetNormalAttackPressed(true);
        _inputHandler.OnGrenadeAttackInput += _player.GrenadeAttack;
        _inputHandler.OnSpecialAttackInput += _player.HandleSpecialStarted;
        _inputHandler.OnSpecialAttackInputCanceled += _player.HandleSpecialCanceled;
        _inputHandler.OnInteractInput += _player.ExecuteInteraction;

        GameManager.Instance.CurrencyManager.OnCreditChanged += _playSceneView.UpdateGoldText;
        GameManager.Instance.CurrencyManager.OnChromeChanged += _playSceneView.UpdateChromeText;
        _player.playerModel.OnHpChanged += _playSceneView.UpdateHPBar;
        _player.OnInteractableDetected += _playSceneView.ShowInteractionUI;
        _player.OnInteractableMissed += _playSceneView.HideInteractionUI;
        _player.OnGrenadeCooltimeChanged += _playSceneView.UpdateGrenadeCoolTime;
        _player.OnGrenadeCountChanged += _playSceneView.UpdateGrenadeCount;
        _mapController.OnDoorInteractionFinished += _playSceneView.ShowRoomLoadingPanel;
        _mapController.OnCurrentRoomCountChanged += _playSceneView.UpdateCurrentRoomCountText;
        _playSceneView.OnRoomLoadingComplete += _mapController.SpawnRoom;
        _timeChecker.OnTimeUpdated += _playSceneView.UpdateTimer;
        _mapController.OnStartRoomExited += _timeChecker.StartTimeCheck;
        _mapController.OnStartRoomExited += _playSceneView.HideTutorialPanel;

        // 튜토리얼 관련 이벤트 연결
        _player.OnNormalAttackStarted += _playSceneView.CheckNormalAttackTutorialComplete;
        _player.OnSpecialAttackStarted += _playSceneView.CheckSpecialAttackTutorialComplete;
        _player.OnDashStarted += _playSceneView.CheckDashTutorialComplete;
        _player.OnGrenadeAttackStarted += _playSceneView.CheckGrenadeAttackTutorialComplete;
        _player.OnInteractionStarted += _playSceneView.CheckInteractTutorialComplete;

        GameManager.Instance.SceneLoadManager.SetCurrentState(LOADSTATE.PlaySceneView);
		}
		private void Start()
		{
		

        if (_player == null)
        {
            Debug.LogError("플레이어가 할당되지 않았습니다!");
            return;
        }
        _player.Initialize();
        _player.OnPlayerDead += _playSceneView.ShowGameOverPanel;

        if (MapController == null)
        {
            Debug.LogError("맵 컨트롤러가 할당되지 않았습니다!");
            return;
        }
        _mapController.Initialize(_player);
        if (_playSceneView == null)
        {
            Debug.LogError("PlaySceneView가 할당되지 않았습니다!");
            return;
        }
        _playSceneView.Initialize(this);
        if (_cameraMover == null)
        {
            Debug.LogError("카메라 무버가 할당되지 않았습니다!");
            return;
        }
        _cameraMover.Initialize(_player);

        if (_timeChecker == null)
        {
            Debug.LogError("TimeChecker가 할당되지 않았습니다!");
            return;
        }
        _timeChecker.Initialize();

        GameManager.Instance.skillManager.SetPlayScene(this);

        bool isMobile = Application.isMobilePlatform;
#if UNITY_ANDROID
        isMobile = true;
#endif
        if (isMobile)
        {

            if (_mobileInputController == null)
            {
                Debug.LogError("MobileInputController가 할당되지 않았습니다!");
                return;
            }
            _mobileInputController.Initialize(_player, _inputHandler);
        }


        _mapController.OnRoomStart += _player.playerModel.AutoHeal;
    }

    private void Update()
    {
    }

    void IsColosseum(bool isColosseum)
    {
        _isColosseumRoom = isColosseum;
        _cameraMover.enabled = !isColosseum;
        _cinemachineBrain.enabled = isColosseum;
        SetCameraProjection(isColosseum);
        SetMouseCursorLock(isColosseum);
    }

    void OnMoveInput(Vector3 moveDir)
    {
        // moveDir는 PlayerInputHandler에서 new Vector3(input.x, 0, input.y)로 전달된다고 가정.
        // 그러므로 전진 성분은 moveDir.z, 좌우 성분은 moveDir.x 입니다.

        Vector3 moveInput = new Vector3(moveDir.x, 0f, moveDir.z); // x:left/right, z:forward/back

        if (!_isColosseumRoom)
        {
            // 콜로세움 방이 아니면 입력값을 그대로 전달하지 말고
            // 카메라 기준 변환 후 전달해야 움직임이 카메라 기준이 됩니다.
            // (기존 코드는 바로 전달하고 다시 변환해서 두 번 호출하는 문제 있었음)
            _player.SetMoveInput(moveInput); // Player.SetMoveInput이 Vector3를 받는다고 가정
            return;
        }

        // 카메라 기준으로 방향 변환

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera not found. Using raw input direction.");

            return;
        }

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 worldDirection = camForward * moveInput.z + camRight * moveInput.x;

        // Player/PlayerMover가 Vector3 world-space 방향을 기대하므로 그대로 전달
        _player.SetMoveInput(worldDirection);
    }

    void SetMouseCursorLock(bool isColosseum)
    {
        if (isColosseum)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void SetCameraProjection(bool isColosseum)
    {
        if (isColosseum)
        {
            Camera.main.orthographic = false;
        }
        else
        {
            Camera.main.orthographic = true;
        }
    }

    private void OnDestroy()
    {
        EventBus.IsColosseumChanged -= IsColosseum;
    }
}
