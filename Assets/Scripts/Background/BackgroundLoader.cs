using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundSceneLoader : MonoBehaviour
{
    [SerializeField] private string backgroundSceneName = "GameScene";
    [SerializeField] private float loadDelay = 1f;

    void Start()
    {
        Invoke("LoadBackground", loadDelay);
    }

    void LoadBackground()
    {
        SceneManager.LoadScene(backgroundSceneName, LoadSceneMode.Additive);
        Invoke("FixCursor", 0.5f);
    }

    void FixCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}