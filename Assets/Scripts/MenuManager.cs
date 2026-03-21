using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button exitGameButton;

    private void Start()
    {
        // Ensure the cursor is visible in the menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Assign button listeners
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(ExitGame);
    }

    public void StartGame()
    {
        // Load your game scene (replace "MapScene" with your actual game scene name)
        SceneManager.LoadScene("DemoScene_Huy");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
