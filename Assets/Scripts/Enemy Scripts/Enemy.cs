using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    private StatsProfile statsProfile;
    [SerializeField] private ObjectiveType enemyObjectiveType = ObjectiveType.KillEnemy;

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

    private void OnEnable()
    {
        if (statsProfile != null) statsProfile.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (statsProfile != null) statsProfile.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.UpdateObjective(enemyObjectiveType);
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
