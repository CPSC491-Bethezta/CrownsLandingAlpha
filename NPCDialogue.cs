using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueData dialogue;

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}