using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private int currentNodeIndex;
    private bool isOpen;

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentNodeIndex = 0;
        isOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        dialogueUI.gameObject.SetActive(true);
        ShowNode();
    }

    /// <summary>Closes the dialogue panel and restores game state.</summary>
    public void EndDialogue()
    {
        isOpen = false;
        currentDialogue = null;
        dialogueUI.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ChooseOption(int choiceIndex)
    {
        DialogueChoice choice = currentDialogue.nodes[currentNodeIndex].choices[choiceIndex];

        // nextNode == -1 is the sentinel value for "end conversation"
        if (choice.nextNode < 0)
        {
            EndDialogue();
            return;
        }

        currentNodeIndex = choice.nextNode;
        ShowNode();
    }

    private void ShowNode()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        dialogueUI.DisplayNode(node);
    }
}
