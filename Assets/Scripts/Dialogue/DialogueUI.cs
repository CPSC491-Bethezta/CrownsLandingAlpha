using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private TMP_Text npcText;
    [SerializeField] private Button[] choiceButtons;

    public void DisplayNode(DialogueNode node)
    {
        if (npcText == null)
        {
            Debug.LogWarning("DialogueUI has no npcText assigned.");
            return;
        }

        if (choiceButtons == null || choiceButtons.Length == 0)
        {
            Debug.LogWarning("DialogueUI has no choiceButtons assigned.");
            return;
        }

        npcText.text = node.npcText;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (node.choices != null && i < node.choices.Length)
            {
                Button button = choiceButtons[i];

                if (button == null)
                {
                    continue;
                }

                button.gameObject.SetActive(true);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = node.choices[i].playerText;
                }

                int index = i;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.ChooseOption(index);
                    }
                    else
                    {
                        Debug.LogWarning("DialogueUI tried to call DialogueManager.Instance, but no instance exists.");
                    }
                });
            }
            else
            {
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }
}