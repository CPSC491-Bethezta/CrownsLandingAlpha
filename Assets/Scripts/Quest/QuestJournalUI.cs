using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown questDropdown;

    private void OnEnable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        RefreshDropdown();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
    }

    private void HandleQuestStarted(QuestDefinition _) => RefreshDropdown();
    private void HandleQuestCompleted(QuestDefinition _) => RefreshDropdown();

    private void RefreshDropdown()
    {
        if (questDropdown == null) return;

        IReadOnlyList<QuestDefinition> active = QuestManager.Instance.GetActiveQuests();

        questDropdown.ClearOptions();

        var options = new List<string>();
        foreach (var quest in active)
            options.Add(quest.questName);

        questDropdown.AddOptions(options);
    }
}
