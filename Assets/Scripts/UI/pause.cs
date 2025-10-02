using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;           // Drag UIDocument from child object
    [SerializeField] private VisualTreeAsset pauseMenuUXML;   // Drag UXML file here
    [SerializeField] private StyleSheet pauseMenuStyle;       // Drag USS file here
    [SerializeField] private Player player;                   // Drag Player script from child object

    private VisualElement pauseMenuRoot;
    private bool isPaused = false;

    private void Start()
    {
        // Check required references
        if (!uiDocument || !pauseMenuUXML || !pauseMenuStyle)
        {
            Debug.LogError("PauseMenu: Missing UI references!");
            enabled = false;
            return;
        }

        // Root element from UIDocument
        var root = uiDocument.rootVisualElement;

        // Clone UXML & attach stylesheet
        pauseMenuRoot = pauseMenuUXML.CloneTree();
        pauseMenuRoot.styleSheets.Add(pauseMenuStyle);
        root.Add(pauseMenuRoot);

        // Start hidden
        pauseMenuRoot.style.display = DisplayStyle.None;

        // Setup button events
        var resumeButton = pauseMenuRoot.Q<Button>("ResumeButton");
        var quitButton = pauseMenuRoot.Q<Button>("QuitButton");

        if (resumeButton != null) resumeButton.clicked += TogglePause;
        else Debug.LogWarning("PauseMenu: ResumeButton missing in UXML");

        if (quitButton != null) quitButton.clicked += QuitGame;
        else Debug.LogWarning("PauseMenu: QuitButton missing in UXML");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc pressed");
            TogglePause();
        }
    }

    private void TogglePause()
    {
        Debug.Log("Toggle Pause called");
        isPaused = !isPaused;

        // Show/hide menu
        if (pauseMenuRoot != null)
            pauseMenuRoot.style.display = isPaused ? DisplayStyle.Flex : DisplayStyle.None;

        // Enable/disable gameplay controls
        if (player != null)
            player.enabled = !isPaused;

        // Pause/resume game time
        Time.timeScale = isPaused ? 0f : 1f;

        // Lock/unlock cursor
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Safety: restore settings if destroyed while paused
        if (isPaused)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
