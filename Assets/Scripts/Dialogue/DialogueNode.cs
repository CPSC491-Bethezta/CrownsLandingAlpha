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
}