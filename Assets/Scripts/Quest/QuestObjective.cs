using System;
using UnityEngine;

public enum ObjectiveType { KillEnemy, CollectItem, ReachLocation, TalkToNPC }

[Serializable]
public class QuestObjective
{
    public string description;
    public ObjectiveType objectiveType;
    public int targetCount;
    [HideInInspector] public int currentCount;

    public bool IsCompleted => currentCount >= targetCount;
}
