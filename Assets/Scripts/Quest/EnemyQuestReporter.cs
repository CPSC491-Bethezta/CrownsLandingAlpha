using UnityEngine;

// Add this component to any enemy prefab to report kills to the QuestManager.
// Requires a StatsProfile on the same GameObject or parent.
public class EnemyQuestReporter : MonoBehaviour
{
    [SerializeField] private ObjectiveType objectiveType = ObjectiveType.KillEnemy;
    private StatsProfile statsProfile;

    private void Awake()
    {
        statsProfile = GetComponent<StatsProfile>();
        if (statsProfile == null)
            statsProfile = GetComponentInParent<StatsProfile>();
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
        QuestManager.Instance.UpdateObjective(objectiveType);
    }
}
