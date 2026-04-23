using UnityEngine;
using UnityEngine.UI;

public class PlayerXP : MonoBehaviour
{
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    public Slider xpBar;

    void Start()
    {
        UpdateXPUI();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateXPUI();
    }

    void LevelUp()
    {
        currentXP = 0;
        Debug.Log("Level Up!");
    }

    void UpdateXPUI()
    {
        if (xpBar != null)
        {
            xpBar.value = (float)currentXP / xpToNextLevel;
        }
    }
}