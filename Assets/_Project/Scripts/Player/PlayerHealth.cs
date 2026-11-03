using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)] private float maxHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth <= 0f;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if(IsDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        Debug.Log($"[PLAYER HEALTH] Health: {CurrentHealth:F1}/{MaxHealth}");

        if(IsDead)
        {
            // Die();
        }
    }

    public void Heal(float amount)
    {
        if(IsDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Min(CurrentHealth + amount, 100f);

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        Debug.Log($"[PLAYER HEALTH] Health: {CurrentHealth:F1}/{MaxHealth}");

    }

    private void Die()
    {
        Debug.Log("[PLAYER HEALTH] Player died!");

        Died?.Invoke();
    }

    [ContextMenu("Test Take Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10f);
    }
}
