using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private Transform questListContainer;
    [SerializeField] private GameObject questButtonPrefab;

    private void OnEnable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
    }

    /// <summary>Called by InventoryUI when the QuestPanel becomes active.</summary>
    public void Show()
    {
        if (journalPanel != null)
            journalPanel.SetActive(true);
        RefreshList();
    }

    /// <summary>Called by InventoryUI when the QuestPanel is hidden.</summary>
    public void Hide()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
        ClearList();
    }

    private void HandleQuestStarted(QuestDefinition _) { RefreshList(); }
    private void HandleQuestCompleted(QuestDefinition _) { RefreshList(); }

    private void RefreshList()
    {
        ClearList();

        if (questListContainer == null || questButtonPrefab == null) return;

        IReadOnlyList<QuestDefinition> active = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in active)
        {
            GameObject entry = Instantiate(questButtonPrefab, questListContainer);
            TMP_Text label = entry.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = quest.questName;
        }
    }

    private void ClearList()
    {
        if (questListContainer == null) return;
        foreach (Transform child in questListContainer)
            Destroy(child.gameObject);
    }
}
