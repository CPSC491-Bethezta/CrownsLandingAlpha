using UnityEngine;

/// <summary>
/// Plays a one-shot AudioClip at the start of the scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SceneStartSound : MonoBehaviour
{
    [Tooltip("AudioClip to play when the scene loads.")]
    [SerializeField] private AudioClip clip;

    [Tooltip("Playback volume.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private void Start()
    {
        if (clip == null)
        {
            Debug.LogWarning("[SceneStartSound] No clip assigned.");
            return;
        }

        GetComponent<AudioSource>().PlayOneShot(clip, volume);
    }
}
