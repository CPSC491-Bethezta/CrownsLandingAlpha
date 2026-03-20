using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class QuestLogUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questLogText;

    private void OnEnable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        QuestManager.Instance.OnObjectiveUpdated += HandleObjectiveUpdated;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        QuestManager.Instance.OnObjectiveUpdated -= HandleObjectiveUpdated;
    }

    private void HandleQuestStarted(QuestDefinition _) => RefreshUI();
    private void HandleQuestCompleted(QuestDefinition _) => RefreshUI();
    private void HandleObjectiveUpdated(QuestDefinition _, QuestObjective __) => RefreshUI();

    private void RefreshUI()
    {
        if (questLogText == null) return;

        var sb = new StringBuilder();
        IReadOnlyList<QuestDefinition> active = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in active)
        {
            sb.AppendLine($"<b>{quest.questName}</b>");
            foreach (var obj in quest.objectives)
            {
                string check = obj.IsCompleted ? "[X]" : "[ ]";
                sb.AppendLine($"  {check} {obj.description} ({obj.currentCount}/{obj.targetCount})");
            }
        }

        questLogText.text = sb.ToString();
    }
}
