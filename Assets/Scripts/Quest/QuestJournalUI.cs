using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private Transform questListContainer;
    [SerializeField] private GameObject questButtonPrefab;
    [SerializeField] private TMP_Text descriptionText;

    private static readonly Color ColorDefault  = new Color(1f,    1f,    1f,    1f);
    private static readonly Color ColorSelected = new Color(1f,    0.85f, 0f,    1f);

    private QuestDefinition selectedQuest;
    private readonly List<(QuestDefinition quest, Image bg)> buttonEntries = new();

    private void OnEnable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted   += HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted   -= HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
    }

    /// <summary>Called by GameMenuController when the quest panel opens.</summary>
    public void Show()
    {
        if (journalPanel != null)
            journalPanel.SetActive(true);
        RefreshList();
    }

    /// <summary>Called by GameMenuController when the quest panel closes.</summary>
    public void Hide()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
        ClearList();
    }

    private void HandleQuestStarted(QuestDefinition _)  => RefreshList();
    private void HandleQuestCompleted(QuestDefinition _) => RefreshList();

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

            Image bg = entry.GetComponent<Image>();
            buttonEntries.Add((quest, bg));

            // Capture for lambda
            QuestDefinition captured = quest;
            Button btn = entry.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectQuest(captured));
        }

        // Re-select the previously selected quest if it's still active, otherwise clear
        if (selectedQuest != null && QuestManager.Instance.IsQuestActive(selectedQuest))
            SelectQuest(selectedQuest);
        else
            SelectQuest(active.Count > 0 ? active[0] : null);
    }

    private void SelectQuest(QuestDefinition quest)
    {
        selectedQuest = quest;

        // Update button highlight colours
        foreach (var (q, bg) in buttonEntries)
        {
            if (bg != null)
                bg.color = (q == selectedQuest) ? ColorSelected : ColorDefault;
        }

        // Update description panel
        if (descriptionText != null)
            descriptionText.text = selectedQuest != null ? selectedQuest.description : string.Empty;
    }

    private void ClearList()
    {
        buttonEntries.Clear();
        if (questListContainer == null) return;
        foreach (Transform child in questListContainer)
            Destroy(child.gameObject);
    }
}
