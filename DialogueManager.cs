using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private int currentNodeIndex;

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentNodeIndex = 0;

        ShowNode();
    }

    void ShowNode()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        dialogueUI.DisplayNode(node);
    }

    public void ChooseOption(int choiceIndex)
    {
        DialogueChoice choice = currentDialogue.nodes[currentNodeIndex].choices[choiceIndex];
        currentNodeIndex = choice.nextNode;

        ShowNode();
    }
}
