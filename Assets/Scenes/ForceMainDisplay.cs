using UnityEngine;

public class ForceMainDisplay : MonoBehaviour
{
    void Awake()
    {
        // Force Unity to use primary display
        Screen.fullScreen = false; // go windowed first (important)
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
    }
}
