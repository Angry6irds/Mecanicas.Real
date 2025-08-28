using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public float maxHealth = 50f;
    public System.Action OnDeath;
    float hp;

    void Awake() { hp = maxHealth; }

    public void ApplyDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0f)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}