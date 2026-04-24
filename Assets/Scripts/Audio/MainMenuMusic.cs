using UnityEngine;

/// <summary>
/// Plays looping background music on the main menu.
/// Lives only in the MainMenu scene — destroyed automatically on scene change.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MainMenuMusic : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private void Start()
    {
        if (musicClip == null)
        {
            Debug.LogWarning("[MainMenuMusic] No music clip assigned.");
            return;
        }

        var source = GetComponent<AudioSource>();
        source.clip = musicClip;
        source.loop = true;
        source.volume = volume;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.Play();
    }
}
