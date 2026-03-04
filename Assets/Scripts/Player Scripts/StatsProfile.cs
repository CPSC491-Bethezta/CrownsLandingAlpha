using UnityEngine;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

// A profile to be added to any given actor.
public class StatsProfile : MonoBehaviour, IDamageable
{

    [SerializeField] private DamagePopup damagePopupPrefab;

    // RESOURCES //
    [SerializeField] private int maxHealth, maxMana, maxStamina;
    private int currentHealth, currentMana, currentStamina;
    public bool IsDead => currentHealth <= 0;
    private int level;

    // EVENTS //
    public event Action OnResourceChanged;


    void Awake()
    {
        SetAllResource();
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        //TestStats();
    }


    // RESOURCE REDUCTION //

    public void TakeDamage(float amount)
    {
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
        CheckDeath();
    }

    public void ReduceMana (int amount)
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
        if (!IsDead) {  return; }
        currentHealth = 0;
        Die();
    }

    public void Die()
    {
        Debug.Log("Death.");
    }
    // Simply forces every resource to be between 0 and and its Max
    private void SetAllResource()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}
