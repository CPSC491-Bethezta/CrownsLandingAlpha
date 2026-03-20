using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    public string questName;
    [TextArea] public string description;
    public List<QuestObjective> objectives;
    public int rewardXP;
    public int rewardGold;
    public string rewardDescription;
}
