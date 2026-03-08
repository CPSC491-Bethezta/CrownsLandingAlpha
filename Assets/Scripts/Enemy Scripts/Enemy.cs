using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    private StatsProfile statsProfile;

    private void Awake()
    {
        statsProfile = GetComponent<StatsProfile>();

        if (statsProfile == null)
        {
            statsProfile = GetComponentInParent<StatsProfile>();
        }

        if (statsProfile == null)
        {
            Debug.LogWarning($"Enemy script on {name} but no StatsProfile found in parent hierarchy.");
        }
    }

    public void TakeDamage(float amount)
    {
        if (statsProfile == null)
        {
            return;
        }

        statsProfile.TakeDamage(amount);
    }
}
