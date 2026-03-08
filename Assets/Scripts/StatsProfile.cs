using UnityEngine;
using System;

// A profile to be added to any given actor.
public class StatsProfile : MonoBehaviour, IDamageable
{
    [SerializeField] private DamagePopup damagePopupPrefab;

    // RESOURCES //
    [SerializeField] private int maxHealth, maxMana, maxStamina;
    private int currentHealth, currentMana, currentStamina;
    private bool hasDied;
    public bool IsDead => currentHealth <= 0;

    // EVENTS //
    public event Action OnResourceChanged;
    public event Action<float> OnDamaged;
    public event Action OnDied;

    private void Awake()
    {
        hasDied = false;
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
    }

    // RESOURCE REDUCTION //
    public void TakeDamage(float amount)
    {
        if (IsDead)
        {
            return;
        }

        // Popup
        if (damagePopupPrefab != null)
        {
            DamagePopup popup = Instantiate(
                damagePopupPrefab,
                transform.position + Vector3.up,
                Quaternion.identity
            );
            popup.Setup(amount);
        }

        currentHealth -= (int)amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Damage Taken. Current Health: " + currentHealth);
        OnResourceChanged?.Invoke();

        if (!IsDead)
        {
            OnDamaged?.Invoke(amount);
        }

        CheckDeath();
    }

    public void ReduceMana(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        Debug.Log("Mana Reduced. Current Mana: " + currentMana);
        OnResourceChanged?.Invoke();
    }

    public void ReduceStamina(int amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        Debug.Log("Stamina Reduced. Current Stamina: " + currentStamina);
        OnResourceChanged?.Invoke();
    }

    // RESOURCE INCREASE //
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Healed Current. Health: " + currentHealth);
        OnResourceChanged?.Invoke();
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        Debug.Log("Mana Restored Current Mana: " + currentMana);
        OnResourceChanged?.Invoke();
    }

    public void RestoreStamina(int amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        Debug.Log("Stamina Restored Current Stamina: " + currentStamina);
        OnResourceChanged?.Invoke();
    }

    public void CheckDeath()
    {
        if (!IsDead)
        {
            return;
        }

        currentHealth = 0;
        Die();
    }

    public void Die()
    {
        if (!IsDead || hasDied)
        {
            return;
        }

        hasDied = true;
        Debug.Log("Death.");
        OnDied?.Invoke();
    }
}
