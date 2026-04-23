using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string settingsSceneName = "Settings";

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnNewGame()
    {
        PlayerPrefs.DeleteKey("SaveData");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnContinue()
    {
        if (PlayerPrefs.HasKey("SaveData"))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.Log("No save found.");
    }

    public void OnSettings()
    {
        SceneManager.LoadScene(settingsSceneName, LoadSceneMode.Additive);
    }

    public void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}