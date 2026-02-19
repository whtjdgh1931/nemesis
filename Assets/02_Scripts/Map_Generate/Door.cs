using System;
using System.Collections;
using System.Resources;
using UnityEngine;

/// <summary>
/// Door: 문 프리팹 루트. DoorInteractor가 실제 입력을 감지하고,
/// Door는 RoomInfo를 보관하여 문 위의 UI/프리뷰를 보여주고 상호작용을 중계한다.
/// 
/// 책임:
/// - 런타임 RoomInfo 보유 (프리팹 직렬화 아님)
/// - DoorInteractor 이벤트 중계
/// - DoorView에 시각 데이터 적용 (RoomInfo -> ResourceManager lookup)
/// - InteractableManager에 등록/해제 (의존성 주입 허용)
/// 
/// 주의:
/// - Initialize로 주입된 의존성 사용 권장 (테스트/풀링 용이)
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] DoorInteractor _doorInteractor; // 반드시 프리팹에 연결되어 있어야 함
    [SerializeField] DoorView _doorView;             // 시각적 표현 담당(아이콘, 프리뷰 등)
    [SerializeField] Animator _doorAnim;            // 문 애니메이터

    Coroutine _doorOpenCoroutine;

    // RoomInfo는 런타임에 주입만 받고 프리팹에 저장되지 않도록 SerializeField 제거
    RoomInfo _roomInfo;
    public RoomInfo RoomInfo => _roomInfo; // 외부는 읽기 전용으로 접근

    // 중계 이벤트: MapController 등에서 구독
    public event Action<IInteractable> DoorInteracted;

    // 등록한 매니저(주입될 수 있음)
    //IInteractableManager _interactableManager;
    //IResourceManager _resourceManager; // ResourceManager 인터페이스(게임의 ResourceManager 래퍼)

    //bool _isInitialized = false;

    // 임시
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            OnRewardSelectionCompleted();
        }
    }

    /// <summary>
    /// 초기화: 반드시 RoomInfo를 주입해야 함.
    /// 의존성 매니저는 주입하지 않으면 GameManager.Instance에서 가져옵니다(기존 호환성 유지).
    /// </summary>
    public void Initialize(RoomInfo info)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (_doorInteractor == null) Debug.LogError("Door.Initialize: _doorInteractor is null on prefab.");

        // 의존성 설정(주입 우선, 없으면 전역에서 가져오기)
        //_interactableManager = interactableManager ?? GameManager.Instance?.InteractableManager;
        //_resourceManager = resourceManager ?? GameManager.Instance?.ResourceManager;

        // 상태 주입
        _roomInfo = info;

        // 방에 정보 넘겨주고 상호작용 비활성화(초기 상태)
        _doorInteractor.SetRoomInfo(info);
        _doorInteractor.ToggleInteraction(false);

        // 방지: 중복 구독
        _doorInteractor.OnInteracted -= OnDoorInteracted;
        _doorInteractor.OnInteracted += OnDoorInteracted;

        // 등록
        //_interactableManager?.Register(_doorInteractor);

        // 뷰에 정보 넘겨주고 뷰 비활성화(초기 상태)
        _doorView.SetRewardView(info);
        _doorView.ToggleReward(false);

        //_isInitialized = true;
    }

    // OnInteracted 중계
    void OnDoorInteracted(IInteractable interactable)
    {
        // 문 열리는 애니메이션 재생하는 코루틴
        _doorOpenCoroutine = StartCoroutine(DoorOpenAnimationRoutine(interactable));
    }

    IEnumerator DoorOpenAnimationRoutine(IInteractable interactable)
    {
        _doorAnim.SetTrigger(Constants.ANIPARAM_ONDOOROPEN);
        GameManager.Instance.SoundManager.PlaySfxForSeconds("DoorOpen", 1.2f);
        yield return new WaitForSeconds(1.5f);
        DoorInteracted?.Invoke(interactable);
    }

    // 안전한 해제: OnDisable은 풀링/씬 전환 시에도 안전하도록 방어적 처리
    void OnDisable()
    {
        // 이벤트 해제
        if (_doorInteractor != null)
            _doorInteractor.OnInteracted -= OnDoorInteracted;
        _doorOpenCoroutine = null;

        ResetForReuse();
        // 매니저에서 해제
        //if (_interactableManager != null)
        //    _interactableManager.Unregister(_doorInteractor);

        // 선택적으로 중계 이벤트 초기화(구독자들이 수동 해제를 더 좋아하면 제거 가능)
        // DoorInteracted = null;
    }

    /// <summary>
    /// 풀링용 초기화 해제: 재사용 시 반드시 호출해야 함.
    /// - 구독 해제, 등록 해제, 뷰 초기화, 내부 RoomInfo null화
    /// </summary>
    public void ResetForReuse()
    {
        if (_doorInteractor != null)
            _doorInteractor.OnInteracted -= OnDoorInteracted;

        //if (_interactableManager != null)
        //    _interactableManager.Unregister(_doorInteractor);

        //_doorView?.ClearPreview();
        _roomInfo = null;
        //_isInitialized = false;
    }

    /// <summary>
    /// 방에서 보상 선택이 완료되었을 때 호출하여
    /// 상호작용을 다시 활성화하고 보상을 뷰에 표시합니다.
    /// </summary>
    public void OnRewardSelectionCompleted()
    {
        // 상호작용 활성화
        _doorInteractor.ToggleInteraction(true);
        // 뷰에서 보상 보여주기
        _doorView.ToggleReward(true);
    }
}