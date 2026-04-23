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
    /// <summary>Optional quest to start when this node is reached via a player choice.</summary>
    public QuestDefinition questToGive;
    /// <summary>Optional item to add to the player's inventory when this node is reached.</summary>
    public InventoryItem itemToGive;
}