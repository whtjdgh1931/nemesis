using UnityEngine;

public class GrenadeDamageArea : AreaDamageBase
{
    [SerializeField]
    private int _grenadeDamage;
    public void SetDamage(int damage)
    {
        _grenadeDamage = damage;
    }


    public void Start()
    {
        CheckTarget();
    }


    public override void ActiveSkill(Transform target)
    {
        target.GetComponent<IDamageable>().TakeDamage(_grenadeDamage, null);
    }

  
}
