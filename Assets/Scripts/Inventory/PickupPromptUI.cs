using UnityEngine;

public class PickupPromptUI : MonoBehaviour
{
    public static PickupPromptUI Instance;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
