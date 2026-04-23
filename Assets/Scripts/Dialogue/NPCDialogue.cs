using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to an NPC. Shows the shared PickupPromptUI when the player enters the
/// trigger radius and starts dialogue when the player presses E.
/// Requires a Collider with isTrigger = true on the same GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] public DialogueData dialogue;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptMessage = "Press E to talk";

    [Header("Quest")]
    [Tooltip("Quest to give the player when this NPC is talked to. Leave empty for no quest.")]
    [SerializeField] private QuestDefinition questToGive;

    private bool playerInRange;

    private void Start()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
            Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        PickupPromptUI.Instance?.Show(promptMessage);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        PickupPromptUI.Instance?.Hide();
    }

    /// <summary>Starts the assigned dialogue. Can also be called directly.</summary>
    public void Interact()
    {
        if (dialogue == null || DialogueManager.Instance == null) return;
        playerInRange = false;
        PickupPromptUI.Instance?.Hide();
        QuestManager.Instance?.UpdateObjective(ObjectiveType.TalkToNPC);

        if (questToGive != null)
            QuestManager.Instance?.StartQuest(questToGive);

        DialogueManager.Instance.StartDialogue(dialogue);
    }
}
