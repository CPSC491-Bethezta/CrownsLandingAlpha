using UnityEngine;
using TMPro;
using System.Collections;

public class LootPopupUI : MonoBehaviour
{
    public static LootPopupUI Instance;

    [SerializeField] private TextMeshProUGUI lootText;
    [SerializeField] private float floatDistance = 60f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Vector2 startPosition = new Vector2(120f, 0f);

    private RectTransform rectTransform;
    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        rectTransform = lootText.GetComponent<RectTransform>();

        if (lootText != null)
            lootText.gameObject.SetActive(false);
    }

    public void ShowLoot(string itemName)
    {
        if (lootText == null)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AnimateLootText(itemName));
    }

    private IEnumerator AnimateLootText(string itemName)
    {
        lootText.gameObject.SetActive(true);
        lootText.text = "+1 " + itemName;

        Color color = lootText.color;
        color.a = 1f;
        lootText.color = color;

        Vector2 basePos = startPosition;
        rectTransform.anchoredPosition = basePos;

        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            rectTransform.anchoredPosition = basePos + new Vector2(0f, floatDistance * t);

            Color newColor = lootText.color;
            newColor.a = 1f - t;
            lootText.color = newColor;

            yield return null;
        }

        lootText.gameObject.SetActive(false);
        currentRoutine = null;
    }
}