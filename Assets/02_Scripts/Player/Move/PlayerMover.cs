using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController _controller;

    [Header("Movement")]
    [SerializeField] float _moveSpeed;
    [SerializeField] float _rotSpeed = 720f;

    [Header("Gravity")]
    [SerializeField] float _gravity = -9.81f;
    [SerializeField] float _terminalVelocity = -50f;
    [SerializeField] float _groundStickVelocity = -2f; // grounded일 때 약간 아래로 유지

    [Header("Ground Check")]
    [SerializeField] LayerMask _groundLayer = ~0;
    //[SerializeField] float _groundCheckDistance = 0.2f; // 캐릭터 바닥에서 얼마나 밑을 체크할지
    [SerializeField] float _groundCheckRadius = 0.3f; // CheckSphere 반지름 (캐릭터 크기에 맞게 조정)

    [Header("Coyote / Snap")]
    [SerializeField] float _coyoteTime = 0.12f; // 바닥 사라져도 잠깐은 떨어지지 않게
    float _coyoteTimer;

    Player _player;
    Vector3 _horizontalVelocity; // x,z 이동 속도 (월드스페이스)
    float _verticalVelocity;
    Quaternion _targetRotation;

    void Awake()
    {
        if (_controller == null)
            _controller = GetComponent<CharacterController>();
        _targetRotation = transform.rotation;
        
    }

    void Update()
    {
        // 1) Ground 체크 (CharacterController.isGrounded 보완)
        bool isGrounded = false;
        if (_controller != null)
        {
            // CharacterController.isGrounded 는 작은 엣지 케이스가 있으므로 보조 레이/스피어 체크 사용
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.CheckSphere(origin + Vector3.down * (_controller.height * 0.5f - _controller.radius + 0.01f),
                                             _groundCheckRadius, _groundLayer, QueryTriggerInteraction.Ignore)
                         || _controller.isGrounded;
        }

        // 2) 코요테 타임 처리
        if (isGrounded)
            _coyoteTimer = _coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;

        bool consideredGrounded = _coyoteTimer > 0f;

        // 3) 수직 속도 관리
        if (consideredGrounded)
        {
            // 바닥에 붙게끔 음수 작은 값 유지 (바닥에서 떨어지지 않도록)
            if (_verticalVelocity < 0f)
                _verticalVelocity = _groundStickVelocity;
        }
        else
        {
            // 중력 적용
            _verticalVelocity += _gravity * Time.deltaTime;
            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity = _terminalVelocity;
        }

        // 4) 총 이동 벡터 계산 및 실제 이동
        Vector3 move = new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z) * Time.deltaTime;
        if (_controller != null)
        {
            if (_player.CanGetInput == false) return;
            _controller.Move(move);
        }
        else
        {
            transform.position += move;
        }

        // 5) 회전 보간
        transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _rotSpeed * Time.deltaTime);
    }

    public void Initialize(Player player)
    {
        _player = player;

        GameManager.Instance.PlayerStatManager.OnPlayerMoveSpeedChange += SetMoveSpeed;
        _moveSpeed = GameManager.Instance.PlayerStatManager.playerMoveSpeed;
    }

    public void Rotate(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.magnitude < 0.01f)
            return;
        _targetRotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// 외부에서 호출: 카메라 기준으로 변환된 world-space direction을 전달하세요.
    /// </summary>
    public void Move(Vector3 direction)
    {
        // direction은 world-space(평면)라고 가정. y는 무시.
        direction.y = 0f;

        if (direction.magnitude < 0.01f)
        {
            _horizontalVelocity = Vector3.zero;
            return;
        }

        _horizontalVelocity = direction.normalized * _moveSpeed;
        
        _targetRotation = Quaternion.LookRotation(direction);
    }

    // (디버그용) 바닥 체크 시각화
    void OnDrawGizmosSelected()
    {
        if (_controller == null) return;
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 spherePos = origin + Vector3.down * (_controller.height * 0.5f - _controller.radius + 0.01f);
        Gizmos.DrawWireSphere(spherePos, _groundCheckRadius);
    }

    public void SetMoveSpeed(float s) => _moveSpeed = s;
}