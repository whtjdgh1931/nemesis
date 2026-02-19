using UnityEngine;

/// <summary>
/// 공격 시 전달되는 컨텍스트 (확장 가능)
/// </summary>
public class AttackContext
{
    public GameObject Source;
    public Transform Origin;
    public Vector3 Direction;
    public float Damage;
    public float Radius;

    public AttackContext(GameObject source)
    {
        Source = source;
    }
}