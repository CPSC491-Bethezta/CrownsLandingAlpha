using System;

[Serializable]
public class DialogueChoice
{
    public string playerText;
    public int nextNode;
}

[Serializable]
public class DialogueNode
{
    public string npcText;
    public DialogueChoice[] choices;

    /// <summary>If true, triggers QuestGiver.Interact() on the source NPC when this node is reached.</summary>
    public bool giveQuest;

    /// <summary>If true, triggers ItemGiver.GiveItem() on the source NPC when this node is reached.</summary>
    public bool giveItem;
}
