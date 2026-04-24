using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to any GameObject that carries an EventSystem or AudioListener.
/// Immediately disables duplicate components on Awake and whenever a new scene loads.
/// The first instance to initialize wins; later duplicates are destroyed.
/// </summary>
public class SingletonEnforcer : MonoBehaviour
{
    private void Awake()
    {
        CleanDuplicates();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanDuplicates();
    }

    private void CleanDuplicates()
    {
        CleanDuplicateEventSystems();
        CleanDuplicateAudioListeners();
    }

    /// <summary>Keeps the first active EventSystem and disables + destroys all others.</summary>
    private static void CleanDuplicateEventSystems()
    {
        var all = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (all.Length <= 1) return;

        EventSystem first = EventSystem.current != null ? EventSystem.current : all[0];

        foreach (var es in all)
        {
            if (es == first) continue;

            // Disable immediately to suppress warnings before end-of-frame Destroy
            es.enabled = false;
            Debug.Log($"[SingletonEnforcer] Destroying duplicate EventSystem on '{es.gameObject.name}'.");
            Destroy(es);

            var input = es.GetComponent<BaseInputModule>();
            if (input != null)
            {
                input.enabled = false;
                Destroy(input);
            }
        }
    }

    /// <summary>Keeps the first active AudioListener and disables + destroys all others.</summary>
    private static void CleanDuplicateAudioListeners()
    {
        var all = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (all.Length <= 1) return;

        foreach (var listener in all)
        {
            if (listener == all[0]) continue;

            listener.enabled = false;
            Debug.Log($"[SingletonEnforcer] Destroying duplicate AudioListener on '{listener.gameObject.name}'.");
            Destroy(listener);
        }
    }
}
