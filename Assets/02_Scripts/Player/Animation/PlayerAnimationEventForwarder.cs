using UnityEngine;

public class PlayerAnimationEventForwarder : MonoBehaviour
{
    Player _player;

    public void Initialize(Player player)
    {
        _player = player;
    }

    // 애니메이션 이벤트에서 이 메서드를 호출하도록 설정
    public void OnAttackFireEvent()
    {
        if (_player == null) return;
        _player.OnAttackFireEvent(); // Player 쪽에 해당 public 메서드가 있어야 함
        //Debug.Log("OnAttackFireEvent forwarded to Player.");
    }

    public void OnAttackHitEvent()
    {
        _player.OnAttackHitEvent();
        //Debug.Log("OnAttackHitEvent forwarded to Player.");
    }

    public void OnAttackEndEvent()
    {
        if (_player == null) return;
        _player.OnAttackEndEvent();
        //Debug.Log("OnAttackEndEvent forwarded to Player.");
    }

    public void OnInvincibleStartEvent()
    {
        if (_player == null) return;
        _player.OnInvincibleStartEvent();
    }

    public void OnInvincibleEndEvent()
    {
        if (_player == null) return;
        _player.OnInvincibleEndEvent();
    }
}