using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHp = 1000f;
    public float CurrentHp { get; private set; }

    private bool isDead;

    private void Awake()
    {
        CurrentHp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        CurrentHp -= amount;
        Debug.Log($"Player took {amount} damage. HP now: {CurrentHp}/{maxHp}");

        if (CurrentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // later: play death anim, disable movement/combat, respawn, etc.
        // GetComponent<PlayerMovement>()?.enabled = false;
        // GetComponent<PlayerCombat>()?.enabled = false;
    }
}