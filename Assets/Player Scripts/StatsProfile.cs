using UnityEngine;

public class StatsProfile : MonoBehaviour
{

    // RESOURCES //
    public int maxHealth, maxMana, maxStamina;
    public int currentHealth, currentMana, currentStamina;

    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        ClampAllResource();
    }


    // RESOURCE REDUCTION //

    public void TakeDamage(int amount)
    {

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Damage Taken. Current Health: " + currentHealth);

        CheckDeath();
    }

    public void ReduceMana (int amount)
    {

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        Debug.Log("Mana Reduced. Current Mana: " + currentMana);
    }

    public void ReduceStamina(int amount)
    {

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        Debug.Log("Stamina Reduced. Current Stamina: " + currentStamina);
    }

    // RESOURCE INCREASE //
    public void Heal(int amount)
    {

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Healed Current. Health: " + currentHealth);
    }

    public void RestoreMana(int amount)
    {

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        Debug.Log("Mana Restored Current Mana: " + currentMana);
    }

    public void RestoreStamina(int amount)
    {

        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        Debug.Log("Stamina Restored Current Stamina: " + currentStamina);
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
    private void ClampAllResource()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}
