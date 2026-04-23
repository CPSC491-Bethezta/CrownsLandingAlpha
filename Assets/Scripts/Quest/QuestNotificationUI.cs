using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays a brief top-center popup whenever a quest is started or completed.
/// Attach to the QuestNotification GameObject and assign the CanvasGroup and label references.
/// </summary>
public class QuestNotificationUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration  = 0.25f;
    [SerializeField] private float holdDuration    = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestNotificationUI] QuestManager.Instance is null in OnEnable — subscription deferred to Start.");
            return;
        }
        Subscribe();
    }

    private void Start()
    {
        // Fallback if OnEnable ran before QuestManager.Awake set the singleton
        Subscribe();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted   -= HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
    }

    private bool isSubscribed;

    private void Subscribe()
    {
        if (isSubscribed || QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestStarted   += HandleQuestStarted;
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        isSubscribed = true;
        Debug.Log("[QuestNotificationUI] Subscribed to QuestManager events.");
    }

    private void HandleQuestStarted(QuestDefinition quest)   => Show($"Quest Started: {quest.questName}");
    private void HandleQuestCompleted(QuestDefinition quest) => Show($"Quest Complete! {quest.questName}");

    /// <summary>Displays an arbitrary message in the popup.</summary>
    public void Show(string message)
    {
        messageText.text = message;
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // Fade in
        yield return Fade(0f, 1f, fadeInDuration);

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        yield return Fade(1f, 0f, fadeOutDuration);

        activeCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
