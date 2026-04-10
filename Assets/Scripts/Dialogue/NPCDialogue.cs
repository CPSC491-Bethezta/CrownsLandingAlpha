using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    public void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("NPCDialogue.Interact called, but no DialogueManager.Instance exists in the scene.");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning($"NPCDialogue on '{gameObject.name}' has no DialogueData assigned.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
    }
}