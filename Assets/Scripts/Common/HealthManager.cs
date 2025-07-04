using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    private bool isInitialized = false;

    public void Initialize(int templateMaxHealth)
    {
        maxHealth = templateMaxHealth;
        currentHealth = maxHealth;
        isInitialized = true;
        Debug.Log($"[{gameObject.name}] HealthManager.Initialize(): Initialized with {maxHealth} health from template.");
    }

    void Start()
    {
        // Fallback initialization if Initialize() wasn't called
        if (!isInitialized && currentHealth == 0)
        {
            currentHealth = maxHealth;
            Debug.LogWarning($"[{gameObject.name}] HealthManager.Start(): Wasn't initialized from template, using default maxHealth: {maxHealth}.");
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        onDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            onDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}