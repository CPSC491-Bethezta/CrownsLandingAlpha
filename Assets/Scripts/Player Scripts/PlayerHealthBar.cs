using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private StatsProfile statsProfile;
    [SerializeField] private Slider healthSlider;

    private void Start()
    {
        if (statsProfile == null)
            statsProfile = FindObjectOfType<StatsProfile>();

        UpdateHealthBar();

        statsProfile.OnResourceChanged += UpdateHealthBar;
    }

    private void OnDestroy()
    {
        if (statsProfile != null)
            statsProfile.OnResourceChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar()
    {
        if (statsProfile == null || healthSlider == null)
            return;

        int currentHealth = (int)statsProfile
            .GetType()
            .GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(statsProfile);

        int maxHealth = (int)statsProfile
            .GetType()
            .GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(statsProfile);

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
