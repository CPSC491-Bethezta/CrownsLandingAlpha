using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text npcText;
    public Button[] choiceButtons;

    public void DisplayNode(DialogueNode node)
    {
        npcText.text = node.npcText;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < node.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);

                choiceButtons[i].GetComponentInChildren<TMP_Text>().text =
                    node.choices[i].playerText;

                int index = i;

                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() =>
                {
                    DialogueManager.Instance.ChooseOption(index);
                });
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
}