using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Plays an AudioClip when the pointer enters the attached UI element.
/// Attach to any GameObject with a raycast-receiving Graphic (e.g., Button).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Tooltip("AudioClip to play on hover.")]
    [SerializeField] private AudioClip hoverClip;

    [Tooltip("Volume for the hover sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Called by the EventSystem when the pointer enters this UI element.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip == null) return;
        _audioSource.PlayOneShot(hoverClip, volume);
    }
}

