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

    /// <summary>Starts the assigned dialogue, passing this NPC as the source.</summary>
    public void Interact()
    {
        if (dialogue == null || DialogueManager.Instance == null) return;
        playerInRange = false;
        PickupPromptUI.Instance?.Hide();
        QuestManager.Instance?.UpdateObjective(ObjectiveType.TalkToNPC);

        DialogueManager.Instance.StartDialogue(dialogue, gameObject);
    }
}
