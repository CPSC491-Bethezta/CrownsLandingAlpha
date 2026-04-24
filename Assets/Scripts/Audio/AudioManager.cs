using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton that manages background music, crossfading between adventure and combat tracks.
/// Enemies call RegisterCombat / UnregisterCombat to signal combat state changes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Tracks")]
    [SerializeField] private AudioClip[] adventureTracks;
    [SerializeField] private AudioClip[] combatTracks;

    [Header("Settings")]
    [SerializeField] private float crossfadeDuration = 1.5f;
    [SerializeField] private float postCombatDelay = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    private AudioSource _sourceA;
    private AudioSource _sourceB;

    private int _activeCombatCount;
    private bool _inCombat;
    private int _lastCombatTrackIndex = -1;

    private Coroutine _crossfadeCoroutine;
    private Coroutine _postCombatCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sourceA = gameObject.AddComponent<AudioSource>();
        _sourceB = gameObject.AddComponent<AudioSource>();

        _sourceA.loop = true;
        _sourceB.loop = true;
        _sourceA.playOnAwake = false;
        _sourceB.playOnAwake = false;
        _sourceA.spatialBlend = 0f;
        _sourceB.spatialBlend = 0f;
    }

    private void Start()
    {
        PlayAdventureMusic();
    }

    /// <summary>
    /// Call when an enemy locks on to the player.
    /// </summary>
    public static void RegisterCombat()
    {
        if (Instance == null) return;
        Instance._activeCombatCount++;
        Instance.OnCombatCountChanged();
    }

    /// <summary>
    /// Call when an enemy loses or abandons the player target.
    /// </summary>
    public static void UnregisterCombat()
    {
        if (Instance == null) return;
        Instance._activeCombatCount = Mathf.Max(0, Instance._activeCombatCount - 1);
        Instance.OnCombatCountChanged();
    }

    private void OnCombatCountChanged()
    {
        if (_activeCombatCount > 0 && !_inCombat)
        {
            if (_postCombatCoroutine != null)
            {
                StopCoroutine(_postCombatCoroutine);
                _postCombatCoroutine = null;
            }

            _inCombat = true;
            PlayCombatMusic();
        }
        else if (_activeCombatCount == 0 && _inCombat)
        {
            _inCombat = false;

            if (_postCombatCoroutine != null)
            {
                StopCoroutine(_postCombatCoroutine);
            }

            _postCombatCoroutine = StartCoroutine(PostCombatDelay());
        }
    }

    private IEnumerator PostCombatDelay()
    {
        yield return new WaitForSeconds(postCombatDelay);
        PlayAdventureMusic();
        _postCombatCoroutine = null;
    }

    private void PlayAdventureMusic()
    {
        if (adventureTracks == null || adventureTracks.Length == 0) return;
        CrossfadeTo(adventureTracks[Random.Range(0, adventureTracks.Length)]);
    }

    private void PlayCombatMusic()
    {
        if (combatTracks == null || combatTracks.Length == 0) return;

        int index = 0;
        if (combatTracks.Length > 1)
        {
            do { index = Random.Range(0, combatTracks.Length); }
            while (index == _lastCombatTrackIndex);
        }

        _lastCombatTrackIndex = index;
        CrossfadeTo(combatTracks[index]);
    }

    private void CrossfadeTo(AudioClip clip)
    {
        if (_crossfadeCoroutine != null)
        {
            StopCoroutine(_crossfadeCoroutine);
        }

        _crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(clip));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        _sourceB.clip = newClip;
        _sourceB.volume = 0f;
        _sourceB.Play();

        float elapsed = 0f;
        float startVolumeA = _sourceA.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);
            _sourceA.volume = Mathf.Lerp(startVolumeA, 0f, t);
            _sourceB.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        _sourceA.Stop();
        _sourceA.clip = null;

        (_sourceA, _sourceB) = (_sourceB, _sourceA);
        _crossfadeCoroutine = null;
    }
}
