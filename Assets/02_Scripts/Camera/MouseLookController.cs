using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookController : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;        // Yaw를 적용할 오브젝트 (플레이어 바디)
    public Transform cameraPivot;       // Pitch(상하)를 적용할 카메라 피벗(카메라의 부모)

    [Header("Input")]
    public InputActionReference lookAction; // Inspector에 생성한 InputActions -> Look 액션 드래그

    [Header("Settings")]
    public float sensitivity = 1.5f;    // 기본 감도 (인게임에 UI로 노출)
    public float smoothing = 0.03f;     // 0 = 즉시, 값이 클수록 부드러움(지연 발생)
    public bool invertY = false;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    Vector2 smoothVelocity;
    Vector2 currentLook; // 스무딩된 델타
    float yaw;   // 좌우 누적
    float pitch; // 상하 누적

    void OnEnable()
    {
        if (lookAction != null && lookAction.action != null)
            lookAction.action.Enable();
    }

    void OnDisable()
    {
        if (lookAction != null && lookAction.action != null)
            lookAction.action.Disable();
    }

    void Start()
    {
        // 초기값 동기화
        yaw = playerBody ? playerBody.localEulerAngles.y : 0f;
        pitch = cameraPivot ? cameraPivot.localEulerAngles.x : 0f;
        // EulerAngles는 0..360이므로 -180..180으로 변환
        if (pitch > 180f) pitch -= 360f;

        // 기본 게임 시작 시 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (lookAction == null || lookAction.action == null) return;

        // 1) 입력 읽기 (마우스 delta)
        Vector2 rawDelta = lookAction.action.ReadValue<Vector2>(); // 보통 픽셀 단위 (프레임당)

        // 2) 감도/반전 적용
        float invert = invertY ? 1f : -1f; // Y 입력을 보정 (마우스 위로 이동하면 pitch 감소 보통)
        Vector2 target = new Vector2(rawDelta.x, rawDelta.y * invert) * sensitivity;

        // 3) 스무딩 (SmoothDamp)
        currentLook = Vector2.SmoothDamp(currentLook, target, ref smoothVelocity, smoothing);

        // 4) 누적 각도 (yaw: 좌우, pitch: 상하)
        yaw += currentLook.x;
        pitch += currentLook.y;

        // 5) pitch 클램프
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 6) 적용: 바디(Yaw)와 카메라 피벗(Pitch)
        if (playerBody != null)
            playerBody.localRotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // 게임 중에 마우스 잠금 토글 필요 시 호출
    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}