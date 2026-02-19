public enum PlayerStateType
{
    Idle,
    Move,
    Dash,
    NormalAttack,
    GrenadeAttack,
    SpecialAttack,
    Stun,
    Dead
}

/// <summary>
/// 상태 머신에서 사용하는 상태 인터페이스
/// </summary>
public interface IState
{
    void Enter();
    void Exit();
    void Update();
}