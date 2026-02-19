using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어의 주요 컴포넌트들을 관리하는 최상위 클래스
/// - 상태머신 전환(EvaluateTransitions)과 입력 소비를 담당
/// </summary>
public class Player : MonoBehaviour
{
    [Header("----- 컴포넌트 참조 -----")]
    [SerializeField] PlayerModel _playerModel;
    [SerializeField] CharacterController _characterController; // 플레이어 캐릭터 컨트롤러 컴포넌트
    [SerializeField] PlayerMover _mover;                       // 플레이어 이동 컴포넌트
    [SerializeField] PlayerDasher _dasher;                     // 플레이어 대시 컴포넌트
    [SerializeField] PlayerWeaponController _weaponController; // 플레이어 무기 관리 컴포넌트
    [SerializeField] PlayerAnimator _animator;                 // 플레이어 애니메이터 컴포넌트
    [SerializeField] PlayerAnimationEventForwarder _forwarder; // 플레이어 애니메이션 이벤트 포워더 컴포넌트
    [SerializeField] PlayerGrenadeAttacker _grenadeAttacker;   // 플레이어 유탄 발사 컴포넌트

    [SerializeField] PlayerNormalAttacker[] _normalAttackers;        // 플레이어 일반 공격 컴포넌트 배열 (무기별로 다름)
    [SerializeField] PlayerSpecialAttacker[] _specialAttackers;      // 플레이어 특수 공격 컴포넌트 배열 (무기별로 다름)

    [Header("----- 상호작용 컴포넌트 -----")]
    [SerializeField] InteractionController _interactionController;      // 상호작용 컨트롤러
    [SerializeField] InteractableDetector _interactableDetector;        // 상호작용 감지기

    [SerializeField] bool _isInteractable; // 상호작용 가능 여부

    [Header("----- 무기가 바뀌면 같이 바뀌는 컴포넌트(읽기 전용) -----")]
    [SerializeField] PlayerNormalAttacker _normalAttacker;     // 플레이어 일반 공격 컴포넌트 (추상/베이스)
    [SerializeField] PlayerSpecialAttacker _specialAttacker;   // 플레이어 특수 공격 컴포넌트
    

    [Header("----- 읽기 전용 -----")]
    [SerializeField] PlayerWeaponSet _currentWeaponSet;        // 현재 플레이어 무기 세트
    [SerializeField] PlayerStateType _currentStateType;        // 현재 플레이어 상태 타입
    [SerializeField] PlayerStatManager _statManager;           // 플레이어 스탯 매니저

    [Header("----- 입력 상태 플래그 -----")]
    // 타입 불일치 문제 해결: Vector3로 통일 (world-space 방향 저장)
    [SerializeField] Vector3 _moveInput;                  // 플레이어 이동 입력 벡터 (world-space)
    [SerializeField] bool _dashPressed;                    // 플레이어 대시 입력 여부 (단발)
    [SerializeField] bool _normalAttackPressed;            // 플레이어 일반 공격 입력 여부 (단발)
    [SerializeField] bool _grenadeAttackPressed;           // 플레이어 유탄 공격 입력 여부 (단발)
    [SerializeField] bool _specialAttackPressed;           // 플레이어 특수 공격 입력 여부 (단발)

    [Header("----- 상태 플래그 (Player가 소유) -----")]
    [SerializeField] bool _canMove;                        // 플레이어 이동 가능 여부
    [SerializeField] bool _isDashing;                      // 플레이어 대시 중 여부 (동기화됨)
    [SerializeField] bool _isNormalAttacking;              // (동기화됨)
    [SerializeField] bool _isSpecialAttacking;             // 플레이어 특수공격 중 여부

    // 읽기 전용 프로퍼티로 외부 접근 제공
    public CharacterController CharacterController => _characterController;
    public PlayerMover Mover => _mover;
    public PlayerWeaponController WeaponController => _weaponController;
    public PlayerNormalAttacker NormalAttacker => _normalAttacker;
    public PlayerSpecialAttacker SpecialAttacker => _specialAttacker;
    public PlayerAnimator Animator => _animator;
    public PlayerModel playerModel => _playerModel;
    public PlayerDasher Dasher => _dasher;
    public PlayerWeaponSet CurrentWeaponSet => _currentWeaponSet;
    public PlayerGrenadeAttacker GrenadeAttacker => _grenadeAttacker;

