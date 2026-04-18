using UnityEngine;

public class DamageToSelf : MonoBehaviour
{
    [SerializeField]
    IDamageable dmg;

    private void Start()
    {
        dmg = GetComponent<IDamageable>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            dmg.TakeDamage(1000f, transform);
        }
    }
}
