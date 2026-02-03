using UnityEngine;
using UnityEngine.UI;

public class HUD_PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button exitButton;

    private void OnEnable()
    {
        GameManager.Instance.InputManager.OnPause += PauseMenuActivation;
        GameManager.Instance.InputManager.OnUnpause += PauseMenuActivation;
    }
    
    private void OnDisable()
    {
        GameManager.Instance.InputManager.OnPause -= PauseMenuActivation;
        GameManager.Instance.InputManager.OnUnpause -= PauseMenuActivation;
    }

    private void Start()
    {
        resumeButton.onClick.AddListener(PauseMenuActivation);
        resumeButton.onClick.AddListener(Resume);
        retryButton.onClick.AddListener(ClickRetry);
        menuButton.onClick.AddListener(ToMainMenu);
        exitButton.onClick.AddListener(ClickExit);
    }

    public void PauseMenuActivation() => panel.SetActive(!panel.activeSelf);

    public void ToMainMenu()
    {
        GameManager.Instance.InputManager.SwitchInputMap(InputMap.Menu);
        GameManager.Instance.LoadScene(Scenes.MainMenu);
    }

    public void Resume()
    {
        GameManager.Instance.InputManager.SwitchInputMap(InputMap.Gameplay);
        GameManager.Instance.StateManager.SwitchState<GameplayState>();
    }

    public void ClickRetry()
    {
        RaceSettingsManager.Instance.SetOponentsAmount();
        GameManager.Instance.LoadScene(Scenes.Gameplay);
    }

    public void ClickExit()
    {
        // Quit the game if you are playing it in Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

        // Quit the game of you are playing it in the game application
        #endif
            Application.Quit();
    }
}