    // MoveInput은 world-space Vector3로 노출
    public bool CanGetInput => EventBus.CanGetInput;
    public Vector3 MoveInput => _moveInput;
    public bool DashPressed => _dashPressed;
    public bool NormalAttackPressed => _normalAttackPressed;
    public bool GrenadeAttackPressed => _grenadeAttackPressed;
    public bool SpecialAttackPressed => _specialAttackPressed;

    public bool CanMove => _canMove;

    // 변경: IsDashing/IsNormalAttacking는 해당 컴포넌트(실제 소유자)를 우선 참조하도록 함.
    public bool IsDashing => _dasher != null ? _dasher.IsDashing : _isDashing;
    public bool IsNormalAttacking { get => _normalAttacker.IsAttacking; set => _isNormalAttacking = _normalAttacker.IsAttacking; }
    public bool IsSpecialAttacking => _isSpecialAttacking;

    #region 이벤트
    public event Action OnNormalAttackStarted;
    public event Action OnGrenadeAttackStarted;
    public event Action OnSpecialAttackStarted;
    public event Action OnDashStarted;
    public event Action OnInteractionStarted;
    public event Action<IInteractable> OnInteractableDetected;
    public event Action OnInteractableMissed;
    public event Action<int, int> OnGrenadeCountChanged; // 현재 유탄 개수, 최대 유탄 개수
    public event Action<float, float> OnGrenadeCooltimeChanged; // 현재 쿨타임, 최대 쿨타임
    public event Action OnPlayerDead;
    #endregion

    [Header("----- 상태 캐시 -----")]
    StateMachine _stateMachine;                              // 플레이어 상태 머신

    Dictionary<WeaponType, PlayerNormalAttacker> _normalAttackerMap = new(); // 무기 타입별 일반 공격 컴포넌트 맵
    Dictionary<WeaponType, PlayerSpecialAttacker> _specialAttackerMap = new(); // 무기 타입별 특수 공격 컴포넌트 맵

    private void Update()
    {
        // 입력에 따른 상태 전환 판단 (우선순위: NormalAttack > Dash > Move > Idle)
        EvaluateTransitions();

        // 단발 입력은 한 프레임 소비 패턴: 프레임 끝에 리셋
        // (EvaluateTransitions 내에서 소비(TryConsume*)되더라도, 안전하게 리셋해 둡니다)
        _dashPressed = false;
        _normalAttackPressed = false;
        _grenadeAttackPressed = false;
        _specialAttackPressed = false;

        // 상태 업데이트 호출
        _stateMachine?.Update();
        _currentStateType = _stateMachine.CurrentType;
    }

    /// <summary>
    /// Player 초기화 함수
    /// - 컴포넌트 초기화, 상태 맵 구성, 초기 상태 설정
    /// </summary>
    public void Initialize()
    {
        
        // 구독: 무기 변경
        if (_weaponController != null)
            _weaponController.OnWeaponChanged += OnWeaponChanged;

        //SubscribeNormalAttacker(_normalAttacker);

        // 상호작용 관련 이벤트 구독
        if (_interactionController != null)
        {
            _interactionController.OnWeaponInteract += OnWeaponInteracted;
            _interactionController.OnDoorInteract += OnDoorInteracted;
        }

        if (_interactableDetector != null)
        {
            _interactableDetector.OnDetected += InteractableDetected;
            _interactableDetector.OnMissed += InteractableMissed;
        }

        // Attacker 타입별로 매핑
        foreach (var attacker in _normalAttackers)
        {
            if (attacker != null)
                _normalAttackerMap[attacker.WeaponType] = attacker;
        }
        foreach (var attacker in _specialAttackers)
        {
            if (attacker != null)
                _specialAttackerMap[attacker.WeaponType] = attacker;
        }
        _grenadeAttacker.OnGrenadeCooltimeChanged += GrenadeCoolTimeChanged;
        _grenadeAttacker.OnGrenadeCountChanged += GrenadeCountChanged;
        _grenadeAttacker.Initialize();
        _mover.Initialize(this);

        _playerModel?.Initialize();
        _playerModel.OnDieEvent += () => OnPlayerDead?.Invoke();
        _dasher?.Initialize(_characterController);
        _forwarder?.Initialize(this);
        _weaponController?.Initialize();

        _interactableDetector.OnInteractionStarted += () => OnInteractionStarted?.Invoke();
        _interactionController?.Initialize();
        //_interactableGuideView.Initialize();

        // Dasher 이벤트 구독 (동기화)
        if (_dasher != null)
        {
            _dasher.DashStarted += OnDasherStarted;
            _dasher.DashEnded += OnDasherEnded;
        }

        // 상태 맵 초기화 및 상태머신 설정
        _stateMachine = new StateMachine(this);
    }

