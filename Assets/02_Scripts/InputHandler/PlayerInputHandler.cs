using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NewInput을 통해 플레이어의 여러가지 입력을 받아오고 
/// 각 입력에 대한 이벤트를 제공하는 클래스
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] float _normalAttackInterval = 0.2f; // 일반공격 입력 반복 간격

    [SerializeField] PlayerInput _playerInput;       // PlayerInput 컴포넌트 참조
    [SerializeField] Camera mainCam;               // 메인 카메라 참조
    [SerializeField] LayerMask _groundLayer;        // Ground 레이어 마스크

    // 특수공격 입력 딜레이(초)
    [Header("Input Tuning")]
    [SerializeField] float _specialAttackInputDelay = 1.0f;

    public event Action<Vector3> OnMoveInput;        // 이동 입력 이벤트

    public event Action OnDashInput;                 // 대시 입력 이벤트
    public event Action OnInteractInput;             // 상호작용 입력 이벤트

    public event Action OnNormalAttackInput;         // 일반공격 입력 이벤트
    public event Action<Vector3> OnGrenadeAttackInput;        // 유탄공격 입력 이벤트
    public event Action OnGrenadeAttackInputEnded;   // 유탄공격 입력 종료 이벤트
    public event Action OnSpecialAttackInput;        // 특수공격 입력 이벤트
    public event Action OnSpecialAttackInputCanceled;   // 특수공격 입력 종료 이벤트

    Vector3 _moveDir;  // 이동 입력을 저장할 변수
    Coroutine _holdAttackRoutine; // 일반공격 입력 코루틴 참조

    // 디바운스용 타임스탬프(중복 호출 방지)
    float _lastNormalAttackStartTime = -1f;
    const float NormalAttackStartDebounce = 0.05f;

    // 특수공격 입력 마지막 시간 기록
    float _lastSpecialAttackInputTime = -Mathf.Infinity;

    private void Awake()
    {
        mainCam = Camera.main;

        if (_playerInput == null)
        {
            Debug.LogError("PlayerInput is NOT assigned on PlayerInputHandler!");
            return;
        }

        var actionMap = _playerInput.actions.FindActionMap("Player");
        if (actionMap == null)
        {
            Debug.LogError("ActionMap 'Player' not found!");
            return;
        }

        var normal = actionMap.FindAction("NormalAttack");
        if (normal == null)
        {
            Debug.LogError("NormalAttack action not found in 'Player' map!");
        }

        // Move, Dash, Interact (unchanged)
        actionMap["Move"].performed += OnMove;
        actionMap["Move"].canceled += OnMove;
        actionMap["Dash"].started += OnDash;
        actionMap["Interact"].started += OnInteract;

        // 변경: NormalAttack은 started가 아니라 performed로 바인딩
        if (normal != null)
        {
            normal.performed += OnNormalAttackPerformed;
            normal.canceled += OnNormalAttackCanceled;
        }

        actionMap["GrenadeAttack"].started += OnGrenadeAttack;
        actionMap["GrenadeAttack"].canceled += OnGrenadeAttack;
        actionMap["SpecialAttack"].started += OnSpecialAttack;
        actionMap["SpecialAttack"].canceled += OnSpecialAttack;
    }

    private void OnDestroy()
    {
        var actionMap = _playerInput.actions.FindActionMap("Player");
        if (actionMap == null) return;

        actionMap["Move"].performed -= OnMove;
        actionMap["Move"].canceled -= OnMove;
        actionMap["Dash"].started -= OnDash;
        actionMap["Interact"].started -= OnInteract;

        var normal = actionMap.FindAction("NormalAttack");
        if (normal != null)
        {
            normal.performed -= OnNormalAttackPerformed;
            normal.canceled -= OnNormalAttackCanceled;
        }

        actionMap["GrenadeAttack"].started -= OnGrenadeAttack;
        actionMap["GrenadeAttack"].canceled -= OnGrenadeAttack;
        actionMap["SpecialAttack"].started -= OnSpecialAttack;
        actionMap["SpecialAttack"].canceled -= OnSpecialAttack;
    }

    private void Update()
    {

        // 일반공격 입력중에는 이동 입력 벡터를 0으로
        if (_holdAttackRoutine != null)
        {
            OnMoveInput?.Invoke(Vector3.zero);
            return;
        }
        OnMoveInput?.Invoke(_moveDir);

#if !UNITY_ANDROID
        OnMoveInput?.Invoke(_moveDir);
#endif
    }

    /// <summary>
    /// 이동 입력을 받아오는 함수
    /// </summary>
    /// <param name="value"></param>
    public void OnMove(InputAction.CallbackContext value)
    {
        if (EventBus.IsRewardSelecting)
            return;
        if (value.performed)
        {
            Vector2 moveDir = value.ReadValue<Vector2>();
            _moveDir = new Vector3(moveDir.x, 0, moveDir.y);
            //Debug.Log($"이동 입력받음: {_moveDir}");
        }
        else if (value.canceled)
        {
            _moveDir = Vector3.zero;
            //Debug.Log("이동 입력 멈춤");
        }
    }

    void OnNormalAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (EventBus.IsRewardSelecting) return;

        // 간단 디바운스: 너무 빠른 연속 호출 차단
        if (Time.time - _lastNormalAttackStartTime < NormalAttackStartDebounce)
            return;
        _lastNormalAttackStartTime = Time.time;

        if (_holdAttackRoutine == null)
            _holdAttackRoutine = StartCoroutine(HoldAttackRoutine());
    }

    void OnNormalAttackCanceled(InputAction.CallbackContext ctx)
    {
        if (EventBus.IsRewardSelecting) return;
        if (_holdAttackRoutine != null)
        {
            StopCoroutine(_holdAttackRoutine);
            _holdAttackRoutine = null;
        }
    }

    IEnumerator HoldAttackRoutine()
    {
        OnNormalAttackInput?.Invoke();

        while (true)
        {
            yield return new WaitForSeconds(_normalAttackInterval);
            OnNormalAttackInput?.Invoke();
        }
    }

    /// <summary>
    /// 대시 입력을 받아오는 함수
    /// </summary>
    /// <param name="value"></param>
    public void OnDash(InputAction.CallbackContext value)
    {
        if (EventBus.IsRewardSelecting)
            return;
        if (value.started)
        {
            OnDashInput?.Invoke();
        }
    }

    /// <summary>
    /// 상호 작용 입력을 받아오는 함수
    /// </summary>
    /// <param name="value"></param>
    public void OnInteract(InputAction.CallbackContext value)
    {
        if (EventBus.IsRewardSelecting)
            return;
        if (value.started)
        {
            OnInteractInput?.Invoke();
        }
    }

    /// <summary>
    /// 유탄 공격 입력을 받아오는 함수
    /// </summary>
    /// <param name="value"></param>
    public void OnGrenadeAttack(InputAction.CallbackContext value)
    {
        if (EventBus.IsRewardSelecting)
            return;
        if (value.started)
        {
            if (EventBus.IsColosseumRoom)
            {
                Vector3 monsterPos = EventBus.EliteBoss.transform.position;
                monsterPos.y = 0f;
                OnGrenadeAttackInput?.Invoke(monsterPos);
                return;
            }
            // 우클릭 시작 시
            Vector3? target = GetMouseGroundPoint();
            if (target.HasValue)
            {
                OnGrenadeAttackInput?.Invoke(target.Value);
            }
        }
        else if (value.canceled)
        {

            OnGrenadeAttackInputEnded?.Invoke();
        }
    }

    /// <summary>
    /// 마우스로 Ground 지점 감지
    /// </summary>
    private Vector3? GetMouseGroundPoint()
    {
        if (EventBus.IsRewardSelecting)
            return null;
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _groundLayer))
        {
            return hit.point;
        }

        return null; // 맞은게 없으면 null 반환
    }



    /// <summary>
    /// 특수공격 입력을 받아오는 함수
    /// </summary>
    /// <param name="value"></param>
    public void OnSpecialAttack(InputAction.CallbackContext value)
    {
        if (EventBus.IsRewardSelecting)
            return;

        if (value.started)
        {
            TryInvokeSpecialInput();
        }
        if (value.canceled)
        {
            OnSpecialAttackInputCanceled?.Invoke();
        }
    }

    // 공통 시도 함수: 특수공격 입력 딜레이 적용 (InputHandler 내부에서 제어)
    bool TryInvokeSpecialInput()
    {
        // 이미 보상 선택 중이면 무시
        if (EventBus.IsRewardSelecting) return false;

        // 딜레이 체크
        if (Time.time - _lastSpecialAttackInputTime < _specialAttackInputDelay)
        {
            // 필요하다면 사운드/피드백 호출 위치
            // Debug.Log("Special attack input ignored: on cooldown.");
            return false;
        }

        _lastSpecialAttackInputTime = Time.time;
        OnSpecialAttackInput?.Invoke();
        return true;
    }

    #region mobile

    /// <summary>
    /// 모바일 입력용: 이동 입력 이벤트 호출
    /// </summary>
    public void TriggerMoveInput(Vector3 moveDir)
    {
        _moveDir = moveDir;
        OnMoveInput?.Invoke(moveDir);
    }

    /// <summary>
    /// 모바일 입력용: 대시 입력 이벤트 호출
    /// </summary>
    public void TriggerDashInput()
    {
        OnDashInput?.Invoke();
    }

    /// <summary>
    /// 모바일 입력용: 일반 공격 입력 이벤트 호출
    /// </summary>
    public void TriggerNormalAttackInput()
    {
        OnNormalAttackInput?.Invoke();
    }

    /// <summary>
    /// 모바일 입력용: 특수 공격 시작 이벤트 호출
    /// </summary>
    public void TriggerSpecialAttackInput()
    {
        // 모바일에서도 동일한 딜레이 규칙 적용
        TryInvokeSpecialInput();
    }

    /// <summary>
    /// 모바일 입력용: 특수 공격 취소 이벤트 호출
    /// </summary>
    public void TriggerSpecialAttackCanceled()
    {
        OnSpecialAttackInputCanceled?.Invoke();
    }

    /// <summary>
    /// 모바일 입력용: 유탄 공격 시작 이벤트 호출
    /// </summary>
    public void TriggerGrenadeAttackInput(Vector3 target)
    {
        OnGrenadeAttackInput?.Invoke(target);
    }

    /// <summary>
    /// 모바일 입력용: 유탄 공격 종료 이벤트 호출
    /// </summary>
    public void TriggerGrenadeAttackEnded()
    {
        OnGrenadeAttackInputEnded?.Invoke();
    }


    #endregion
}