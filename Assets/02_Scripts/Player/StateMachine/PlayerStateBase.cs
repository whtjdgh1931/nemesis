using UnityEngine;

public abstract class PlayerStateBase : IState
{
    protected Player _player;

    public PlayerStateBase(Player player)
    {
        _player = player;
    }

    public virtual void Enter()
    {
        
    }

    public virtual void Exit()
    {
        
    }

    public virtual void Update()
    {
        
    }
}
