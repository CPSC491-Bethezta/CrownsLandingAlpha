using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private GameObject sourceNPC;
    private int currentNodeIndex;
    private bool isOpen;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>Opens a dialogue, tracking the source NPC for reward components.</summary>
    public void StartDialogue(DialogueData dialogue, GameObject npc = null)
    {
        currentDialogue = dialogue;
        sourceNPC = npc;
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
        sourceNPC = null;
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

        // Grant rewards from the destination node when the player navigates to it
        GrantNodeRewards(currentDialogue.nodes[currentNodeIndex]);

        ShowNode();
    }

    /// <summary>Displays the current dialogue node without granting any rewards.</summary>
    private void ShowNode()
    {
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        dialogueUI.DisplayNode(node);
    }

    /// <summary>Triggers QuestGiver / ItemGiver on the source NPC if the node flags are set.</summary>
    private void GrantNodeRewards(DialogueNode node)
    {
        if (sourceNPC == null) return;

        if (node.giveQuest)
        {
            var questGiver = sourceNPC.GetComponent<QuestGiver>();
            if (questGiver != null)
                questGiver.Interact();
            else
                Debug.LogWarning("[DialogueManager] giveQuest is true but no QuestGiver found on NPC.", sourceNPC);
        }

        if (node.giveItem)
        {
            var itemGiver = sourceNPC.GetComponent<ItemGiver>();
            if (itemGiver != null)
                itemGiver.GiveItem();
            else
                Debug.LogWarning("[DialogueManager] giveItem is true but no ItemGiver found on NPC.", sourceNPC);
        }
    }
}