    void OnDestroy()
    {
        // 안전하게 모든 구독 해제
        if (_dasher != null)
        {
            _dasher.DashStarted -= OnDasherStarted;
            _dasher.DashEnded -= OnDasherEnded;
        }

        //if (_normalAttacker != null)
        //{
        //    UnsubscribeNormalAttacker(_normalAttacker);
        //}

        if (_weaponController != null)
        {
            _weaponController.OnWeaponChanged -= OnWeaponChanged;
        }

        if (_interactionController != null)
        {
            _interactionController.OnWeaponInteract -= OnWeaponInteracted;
            _interactionController.OnDoorInteract -= OnDoorInteracted;
        }

        if (_interactableDetector != null)
        {
            _interactableDetector.OnDetected -= InteractableDetected;
            _interactableDetector.OnMissed -= InteractableMissed;
        }
    }

    void OnDasherStarted()
    {
        // 동기화: 에디터에서 값 확인용으로도 남겨둠
        _isDashing = true;
        OnDashStarted?.Invoke();
    }

    void OnDasherEnded()
    {
        _isDashing = false;
    }

    #region 입력 플래그 Setters (InputHandler -> Player)
    // 외부에서 world-space Vector3를 전달하도록 통일
    public void SetMoveInput(Vector3 moveInput)
    {
        if (!EventBus.CanGetInput)
        {
            _moveInput = Vector3.zero;
            return;   // 입력 잠금 상태면 무시
        }

        _moveInput = moveInput;
    }
    public void SetDashPressed(bool isPressed)
    {
        if (!EventBus.CanGetInput) return;   // 입력 잠금 상태면 무시
        _dashPressed = isPressed;
    }
    public void SetNormalAttackPressed(bool isPressed)
    {
        if( !EventBus.CanGetInput) return;   // 입력 잠금 상태면 무시
        _normalAttackPressed = isPressed;
    }
    public void SetGrenadeAttackPressed(bool isPressed)
    {
        if (!EventBus.CanGetInput) return;   // 입력 잠금 상태면 무시
        _grenadeAttackPressed = isPressed;
    }
    public void SetSpecialAttackPressed(bool isPressed)
    {
        if (!EventBus.CanGetInput) return;   // 입력 잠금 상태면 무시
        _specialAttackPressed = isPressed;
    }
    #endregion

    #region 상태 플래그 Setters (상태 Enter/Exit에서 호출)
    public void SetCanMove(bool canMove) => _canMove = canMove;
    public void SetIsSpecialAttacking(bool isSpecialAttacking) => _isSpecialAttacking = isSpecialAttacking;
    #endregion

