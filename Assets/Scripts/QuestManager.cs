using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get { return s_Instance; } }
    private static QuestManager s_Instance;

    [SerializeField] private List<QuestDefinition> availableQuests;
    [SerializeField] private List<QuestDefinition> starterQuests;
    private List<QuestDefinition> activeQuests = new List<QuestDefinition>();
    private List<QuestDefinition> completedQuests = new List<QuestDefinition>();

    public event Action<QuestDefinition> OnQuestStarted;
    public event Action<QuestDefinition> OnQuestCompleted;
    public event Action<QuestDefinition, QuestObjective> OnObjectiveUpdated;

    private void Awake()
    {
        s_Instance = this;

        // Reset runtime state on ScriptableObject objectives (editor play-mode safety)
        foreach (var quest in availableQuests)
            foreach (var obj in quest.objectives)
                obj.currentCount = 0;
    }

    private void Start()
    {
        // Deferred to Start so all listeners (QuestNotificationUI, QuestLogUI, etc.)
        // have subscribed during Awake/OnEnable before events fire.
        foreach (var quest in starterQuests)
            StartQuest(quest);
    }

    public bool StartQuest(QuestDefinition quest)
    {
        if (quest == null || IsQuestActive(quest) || IsQuestCompleted(quest)) return false;
        activeQuests.Add(quest);
        OnQuestStarted?.Invoke(quest);

        Debug.Log("Quest Began: " + quest.questName);
        return true;
    }

    public bool IsQuestActive(QuestDefinition quest) => activeQuests.Contains(quest);
    public bool IsQuestCompleted(QuestDefinition quest) => completedQuests.Contains(quest);
    public IReadOnlyList<QuestDefinition> GetActiveQuests() => activeQuests;

    public void UpdateObjective(ObjectiveType type, int amount = 1)
    {
        foreach (var quest in activeQuests.ToList())
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.objectiveType == type && !obj.IsCompleted)
                {
                    obj.currentCount += amount;
                    OnObjectiveUpdated?.Invoke(quest, obj);
                    CheckQuestCompletion(quest);
                    break;
                }
            }
        }
    }

    private void CheckQuestCompletion(QuestDefinition quest)
    {
        foreach (var obj in quest.objectives)
            if (!obj.IsCompleted) return;
        CompleteQuest(quest);
    }

    private void CompleteQuest(QuestDefinition quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"[QuestManager] Quest completed: {quest.questName}");
    }
}
