using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHealth;
    public int currentHealth;

    float h; 
    float v;


    void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        transform.Translate(h * Time.deltaTime * 5, 0, v * Time.deltaTime * 5);
    }

    public void TakeDamage(float damage, Transform attacker = null)
    {
        currentHealth -= (int)damage; 
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
