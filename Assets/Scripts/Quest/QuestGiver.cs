using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] private QuestDefinition quest;
    public bool retrieveOnContact;

    public void Interact()
    {
        if (quest == null || QuestManager.Instance == null) return;

        if (QuestManager.Instance.IsQuestCompleted(quest))
        {
            Debug.Log($"[QuestGiver] {quest.questName} already completed.");
            return;
        }

        if (QuestManager.Instance.IsQuestActive(quest))
        {
            Debug.Log($"[QuestGiver] {quest.questName} already active.");
            return;
        }

        QuestManager.Instance.StartQuest(quest);
    }

    
}
