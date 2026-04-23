using TMPro;
using UnityEngine;

public class PickupPromptUI : MonoBehaviour
{
    public static PickupPromptUI Instance;

    [SerializeField] private TMP_Text label;

    private string defaultText;

    private void Awake()
    {
        Instance = this;

        if (label == null)
            label = GetComponentInChildren<TMP_Text>();

        defaultText = label != null ? label.text : string.Empty;

        gameObject.SetActive(false);
    }

    /// <summary>Shows the prompt with its default text.</summary>
    public void Show()
    {
        if (label != null)
            label.text = defaultText;
        gameObject.SetActive(true);
    }

    /// <summary>Shows the prompt with a custom message.</summary>
    public void Show(string message)
    {
        if (label != null)
            label.text = message;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
