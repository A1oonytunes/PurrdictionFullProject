using UnityEngine;
using UnityEngine.InputSystem; // <-- Add this namespace
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument pauseMenuDocument;

    [Header("Input Control")]
    [Tooltip("Reference to the PlayerInput component on the player GameObject.")]
    [SerializeField] private PlayerInput playerInput;

    private VisualElement panel;
    private Button resumeButton;
    private Button settingsButton;
    private Button quitButton;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuDocument == null) pauseMenuDocument = GetComponentInChildren<UIDocument>();
        SetupUI();

        // On game start, ensure the UI action map is NOT active.
        // The PlayerInput component's "Default Scheme" should be set to "Player".
    }

    void SetupUI()
    {
        if (pauseMenuDocument == null) return;
        var root = pauseMenuDocument.rootVisualElement;

        panel = root.Q<VisualElement>("Panel");
        resumeButton = root.Q<Button>("Resume");
        settingsButton = root.Q<Button>("Settings");
        quitButton = root.Q<Button>("Quit");

        if (resumeButton != null) resumeButton.clicked += ResumeGame;
        if (settingsButton != null) settingsButton.clicked += OpenSettings;
        if (quitButton != null) quitButton.clicked += QuitGame;

        HidePauseMenu();
    }

    void Update()
    {
        // It's best practice to have the Pause action inside one of your action maps.
        // For simplicity, we'll keep using GetKeyDown, which works regardless of the active map.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
            ShowPauseMenu();
        else
            ResumeGame();
    }

    public void ShowPauseMenu()
    {
        if (panel == null || playerInput == null) return;

        isPaused = true;
        panel.style.display = DisplayStyle.Flex;

        // The core logic: Tell the PlayerInput component to switch its active map.
        // This automatically disables "Player" and enables "UI".
        playerInput.SwitchCurrentActionMap("UI");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (panel == null || playerInput == null) return;

        isPaused = false;
        HidePauseMenu();

        // The core logic: Switch the active map back to "Player".
        // This automatically disables "UI" and re-enables "Player".
        playerInput.SwitchCurrentActionMap("Player");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HidePauseMenu()
    {
        if (panel != null)
        {
            panel.style.display = DisplayStyle.None;
        }
    }

    private void OpenSettings()
    {
        Debug.Log("Settings button clicked!");
    }

    private void QuitGame()
    {
        Debug.Log("Quit button clicked!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }
}