using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private int currentNodeIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("DialogueManager.StartDialogue called with null dialogue.");
            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueManager has no DialogueUI assigned.");
            return;
        }

        if (dialogue.nodes == null || dialogue.nodes.Length == 0)
        {
            Debug.LogWarning("DialogueManager.StartDialogue called with dialogue that has no nodes.");
            return;
        }

        currentDialogue = dialogue;
        currentNodeIndex = 0;

        ShowNode();
    }

    private void ShowNode()
    {
        if (currentDialogue == null)
        {
            Debug.LogWarning("DialogueManager.ShowNode called with no active dialogue.");
            return;
        }

        if (currentNodeIndex < 0 || currentNodeIndex >= currentDialogue.nodes.Length)
        {
            Debug.LogWarning($"DialogueManager.ShowNode index {currentNodeIndex} is out of range.");
            return;
        }

        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        dialogueUI.DisplayNode(node);
    }

    public void ChooseOption(int choiceIndex)
    {
        if (currentDialogue == null)
        {
            Debug.LogWarning("DialogueManager.ChooseOption called with no active dialogue.");
            return;
        }

        if (currentNodeIndex < 0 || currentNodeIndex >= currentDialogue.nodes.Length)
        {
            Debug.LogWarning($"DialogueManager.ChooseOption index {currentNodeIndex} is out of range.");
            return;
        }

        DialogueNode node = currentDialogue.nodes[currentNodeIndex];

        if (node.choices == null || choiceIndex < 0 || choiceIndex >= node.choices.Length)
        {
            Debug.LogWarning($"DialogueManager.ChooseOption choiceIndex {choiceIndex} is invalid.");
            return;
        }

        DialogueChoice choice = node.choices[choiceIndex];

        if (choice.nextNode < 0 || choice.nextNode >= currentDialogue.nodes.Length)
        {
            Debug.LogWarning($"DialogueManager.ChooseOption nextNode {choice.nextNode} is out of range.");
            return;
        }

        currentNodeIndex = choice.nextNode;
        ShowNode();
    }
}