    #region 상태 전환 평가 및 처리
    void EvaluateTransitions()
    {
        // 1) 일반 공격 입력 처리
        if (_normalAttackPressed && TryConsumeNormalAttack())
        {
            if (_normalAttacker != null && !IsDashing && !_isSpecialAttacking)
            {
                bool accepted = false;
                try
                {
                    // 1) 먼저 RequestAttack 호출해서 시작 또는 큐잉 가능한지 확인
                    accepted = _normalAttacker.RequestAttack();

                    // 2) Request가 받아졌다면 상태 전환은 현재 상태를 보고 결정
                    if (accepted)
                    {
                        if (_stateMachine.CurrentType != PlayerStateType.NormalAttack)
                        {
                            bool changed = _stateMachine.ChangeState(PlayerStateType.NormalAttack);
                        }
                        else
                        {
                            // 이미 NormalAttack 상태라면 ChangeState 하지 않음.
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"RequestAttack exception: {ex}");
                }

                return;
            }
        }

        // 2) 대시
        if (_dashPressed && TryConsumeDash())
        {
            if (!IsNormalAttacking && !_isSpecialAttacking && !IsDashing)
            {
                _stateMachine.ChangeState(PlayerStateType.Dash);
                return;
            }
        }

        // 3) 이동
        if (_moveInput.sqrMagnitude > 0.01f && !IsNormalAttacking && !_isSpecialAttacking && !IsDashing)
        {
            if (_stateMachine.CurrentType != PlayerStateType.Move)
            {
                _stateMachine.ChangeState(PlayerStateType.Move);
            }
            return;
        }

        // 4) 기본 Idle
        if (_stateMachine.CurrentType != PlayerStateType.Idle && !IsNormalAttacking && !IsDashing && !_isSpecialAttacking)
        {
            _stateMachine.ChangeState(PlayerStateType.Idle);
        }
    }

    bool TryConsumeNormalAttack()
    {
        if (!_normalAttackPressed) return false;
        _normalAttackPressed = false;
        return true;
    }

    bool TryConsumeSpecialAttack()
    {
        if (!_specialAttackPressed) return false;
        _specialAttackPressed = false;
        return true;
    }

    bool TryConsumeDash()
    {
        if (!_dashPressed) return false;
        _dashPressed = false;
        return true;
    }
    #endregion

    #region 이동 관련 공개 함수들
    public void SetToIdle()
    {
        _stateMachine.ChangeState(PlayerStateType.Idle);
    }

    public void Move(Vector3 moveDir)
    {
        _mover.Move(moveDir);
        _animator.OnMove(moveDir.magnitude);
    }

    public void StopMove()
    {
        _mover.Move(Vector3.zero);
        _animator.OnMove(0f);
    }
    #endregion

    #region 무기 변경 처리
    void OnWeaponChanged(Weapon weapon)
    {
        // 이전 attacker 언구독 -> 새 attacker 할당 -> 새 attacker 구독
        var prevAttacker = _normalAttacker;

        _currentWeaponSet = GameManager.Instance.DataManager.WeaponSetMap[weapon.WeaponType];

        // 먼저 할당하기 전에 이전 구독 해제
        //if (prevAttacker != null)
        //    UnsubscribeNormalAttacker(prevAttacker);

        _normalAttacker = _normalAttackerMap.ContainsKey(weapon.WeaponType) ? _normalAttackerMap[weapon.WeaponType] : null;
        _specialAttacker = _specialAttackerMap.ContainsKey(weapon.WeaponType) ? _specialAttackerMap[weapon.WeaponType] : null;

        //SubscribeNormalAttacker(_normalAttacker);

        if (weapon.WeaponType == WeaponType.Rifle)
        {
            PlayerRifleNormalAttacker playerRifleNormalAttacker = _normalAttacker as PlayerRifleNormalAttacker;
            PlayerRifleSpecialAttacker playerRifleSpecialAttacker = _specialAttacker as PlayerRifleSpecialAttacker;
            RifleWeapon rifleWeapon = weapon as RifleWeapon;

            playerRifleNormalAttacker?.Initialize(this, rifleWeapon.FirePos);
            playerRifleSpecialAttacker?.Initialize(this, rifleWeapon.FirePos);
        }

        if (weapon.WeaponType == WeaponType.Blade)
        {
            PlayerBladeNormalAttacker playerBladeNormalAttacker = _normalAttacker as PlayerBladeNormalAttacker;
            PlayerBladeSpecialAttacker playerBladeSpecialAttacker = _specialAttacker as PlayerBladeSpecialAttacker;
            playerBladeNormalAttacker?.Initialize(this);
            playerBladeSpecialAttacker?.Initialize(this);
        }

        _animator.SetAnimator(_currentWeaponSet.AnimController);
    }

    public void OnAttackFireEvent()
    {
        if (_normalAttacker == null) return;

        if (_normalAttacker is PlayerRifleNormalAttacker rifle)
        {
            rifle.FireNow();
            OnNormalAttackStarted?.Invoke();
            return;
        }

        if (_normalAttacker is PlayerBladeNormalAttacker blade)
        {
            OnNormalAttackStarted?.Invoke();
            return;
        }
    }

    public void OnAttackHitEvent()
    {
        if (_normalAttacker is PlayerBladeNormalAttacker blade)
        {
            blade.DoMeleeHit();
        }
    }

    public void OnAttackEndEvent()
    {
        if (_normalAttacker == null) return;

        //if (_normalAttacker is PlayerRifleNormalAttacker rifle)
        //{
        //    rifle.OnAnimationAttackEnd();
        //    return;
        //}
        if (_normalAttacker is PlayerBladeNormalAttacker blade)
        {
            blade.OnAnimationAttackEnd();
            return;
        }

        _normalAttacker.EndAttack();
    }

    public void OnInvincibleStartEvent()
    {
        _playerModel.SetIsInvincibility(true);
    }

    public void OnInvincibleEndEvent()
    {
        _playerModel.SetIsInvincibility(false);
    }
    #endregion

    #region 특수공격 처리
    public void HandleSpecialStarted()
    {
        if (_isDashing || _isNormalAttacking || _isSpecialAttacking) return;

        if (_specialAttacker != null && _specialAttacker.RequestSpecial())
        {
            _stateMachine.ChangeState(PlayerStateType.SpecialAttack);
            OnSpecialAttackStarted?.Invoke();
        }
    }

    public void HandleSpecialCanceled()
    {
        if (_specialAttacker != null && _specialAttacker.IsCharging)
        {
            _specialAttacker.StopChargeAndFire();
        }
    }
 #endregion

    #region 유탄 공격 처리
    public void GrenadeAttack(Vector3 mousePos)
    {
        _grenadeAttacker.SetMousePos(mousePos);
        if (_grenadeAttacker.RequestAttack())
        {
            OnGrenadeAttackStarted?.Invoke();
        }
        
        _grenadeAttackPressed = false;
    }



    public void GrenadeCountChanged(int currentCount, int maxCount)
    {
        OnGrenadeCountChanged?.Invoke(currentCount, maxCount);
    }

    public void GrenadeCoolTimeChanged(float coolTimeRemaining, float maxCoolTime)
    {
        OnGrenadeCooltimeChanged?.Invoke(coolTimeRemaining, maxCoolTime);
    }

    #endregion

    #region 상호작용 처리
    public void OnWeaponInteracted(WeaponType newWeaponType)
    {
        _weaponController.ChangeWeapon(newWeaponType);
    }

    public void OnDoorInteracted(RoomInfo roomInfo)
    {
        if (roomInfo == null)
        {
            Debug.LogError("Player.OnDoorInteracted 호출 시 roomInfo가 null입니다! 호출자 스택을 확인하세요.");
            return;
        }
    }

    public void ExecuteInteraction()
    {
        if (!_isInteractable) return;

        _interactableDetector.ExecuteInteraction();

        InteractableMissed(); //상호작용 한번 하면 UI 사라지게
    }

    public void InteractableDetected(IInteractable interactable)
    {
        _isInteractable = true;
        OnInteractableDetected?.Invoke(interactable);
    }

    public void InteractableMissed()
    {
        _isInteractable = false;
        OnInteractableMissed?.Invoke();
    }

    /// <summary>
    /// 플레이어가 문과 상호작용할 때 실행되는 연출용 코루틴
    /// 문이 열리고 난 뒤에 위치를 문 앞으로 이동시킨 후 문으로 들어가는 연출을 재생함
    /// MapController에서 호출
    /// </summary>
    /// <returns></returns>
    public IEnumerator DoorInteractionRoutine()
    {
        DoorInteractor doorInteractor = _interactableDetector.DetectedInteractable as DoorInteractor;
        if (doorInteractor == null)
        {
            yield break;
        }
        EventBus.SetCanGetInput(false); // 입력 잠금
                              
        // 플레이어를 문의 위치에서 1만큼 앞으로 이동
        Vector3 targetPos = doorInteractor.transform.position + doorInteractor.transform.forward * 5f;
        targetPos.y = transform.position.y; // Y축은 현재 플레이어 높이 유지
        gameObject.SetActive(false); // 이동 중 충돌 방지를 위해 비활성화
        transform.position = targetPos;     // 플레이어를 문 앞으로 이동
        transform.forward = -doorInteractor.transform.forward;  // 문 쪽을 바라보게 회전
        gameObject.SetActive(true);

        // 문으로 1만큼 이동하는 연출 재생
        _characterController.enabled = false; // 충돌 방지 위해 비활성화
        float moveDuration = 1.0f; // 이동 시간
        float elapsed = 0f; // 경과 시간
        Vector3 startPos = transform.position; // 시작 위치
        Vector3 endPos = doorInteractor.transform.position; // 끝 위치 (문 중앙)
        doorInteractor.ToggleInteraction(false);
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            _animator.OnMove(1f); // 걷기 애니메이션 재생
            yield return null;
        }
        transform.position = endPos; // 정확히 끝 위치로 설정
        _characterController.enabled = true; // 충돌 방지 위해 다시 활성화

        EventBus.SetCanGetInput(true); // 입력 잠금 해제    
        yield return null;
        // 상호작용 완료 후 처리 (맵 전환 등)는 MapController에서 담당
        doorInteractor.ToggleInteraction(true);
    }
    #endregion
}