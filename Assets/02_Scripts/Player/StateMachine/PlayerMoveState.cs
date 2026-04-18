public class PlayerMoveState : PlayerStateBase
{

    public PlayerMoveState(Player player) : base(player)
    {
    }

    public override void Enter()
    {

    }

    public override void Update()
    {
        _player.Move(_player.MoveInput);
    }

    public override void Exit()
    {
        _player.StopMove();
    }
}
